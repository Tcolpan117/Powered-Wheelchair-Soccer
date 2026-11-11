using UnityEngine;

/// <summary>One pursuer per team, one defender, and a goalkeeper confined to its pen.</summary>
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Player), typeof(PlayerNavigation), typeof(PlayerAwareness))]
public class PlayerStateMachine : MonoBehaviour
{
    public enum State { Disabled, HumanControlled, HoldPosition, ChaseBall, Defend, GuardGoal }

    // 1. Tactical tuning; these are behaviors, not changes to the existing role names.
    [SerializeField] private Transform homePosition;
    [SerializeField, Min(0.1f)] private float playerStopDistance = 1.5f;
    [SerializeField, Range(0.1f, 1f)] private float goalkeeperSpeed = 0.4f;
    [SerializeField, Min(0f)] private float penMargin = 1f;
    [SerializeField, Range(0.1f, 0.8f)] private float defensiveDepth = 0.4f;
    [SerializeField] private string currentState;

    public State CurrentState { get; private set; } = State.Disabled;
    public bool IsYielding { get; private set; }
    public Vector3 Target { get; private set; }

    private Player player;
    private PlayerNavigation navigation;
    private PlayerAwareness awareness;
    private Vector3 initialPosition;
    private Vector3 Home => homePosition != null ? homePosition.position : initialPosition;

    // 2. Connect to existing components and match-reset events.
    private void Awake()
    {
        player = GetComponent<Player>();
        navigation = GetComponent<PlayerNavigation>();
        awareness = GetComponent<PlayerAwareness>();
    }

    private void Start() => initialPosition = transform.position;
    private void OnEnable() => Goal.TriggerEvent += HandleGoal;

    private void OnDisable()
    {
        Goal.TriggerEvent -= HandleGoal;
        SetState(State.Disabled);
        if (player != null) player.StopDriving();
        if (navigation != null) navigation.ClearPath();
    }

    private void HandleGoal(string triggerId, string causeId)
    {
        if (causeId != nameof(Prop.Ball) || (triggerId != "HomeGoal" && triggerId != "AwayGoal")) return;
        awareness.Invalidate();
        PlayerAwareness.ResetPursuers();
        navigation.ClearPath();
        player.StopDriving();
    }

    // 3. Human input always wins. AI chooses a target, then asks Unity for its route.
    private void Update()
    {
        IsYielding = false;
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
        Bounds? movementBounds = null;
        float speed = 1f;
        State next;

        if (ball == null || !ball.isActiveAndEnabled)
        {
            Target = Home;
            next = State.HoldPosition;
        }
        else if (player.Role == Role.Goaly)
        {
            // Missing pen configuration must not send a goalkeeper chasing across the court.
            if (!TryGetPen(out Bounds pen))
            {
                Hold();
                return;
            }
            Target = new Vector3(Mathf.Clamp(Home.x, pen.min.x, pen.max.x), Home.y,
                Mathf.Clamp(ball.GetPosition().z, pen.min.z, pen.max.z));
            movementBounds = pen;
            speed = goalkeeperSpeed;
            next = State.GuardGoal;
        }
        else if (awareness.GetPursuer(player.Team) == player)
        {
            Target = ball.GetPosition();
            next = State.ChaseBall;
        }
        else
        {
            Target = DefensiveTarget(ball.GetPosition());
            next = State.Defend;
        }

        SetState(next);
        navigation.MoveTo(Target);
        // Only yield to a teammate; stopping for an opponent would prevent contesting the ball.
        if (awareness.TeammateAhead(playerStopDistance, navigation.Direction))
        {
            player.StopDriving();
            IsYielding = true;
            return;
        }

        float distance = PlayerNavigation.Distance(player.GetPosition(), Target);
        float approachSpeed = Mathf.Clamp01(distance / 1.5f);
        player.Drive(navigation.Direction, speed * approachSpeed, movementBounds);
    }

    // 4. Stay goal-side of the ball and retain the field player's left/right lane.
    private Vector3 DefensiveTarget(Vector3 ball)
    {
        Vector3 goal = Home;
        if (WorldRegister.TryGetItem(player.Team.ToString() + Zone.Goal, out var item)
            && item.gameObject != null)
            goal = item.gameObject.transform.position;
        else
            goal.x += player.Team == Team.Home ? -6f : 6f;

        Vector3 target = Vector3.Lerp(goal, ball, defensiveDepth);
        // Do not move deeper behind the goalkeeper or beyond this player's starting line.
        target.x = player.Team == Team.Home
            ? Mathf.Clamp(target.x, goal.x + 3f, Home.x)
            : Mathf.Clamp(target.x, Home.x, goal.x - 3f);
        target.z = Mathf.Lerp(Home.z, ball.z, 0.25f);
        target.y = Home.y;
        return target;
    }

    private bool TryGetPen(out Bounds bounds)
    {
        bounds = default;
        if (!WorldRegister.TryGetItem(player.Team.ToString() + Zone.Pen, out var item)
            || item.gameObject == null || !item.gameObject.TryGetComponent<Renderer>(out var renderer))
            return false;

        bounds = renderer.bounds;
        Vector3 size = bounds.size;
        size.x = Mathf.Max(0f, size.x - penMargin * 2f);
        size.z = Mathf.Max(0f, size.z - penMargin * 2f);
        bounds.size = size;
        return size.x > 0f && size.z > 0f;
    }

    private void Hold()
    {
        SetState(State.HoldPosition);
        navigation.Stop();
        player.StopDriving();
    }

    private void SetState(State next)
    {
        if (CurrentState != next)
        {
            navigation?.ClearPath();
            player?.StopDriving();
            CurrentState = next;
            currentState = next.ToString();
        }
    }
}


