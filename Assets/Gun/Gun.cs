using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI.Table;

public enum GunType
{
    Automatic,
    SemiAutomatic,
    Shotgun
}
public class Gun : EntityItem
{
    [SerializeField] GunType g = GunType.Automatic;
    [SerializeField] new Rigidbody rigidbody;
    public EntityMovement entityMovement;
    [SerializeField] new Collider collider;
    [SerializeField] Animator animator;
    [SerializeField] float minAngle = -60;
    [SerializeField] float maxAngle = 80;
    [SerializeField] Transform tip;
    Vector3 defaultPosition;
    [SerializeField] Vector3 armOffset;
    [SerializeField] public bool ShootStraight = false;
    [SerializeField] public bool NoRecoil = false;
    [SerializeField] GameObject bulletPrefab;

    [Header("Recoil")]
    [SerializeField] float cameraVRecoilMult = 1;
    [SerializeField] float cameraHRecoilMult = 1;
    [SerializeField] Kutie.SpringParameters hRotRecoil;
    [SerializeField] float maxHRot;
    [SerializeField] float hRotVel;
    [SerializeField] Kutie.SpringParameters vRotRecoil;
    [SerializeField] float maxVRot;
    [SerializeField] float vRotVel;
    [SerializeField] Kutie.SpringParameters hTransRecoil;
    [SerializeField] float maxHTrans;
    [SerializeField] float hTransVel;
    [SerializeField] Kutie.SpringParameters vTransRecoil;
    [SerializeField] float maxVTrans;
    [SerializeField] float vTransVel;
    [SerializeField] float vTransKick;
    [SerializeField] Kutie.SpringParameters zTransRecoil;
    [SerializeField] float maxZTrans;
    [SerializeField] float zTransVel;
    [SerializeField] float zTransKick;

    Kutie.SpringFloat hRotSpring;
    Kutie.SpringFloat vRotSpring;
    Kutie.SpringFloat hTransSpring;
    Kutie.SpringFloat vTransSpring;
    Kutie.SpringFloat zTransSpring;

    [Header("Gun Stats")]
    [SerializeField] float shotInterval = 1.0f;
    [SerializeField] float muzzleVelocity = 900;
    [SerializeField] float energyPenaltyMult = 300000;
    [SerializeField] float damage = 30;

    [System.NonSerialized] public bool Shooting = false;
    [System.NonSerialized] public IBulletCaster BulletCaster = new RayBulletCaster();
    float lastShotTime = float.NegativeInfinity;

    [SerializeField] GameObject arms;
    [SerializeField] GameObject gloves;
    [SerializeField] GameObject shirt;
    SkinnedMeshRenderer armsRenderer;
    SkinnedMeshRenderer glovesRenderer;
    SkinnedMeshRenderer shirtRenderer;
    float overallRecoilMult = 1f;
    private float vBaseRotation = 0;

    bool hasArms, hasGloves, hasShirt = false;
    public float vRotation
    {
        get => vBaseRotation + vRotSpring.CurrentValue;
        set
        {
            value = Mathf.Clamp(value, -maxAngle, -minAngle);
            vBaseRotation = value;
            //vRotSpring.TargetValue = value;
            //vRotSpring.LockToTarget();
        }
    }

    public float hRotation
    {
        get => hRotSpring.CurrentValue;
        set
        {
            value = Mathf.Clamp(value, -maxAngle, -minAngle);
            hRotSpring.TargetValue = value;
            hRotSpring.LockToTarget();
        }
    }

    public float vTargetRotation
    {
        get => vRotSpring.TargetValue;
        set
        {
            value = Mathf.Clamp(value, -maxAngle, -minAngle);
            vRotSpring.TargetValue = value;
        }
    }
    public float hTargetRotation
    {
        get => hRotSpring.TargetValue;
        set
        {
            value = Mathf.Clamp(value, -maxAngle, -minAngle);
            hRotSpring.TargetValue = value;
        }
    }

    public float CurrentRecoil => vRotSpring.CurrentValue;

    public float CombinedRotation => vRotation - vRotSpring.CurrentValue;
    RaycastHit[] raycastHits = new RaycastHit[64];

    private Vector2 cameraRecoilOffset = Vector2.zero;
    private Vector2 prevCameraRecoilOffset = Vector2.zero;
    [SerializeField] float cameraRecoilReturnSpeed = 8f;

