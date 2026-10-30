using UnityEngine;
using UnityEngine.InputSystem;

// Reads the player's joystick and passes it to this chair's motor.
// Every chair can carry this component, but it should only be enabled on the chair the
// player is controlling. An AI driver takes its place on the other chairs.
[RequireComponent(typeof(WheelchairMotor))]
public class PlayerWheelchairDriver : MonoBehaviour
{
    [Tooltip("Vector2 action: x = turn, y = forward. Use WheelchairControls > Wheelchair > Drive.")]
    [SerializeField] private InputActionReference driveAction;

    private WheelchairMotor motor;

    private void Awake()
    {
        motor = GetComponent<WheelchairMotor>();

        if (driveAction == null)
            Debug.LogWarning($"{name}: no Drive action assigned to PlayerWheelchairDriver.", this);
    }

    private void OnEnable()
    {
        // Actions in their own asset start disabled, so turn this one on.
        // It is deliberately not turned off in OnDisable: when control moves to another chair,
        // that chair's driver may already be using the same action.
        if (driveAction != null) driveAction.action.Enable();
    }

    private void OnDisable()
    {
        // Let go of the stick so the chair brakes instead of carrying on with the last input.
        if (motor != null) motor.SetDriveInput(Vector2.zero);
    }

    private void Update()
    {
        Vector2 stick = driveAction != null ? driveAction.action.ReadValue<Vector2>() : Vector2.zero;
        motor.SetDriveInput(ApplyDeadzone(stick, motor.Stats.deadzone));
    }

    // Ignores small stick drift, then rescales so input still starts smoothly from zero.
    private static Vector2 ApplyDeadzone(Vector2 stick, float deadzone)
    {
        float magnitude = stick.magnitude;
        if (magnitude <= deadzone) return Vector2.zero;

        float scaled = Mathf.InverseLerp(deadzone, 1f, Mathf.Min(magnitude, 1f));
        return stick / magnitude * scaled;
    }
}
