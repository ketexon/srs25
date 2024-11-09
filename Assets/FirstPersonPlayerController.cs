using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class FirstPersonPlayerController : PlayerController
{
    protected override void OnEnable()
    {
        base.OnEnable();

        movement.LookAt(Vector3.forward);
        camera.transform.rotation = Quaternion.identity;
    }

    public void OnLook(InputValue inputValue)
    {
        if (!enabled) return;

        Vector2 delta = inputValue.Get<Vector2>();
        movement.LookDelta(delta * Settings.Sensitivity);
        gun.SetRotation(movement.Pitch);
        Debug.Log(movement.Pitch);
    }

    public void OnMove(InputValue inputValue)
    {
        if (!enabled) return;

        var inputDir = inputValue.Get<Vector2>();
        movement.MoveDir = (
            transform.right * inputDir.x 
            + transform.forward * inputDir.y
        ).normalized;
    }
}
