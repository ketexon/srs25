using Unity.Cinemachine;
using UnityEngine;

public class EntityMovement : MonoBehaviour
{
    const float ROTATE_SPEED = 8;

    [SerializeField] float movementSpeed;
    [SerializeField] float minPitch = -60;
    [SerializeField] float maxPitch = 80;
    [SerializeField] public Transform Eyes;

    [System.NonSerialized] public Transform TargetTransform;

    Vector3 moveDir;

    float _yaw = 0;
    public float Yaw
    {
        get => _yaw;
        protected set
        {
            _yaw = value;
            if (TargetTransform)
            {
                TargetTransform.rotation = Quaternion.AngleAxis(Yaw, Vector3.up);
            }
        }
    }

    float _pitch = 0;
    public float Pitch
    {
        get => _pitch;
        protected set
        {
            _pitch = Mathf.Clamp(value, minPitch, maxPitch);
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

    virtual protected void Awake()
    {
        var targetTransformGO = new GameObject($"{gameObject.name} Target Transform");
        TargetTransform = targetTransformGO.transform;
        Yaw = transform.rotation.eulerAngles.y;
        Pitch = Eyes.rotation.eulerAngles.x;
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

    public void LookAt(Vector3 worldPoint)
    {
        Vector3 delta = worldPoint - Eyes.transform.position;
        LookDir(delta.normalized);
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
            Quaternion.AngleAxis(-Yaw, Vector3.up) * dir,
            Vector3.right
        );
    }

    void Update()
    {
        transform.position += movementSpeed * Time.deltaTime * moveDir;
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            TargetTransform.rotation,
            Time.deltaTime * ROTATE_SPEED
        );
        Eyes.localRotation = Quaternion.AngleAxis(Pitch, Vector3.right);
    }
}