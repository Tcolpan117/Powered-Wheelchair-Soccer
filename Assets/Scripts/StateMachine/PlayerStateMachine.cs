using UnityEngine;

/// <summary>
/// AI for one chair. Each team's pursuer lines up behind the ball and dribbles it at the opponent goal;
/// the other fielder supports the attack past halfway and defends otherwise; the goalkeeper guards its pen.
/// Chairs crowded together for over a second break apart, and a pinned chair backs out.
/// Human control always wins.
/// </summary>
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Player), typeof(PlayerNavigation), typeof(PlayerAwareness))]
public class PlayerStateMachine : MonoBehaviour
{
    private enum State { Disabled, HumanControlled, HoldPosition, ChaseBall, Defend, GuardGoal, Attack, Support, Disperse }

    [SerializeField] private Transform homePosition;
    [SerializeField] private State state; // Shown in the Inspector for debugging.

    // 1. Tuning.
    private const float StopDistance = 1.5f;      // Yield when a teammate is this close ahead.
    private const float SlowDownDistance = 1.5f;  // Ease off the motor within this distance of the target.
    private const float GoalkeeperSpeed = 0.4f;
    private const float PenMargin = 1f;
    private const float DefensiveDepth = 0.4f;    // 0 = at own goal, 1 = at the ball.
    private const float SeparationRadius = 3f;    // Keep <= PlayerAwareness bubble radius.
    private const float SeparationWeight = 2f;
    private const float LineUpDistance = 1.2f;    // Behind the ball, before dribbling.
    private const float DribbleDistance = 2f;     // Past the ball, while dribbling.
    private const float DribbleRange = 2.5f;      // A lined-up pursuer this close to the ball starts dribbling.
    private const float SupportDepth = 0.5f;      // 0 = at the ball, 1 = at the opponent goal.
    private const float AttackLine = 0.5f;        // Ball progress (0 own goal, 1 opponent goal) where the second fielder attacks.
    private const float ClusterRadius = 1.5f;
    private const float ClusterTime = 1f;
    private const float DisperseDuration = 0.75f;
    private const float DisperseDistance = 3f;
    private const float StuckSpeed = 0.3f;        // Below this (m/s) while trying to drive, the chair counts as pinned.
    private const float StuckTime = 0.6f;
    private const float EscapeDuration = 0.6f;
    private const float EscapeDistance = 2f;
    private const float LineUpPatience = 0.75f;   // Time near the ball before pushing it without lining up.

    // Registry ids, indexed by Team. Built once; string concatenation every frame allocates garbage.
    private static readonly string[] goalIds = { "HomeGoal", "AwayGoal" };
    private static readonly string[] penIds = { "HomePen", "AwayPen" };

    private Player player;
    private PlayerNavigation navigation;
    private PlayerAwareness awareness;
    private Renderer penRenderer;
    private Rigidbody body;
    private Vector3 initialPosition;
    private float stuckSince = -1f;
    private float closeSince = -1f;
    private float crowdedSince = -1f;
    private float disperseUntil;
    private Vector3 disperseTarget;

    private Vector3 Home => homePosition != null ? homePosition.position : initialPosition;
    private Team Opponent => player.Team == Team.Home ? Team.Away : Team.Home;

    // 2. Connect to existing components and match-reset events.
    private void Awake()
    {
        player = GetComponent<Player>();
        navigation = GetComponent<PlayerNavigation>();
        awareness = GetComponent<PlayerAwareness>();
        body = GetComponent<Rigidbody>();
    }

    private void Start() => initialPosition = transform.position;
    private void OnEnable() => Goal.TriggerEvent += HandleGoal;

    private void OnDisable()
    {
        Goal.TriggerEvent -= HandleGoal;
        ResetDisperse();
        SetState(State.Disabled);
    }

