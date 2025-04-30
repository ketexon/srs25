using Kutie.Extensions;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public enum EntityMovementMove
{
    Transform,
    Rigidbody
}

public class EntityMovement : MonoBehaviour
{
    const float ROTATE_SPEED = 8;
    [SerializeField] float baseOffset = 1.06f;
    [SerializeField] float minPitch = -60;
    [SerializeField] float maxPitch = 80;
    [SerializeField] public Transform Eyes;
    [SerializeField] public EntityMovementMove Mode = EntityMovementMove.Transform;
    [SerializeField] Rigidbody rb;
    [System.NonSerialized] public Transform TargetTransform;
    [SerializeField] EntityStats entityStats;
    [System.NonSerialized] public float MovementSpeed;
    private bool isGrounded = true;
    private bool isSliding = false;
    private Vector3 slideDir = Vector3.zero;
    private float slideVal = 0f;
    [SerializeField] private float slideMult = 2f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private Transform groundCheckOrigin;
    [SerializeField] private float airSlideMult = 0.6f;
    [SerializeField] private float slideDecayTime = 0.4f;
    private float lastJumpTime = -1f;
    private float lastSlideTime = -1f;

    public bool Walking => rb.linearVelocity.XZ().magnitude > 0.1f;

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
        MovementSpeed = entityStats.GetStat(EntityStats.StatType.Speed)*16f;
        TargetTransform = targetTransformGO.transform;
        Yaw = transform.rotation.eulerAngles.y;
        Pitch = Eyes.rotation.eulerAngles.x;

        if (rb)
        {
            rb.transform.SetParent(null);
        }
    }

    void OnEnable()
    {
        entityStats.OnStatChanged.AddListener(OnStatsChanged);
    }

    void OnDisable()
    {
        entityStats.OnStatChanged.RemoveListener(OnStatsChanged);
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

    void OnJump()
    {
        if (Time.time - lastJumpTime > 0.3f)
        {
            CheckGrounded();
            if (isGrounded)
            {
                rb.AddForce(rb.transform.up * 5, ForceMode.Impulse);
                isGrounded = false;
                lastJumpTime = Time.time;
            }
        }
    }

    void OnSlide()
    {
        if (!isSliding)
        {
            Debug.Log(moveDir);
            isSliding = true;
            slideDir = new Vector3(moveDir.x, 0f, moveDir.z);
            slideVal = MovementSpeed * slideMult;
            lastSlideTime = Time.time;
        }
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
        if(Time.time - lastJumpTime > 0.3f)
        {
            CheckGrounded();
        }
        Debug.Log(isGrounded);
        if (Mode == EntityMovementMove.Transform)
        {
            transform.position += MovementSpeed * Time.deltaTime * moveDir;
        }
        else
        {
            var vel = MovementSpeed * moveDir;
            rb.linearVelocity = new(vel.x, rb.linearVelocity.y, vel.z);
            float airSpeed = 1f;
            if (!isGrounded)
            {
                airSpeed = airSlideMult;
            }
            rb.linearVelocity += new Vector3(slideDir.x * slideVal*airSpeed, 0, slideDir.z * slideVal*airSpeed);

            if (isGrounded&&Time.time-lastSlideTime>slideDecayTime)
                slideVal = Mathf.Lerp(slideVal, 0, Time.deltaTime * 5);
            else if (rb.linearVelocity.XZ().magnitude > maxSpeed)
            {
                Vector2 xzVec = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
                xzVec.Normalize();
                rb.linearVelocity = new Vector3(xzVec.x * maxSpeed, rb.linearVelocity.y, xzVec.y * maxSpeed);
            }

            if (slideVal <= 0.5f)
            {
                isSliding = false;
                slideVal = 0;
            }

            transform.position = rb.transform.position;
        }

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            TargetTransform.rotation,
            Time.deltaTime * ROTATE_SPEED
        );

        Eyes.localRotation = Quaternion.AngleAxis(Pitch, Vector3.right);
    }

    void CheckGrounded()
    {
        if (groundCheckOrigin != null)
        {
            float sphereRadius = 0.1f;
            int layerMask = ~(LayerMask.GetMask("Player", "Entity"));
            bool hitGround = Physics.CheckSphere(groundCheckOrigin.position, sphereRadius, layerMask, QueryTriggerInteraction.Ignore);

            if (hitGround)
            {
                bool prevGrounded = isGrounded;
                isGrounded = true;

                if (!prevGrounded && isGrounded)
                {
                    isSliding = false;
                    slideVal = 0;
                }
            }
            else
            {
                isGrounded = false;
            }
        }
    }

    public void Teleport(Vector3 location)
    {
        location.y += baseOffset;
        rb.position = location;
        transform.position = location;
    }

    void OnStatsChanged(EntityStats.StatType stat, float value)
    {
        switch (stat)
        {
            case EntityStats.StatType.Speed:
                MovementSpeed = value*16f;
                break;
        }
    }
}
