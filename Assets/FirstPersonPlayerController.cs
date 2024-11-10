using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonPlayerController : PlayerController
{
    protected override void OnEnable()
    {
        base.OnEnable();

        movement.LookAt(Vector3.forward);
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

        var inputDir = inputValue.Get<Vector2>();
        movement.MoveDir = (
            transform.right * inputDir.x
            + transform.forward * inputDir.y
        ).normalized;
    }
}
