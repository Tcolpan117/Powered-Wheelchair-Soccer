using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Asks Unity's NavMeshAgent for a route and exposes its next steering direction.
/// The wheelchair motor remains responsible for Rigidbody movement.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class PlayerNavigation : MonoBehaviour
{
    // 1. Configure sampling here; configure agent type, areas, and stopping distance
    // on the NavMeshAgent itself so there is only one source for those settings.
    [SerializeField, Min(0.1f)] private float sampleDistance = 2f;
    [SerializeField, Min(0.01f)] private float maxHorizontalSnap = 0.15f;

    private const float RepathInterval = 0.2f;
    private NavMeshAgent agent;
    private float nextPathTime;
    private Vector3 requestedTarget;
    private bool destinationRequested;

    private bool AgentReady => agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
    public bool HasPath => isActiveAndEnabled && AgentReady && !agent.pathPending
        && agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathComplete;
    public Vector3 Direction { get; private set; }

    // 2. Use the agent for planning without letting it move the physics-driven chair.
    private void Awake()
    {
        if (!TryGetComponent(out agent))
        {
            Debug.LogError("PlayerNavigation requires a NavMeshAgent on this player.", this);
            enabled = false;
            return;
        }

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.autoTraverseOffMeshLink = false;
        agent.autoRepath = true;
        ClearPath();
    }

    private void OnDisable() => ClearPath();

    // 3. Synchronize with the chair, request a destination, and read Unity's steering.
    public void MoveTo(Vector3 target)
    {
        Direction = Vector3.zero;
        if (!isActiveAndEnabled || !AgentReady || Arrived(target))
        {
            Stop();
            return;
        }

        // The physical chair leads; the agent must not simulate driving ahead of it.
        agent.isStopped = true;
        agent.nextPosition = transform.position;
        if (Distance(agent.nextPosition, transform.position) > maxHorizontalSnap)
        {
            Stop();
            return;
        }

        if (agent.pathPending)
        {
            nextPathTime = Time.time + RepathInterval;
            return;
        }

        if (Time.time >= nextPathTime
            && (!destinationRequested || (target - requestedTarget).sqrMagnitude > 0.0625f
                || !agent.hasPath || agent.isPathStale))
        {
            nextPathTime = Time.time + RepathInterval;
            var filter = new NavMeshQueryFilter
            {
                agentTypeID = agent.agentTypeID,
                areaMask = agent.areaMask
            };

            if (!NavMesh.SamplePosition(target, out NavMeshHit hit, sampleDistance, filter)
                || !agent.SetDestination(hit.position))
            {
                Stop();
                return;
            }
            requestedTarget = target;
            destinationRequested = true;
        }

        // Path calculation is asynchronous. Never steer along a partial or pending route.
        if (!HasPath || agent.isOnOffMeshLink) return;

        Vector3 direction = agent.steeringTarget - transform.position;
        direction.y = 0f;
        Direction = direction.normalized;
    }

    public bool Arrived(Vector3 target) =>
        agent != null && Distance(transform.position, target) <= agent.stoppingDistance;

    // 4. Clear Unity's route when stopping, changing state, or resetting the match.
    public void Stop() => ClearPath();

    public void ClearPath()
    {
        Direction = Vector3.zero;
        nextPathTime = 0f;
        destinationRequested = false;
        if (!AgentReady) return;

        agent.isStopped = true;
        // ResetPath completes an active off-mesh link. Do not teleport a physics chair.
        if (!agent.isOnOffMeshLink) agent.ResetPath();
    }

    // Court distances ignore height.
    public static float Distance(Vector3 a, Vector3 b)
    {
        a.y = b.y;
        return Vector3.Distance(a, b);
    }
}