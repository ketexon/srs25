using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

public class Gun : MonoBehaviour
{
    [SerializeField] new Rigidbody rigidbody;
    [SerializeField] new Collider collider;
    [SerializeField] Entity entity;
    [SerializeField] float minAngle = -60;
    [SerializeField] float maxAngle = 80;
    [SerializeField] Transform tip;
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

    public float CombinedRotation => Rotation + recoilSpring.CurrentValue;

    // this is provided to bullet so that
    // they don't have to alloc
    RaycastHit[] raycastHits = new RaycastHit[64];

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
        transform.localRotation = Quaternion.AngleAxis(
            actualRotation,
            Vector3.right
        );
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
                        recoilSpring.CurrentValue,
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

        recoilSpring.Velocity -= recoilAmplitude;
    }

    public void Drop()
    {
        rigidbody.isKinematic = false;
        collider.enabled = true;
    }
}
