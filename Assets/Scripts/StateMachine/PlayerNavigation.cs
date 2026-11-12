using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Asks Unity's NavMeshAgent for a route and exposes its next steering direction.
/// The wheelchair motor remains responsible for Rigidbody movement.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class PlayerNavigation : MonoBehaviour
{
    // 1. Tuning. Agent type, areas and stopping distance live on the NavMeshAgent itself,
    // so there is only one source for those settings.
    private const float SampleDistance = 2f;    // A target this close to the NavMesh can still be routed to.
    private const float MaxSnap = 0.15f;        // A chair further than this off the NavMesh gets no route.
    private const float RepathInterval = 0.2f;
    private const float RepathDistance = 0.25f; // Re-route when the target moves this far.

    private NavMeshAgent agent;
    private float nextPathTime;
    private Vector3? requestedTarget;

    public Vector3 Direction { get; private set; }
    private bool AgentReady => agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;

    // 2. Use the agent for planning without letting it move the physics-driven chair.
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
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
            ClearPath();
            return;
        }

        // The physical chair leads; the agent must not simulate driving ahead of it.
        agent.isStopped = true;
        agent.nextPosition = transform.position;
        if (Distance(agent.nextPosition, transform.position) > MaxSnap)
        {
            ClearPath();
            return;
        }

        if (agent.pathPending)
        {
            nextPathTime = Time.time + RepathInterval;
            return;
        }

        bool targetMoved = requestedTarget == null
            || (target - requestedTarget.Value).sqrMagnitude > RepathDistance * RepathDistance;
        if (Time.time >= nextPathTime && (targetMoved || !agent.hasPath || agent.isPathStale))
        {
            nextPathTime = Time.time + RepathInterval;
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            if (!NavMesh.SamplePosition(target, out NavMeshHit hit, SampleDistance, filter)
                || !agent.SetDestination(hit.position))
            {
                ClearPath();
                return;
            }
            requestedTarget = target;
        }

        // Path calculation is asynchronous. Never steer along a partial or pending route.
        if (!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete || agent.isOnOffMeshLink) return;
        Vector3 direction = agent.steeringTarget - transform.position;
        direction.y = 0f;
        Direction = direction.normalized;
    }

    public bool Arrived(Vector3 target) =>
        agent != null && Distance(transform.position, target) <= agent.stoppingDistance;

    // 4. Clear Unity's route when stopping, changing state, or resetting the match.
    public void ClearPath()
    {
        Direction = Vector3.zero;
        nextPathTime = 0f;
        requestedTarget = null;
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