    private void HandleGoal(string triggerId, string causeId)
    {
        if (causeId != nameof(Prop.Ball) || (triggerId != goalIds[0] && triggerId != goalIds[1])) return;
        awareness.Invalidate();
        PlayerAwareness.ResetPursuers();
        ResetDisperse();
        navigation.ClearPath();
        player.StopDriving();
    }

    // 3. Human input always wins. AI picks a state and target, then drives along Unity's route.
    private void Update()
    {
        if (player.IsPossessed())
        {
            SetState(State.HumanControlled);
            return;
        }
        if (!player.isActiveAndEnabled || !navigation.isActiveAndEnabled || !awareness.isActiveAndEnabled)
        {
            Hold();
            return;
        }

        awareness.Refresh();
        Ball ball = awareness.Ball;
        if (ball == null || !ball.isActiveAndEnabled)
        {
            Go(State.HoldPosition, Home);
            return;
        }
        Vector3 ballPosition = ball.GetPosition();

        if (player.Role == Role.Goaly)
        {
            // Missing pen configuration must not send a goalkeeper chasing across the court.
            if (!TryGetPen(out Bounds pen)) Hold();
            else Go(State.GuardGoal, new Vector3(Mathf.Clamp(Home.x, pen.min.x, pen.max.x), Home.y,
                Mathf.Clamp(ballPosition.z, pen.min.z, pen.max.z)), GoalkeeperSpeed, pen);
            return;
        }

        bool hasOwnGoal = TryGetGoal(player.Team, out Vector3 ownGoal);
        bool hasOpponentGoal = TryGetGoal(Opponent, out Vector3 opponentGoal);
        // A small margin either side of the attack line stops the second fielder flipping every frame.
        float attackLine = AttackLine + (state == State.Support ? -0.05f : 0.05f);

        if (Dispersing())
            Go(State.Disperse, disperseTarget);
        else if (awareness.GetPursuer(player.Team) == player)
            Pursue(ballPosition, hasOpponentGoal, opponentGoal);
        else if (hasOwnGoal && hasOpponentGoal && Progress(ballPosition, ownGoal, opponentGoal) >= attackLine)
            Go(State.Support, SupportTarget(ballPosition, opponentGoal) + SeparationWeight * PushAway(SeparationRadius, true, out _));
        else
            Go(State.Defend, DefensiveTarget(ballPosition, hasOwnGoal, ownGoal) + SeparationWeight * PushAway(SeparationRadius, true, out _));
    }

    private void Go(State next, Vector3 target, float speed = 1f, Bounds? bounds = null)
    {
        SetState(next);
        navigation.MoveTo(target);
        // Steer straight at the target when dribbling (a short straight push), or whenever Unity has no
        // route yet: the path is still being calculated, or the target is off the NavMesh (a ball against
        // a wall or in the goal mouth). Otherwise the chair would sit still waiting for a route.
        Vector3 direction = navigation.Direction;
        if (next == State.Attack || (direction == Vector3.zero && !navigation.Arrived(target)))
            direction = Flat(target - player.GetPosition()).normalized;

        // Only yield to a teammate; stopping for an opponent would prevent contesting the ball.
        // A dispersing chair never yields, or a cluster could hold itself in place.
        if (next != State.Disperse && awareness.TeammateAhead(StopDistance, direction))
        {
            player.StopDriving();
            return;
        }
        float approach = Mathf.Clamp01(PlayerNavigation.Distance(player.GetPosition(), target) / SlowDownDistance);
        player.Drive(direction, speed * approach, bounds);
        CheckStuck(next, direction, speed * approach);
    }

