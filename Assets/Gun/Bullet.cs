using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Performs a cast for a bullet
/// See <see cref="RayBulletCaster"/> and <see cref="BoxBulletCaster"/>.
/// Used primarily by SideView and TopDown player controllers
/// </summary>
public interface IBulletCaster {
    int Cast(Bullet bullet, RaycastHit[] hitResults);
    HashSet<BodyPart> IgnoreBodyParts => null;
    void DebugDraw(Bullet bullet);
}

public class RayBulletCaster : IBulletCaster
{
    public int Cast(Bullet bullet, RaycastHit[] hitResults)
    {
        return Kutie.Physics.RaycastNonAllocSorted(
            bullet.transform.position,
            bullet.transform.forward,
            hitResults,
            float.PositiveInfinity,
            bullet.BulletHitboxLayerMask,
            QueryTriggerInteraction.Ignore
        );
    }

    public void DebugDraw(Bullet bullet){
        Debug.DrawRay(bullet.transform.position, bullet.transform.forward * 5);
    }
}

public class BoxBulletCaster : IBulletCaster
{
    public Vector3 HalfExtents;
    public Quaternion Orientation = Quaternion.identity;

    HashSet<BodyPart> _ignoreBodyParts = new() { BodyPart.LeftArm };
    public HashSet<BodyPart> IgnoreBodyParts => _ignoreBodyParts;

    public int Cast(Bullet bullet, RaycastHit[] hitResults)
    {
        return Kutie.Physics.BoxCastNonAllocSorted(
            bullet.transform.position,
            HalfExtents,
            bullet.transform.forward,
            hitResults,
            Quaternion.identity,
            float.PositiveInfinity,
            bullet.BulletHitboxLayerMask,
            QueryTriggerInteraction.Ignore
        );
    }

    public void DebugDraw(Bullet bullet){
        Gizmos.DrawCube(bullet.transform.position, HalfExtents);
    }
}

public class Bullet : MonoBehaviour
{
    [SerializeField] public LayerMask BulletHitboxLayerMask;
    [SerializeField] GameObject tracerPrefab;
    [SerializeField] GameObject bloodSplatterPrefab;

    [System.NonSerialized] public IBulletCaster Caster;
    [System.NonSerialized] public float Velocity;
    [System.NonSerialized] public float EnergyPenaltyMult;
    [System.NonSerialized] public float Damage;

    [System.NonSerialized] public RaycastHit[] RaycastHits;

    void Start()
    {
        var nHits = Caster.Cast(this, RaycastHits);

        float remainingVel = Velocity;
        var startPoint = transform.position;
        var endPoint = startPoint;
        var endNormal = Vector3.zero;
        for (int i = 0; i < nHits && remainingVel > 0; ++i){
            var hit = RaycastHits[i];

            endPoint = hit.point;
            endNormal = hit.normal;

            var hitBox = hit.collider.GetComponent<BulletHitBox>();
            // if it doesn't have a hitbox, it is
            // a static object
            if(!hitBox){
                remainingVel = 0;
                break;
            }
            if(
                hitBox is EntityHitBox entityHitBox
                && Caster.IgnoreBodyParts is not null
                && Caster.IgnoreBodyParts.Contains(entityHitBox.BodyPart)
            ) {
                continue;
            }

            // damage is proportional to the change in velocity
            // E_0 = v_0^2, E_f = v_f^2
            // E_f = E_0 - dE
            // v_f = sqrt(v_0^2 - dE)
            var startVel = remainingVel;
            remainingVel = Mathf.Sqrt(
                startVel * startVel
                - hitBox.EnergyPenalty * EnergyPenaltyMult
            );
            hitBox.OnHit(Damage);
            Instantiate(
                bloodSplatterPrefab,
                hit.point,
                Quaternion.LookRotation(hit.normal)
            );
        }

        // the bullet never stopped
        if(remainingVel > 0)
        {
            endPoint += transform.forward * 100;
        }

        var tracerGO = Instantiate(tracerPrefab, startPoint, transform.rotation);
        var tracer = tracerGO.GetComponent<Tracer>();
        tracer.Hit = remainingVel <= 0;
        tracer.EndPoint = endPoint;
        tracer.EndNormal = endNormal;
        Destroy(gameObject);
    }

    void OnDrawGizmos() {
        Caster?.DebugDraw(this);
    }
}