    public override void Use(bool start = true)
    {
        Shooting = start;
    }

    void OnEnable()
    {
        armsRenderer = arms.GetComponent<SkinnedMeshRenderer>();
        glovesRenderer = gloves.GetComponent<SkinnedMeshRenderer>();
        shirtRenderer = shirt.GetComponent<SkinnedMeshRenderer>();
        if (!entity) return;
        entity.Stats.StatChangedEvent.AddListener(OnStatChanged);
    }

    void OnDisable()
    {
        if(!entity) return;
        Shooting = false;
        entity.Stats.StatChangedEvent.RemoveListener(OnStatChanged);
    }

    private void Awake()
    {
        
        hRotSpring = new(0, hRotRecoil);
        vRotSpring = new(0, vRotRecoil);
        hTransSpring = new(0, vTransRecoil);
        vTransSpring = new(0, hTransRecoil);
        zTransSpring = new(0, zTransRecoil);
        defaultPosition = transform.localPosition;
        if (entity)
        entityMovement = entity.GetComponent<EntityMovement>();
    }

    public void PointAt(Vector3 position, bool instant = false)
    {
        var delta = position - transform.position;
        delta -= Vector3.Dot(delta, entity.transform.right) * entity.transform.right;
        var angle = Vector3.SignedAngle(
            entity.transform.forward,
            delta.normalized,
            entity.transform.right
        );
        if (instant)
        {
            vRotation = angle;
        }
        else
        {
            vTargetRotation = angle;
        }
    }

    void Update()
    {
        if (Shooting && g == GunType.Automatic)
        {
            Shoot();
        }
        else if ((g == GunType.SemiAutomatic || g == GunType.Shotgun) && Shooting)
        {
            Shoot();
            Shooting = false;
        }
        if (entity != null && LayerMask.LayerToName(entity.gameObject.layer)=="Player")
        {
            armsRenderer.enabled = true;
            glovesRenderer.enabled = true;
            shirtRenderer.enabled = true;
        }
        else
        {
            armsRenderer.enabled = false;
            glovesRenderer.enabled = false;
            shirtRenderer.enabled = false;
        }
            var actualRotation = NoRecoil ? vRotation : CombinedRotation;
        Quaternion rotationY = Quaternion.AngleAxis(
            hRotSpring.CurrentValue,
            Vector3.up
        );

        Quaternion rotationX = Quaternion.AngleAxis(
            vRotation + vRotSpring.CurrentValue,
            Vector3.right
        );
        if (entity != null)
        {
            transform.localRotation = rotationX * rotationY;
            transform.localPosition = defaultPosition - transform.localRotation * (Vector3.forward * zTransSpring.CurrentValue * 0.0005f * (overallRecoilMult / 2));
        }
            if (animator)
        {
            UpdateAnimator();
        }

        if (!Shooting)
        {
            cameraRecoilOffset = Vector2.Lerp(cameraRecoilOffset, Vector2.zero, Time.deltaTime * cameraRecoilReturnSpeed);
        }

        Vector2 camRecoilDelta = cameraRecoilOffset - prevCameraRecoilOffset;

        if (camRecoilDelta.sqrMagnitude > 0.000001f)
        {
            entityMovement.LookDelta(camRecoilDelta);
        }

        prevCameraRecoilOffset = cameraRecoilOffset;
    }

    private void FixedUpdate()
    {
        vRotSpring.Update(Time.fixedDeltaTime);
        hRotSpring.Update(Time.fixedDeltaTime);
        hTransSpring.Update(Time.fixedDeltaTime);
        vTransSpring.Update(Time.fixedDeltaTime);
        zTransSpring.Update(Time.fixedDeltaTime);
    }

    void UpdateAnimator()
    {
        if (!animator) return;
        animator.SetBool("shooting", Shooting);
        if (!entity) return;
        animator.SetBool("walking", entity.Movement.Walking);
    }

