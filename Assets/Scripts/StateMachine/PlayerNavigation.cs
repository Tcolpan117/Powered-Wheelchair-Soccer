using UnityEngine;
using UnityEngine.AI;

// Calculates paths only. WheelchairMotor remains the sole mover of the Rigidbody.
[RequireComponent(typeof(WheelchairMotor))]
public class PlayerNavigation : MonoBehaviour
{
    [SerializeField] private int agentTypeId;
    [SerializeField] private int areaMask = NavMesh.AllAreas;
    [SerializeField, Min(0.1f)] private float sampleDistance = 2f;
    [SerializeField, Min(0.01f)] private float maxHorizontalSnap = 0.15f;
    [SerializeField, Min(0.1f)] private float stoppingDistance = 0.8f;
    [SerializeField, Min(0.05f)] private float cornerDistance = 0.25f;
    private WheelchairMotor motor;
    private NavMeshPath path;
    private Vector3[] corners = System.Array.Empty<Vector3>();
    private int corner;
    private float nextPathTime;
    public bool HasPath { get; private set; }

    private void Awake()
    {
        motor = GetComponent<WheelchairMotor>();
        path = new NavMeshPath();
    }

    public static float Distance(Vector3 a, Vector3 b)
    {
        a.y = b.y;
        return Vector3.Distance(a, b);
    }

    public bool Arrived(Vector3 target) => Distance(transform.position, target) <= stoppingDistance;
    public void Stop()
    {
        motor.SetDriveInput(Vector2.zero);
        ClearPath();
    }
    public void ClearPath()
    {
        HasPath = false;
        corners = System.Array.Empty<Vector3>();
        nextPathTime = 0f;
    }

    public void MoveTo(Vector3 target)
    {
        if (Arrived(target)) { Stop(); return; }
        var filter = new NavMeshQueryFilter { agentTypeID = agentTypeId, areaMask = areaMask };
        if (!NavMesh.SamplePosition(transform.position, out var source, sampleDistance, filter) ||
            Distance(source.position, transform.position) > maxHorizontalSnap)
        { Stop(); return; }

        if (Time.time >= nextPathTime)
        {
            nextPathTime = Time.time + 0.2f;
            HasPath = NavMesh.SamplePosition(target, out var destination, sampleDistance, filter)
                && Distance(target, destination.position) <= maxHorizontalSnap
                && NavMesh.CalculatePath(source.position, destination.position, filter, path)
                && path.status == NavMeshPathStatus.PathComplete;
            corners = HasPath ? path.corners : System.Array.Empty<Vector3>();
            corner = 1;
        }
        if (!HasPath || corners.Length < 2) { motor.SetDriveInput(Vector2.zero); return; }
        while (corner < corners.Length - 1 && Distance(transform.position, corners[corner]) < cornerDistance)
            corner++;
        // Reject stale segments after a reset/teleport or a changed NavMesh.
        if (NavMesh.Raycast(source.position, corners[corner], out _, filter)) { Stop(); return; }
        Vector3 direction = corners[corner] - transform.position;
        direction.y = 0f;
        float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        float turn = Mathf.Clamp(angle / 45f, -1f, 1f);
        float remaining = Distance(transform.position, corners[corner]);
        float grip = Mathf.Min(motor.Stats.braking, motor.Stats.driveGrip);
        float safeSpeed = Mathf.Sqrt(2f * Mathf.Max(0f, grip) * Mathf.Max(0f, remaining - cornerDistance));
        float drive = Mathf.Clamp01(safeSpeed / Mathf.Max(0.01f, motor.Stats.maxForwardSpeed));
        drive *= Mathf.Clamp01(1f - Mathf.Abs(angle) / 60f);
        // Look ahead along actual velocity/heading before applying more drive near an edge.
        float speed = Mathf.Abs(motor.ForwardSpeed);
        float lookAhead = speed * speed / (2f * Mathf.Max(0.01f, grip)) + 0.3f;
        Vector3 forward = transform.forward; forward.y = 0f;
        if (NavMesh.Raycast(source.position, source.position + forward.normalized * lookAhead, out _, filter))
            drive = 0f;
        motor.SetDriveInput(new Vector2(turn, drive));
    }
}