using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SidePlayerController : PlayerController
{
    protected override void OnEnable()
    {
        base.OnEnable();

        movement.LookAt(Vector3.forward);
    }

    public void OnPoint(InputValue inputValue)
    {
        if (!enabled) return;

        Vector2 screenPoint = inputValue.Get<Vector2>();

        Plane plane = new(
            (Camera.main.transform.position - transform.position).normalized,
            transform.position
        );

        Ray ray = Camera.main.ScreenPointToRay(new(screenPoint.x, screenPoint.y, Camera.main.nearClipPlane));
        if(plane.Raycast(ray, out var enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            Vector3 delta = worldPoint - transform.position;
            float dir = Vector3.Dot(transform.forward, delta);
            if(dir < 0)
            {
                movement.LookDir(-transform.forward);
            }
            gun.PointAt(worldPoint);
        }
    }

    public void OnMove(InputValue inputValue)
    {
        if (!enabled) return;

        var right = Vector3.Cross(Vector3.up, camera.transform.forward);
        var inputDir = inputValue.Get<Vector2>();
        movement.MoveDir = (right * inputDir.x).normalized;
    }
}