    public void Shoot()
    {
        if (Time.time > lastShotTime + shotInterval)
        {
            lastShotTime = Time.time;

            Vector3 spawnPos;
            Quaternion spawnRot;
            if (ShootStraight)
            {
                spawnPos = entity.Movement.Eyes.position + Vector3.Project(
                    tip.transform.position - entity.Movement.Eyes.position,
                    entity.Movement.Eyes.transform.forward
                );
                spawnRot = entity.Movement.Eyes.transform.rotation;
                if (!NoRecoil)
                {
                    spawnRot = Quaternion.AngleAxis(
                        -hTransSpring.CurrentValue,
                        entity.Movement.Eyes.transform.right
                    ) * spawnRot;
                }
            }
            else
            {
                spawnPos = tip.transform.position;
                spawnRot = tip.transform.rotation;
            }
            if (g == GunType.Automatic || g == GunType.SemiAutomatic)
            {
                SpawnBullet(spawnPos, spawnRot);
            }
            else if (g == GunType.Shotgun)
            {
                for (int i = 0; i < 10; ++i)
                {
                    float spreadAngleX = Random.Range(-2.5f, 2.5f);
                    float spreadAngleY = Random.Range(-2.5f, 2.5f);
                    Quaternion rot = spawnRot;
                    rot.eulerAngles += new Vector3(spreadAngleX, spreadAngleY, 0);
                    SpawnBullet(spawnPos, rot);
                }

            }
            if(g == GunType.Shotgun || g == GunType.SemiAutomatic)
            {
                UpdateRecoil();
            }
        }
        if(g==GunType.Automatic)
        {
            UpdateRecoil();
        }

        //Debug.Log(rigidbody.linearVelocity);
    }

    void SpawnBullet(Vector3 spawnPos, Quaternion spawnRot)
    {
        var bulletGO = Instantiate(bulletPrefab, spawnPos, spawnRot);
        var bullet = bulletGO.GetComponent<Bullet>();
        bullet.GiveTracerVel(entityMovement.rb.linearVelocity);
        bullet.Caster = BulletCaster;
        bullet.RaycastHits = raycastHits;
        bullet.Velocity = muzzleVelocity;
        bullet.EnergyPenaltyMult = energyPenaltyMult;
        bullet.Damage = damage;
    }
    private void UpdateRecoil()
    {
        float vRot = Random.Range(0, vRotVel);
        float hRot = Random.Range(-hRotVel, hRotVel);
        if (!NoRecoil)
        {
            vRotSpring.Velocity -= 0.25f * vRot * overallRecoilMult;
            hRotSpring.Velocity += hRot * overallRecoilMult;
            zTransSpring.CurrentValue += zTransKick;
            float vRotFixed = (vRotSpring.Velocity < 0) ? vRotSpring.Velocity : 0.5f * vRotSpring.Velocity;

            cameraRecoilOffset.x += 0.005f * cameraHRecoilMult * hRot;
            cameraRecoilOffset.y -= 0.025f * cameraVRecoilMult * vRotFixed;
            //clamping
            //cameraRecoilOffset.x = Mathf.Clamp(cameraRecoilOffset.x, -maxHRot, maxHRot);
            //cameraRecoilOffset.y = Mathf.Clamp(cameraRecoilOffset.y, -maxVRot, maxVRot);
        }
    }
    public void Drop()
    {
        rigidbody.isKinematic = false;
        collider.enabled = true;
        entity.Gun = null;
        transform.SetParent(null, true);
        transform.rotation = Quaternion.Euler(0, 0, 90);
        rigidbody.AddForce(entity.transform.forward*10, ForceMode.Impulse);
        entity = null;
    }

    public void PickUp(Entity entity)
    {
        rigidbody.isKinematic = true;
        collider.enabled = false;
        this.entity = entity;
        transform.localPosition = Vector3.zero;
        transform.rotation = entity.transform.rotation;
        defaultPosition = transform.localPosition;
        transform.forward = entity.transform.forward;
        if (entity.Movement != null)
        {
            vBaseRotation = entity.Movement.Pitch;
        }
        transform.SetParent(entity.transform, true);
        entityMovement = entity.GetComponent<EntityMovement>();
        entity.Gun = this;
        defaultPosition = armOffset;
        transform.localPosition = defaultPosition;
    }

    void OnStatChanged(EntityStats.StatType type, float value)
    {
        switch (type)
        {
            case EntityStats.StatType.Strength:
                overallRecoilMult = 0.31f / (value + 0.1f);
                break;
        }
    }
}
