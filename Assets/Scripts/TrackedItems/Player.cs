using UnityEngine;
using Unity.XR.CoreUtils;

// Keeps the existing Inspector layout and XR-following behavior.
[System.Serializable]
public class Controller
{
    public XROrigin xrRig;
    public Rigidbody body;
    public Vector3 rigOffset;

    private Transform avatar;
    private Transform rig;
    private Vector3 rigLastPosition;

    public void Initialize(Transform playerTransform) => avatar = playerTransform;

    public void Embody(XROrigin origin)
    {
        xrRig = origin;
        rig = origin.transform;
        rig.rotation = avatar.rotation;
        AlignRig();
    }

    public void Release()
    {
        rig = null;
        xrRig = null;
    }

    public void AlignRig()
    {
        if (rig == null) return;
        rig.position = avatar.position + rig.rotation * rigOffset;
        rigLastPosition = rig.position;
    }

    public void Turn(Quaternion direction)
    {
        avatar.rotation = direction;
        if (rig != null) rig.rotation = direction;
    }

    public void FollowRig()
    {
        if (rig == null || Time.deltaTime <= 0f) return;
        Vector3 velocity = (rig.position - rigLastPosition) / Time.deltaTime;
        velocity.y = body.linearVelocity.y;
        body.linearVelocity = velocity;
        AlignRig();
        avatar.rotation = rig.rotation;
    }
}

