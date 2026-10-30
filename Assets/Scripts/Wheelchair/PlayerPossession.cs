using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

// Puts the player in one of the chairs on the field ("possessing" it):
//  - attaches the XR rig to the chair's SeatAnchor so the player moves with the chair,
//  - switches off the rig's own movement so the left stick only drives the chair,
//  - turns on that chair's PlayerWheelchairDriver,
//  - hides the chair's head so the camera isn't inside it.
// Goes on the XR Origin (XR Rig).
[RequireComponent(typeof(XROrigin))]
public class PlayerPossession : MonoBehaviour
{
    [Header("Chairs")]
    [Tooltip("Registry id of the chair the player starts in.")]
    [SerializeField] private string startingId = "HomeLeftFielder";

    [Tooltip("Registry ids of the chairs the player is allowed to take over.")]
    [SerializeField] private string[] possessableIds = { "HomeLeftFielder", "HomeRightFielder", "HomeGoaly" };

    [Tooltip("Name of the child object on each chair where the player's eyes go.")]
    [SerializeField] private string seatAnchorName = "SeatAnchor";

    [Header("Rig")]
    [Tooltip("Rig objects to switch off while seated. If left empty, the rig's Locomotion object is found automatically.")]
    [SerializeField] private List<GameObject> disableWhileSeated = new List<GameObject>();

    [Header("Hide Own Head")]
    [Tooltip("Name of the head bone in the player model. It is shrunk to nothing on the chair you're in.")]
    [SerializeField] private string headBoneName = "spine.006";

    [Header("Recentering")]
    [Tooltip("Seconds to wait for the headset to start tracking before lining the view up with the seat anyway.")]
    [SerializeField, Min(0f)] private float trackingWaitSeconds = 3f;

    private const float HiddenScale = 0.0001f;

    private XROrigin xrOrigin;
    private CharacterController characterController;
    private Transform originalParent;

    private readonly Dictionary<GameObject, bool> savedActiveStates = new Dictionary<GameObject, bool>();
    private bool characterControllerWasEnabled;

    private Transform seat;
    private PlayerWheelchairDriver driver;
    private Transform hiddenHead;
    private Vector3 hiddenHeadScale;
    private Coroutine recenterRoutine;

    /// <summary>Registry id of the chair the player is in, or null.</summary>
    public string CurrentId { get; private set; }

    /// <summary>The chair the player is in, or null.</summary>
    public GameObject CurrentChair { get; private set; }

    public bool IsSeated => CurrentChair != null;

    private void Awake()
    {
        xrOrigin = GetComponent<XROrigin>();
        characterController = GetComponent<CharacterController>();
        originalParent = transform.parent;

        if (disableWhileSeated.Count == 0)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
                if (child.name == "Locomotion") disableWhileSeated.Add(child.gameObject);
        }
    }

    private IEnumerator Start()
    {
        // Chairs add themselves to the WorldRegister in their own Start, so wait one frame first.
        yield return null;
        Possess(startingId);
    }

    private void LateUpdate()
    {
        // Re-apply every frame in case an Animator on the model resets the head bone.
        if (hiddenHead != null) hiddenHead.localScale = Vector3.one * HiddenScale;
    }

    /// <summary>Moves the player into the chair with this registry id. Returns false if that isn't possible.</summary>
    public bool Possess(string id)
    {
        if (System.Array.IndexOf(possessableIds, id) < 0)
        {
            Debug.LogError($"PlayerPossession: '{id}' is not in the list of possessable chairs.", this);
            return false;
        }

        if (!WorldRegister.TryGetItem(id, out RegistryItem item))
        {
            Debug.LogError($"PlayerPossession: no chair is registered with the id '{id}'.", this);
            return false;
        }

        GameObject chair = item.gameObject;
        if (chair == CurrentChair) return true;

        Transform newSeat = chair.transform.Find(seatAnchorName);
        PlayerWheelchairDriver newDriver = chair.GetComponent<PlayerWheelchairDriver>();
        if (newSeat == null || newDriver == null)
        {
            Debug.LogError($"PlayerPossession: '{id}' needs a child called '{seatAnchorName}' and a PlayerWheelchairDriver component.", chair);
            return false;
        }

        if (IsSeated) ReleaseCurrentChair();
        else SetRigMovement(false);

        CurrentId = id;
        CurrentChair = chair;
        seat = newSeat;
        driver = newDriver;

        transform.SetParent(seat, true);
        driver.enabled = true;
        // An AI driver on this chair (still to be written) gets switched off here too.
        HideHead(chair);

        Recenter();
        if (recenterRoutine != null) StopCoroutine(recenterRoutine);
        recenterRoutine = StartCoroutine(RecenterOnceTracking());
        return true;
    }

    /// <summary>Takes the player out of their chair and gives the rig its own movement back.</summary>
    public void Unpossess()
    {
        if (!IsSeated) return;

        if (recenterRoutine != null) StopCoroutine(recenterRoutine);
        recenterRoutine = null;

        ReleaseCurrentChair();
        SetRigMovement(true);
    }

    /// <summary>Lines the player's view up with the seat: eyes at the SeatAnchor, facing the chair's forward.</summary>
    public void Recenter()
    {
        if (seat == null) return;

        xrOrigin.MatchOriginUpCameraForward(seat.up, seat.forward);
        xrOrigin.MoveCameraToWorldLocation(seat.position);
    }

    private void ReleaseCurrentChair()
    {
        if (driver != null) driver.enabled = false;
        RestoreHead();
        transform.SetParent(originalParent, true);

        CurrentId = null;
        CurrentChair = null;
        seat = null;
        driver = null;
    }

    // The headset can take a few frames to start reporting where your head is,
    // so line the view up again once it does.
    private IEnumerator RecenterOnceTracking()
    {
        float giveUpAt = Time.realtimeSinceStartup + trackingWaitSeconds;
        while (!IsHeadTracked() && Time.realtimeSinceStartup < giveUpAt)
            yield return null;

        yield return null; // let the camera pick up the tracked pose
        Recenter();
        recenterRoutine = null;
    }

    private static bool IsHeadTracked()
    {
        InputDevice head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        return head.isValid && head.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked) && tracked;
    }

    private void SetRigMovement(bool on)
    {
        if (!on)
        {
            savedActiveStates.Clear();
            foreach (GameObject obj in disableWhileSeated)
            {
                if (obj == null) continue;
                savedActiveStates[obj] = obj.activeSelf;
                obj.SetActive(false);
            }

            if (characterController != null)
            {
                characterControllerWasEnabled = characterController.enabled;
                characterController.enabled = false;
            }
        }
        else
        {
            foreach (KeyValuePair<GameObject, bool> saved in savedActiveStates)
                if (saved.Key != null) saved.Key.SetActive(saved.Value);
            savedActiveStates.Clear();

            if (characterController != null) characterController.enabled = characterControllerWasEnabled;
        }
    }

    private void HideHead(GameObject chair)
    {
        foreach (Transform child in chair.GetComponentsInChildren<Transform>(true))
        {
            if (child.name != headBoneName) continue;

            hiddenHead = child;
            hiddenHeadScale = child.localScale;
            child.localScale = Vector3.one * HiddenScale;
            return;
        }

        Debug.LogWarning($"PlayerPossession: no head bone called '{headBoneName}' found on {chair.name}.", chair);
    }

    private void RestoreHead()
    {
        if (hiddenHead != null) hiddenHead.localScale = hiddenHeadScale;
        hiddenHead = null;
    }
}