    // 4. Attack: line up behind the ball, then drive through it toward the opponent goal.
    private void Pursue(Vector3 ball, bool hasGoal, Vector3 goal)
    {
        Vector3 aim = Flat(goal - ball);
        // Without an opponent goal, fall back to chasing the ball itself.
        if (!hasGoal || aim.sqrMagnitude < 0.01f)
        {
            Go(State.ChaseBall, ball);
            return;
        }
        aim.Normalize();

        Vector3 toBall = Flat(ball - player.GetPosition());
        float distance = toBall.magnitude;
        float alignment = distance > 0.01f ? Vector3.Dot(toBall / distance, aim) : 1f;
        // Close to the ball on the goal-facing side but unable to line up (the ball is against a wall,
        // or keeps rolling away): after LineUpPatience, push it toward goal from where the chair is.
        if (distance > DribbleRange || alignment <= 0f) closeSince = -1f;
        else if (closeSince < 0f) closeSince = Time.time;
        bool impatient = closeSince >= 0f && Time.time - closeSince > LineUpPatience;

        // Looser limits once dribbling, so the state does not flicker on every bump.
        bool dribbling = state == State.Attack;
        if (distance <= DribbleRange * (dribbling ? 1.5f : 1f) && (impatient || alignment >= (dribbling ? 0.4f : 0.7f)))
        {
            Go(State.Attack, ball + aim * DribbleDistance);
            return;
        }

        Vector3 target = ball - aim * LineUpDistance;
        if (alignment < 0f)
        {
            // On the goal side of the ball: swing wide so the chair does not knock it toward its own goal.
            Vector3 side = Vector3.Cross(Vector3.up, aim);
            if (Vector3.Dot(player.GetPosition() - ball, side) < 0f) side = -side;
            target += side * LineUpDistance;
        }
        Go(State.ChaseBall, target);
    }

    // Ball progress along the court: 0 at own goal, 1 at the opponent goal.
    private static float Progress(Vector3 ball, Vector3 ownGoal, Vector3 opponentGoal)
    {
        Vector3 axis = Flat(opponentGoal - ownGoal);
        return axis.sqrMagnitude < 0.01f ? 0f : Vector3.Dot(Flat(ball - ownGoal), axis) / axis.sqrMagnitude;
    }

    // Wait between the ball and the opponent goal, in this player's own lane, ready for a rebound.
    private Vector3 SupportTarget(Vector3 ball, Vector3 opponentGoal)
    {
        Vector3 target = Vector3.Lerp(ball, opponentGoal, SupportDepth);
        target.z = Mathf.Lerp(Home.z, ball.z, 0.25f);
        target.y = Home.y;
        return target;
    }

    // 5. Stay goal-side of the ball and retain the field player's left/right lane.
    private Vector3 DefensiveTarget(Vector3 ball, bool hasGoal, Vector3 goal)
    {
        if (!hasGoal) goal = Home + (player.Team == Team.Home ? Vector3.left : Vector3.right) * 6f;
        Vector3 target = Vector3.Lerp(goal, ball, DefensiveDepth);
        // Do not move deeper behind the goalkeeper or beyond this player's starting line.
        target.x = player.Team == Team.Home
            ? Mathf.Clamp(target.x, goal.x + 3f, Home.x)
            : Mathf.Clamp(target.x, Home.x, goal.x - 3f);
        target.z = Mathf.Lerp(Home.z, ball.z, 0.25f);
        target.y = Home.y;
        return target;
    }

    // 6. Disperse: once this chair has had another within ClusterRadius for ClusterTime,
    // it drives away from every chair in the group (2, 3, 4, 5 or more) for DisperseDuration.
    private bool Dispersing()
    {
        if (Time.time < disperseUntil) return true;
        Vector3 away = PushAway(ClusterRadius, false, out bool crowded);
        if (!crowded)
        {
            crowdedSince = -1f;
            return false;
        }
        if (crowdedSince < 0f) crowdedSince = Time.time;
        if (Time.time - crowdedSince < ClusterTime) return false;

        if (away.sqrMagnitude < 0.0001f)
        {
            // Boxed in evenly (the middle of a group): pick a random way out so middle chairs spread.
            Vector2 random = Random.insideUnitCircle;
            away = random.sqrMagnitude > 0f ? new Vector3(random.x, 0f, random.y) : transform.right;
        }
        disperseTarget = player.GetPosition() + away.normalized * DisperseDistance;
        disperseUntil = Time.time + DisperseDuration;
        crowdedSince = -1f;
        return true;
    }

