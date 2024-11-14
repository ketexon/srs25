using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonPlayerController : PlayerController
{
    Vector2 inputDir = Vector2.zero;

    protected override void OnEnable()
    {
        base.OnEnable();

        camera.transform.rotation = Quaternion.identity;
        gun.BulletCaster = new RayBulletCaster();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        Cursor.lockState = CursorLockMode.None;
    }

    public void OnLook(InputValue inputValue)
    {
        if (!enabled) return;
        if(Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 delta = inputValue.Get<Vector2>();
        movement.LookDelta(delta * Settings.Sensitivity);
        gun.SetRotation(movement.Pitch);
        UpdateMovementDir();
    }

    public void OnClick(){
        if(!enabled) return;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnUnlockCursor(){
        if(!enabled) return;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnMove(InputValue inputValue)
    {
        if (!enabled) return;

        inputDir = inputValue.Get<Vector2>();
        UpdateMovementDir();
    }

    void UpdateMovementDir()
    {
        movement.MoveDir = (
            movement.TargetTransform.right * inputDir.x
            + movement.TargetTransform.forward * inputDir.y
        ).normalized;
    }
}
