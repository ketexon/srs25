using UnityEngine;
using UnityEngine.UIElements;

public class Gun : MonoBehaviour
{
    [SerializeField] Entity entity;
    [SerializeField] float minAngle = -60;
    [SerializeField] float maxAngle = 80;
    [SerializeField] Transform tip;
    [SerializeField] GameObject bulletPrefab;

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

    // this is provided to bullet so that
    // they don't have to alloc
    RaycastHit[] raycastHits = new RaycastHit[64];

    public void PointAt(Vector3 position)
    {
        // project position into forward-up plane
        var delta = position - transform.position;
        delta -= Vector3.Dot(delta, entity.transform.right) * entity.transform.right;
        var angle = Vector3.SignedAngle(
            entity.transform.forward,
            delta.normalized,
            entity.transform.right
        );
        SetRotation(angle);
    }

    public void SetRotation(float rotation)
    {
        rotation = Mathf.Clamp(rotation, -maxAngle, -minAngle);
        transform.localRotation = Quaternion.AngleAxis(
            rotation,
            Vector3.right
        );
    }

    void Update()
    {
        if (Shooting)
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        if (Time.time > lastShotTime + shotInterval)
        {
            lastShotTime = Time.time;

            var bulletGO = Instantiate(bulletPrefab, tip.transform.position, tip.transform.rotation);
            var bullet = bulletGO.GetComponent<Bullet>();
            bullet.Caster = BulletCaster;
            bullet.RaycastHits = raycastHits;
            bullet.Velocity = muzzleVelocity;
            bullet.EnergyPenaltyMult = energyPenaltyMult;
            bullet.Damage = damage;
        }
    }
}
