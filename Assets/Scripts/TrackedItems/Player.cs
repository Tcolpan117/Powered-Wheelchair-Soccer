using UnityEngine;
using Unity.XR.CoreUtils;

[System.Serializable]
public class Controller
{
    public XROrigin xrRig;
    public Rigidbody body;
    public Vector3 rigOffset;

    private Transform avatar;
    private Transform rig;
    private Vector3 rigLastPosition;

    private static bool isPossessed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetIsPossessed() => isPossessed = false;

    private void PlaceRigInAvatar()
    {
        rig.position = avatar.position + rig.rotation * rigOffset;
        rigLastPosition = rig.position;
    }

    public void Embody(Transform avatarTransform)
    {
        avatar = avatarTransform;

        if (isPossessed || xrRig == null || body == null) return;

        isPossessed = true;
        rig = xrRig.transform;
        rig.rotation = avatar.rotation;
        PlaceRigInAvatar();
    }

    public void Turn(Quaternion direction)
    {
        avatar.rotation = direction;
        if (rig != null) rig.rotation = direction;
    }

    public void FollowRig()
    {
        if (rig == null) return;

        Vector3 velocity = (rig.position - rigLastPosition) / Time.deltaTime;
        velocity.y = body.linearVelocity.y; // gravity decides falling, not the rig
        body.linearVelocity = velocity;

        PlaceRigInAvatar();
        avatar.rotation = rig.rotation;
    }
}

public class Player : MonoBehaviour, IRegistryProvider
{
    public RegistryItem item { get; private set; }

    [SerializeField] private string id;
    [SerializeField] private Team team;
    [SerializeField] private Role role;
    [SerializeField] private Controller avatar;

    private void ConstructRegistryItem() => item = new RegistryItem(id, gameObject);

    void Awake()
    {
        ConstructRegistryItem();
        avatar.Embody(transform);
    }

    void Start() => WorldRegister.Register(item);
    void Update() => avatar.FollowRig();
    void OnDestroy() => WorldRegister.UnRegister(item);
    

    private Vector3 SpawnPosition(string teamSide)
    {
        bool isGoalie = id.EndsWith("Goaly");
        if (isGoalie)
            return WorldRegister.GetItem(teamSide + "Pen").Get<Pen>().GetPosition();

        string fieldName = id.Contains("Left") ? "LeftField" : "RightField";
        return WorldRegister.GetItem(teamSide + fieldName).Get<Field>().GetPosition();
    }

    public void ResetPlayer()
    {
        bool isAway = id.StartsWith("Away");
        string teamSide = isAway ? "Away" : "Home";
        float facingAngle = isAway ? 270f : 90f;

        avatar.Turn(Quaternion.Euler(0f, facingAngle, 0f));
        transform.localPosition = SpawnPosition(teamSide);
    }

   

    public Vector3 GetPosition() => transform.position;
}