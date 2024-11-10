using System.Collections.Generic;
using UnityEngine;

public interface IBulletCaster {
    int Cast(Bullet bullet, RaycastHit[] hitResults);
    HashSet<BodyPart> IgnoreBodyParts => null;
    void DebugDraw(Bullet bullet);
}

public class RayBulletCaster : IBulletCaster
{
    public int Cast(Bullet bullet, RaycastHit[] hitResults)
    {
        Debug.Log(Physics.Raycast(
            bullet.transform.position,
            bullet.transform.forward,
            float.PositiveInfinity,
            bullet.BulletHitboxLayerMask
        ));
        return Physics.RaycastNonAlloc(
            bullet.transform.position,
            bullet.transform.forward,
            hitResults,
            float.PositiveInfinity,
            bullet.BulletHitboxLayerMask,
            QueryTriggerInteraction.Collide
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

    HashSet<BodyPart> _ignoreBodyParts = new() { BodyPart.Arm };
    public HashSet<BodyPart> IgnoreBodyParts => _ignoreBodyParts;

    public int Cast(Bullet bullet, RaycastHit[] hitResults)
    {
        return Physics.BoxCastNonAlloc(
            bullet.transform.position,
            HalfExtents,
            bullet.transform.forward,
            hitResults,
            Quaternion.identity,
            float.PositiveInfinity,
            bullet.BulletHitboxLayerMask,
            QueryTriggerInteraction.Collide
        );
    }

    public void DebugDraw(Bullet bullet){
        Gizmos.DrawCube(bullet.transform.position, HalfExtents);
    }
}

public class Bullet : MonoBehaviour
{
    [SerializeField] public LayerMask BulletHitboxLayerMask;

    [System.NonSerialized] public IBulletCaster Caster;
    [System.NonSerialized] public float Velocity;
    [System.NonSerialized] public float EnergyPenaltyMult;

    [System.NonSerialized] public RaycastHit[] RaycastHits;

    void Start()
    {
        var nHits = Caster.Cast(this, RaycastHits);
        float remainingVel = Velocity;
        Debug.Log(nHits);
        for(int i = 0; i < nHits && remainingVel > 0; ++i){
            var hit = RaycastHits[i];
            var collider = hit.collider;
            var hitBox = collider.GetComponent<BulletHitBox>();
            if(!hitBox){
                continue;
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
            var dVel = startVel - Mathf.Max(0, remainingVel);
            hitBox.OnHit(dVel);
            Debug.Log(remainingVel);
        }
    }

    void OnDrawGizmos() {
        Caster?.DebugDraw(this);
    }
}
