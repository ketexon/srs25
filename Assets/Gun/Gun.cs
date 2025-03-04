using UnityEngine;

public class Gun : EntityItem
{
    [SerializeField] new Rigidbody rigidbody;
    [SerializeField] new Collider collider;
    [SerializeField] Animator animator;
    [SerializeField] float minAngle = -60;
    [SerializeField] float maxAngle = 80;
    [SerializeField] Transform tip;
    Vector3 defaultPosition = new Vector3(0.11f, 0.385f, 0.007f);
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

    [SerializeField] Kutie.SpringFloatValue rotationSpring;

    [Header("Recoil")]
    [SerializeField] Kutie.SpringFloatValue recoilSpring;
    [SerializeField] float recoilAmplitude = 10;
    [SerializeField] Kutie.SpringFloatValue HorizRecoilSpring;

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


    public float Rotation {
        get => rotationSpring.CurrentValue;
        set {
            value = Mathf.Clamp(value, -maxAngle, -minAngle);
            rotationSpring.TargetValue = value;
            rotationSpring.LockToTarget();
        }
    }

    public float TargetRotation {
        get => rotationSpring.TargetValue;
        set {
            value = Mathf.Clamp(value, -maxAngle, -minAngle);
            rotationSpring.TargetValue = value;
        }
    }

    public float CurrentRecoil => recoilSpring.CurrentValue;

    public float CombinedRotation => Rotation - recoilSpring.CurrentValue;

    // this is provided to bullet so that
    // they don't have to alloc
    RaycastHit[] raycastHits = new RaycastHit[64];

    public override void Use(bool start = true)
    {
        Shooting = start;
    }

    void OnDisable(){
        Shooting = false;
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
            Rotation = angle;
        }
        else {
            TargetRotation = angle;
        }
    }

    void Update()
    {
        if (Shooting)
        {
            Shoot();
        }

        var actualRotation = NoRecoil ? Rotation : CombinedRotation;
        Quaternion rotationX = Quaternion.AngleAxis(
            actualRotation,
            Vector3.right
        );
        
        Quaternion rotationY = Quaternion.AngleAxis(
           HorizRecoilSpring.CurrentValue,
            Vector3.up
        );
        
        transform.localRotation = rotationX*rotationY;
        if (transform.localPosition!=defaultPosition)
        {
            transform.localPosition += (defaultPosition-transform.localPosition) / 20;
        }
        
        if(animator){
            UpdateAnimator();
        }
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
                        -recoilSpring.CurrentValue,
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
            float yRadians = CombinedRotation * Mathf.PI / 180;
            transform.localPosition -= new Vector3(0, -0.0005f*recoilAmplitude*Mathf.Sin(yRadians), 0.0005f*recoilAmplitude*Mathf.Cos(yRadians));
        }

        recoilSpring.Velocity += recoilAmplitude;
        float horizRotation = Random.Range(-recoilAmplitude, recoilAmplitude);
        HorizRecoilSpring.Velocity += horizRotation;
    }

    public void Drop()
    {
        rigidbody.isKinematic = false;
        collider.enabled = true;
    }
}