    // A field chair asked to drive but barely moving for StuckTime is pinned: pushing against the ball
    // and an opponent, against a chair that will not give way (a human's), or wedged against a wall or
    // goal so it cannot turn. Back out diagonally for EscapeDuration, then come back at a new angle.
    // Backing out always works: anything pinning the chair is in front of it or beside it.
    private void CheckStuck(State current, Vector3 direction, float scale)
    {
        bool tryingToMove = player.Role != Role.Goaly && current != State.Disperse && scale > 0.3f
            && direction != Vector3.zero;
        if (!tryingToMove || Flat(body.linearVelocity).sqrMagnitude > StuckSpeed * StuckSpeed)
        {
            stuckSince = -1f;
            return;
        }
        if (stuckSince < 0f) stuckSince = Time.time;
        if (Time.time - stuckSince < StuckTime) return;

        // Back out toward the side with more room. Player reverses when its route is behind it.
        Vector3 forward = Flat(transform.forward).normalized;
        Vector3 side = Vector3.Cross(Vector3.up, forward);
        float room = Vector3.Dot(PushAway(SeparationRadius, false, out _), side);
        if (room < 0f || (room == 0f && Random.value < 0.5f)) side = -side;
        disperseTarget = player.GetPosition() + (side * 0.7f - forward).normalized * EscapeDistance;
        disperseUntil = Time.time + EscapeDuration;
        stuckSince = -1f;
    }

    private void ResetDisperse()
    {
        crowdedSince = -1f;
        disperseUntil = 0f;
        stuckSince = -1f;
        closeSince = -1f;
    }

    // Sum of pushes away from nearby chairs within radius, stronger when closer.
    private Vector3 PushAway(float radius, bool teammatesOnly, out bool crowded)
    {
        Vector3 push = Vector3.zero;
        crowded = false;
        foreach (Player other in awareness.NearbyPlayers)
        {
            if (other == null || (teammatesOnly && other.Team != player.Team)) continue;
            Vector3 away = Flat(player.GetPosition() - other.GetPosition());
            float distance = away.magnitude;
            if (distance >= radius) continue;
            crowded = true;
            if (distance > 0.01f) push += away / distance * (1f - distance / radius);
        }
        return push;
    }

    // 7. Shared helpers.
    private static bool TryGetGoal(Team team, out Vector3 position)
    {
        bool found = WorldRegister.TryGetItem(goalIds[(int)team], out var item) && item.gameObject != null;
        position = found ? item.gameObject.transform.position : default;
        return found;
    }

    private bool TryGetPen(out Bounds bounds)
    {
        // Look the renderer up once; Unity's null check repeats the lookup if the pen is destroyed.
        if (penRenderer == null && WorldRegister.TryGetItem(penIds[(int)player.Team], out var item)
            && item.gameObject != null)
            item.gameObject.TryGetComponent(out penRenderer);

        bounds = default;
        if (penRenderer == null) return false;
        bounds = penRenderer.bounds;
        Vector3 size = bounds.size;
        size.x = Mathf.Max(0f, size.x - PenMargin * 2f);
        size.z = Mathf.Max(0f, size.z - PenMargin * 2f);
        bounds.size = size;
        return size.x > 0f && size.z > 0f;
    }

    private static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        return v;
    }

    private void Hold()
    {
        SetState(State.HoldPosition);
        navigation.ClearPath();
        player.StopDriving();
    }

    private void SetState(State next)
    {
        if (state == next) return;
        // != null, not ?., so Unity's destroyed-object check applies.
        if (navigation != null) navigation.ClearPath();
        if (player != null) player.StopDriving();
        state = next;
    }
}
