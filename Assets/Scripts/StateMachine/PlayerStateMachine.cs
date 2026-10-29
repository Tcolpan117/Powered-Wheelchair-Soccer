using UnityEngine;

// Runs after human input: never sends motor commands while the human driver is enabled.
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Player), typeof(PlayerNavigation), typeof(PlayerAwareness))]
public class PlayerStateMachine : MonoBehaviour
{
    [Tooltip("Optional return point. Otherwise uses the chair's position at Start.")]
    [SerializeField] private Transform homePosition;
    [SerializeField, Min(0.1f)] private float playerStopDistance = 1.5f;
    [SerializeField] private string currentState;
    public IPlayerState CurrentState { get; private set; }
    private PlayerNavigation navigation;
    private PlayerAwareness awareness;
    private PlayerWheelchairDriver human;
    private Vector3 initialPosition;
    private IPlayerState humanState, holdState, chaseState, returnState, yieldState;
    private bool wasHuman;
    private Vector3 Home => homePosition != null ? homePosition.position : initialPosition;
    private bool HumanControlled => human != null && human.isActiveAndEnabled;

    private void Awake()
    {
        navigation = GetComponent<PlayerNavigation>();
        awareness = GetComponent<PlayerAwareness>();
        human = GetComponent<PlayerWheelchairDriver>();
        humanState = new ActionState("HumanControlled", () => { });
        holdState = new ActionState("HoldPosition", navigation.Stop);
        chaseState = new ActionState("ChaseBall", () => navigation.MoveTo(awareness.Ball.GetPosition()));
        returnState = new ActionState("ReturnToPosition", () => navigation.MoveTo(Home));
        yieldState = new ActionState("YieldToPlayer", navigation.Stop);
    }
    private void Start() => initialPosition = transform.position;

    private void Update()
    {
        bool isHuman = HumanControlled;
        if (isHuman != wasHuman) navigation.ClearPath();
        wasHuman = isHuman;
        awareness.Refresh();
        IPlayerState next = isHuman ? humanState
            : awareness.PlayerAhead(playerStopDistance) ? yieldState
            : awareness.Ball != null && awareness.BallInBubble ? chaseState
            : navigation.Arrived(Home) ? holdState : returnState;
        ChangeState(next);
        CurrentState.Tick();
    }

    private void ChangeState(IPlayerState next)
    {
        if (ReferenceEquals(CurrentState, next)) return;
        CurrentState?.Exit();
        navigation.ClearPath();
        CurrentState = next;
        currentState = next.Name;
        next.Enter();
    }

    private void OnDisable()
    {
        CurrentState?.Exit();
        CurrentState = null;
        currentState = "Disabled";
        if (navigation != null && !HumanControlled) navigation.Stop();
    }

    // Small reusable state implementation; replace actions with dedicated state classes as tactics grow.
    private sealed class ActionState : IPlayerState
    {
        public string Name { get; }
        private readonly System.Action action;
        public ActionState(string name, System.Action action) { Name = name; this.action = action; }
        public void Enter() { }
        public void Tick() => action();
        public void Exit() { }
    }
}