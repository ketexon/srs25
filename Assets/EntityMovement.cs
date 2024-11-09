using Unity.Cinemachine;
using UnityEngine;

public class EntityMovement : MonoBehaviour
{
    [SerializeField] float movementSpeed;
    [SerializeField] CinemachineCamera firstPersonCamera;
    [SerializeField] float minPitch = -60;
    [SerializeField] float maxPitch = 80;

    Vector3 moveDir;

    float _yaw = 0;
    public float Yaw
    {
        get => _yaw;
        protected set
        {
            _yaw = value;
            transform.rotation = Quaternion.AngleAxis(Yaw, Vector3.up);
        }
    }

    float _pitch = 0;
    public float Pitch
    {
        get => _pitch;
        protected set
        {
            _pitch = Mathf.Clamp(value, minPitch, maxPitch);
            if (firstPersonCamera)
            {
                firstPersonCamera.transform.localRotation = Quaternion.AngleAxis(
                    Pitch,
                    Vector3.right
                );
            }
        }
    }

    public Vector3 MoveDir
    {
        get => moveDir;
        set
        {
            moveDir = value;
            moveDir.y = 0;
        }
    }

    public void LookDelta(float deltaYaw)
    {
        Yaw += deltaYaw;
    }

    public void LookDelta(Vector2 delta)
    {
        Yaw += delta.x;
        Pitch -= delta.y;
    }

    public void LookAt(Vector3 point)
    {
        Vector3 delta = point - transform.position;
        LookDir(delta);
    }

    public void LookDir(Vector3 dir)
    {
        Yaw = Vector3.SignedAngle(
            Vector3.forward,
            new Vector3(dir.x, 0, dir.z),
            Vector3.up
        );

        Pitch = Vector3.SignedAngle(
            Vector3.forward,
            dir,
            Vector3.right
        );
    }

    void Update()
    {
        transform.position += movementSpeed * Time.deltaTime * moveDir;
    }
}