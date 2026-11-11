using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Senses players and the ball inside a horizontal bubble, optionally limited by a pen.
/// Refresh updates the snapshot; queries read that snapshot until the next scan.
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerAwareness : MonoBehaviour
{
    // 1. Configure the sensing area.
    [Tooltip("Fallback radius when this player has no SpaceBubble sphere collider.")]
    [SerializeField, Min(0.1f)] private float radius = 4f;
    [SerializeField] private Pen pen;
    [SerializeField] private string ballId = nameof(Prop.Ball);

    private const float ScanInterval = 0.1f;
    private const float ForwardConeCosine = 0.5f; // 60 degrees to either side.
    private readonly List<Player> nearbyPlayers = new();
    private Player self;
    private SphereCollider spaceBubble;
    private float nextScan;

    public IReadOnlyList<Player> NearbyPlayers => nearbyPlayers;
    public Ball Ball { get; private set; }
    public bool BallInBubble { get; private set; }

    // All chairs scanning in one frame can share Unity's scene lookup.
    private static Player[] scenePlayers;
    private static int scenePlayersFrame = -1;
    private static readonly Dictionary<Team, Player> pursuers = new();
    private static readonly Dictionary<Team, float> assignmentUntil = new();
    private static float nextAssignment;

    public static void ResetPursuers()
    {
        pursuers.Clear();
        assignmentUntil.Clear();
        nextAssignment = 0f;
    }

    // 2. Initialize references and reset shared data when entering Play mode.
    private void Awake()
    {
        self = GetComponent<Player>();
        spaceBubble = transform.Find("SpaceBubble")?.GetComponent<SphereCollider>();
    }
    private void OnEnable() => Invalidate();
    private void OnDisable() => Invalidate();

    public void Invalidate()
    {
        nextScan = 0f;
        Ball = null;
        BallInBubble = false;
        nearbyPlayers.Clear();
        scenePlayersFrame = -1;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetScenePlayers()
    {
        ResetPursuers();
        scenePlayers = null;
        scenePlayersFrame = -1;
    }

    // 3. Refresh the snapshot at a fixed cadence.
    public void Refresh()
    {
        if (!isActiveAndEnabled || Time.time < nextScan) return;
        nextScan = Time.time + ScanInterval;

        Ball = WorldRegister.TryGetItem(ballId, out RegistryItem item)
            && item.gameObject != null ? item.Get<Ball>() : null;
        if (Ball != null && !Ball.isActiveAndEnabled) Ball = null;
        BallInBubble = Ball != null && Contains(Ball.GetPosition());

        nearbyPlayers.Clear();
        foreach (Player player in GetScenePlayers())
        {
            if (player != null && player != self && player.isActiveAndEnabled
                && Contains(player.GetPosition()))
                nearbyPlayers.Add(player);
        }
    }

    // 4. Answer spatial questions on the court's XZ plane.
    public bool Contains(Vector3 position)
    {
        Bounds bubble = BubbleBounds;
        Vector3 offset = position - bubble.center;
        offset.y = 0f;
        float bubbleRadius = bubble.extents.x;
        if (bubbleRadius <= 0f || offset.sqrMagnitude > bubbleRadius * bubbleRadius) return false;
        if (pen == null) return true;

        // Like Pen.GetBounds, use geometry even when MeshToggler hides the renderer.
        return pen.TryGetComponent<Renderer>(out var renderer)
            && ContainsOnCourt(renderer.bounds, position);
    }

    public static bool ContainsOnCourt(Bounds bounds, Vector3 position)
    {
        position.y = bounds.center.y;
        return bounds.Contains(position);
    }

    // Both teams share one decision, so every chair agrees on its team's pursuer.
    public Player GetPursuer(Team team)
    {
        if (Ball == null || !Ball.isActiveAndEnabled) return null;
        if (Time.time >= nextAssignment)
        {
            nextAssignment = Time.time + 0.25f;
            ChoosePursuer(Team.Home);
            ChoosePursuer(Team.Away);
        }
        return pursuers.TryGetValue(team, out var player) && Eligible(player, team) ? player : null;
    }

    private void ChoosePursuer(Team team)
    {
        Vector3 ball = Ball.GetPosition();
        Player nearest = null;
        float nearestDistance = float.PositiveInfinity;
        foreach (Player candidate in GetScenePlayers())
        {
            if (!Eligible(candidate, team)) continue;
            float distance = PlayerNavigation.Distance(candidate.GetPosition(), ball);
            if (distance < nearestDistance
                || (Mathf.Approximately(distance, nearestDistance)
                    && nearest != null && (int)candidate.Role < (int)nearest.Role))
            {
                nearest = candidate;
                nearestDistance = distance;
            }
        }

        pursuers.TryGetValue(team, out var current);
        if (Eligible(current, team))
        {
            if (assignmentUntil.TryGetValue(team, out float until) && Time.time < until) return;
            if (PlayerNavigation.Distance(current.GetPosition(), ball) <= nearestDistance + 0.75f) return;
        }

        pursuers[team] = nearest;
        assignmentUntil[team] = Time.time + 0.75f;
    }

    private static bool Eligible(Player player, Team team) =>
        player != null && player.isActiveAndEnabled && player.Team == team && player.Role != Role.Goaly
        && (player.IsPossessed()
            || (player.TryGetComponent<PlayerStateMachine>(out var machine) && machine.isActiveAndEnabled
                && player.TryGetComponent<PlayerNavigation>(out var navigation) && navigation.isActiveAndEnabled));

    public bool TeammateAhead(float distance, Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return false;
        direction.Normalize();
        Player pursuer = GetPursuer(self.Team);
        foreach (Player player in nearbyPlayers)
        {
            if (player == null || !player.isActiveAndEnabled || player.Team != self.Team) continue;
            // Give human drivers priority; otherwise the defender yields to the pursuer.
            if (!player.IsPossessed() && player != pursuer) continue;
            Vector3 offset = player.GetPosition() - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude < distance * distance
                && Vector3.Dot(direction, offset.normalized) > ForwardConeCosine)
                return true;
        }
        return false;
    }
    // 5. Keep discovery and editor visualization separate from sensing.
    private static Player[] GetScenePlayers()
    {
        if (scenePlayers == null || scenePlayersFrame != Time.frameCount)
        {
            scenePlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);
            scenePlayersFrame = Time.frameCount;
        }
        return scenePlayers;
    }

    // Unity supplies the sphere's world-space center and radius, including its scale.
    // Disabled colliders have empty bounds, so a disabled bubble senses nothing.
    private Bounds BubbleBounds => spaceBubble != null
        ? spaceBubble.bounds
        : new Bounds(transform.position, Vector3.one * (radius * 2f));

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        // Awake has not run in edit mode; let the existing collider draw its own outline.
        if (!Application.isPlaying && transform.Find("SpaceBubble") != null) return;
        Bounds bubble = BubbleBounds;
        Gizmos.DrawWireSphere(bubble.center, bubble.extents.x);
    }
}
