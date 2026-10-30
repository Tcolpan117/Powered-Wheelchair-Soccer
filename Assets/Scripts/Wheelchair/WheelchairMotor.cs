using UnityEngine;

// Moves a wheelchair like a real power chair: the joystick sets a target speed and turn rate,
// and the chair accelerates toward them.
//
// Anything can drive a chair by calling SetDriveInput every frame: the player's input script,
// or an AI script. Only one driver should control a chair at a time.
[RequireComponent(typeof(Rigidbody))]
public class WheelchairMotor : MonoBehaviour
{
    [SerializeField] private WheelchairStats stats;

    [Header("Stability")]
    [Tooltip("Stops the chair tipping over by locking rotation on X and Z.")]
    [SerializeField] private bool lockTipping = true;

    [Header("Ground Check")]
    [Tooltip("Collider used to find the bottom of the chair. Uses the collider on this object if left empty.")]
    [SerializeField] private Collider bodyCollider;

    [Tooltip("How far below the chair the ground can be and still count as touching it, in meters.")]
    [SerializeField, Min(0f)] private float groundCheckDistance = 0.1f;

    [SerializeField] private LayerMask groundLayers = ~0;

    private Rigidbody rb;
    private Vector2 stickInput;    // x = turn, y = forward
    private float targetSpeed;     // m/s the chair's controller is currently asking for
    private float targetTurnRate;  // degrees per second the controller is currently asking for

    public WheelchairStats Stats => stats;

    /// <summary>The stick input being used right now (x = turn, y = forward).</summary>
    public Vector2 DriveInput => stickInput;

    public bool IsGrounded { get; private set; }

    /// <summary>Actual speed along the chair's forward direction in m/s. Negative when reversing.</summary>
    public float ForwardSpeed => rb != null ? Vector3.Dot(rb.linearVelocity, Flat(transform.forward)) : 0f;

    /// <summary>Actual turn rate in degrees per second. Positive = turning right.</summary>
    public float TurnRate => rb != null ? rb.angularVelocity.y * Mathf.Rad2Deg : 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (bodyCollider == null) bodyCollider = GetComponent<Collider>();

        if (stats == null)
        {
            Debug.LogWarning($"{name}: no WheelchairStats assigned, using default values.", this);
            stats = ScriptableObject.CreateInstance<WheelchairStats>();
        }

        if (lockTipping)
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    /// <summary>
    /// Joystick input. x = turn (-1 left, 1 right), y = drive (-1 reverse, 1 forward).
    /// Call this every frame from whatever is driving the chair. Small values are used exactly as given,
    /// so AI can make fine adjustments. Any deadzone belongs in the driver, not here.
    /// </summary>
    public void SetDriveInput(Vector2 stick) => stickInput = Vector2.ClampMagnitude(stick, 1f);

    /// <summary>Clears input and any speed the controller was building up. Call after teleporting the chair.</summary>
    public void Stop()
    {
        stickInput = Vector2.zero;
        targetSpeed = 0f;
        targetTurnRate = 0f;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        UpdateTargets(dt);

        IsGrounded = CheckGrounded();
        if (!IsGrounded || rb.isKinematic) return; // wheels off the ground can't push

        ApplyDrive(dt);
        ApplyTurn(dt);
    }

    // What the chair's controller is asking for, ramped up and down so the chair doesn't jerk.
    private void UpdateTargets(float dt)
    {
        float maxSpeed = stickInput.y >= 0f ? stats.maxForwardSpeed : stats.maxReverseSpeed;
        float wantedSpeed = stickInput.y * maxSpeed;

        bool speedingUp = Mathf.Abs(wantedSpeed) > Mathf.Abs(targetSpeed)
                          && (targetSpeed == 0f || Mathf.Sign(wantedSpeed) == Mathf.Sign(targetSpeed));
        float speedRate = speedingUp ? stats.acceleration : stats.braking;
        targetSpeed = Mathf.MoveTowards(targetSpeed, wantedSpeed, speedRate * dt);

        // Turn slower the faster the chair is going.
        float speedFraction = stats.maxForwardSpeed > 0f
            ? Mathf.Clamp01(Mathf.Abs(targetSpeed) / stats.maxForwardSpeed)
            : 0f;
        float turnLimit = Mathf.Lerp(stats.maxTurnRate, stats.maxTurnRateAtTopSpeed, speedFraction);
        float wantedTurnRate = stickInput.x * turnLimit;
        targetTurnRate = Mathf.MoveTowards(targetTurnRate, wantedTurnRate, stats.turnAcceleration * dt);
    }

    // Push toward the target speed, but never harder than the wheels' grip allows,
    // so other chairs and walls can still shove this one.
    private void ApplyDrive(float dt)
    {
        Vector3 forward = Flat(transform.forward);
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 velocity = rb.linearVelocity;

        float maxDriveChange = stats.driveGrip * dt;
        float maxSideChange = stats.sideGrip * dt;

        float forwardChange = Mathf.Clamp(targetSpeed - Vector3.Dot(velocity, forward), -maxDriveChange, maxDriveChange);
        float sideChange = Mathf.Clamp(-Vector3.Dot(velocity, right), -maxSideChange, maxSideChange);

        // Only horizontal changes, so gravity is left alone.
        rb.AddForce(forward * forwardChange + right * sideChange, ForceMode.VelocityChange);
    }

    private void ApplyTurn(float dt)
    {
        float maxTurnChange = stats.turnGrip * dt;
        float turnChange = Mathf.Clamp(targetTurnRate - TurnRate, -maxTurnChange, maxTurnChange);

        rb.AddTorque(Vector3.up * (turnChange * Mathf.Deg2Rad), ForceMode.VelocityChange);
    }

    private bool CheckGrounded()
    {
        if (bodyCollider == null) return true;

        Bounds bounds = bodyCollider.bounds;
        float rayLength = bounds.extents.y + groundCheckDistance;

        // Triggers (goal boxes, other chairs' space bubbles) don't count as ground.
        return Physics.Raycast(bounds.center, Vector3.down, rayLength, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private static Vector3 Flat(Vector3 direction)
    {
        direction.y = 0f;
        return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
    }

    private void OnDrawGizmosSelected()
    {
        Collider body = bodyCollider != null ? bodyCollider : GetComponent<Collider>();
        if (body == null) return;

        Bounds bounds = body.bounds;
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(bounds.center, bounds.center + Vector3.down * (bounds.extents.y + groundCheckDistance));
    }
}
