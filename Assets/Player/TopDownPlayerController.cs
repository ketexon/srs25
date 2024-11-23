using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TopDownPlayerController : PlayerController
{
    public void OnPoint(InputValue inputValue)
    {
        if (!enabled) return;

        Vector2 screenPoint = inputValue.Get<Vector2>();

        Plane plane = new(
            (Camera.main.transform.position - transform.position).normalized,
            transform.position
        );

        Ray ray = Camera.main.ScreenPointToRay(new(screenPoint.x, screenPoint.y, Camera.main.nearClipPlane));
        if (plane.Raycast(ray, out var enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            movement.LookAt(worldPoint);
        }
    }

    public void OnMove(InputValue inputValue)
    {
        if (!enabled) return;

        var inputDir = inputValue.Get<Vector2>();
        movement.MoveDir = new(
            inputDir.x,
            0,
            inputDir.y
        );
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        gun.SetRotation(0);
    }
}
