using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] EntityMovement movement;

    void Update()
    {
        var pitchQuat = Quaternion.AngleAxis(
            movement.Pitch,
            Vector3.right
        );
        var yawQuat = movement.TargetTransform.rotation;
        transform.rotation = yawQuat * pitchQuat;
    }
}