using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class ArcMoveProvider : ContinuousMoveProvider
{
    [SerializeField] Transform rig;          // drag XR Origin (XR Rig) here
    [SerializeField] float turnSpeed = 90f;  // degrees per second

    protected override Vector3 ComputeDesiredMove(Vector2 input)
    {
        // Stick X rotates the rig around its own origin
        rig.Rotate(0f, input.x * turnSpeed * Time.deltaTime, 0f);

        // Stick Y drives forward/back along the (just updated) forward direction
        input.x = 0f;
        return base.ComputeDesiredMove(input);
    }
}
