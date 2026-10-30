using UnityEngine;

// Tuning values shared by every wheelchair, so player and AI chairs drive with the same limits.
// Create one in the Project window: right-click > Create > Wheelchair > Stats.
[CreateAssetMenu(fileName = "WheelchairStats", menuName = "Wheelchair/Stats")]
public class WheelchairStats : ScriptableObject
{
    [Header("Speed")]
    [Tooltip("Top forward speed in m/s. Power soccer rules cap chairs at 10 km/h (about 2.78 m/s).")]
    [Min(0f)] public float maxForwardSpeed = 2.78f;

    [Tooltip("Top reverse speed in m/s.")]
    [Min(0f)] public float maxReverseSpeed = 1.4f;

    [Tooltip("How quickly the chair speeds up, in m/s per second.")]
    [Min(0f)] public float acceleration = 3f;

    [Tooltip("How quickly the chair slows down or changes direction, in m/s per second.")]
    [Min(0f)] public float braking = 5f;

    [Header("Turning")]
    [Tooltip("Turn rate in degrees per second when standing still (spinning in place).")]
    [Min(0f)] public float maxTurnRate = 240f;

    [Tooltip("Turn rate in degrees per second at top speed. Real chair controllers turn slower when moving fast.")]
    [Min(0f)] public float maxTurnRateAtTopSpeed = 90f;

    [Tooltip("How quickly the turn rate changes, in degrees per second per second.")]
    [Min(0f)] public float turnAcceleration = 720f;

    [Header("Grip")]
    [Tooltip("The most the wheels can change forward speed, in m/s per second. Higher = harder to shove.")]
    [Min(0f)] public float driveGrip = 10f;

    [Tooltip("How strongly sideways sliding is cancelled, in m/s per second. Higher = less sliding after bumps.")]
    [Min(0f)] public float sideGrip = 12f;

    [Tooltip("The most the wheels can change the turn rate, in degrees per second per second.")]
    [Min(0f)] public float turnGrip = 1440f;

    [Header("Player Input")]
    [Tooltip("Player stick values smaller than this are treated as zero. Applied by PlayerWheelchairDriver only, so AI input is not affected.")]
    [Range(0f, 0.5f)] public float deadzone = 0.1f;
}
