using UnityEngine;
using Unity.XR.CoreUtils;

[System.Serializable]
public class XRPossession
{
    public XROrigin rig;
    public Vector3 offset;
    public Rigidbody rb;

    private static bool claimed;
    private Transform body;
    private Transform origin;
    private Vector3 prevPos;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetClaim() => claimed = false;

    private void Move() => origin.position = prevPos = body.position + origin.rotation * offset;

    public void Init(Transform player)
    {
        body = player;
        if (claimed || rig == null || rb == null) return;

        claimed = true;
        origin = rig.transform;
        Move();
    }

    public void Face(Quaternion rotation)
    {
        body.rotation = rotation;
        if (origin != null) origin.rotation = rotation;
    }

    public void Follow()
    {
        if (origin == null) return;

        rb.linearVelocity = (origin.position - prevPos) / Time.deltaTime;
        Move();
        body.rotation = origin.rotation;
    }
}

public class Player : MonoBehaviour, IRegistryProvider
{
    public RegistryItem item {get; private set;}

    [SerializeField] private string id;
    [SerializeField] private Team team;
    [SerializeField] private Role role;
    [SerializeField] private XRPossession Controller;

    private void ConstructRegistryItem() => item = new RegistryItem(id, gameObject);

    void Awake()
    {
        ConstructRegistryItem();
        Controller.Init(transform);
        Controller.Face(transform.rotation);
    }

    void Start() => WorldRegister.Register(item);
    void OnDestroy() => WorldRegister.UnRegister(item);

    public Vector3 GetPosition() => transform.position;

    private Vector3 SpawnPosition(string side)
    {
        if (id.EndsWith("Goaly"))
            return WorldRegister.GetItem(side + "Pen").Get<Pen>().GetPosition();

        string field = id.Contains("Left") ? "LeftField" : "RightField";
        return WorldRegister.GetItem(side + field).Get<Field>().GetPosition();
    }

    public void ResetPlayer()
    {
        bool away = id.StartsWith("Away");

        Controller.Face(Quaternion.Euler(0f, away ? 270f : 90f, 0f));
        transform.localPosition = SpawnPosition(away ? "Away" : "Home");
    }

    void Update() => Controller.Follow();
}