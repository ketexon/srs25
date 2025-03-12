using UnityEngine;

public class Gun : EntityItem
{
    [SerializeField] new Rigidbody rigidbody;
    public EntityMovement entityMovement;
    [SerializeField] new Collider collider;
    [SerializeField] Animator animator;
    [SerializeField] float minAngle = -60;
    [SerializeField] float maxAngle = 80;
    [SerializeField] Transform tip;
    Vector3 defaultPosition;
    /// <summary>
    /// if true, the bullet will spawn in front
    /// of the entity instead of at the tip of the gun.
    /// it still has recoil
    /// </summary>
    [SerializeField] public bool ShootStraight = false;

    /// <summary>
    /// if true, the gun will not have any rotation
    /// due to recoil.
    /// </summary>
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

    /// <summary>
    /// This field represents a multiplier for how
    /// much energy should be lost when colliding with an object
    /// The higher it is, the more energy lost, which represents
    /// a larger bullet
    /// </summary>
    [SerializeField] float energyPenaltyMult = 300000;
    [SerializeField] float damage = 30;

    [System.NonSerialized] public bool Shooting = false;
    [System.NonSerialized] public IBulletCaster BulletCaster = new RayBulletCaster();
    float lastShotTime = float.NegativeInfinity;

    private float vBaseRotation = 0;
    public float vRotation {
        get => vBaseRotation + vRotSpring.CurrentValue;
        set {
            value = Mathf.Clamp(value, -maxAngle, -minAngle);
            vBaseRotation = value;
            //vRotSpring.TargetValue = value;
            //vRotSpring.LockToTarget();
        }
    }

    public float hRotation {
        get => hRotSpring.CurrentValue;
        set {
            value = Mathf.Clamp(value, -maxAngle, -minAngle);
            hRotSpring.TargetValue = value;
            hRotSpring.LockToTarget();
        }
    }

    public float vTargetRotation {
        get => vRotSpring.TargetValue;
        set {
            value = Mathf.Clamp(value, -maxAngle, -minAngle);
            vRotSpring.TargetValue = value;
        }
    }
    public float hTargetRotation {
        get => hRotSpring.TargetValue;
        set {
            value = Mathf.Clamp(value, -maxAngle, -minAngle);
            hRotSpring.TargetValue = value;
        }
    }

    public float CurrentRecoil => vRotSpring.CurrentValue;

    public float CombinedRotation => vRotation - vRotSpring.CurrentValue;
    // this is provided to bullet so that
    // they don't have to alloc
    RaycastHit[] raycastHits = new RaycastHit[64];

    public override void Use(bool start = true)
    {
        Shooting = start;
    }

    void OnEnable()
    {
        entity.Stats.OnStatChanged.AddListener(OnStatChanged);
    }

    void OnDisable(){
        Shooting = false;
        entity.Stats.OnStatChanged.RemoveListener(OnStatChanged);
    }

    private void Awake()
    {
        hRotSpring = new(0, hRotRecoil);
        vRotSpring = new(0, vRotRecoil);
        hTransSpring = new(0, vTransRecoil);
        vTransSpring = new(0, hTransRecoil);
        zTransSpring = new(0, zTransRecoil);
        defaultPosition = transform.localPosition;
        entityMovement = entity.GetComponent<EntityMovement>();
    }

    public void PointAt(Vector3 position, bool instant = false)
    {
        // project position into forward-up plane
        var delta = position - transform.position;
        delta -= Vector3.Dot(delta, entity.transform.right) * entity.transform.right;
        var angle = Vector3.SignedAngle(
            entity.transform.forward,
            delta.normalized,
            entity.transform.right
        );
        if(instant){
            vRotation = angle;
        }
        else {
            vTargetRotation = angle;
        }
    }

    void Update()
    {
        if (Shooting)
        {
            Shoot();
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
        transform.localRotation = rotationX*rotationY;
        transform.localPosition = defaultPosition - transform.localRotation * (Vector3.forward * zTransSpring.CurrentValue *0.0005f);
        if (animator){
            UpdateAnimator();
        }
    }

    private void FixedUpdate()
    {
        vRotSpring.Update(Time.fixedDeltaTime);
        hRotSpring.Update(Time.fixedDeltaTime);
        hTransSpring.Update(Time.fixedDeltaTime);
        vTransSpring.Update(Time.fixedDeltaTime);
        zTransSpring.Update(Time.fixedDeltaTime);
    }

    void UpdateAnimator(){
        animator.SetBool("shooting", Shooting);
        animator.SetBool("walking", entity.Movement.Walking);
    }

    public void Shoot()
    {
        if (Time.time > lastShotTime + shotInterval)
        {
            lastShotTime = Time.time;

            Vector3 spawnPos;
            Quaternion spawnRot;
            if(ShootStraight){
                spawnPos = entity.Movement.Eyes.position + Vector3.Project(
                    tip.transform.position - entity.Movement.Eyes.position,
                    entity.Movement.Eyes.transform.forward
                );
                spawnRot = entity.Movement.Eyes.transform.rotation;
                if(!NoRecoil){
                    spawnRot = Quaternion.AngleAxis(
                        -hTransSpring.CurrentValue,
                        entity.Movement.Eyes.transform.right
                    ) * spawnRot;
                }
            }
            else {
                spawnPos = tip.transform.position;
                spawnRot = tip.transform.rotation;
            }
            var bulletGO = Instantiate(bulletPrefab, spawnPos, spawnRot);
            var bullet = bulletGO.GetComponent<Bullet>();
            bullet.Caster = BulletCaster;
            bullet.RaycastHits = raycastHits;
            bullet.Velocity = muzzleVelocity;
            bullet.EnergyPenaltyMult = energyPenaltyMult;
            bullet.Damage = damage;

        }

        //hTransSpring.Velocity += hTransVel;
        float vRot = Random.Range(0, vRotVel);
        float hRot = Random.Range(-hRotVel, hRotVel);
        if (!NoRecoil)
        {
            vRotSpring.Velocity -= 0.25f * vRot;
            hRotSpring.Velocity += hRot;
            zTransSpring.CurrentValue += zTransKick;
            entityMovement.LookDelta(new Vector2(0.005f*cameraHRecoilMult*hRotSpring.Velocity, -0.025f*cameraVRecoilMult* vRotSpring.Velocity));

        }

    }

    public void Drop()
    {
        rigidbody.isKinematic = false;
        collider.enabled = true;
        entity = null;
    }
    public void PickUp(Entity entity)
    {
        rigidbody.isKinematic = true;
        collider.enabled = false;
        this.entity = entity;
    }

    void OnStatChanged(EntityStats.StatType type, float value)
    {
        switch(type){
            case EntityStats.StatType.Strength:
                damage = 10*value;
                break;
        }
    }
}