/// <summary>Owns identity, XR possession, and the chair's physics motor.</summary>
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour, IRegistryProvider
{
    // 1. Identity and existing XR references.
    [SerializeField] private Team team;
    [SerializeField] private Role role;
    [SerializeField] private Zone zone;
    [SerializeField] private Controller avatar = new();

    [Header("AI driving")]
    [SerializeField, Min(0.1f)] private float driveSpeed = 3f;
    [SerializeField, Min(0.1f)] private float driveAcceleration = 8f;
    [SerializeField, Min(1f)] private float turnSpeed = 100f;

    public RegistryItem item { get; private set; }
    public Team Team => team;
    public Role Role => role;
    public static Player PossessedPlayer { get; private set; }

    private Vector3 spawnPosition;
    private Vector3 driveDirection;
    private float speedScale;
    private float commandTime;
    private Bounds? driveBounds;
    private Collider chassis;
    private PhysicsMaterial originalMaterial;
    private PhysicsMaterial rollingMaterial;

    // 2. Initialize once; let every player's Awake finish before claiming the XR rig.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPossession() => PossessedPlayer = null;

    private void Awake()
    {
        avatar ??= new Controller();
        if (avatar.body == null) avatar.body = GetComponent<Rigidbody>();
        avatar.Initialize(transform);
        spawnPosition = transform.position;
        // The motor supplies rolling resistance and braking; a sliding box's friction
        // would otherwise pin the chair to the court. Keep the human's material intact.
        if (TryGetComponent(out chassis))
        {
            originalMaterial = chassis.sharedMaterial;
            rollingMaterial = originalMaterial != null ? Instantiate(originalMaterial) : new PhysicsMaterial();
            rollingMaterial.name = "AI chair rolling contact";
            rollingMaterial.staticFriction = rollingMaterial.dynamicFriction = 0f;
            rollingMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
        }
        item = new RegistryItem(team.ToString() + role.ToString(), gameObject);
    }

    private void Start()
    {
        WorldRegister.Register(item);
        if (avatar.xrRig != null && PossessedPlayer == null) TakeControl(avatar.xrRig);
    }

    private void OnEnable() => UpdateContactMaterial();

    private void Update()
    {
        if (IsPossessed()) avatar.FollowRig();
    }

    private void OnDisable()
    {
        StopDriving();
        ReleaseControl();
        UpdateContactMaterial();
    }

    private void OnDestroy()
    {
        if (item != null) WorldRegister.UnRegister(item);
        if (rollingMaterial != null) Destroy(rollingMaterial);
    }

    // 3. A future selection screen calls this on the chosen player before the match.
    public bool IsPossessed() => PossessedPlayer == this;

    public bool TakeControl(XROrigin origin)
    {
        if (origin == null || !isActiveAndEnabled || avatar.body == null) return false;
        if (IsPossessed() && avatar.xrRig == origin) return true;

        PossessedPlayer?.ReleaseControl();
        StopDriving();
        ResetVelocity();
        PossessedPlayer = this;
        avatar.Embody(origin);
        UpdateContactMaterial();
        return true;
    }

    public void ReleaseControl()
    {
        if (!IsPossessed()) return;
        PossessedPlayer = null;
        avatar.Release();
        UpdateContactMaterial();
        StopDriving();
        ResetVelocity();
    }

    private void UpdateContactMaterial()
    {
        if (chassis != null)
            chassis.sharedMaterial = isActiveAndEnabled && !IsPossessed() ? rollingMaterial : originalMaterial;
    }

    // 4. AI submits a direction; only the physics loop drives the chair.
    public void Drive(Vector3 direction, float scale = 1f, Bounds? bounds = null)
    {
        if (IsPossessed()) return;
        direction.y = 0f;
        driveDirection = direction.normalized;
        speedScale = Mathf.Clamp01(scale);
        driveBounds = bounds;
        commandTime = Time.time;
    }

    public void StopDriving()
    {
        driveDirection = Vector3.zero;
        speedScale = 0f;
        driveBounds = null;
    }

    private void FixedUpdate()
    {
        if (IsPossessed() || avatar.body.isKinematic) return;
        if (Time.time - commandTime > 0.25f) StopDriving();

        Rigidbody body = avatar.body;
        Vector3 desiredVelocity = Vector3.zero;
        if (driveDirection.sqrMagnitude > 0f)
        {
            Quaternion rotation = Quaternion.RotateTowards(body.rotation,
                Quaternion.LookRotation(driveDirection), turnSpeed * Time.fixedDeltaTime);
            body.MoveRotation(rotation);
            Vector3 forward = rotation * Vector3.forward;
            forward.y = 0f;
            forward.Normalize();

            // Turn in place for sharp corners; accelerate as the chair faces its route.
            float alignment = Mathf.InverseLerp(0.5f, 1f, Vector3.Dot(forward, driveDirection));
            desiredVelocity = forward * (driveSpeed * speedScale * alignment);
        }

        Vector3 velocity = body.linearVelocity;
        velocity.y = 0f;
        Vector3 nextVelocity = Vector3.MoveTowards(velocity, desiredVelocity, driveAcceleration * Time.fixedDeltaTime);
        if (driveBounds.HasValue)
        {
            Bounds bounds = driveBounds.Value;
            // Limit motor-driven travel at the pen edge. External collision impulses remain physical.
            nextVelocity.x = Mathf.Clamp(nextVelocity.x,
                Mathf.Min(0f, (bounds.min.x - body.position.x) / Time.fixedDeltaTime),
                Mathf.Max(0f, (bounds.max.x - body.position.x) / Time.fixedDeltaTime));
            nextVelocity.z = Mathf.Clamp(nextVelocity.z,
                Mathf.Min(0f, (bounds.min.z - body.position.z) / Time.fixedDeltaTime),
                Mathf.Max(0f, (bounds.max.z - body.position.z) / Time.fixedDeltaTime));
        }
        body.AddForce((nextVelocity - velocity) / Time.fixedDeltaTime, ForceMode.Acceleration);
    }

    // 5. Reset in world space, matching Pen.GetPosition and Field.GetPosition.
    public void ResetPlayer()
    {
        StopDriving();
        Vector3 position = spawnPosition;
        if (WorldRegister.TryGetItem(team.ToString() + zone.ToString(), out var location)
            && location.gameObject != null)
            position = location.gameObject.transform.position;

        // Zone markers may sit on the floor; preserve the chair pivot clearance.
        position.y = spawnPosition.y;
        avatar.body.position = position;
        transform.position = position;
        Quaternion rotation = Quaternion.Euler(0f, team == Team.Away ? 270f : 90f, 0f);
        avatar.body.rotation = rotation;
        avatar.Turn(rotation);
        ResetVelocity();
        avatar.AlignRig();
        if (TryGetComponent<PlayerNavigation>(out var navigation)) navigation.ClearPath();
    }

    private void ResetVelocity()
    {
        if (avatar.body.isKinematic) return;
        avatar.body.linearVelocity = Vector3.zero;
        avatar.body.angularVelocity = Vector3.zero;
    }

    public Vector3 GetPosition() => transform.position;
}