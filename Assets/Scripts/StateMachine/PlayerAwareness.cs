using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Senses the ball and nearby players inside a horizontal bubble, and picks one pursuer per team.
/// Refresh updates the snapshot; queries read that snapshot until the next scan.
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerAwareness : MonoBehaviour
{
    // 1. Tuning.
    private const float FallbackRadius = 4f;       // Used when this player has no SpaceBubble sphere collider.
    private const float ScanInterval = 0.1f;
    private const float ScenePlayersInterval = 0.5f;
    private const float ForwardConeCosine = 0.5f;  // 60 degrees to either side.
    private const float TurnPenaltyMeters = 3f;
    private const float ReassignInterval = 0.25f;  // How often pursuers are reconsidered.
    private const float MinAssignment = 0.75f;     // A new pursuer keeps the job at least this long.
    private const float SwitchMargin = 0.75f;      // A challenger must be this much cheaper to take over.

    private readonly List<Player> nearbyPlayers = new();
    private Player self;
    private SphereCollider spaceBubble;
    private float nextScan;
    private float scanPhase; // Spreads chairs' scans across frames instead of all on one.

    public IReadOnlyList<Player> NearbyPlayers => nearbyPlayers;
    public Ball Ball { get; private set; }

    // Shared by every chair: one scene lookup, and one pursuer decision per team (indexed by Team).
    private static Player[] scenePlayers;
    private static float scenePlayersUntil;
    private static readonly Player[] pursuers = new Player[2];
    private static readonly float[] assignedUntil = new float[2];
    private static float nextAssignment;

    // 2. Initialize references and reset shared data when entering Play mode or after a goal.
    private void Awake()
    {
        self = GetComponent<Player>();
        spaceBubble = transform.Find("SpaceBubble")?.GetComponent<SphereCollider>();
        scanPhase = Random.value * ScanInterval;
    }

    private void OnEnable() => Invalidate();
    private void OnDisable() => Invalidate();

    public void Invalidate()
    {
        nextScan = 0f;
        Ball = null;
        nearbyPlayers.Clear();
        scenePlayersUntil = 0f;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetPursuers()
    {
        pursuers[0] = pursuers[1] = null;
        assignedUntil[0] = assignedUntil[1] = 0f;
        nextAssignment = 0f;
        scenePlayers = null;
        scenePlayersUntil = 0f;
    }

    // 3. Refresh the snapshot on this chair's own phase of the scan interval.
    public void Refresh()
    {
        if (!isActiveAndEnabled || Time.time < nextScan) return;
        nextScan = (Mathf.Floor((Time.time - scanPhase) / ScanInterval) + 1f) * ScanInterval + scanPhase;

        Ball = WorldRegister.TryGetItem(nameof(Prop.Ball), out RegistryItem item)
            && item.gameObject != null ? item.Get<Ball>() : null;
        if (Ball != null && !Ball.isActiveAndEnabled) Ball = null;

        Bounds bubble = BubbleBounds;
        float radius = bubble.extents.x;
        nearbyPlayers.Clear();
        foreach (Player player in ScenePlayers())
        {
            if (player == null || player == self || !player.isActiveAndEnabled) continue;
            Vector3 offset = player.GetPosition() - bubble.center;
            offset.y = 0f;
            if (radius > 0f && offset.sqrMagnitude <= radius * radius) nearbyPlayers.Add(player);
        }
    }

    // 4. Both teams share one decision, so every chair agrees on its team's pursuer.
    public Player GetPursuer(Team team)
    {
        if (Ball == null || !Ball.isActiveAndEnabled) return null;
        if (Time.time >= nextAssignment)
        {
            nextAssignment = Time.time + ReassignInterval;
            ChoosePursuer(Team.Home);
            ChoosePursuer(Team.Away);
        }
        Player pursuer = pursuers[(int)team];
        return Eligible(pursuer, team) ? pursuer : null;
    }

    private void ChoosePursuer(Team team)
    {
        Vector3 ball = Ball.GetPosition();
        Player best = null;
        float bestCost = float.PositiveInfinity;
        foreach (Player candidate in ScenePlayers())
        {
            if (!Eligible(candidate, team)) continue;
            float cost = PursuitCost(candidate, ball);
            // On a tie, the lower role number wins so the choice is stable.
            if (cost < bestCost || (Mathf.Approximately(cost, bestCost) && best != null && candidate.Role < best.Role))
            {
                best = candidate;
                bestCost = cost;
            }
        }

        // Keep the current pursuer for its minimum time, and unless a challenger is clearly better.
        int index = (int)team;
        Player current = pursuers[index];
        if (Eligible(current, team)
            && (Time.time < assignedUntil[index] || PursuitCost(current, ball) <= bestCost + SwitchMargin))
            return;

        pursuers[index] = best;
        assignedUntil[index] = Time.time + MinAssignment;
    }

    // Distance to the ball plus a penalty for having to turn toward it.
    private static float PursuitCost(Player candidate, Vector3 ball)
    {
        Vector3 to = ball - candidate.GetPosition();
        to.y = 0f;
        float distance = to.magnitude;
        if (distance < 0.01f) return 0f;
        Vector3 forward = candidate.transform.forward;
        forward.y = 0f;
        return distance + Vector3.Angle(forward, to) / 180f * TurnPenaltyMeters;
    }

    private static bool Eligible(Player player, Team team) =>
        player != null && player.isActiveAndEnabled && player.Team == team && player.Role != Role.Goaly
        && (player.IsPossessed()
            || (player.TryGetComponent(out PlayerStateMachine machine) && machine.isActiveAndEnabled
                && player.TryGetComponent(out PlayerNavigation navigation) && navigation.isActiveAndEnabled));

    // 5. True when a human-driven teammate or this team's pursuer is close ahead in the given direction.
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

    // 6. Discovery and editor visualization.
    private static Player[] ScenePlayers()
    {
        // shortcut: a Player with no PlayerAwareness that spawns mid-match is seen up to 0.5 s late.
        if (scenePlayers == null || Time.time >= scenePlayersUntil)
        {
            scenePlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);
            scenePlayersUntil = Time.time + ScenePlayersInterval;
        }
        return scenePlayers;
    }

    // Unity supplies the sphere's world-space center and radius, including its scale.
    // Disabled colliders have empty bounds, so a disabled bubble senses nothing.
    private Bounds BubbleBounds => spaceBubble != null
        ? spaceBubble.bounds
        : new Bounds(transform.position, Vector3.one * (FallbackRadius * 2f));

    private void OnDrawGizmosSelected()
    {
        // Awake has not run in edit mode; let the existing collider draw its own outline.
        if (!Application.isPlaying && transform.Find("SpaceBubble") != null) return;
        Gizmos.color = Color.cyan;
        Bounds bubble = BubbleBounds;
        Gizmos.DrawWireSphere(bubble.center, bubble.extents.x);
    }
}
