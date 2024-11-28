using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyVisionCone : MonoBehaviour
{
    [SerializeField] HumanModel model;
    [SerializeField] float fov = 45.0f;
    [SerializeField] float range = 10.0f;
    [SerializeField] LayerMask entityLayerMask;
    [SerializeField] LayerMask collisionLayerMask;

    [SerializeField] public UnityEvent<EntityHitBox> NewHitBoxVisibleEvent;
    [SerializeField] public UnityEvent<EntityHitBox> HitBoxInvisibleEvent;

    new SphereCollider collider;

    // sorted set for fast iteration
    // could, in theory, use hashset
    // in theory, there are at most 8 elements (only the player)
    // in this set, so its not terrible performance
    readonly ISet<EntityHitBox> inRangeHitBoxes = new SortedSet<EntityHitBox>(new Kutie.ObjectComparer());
    [System.NonSerialized] readonly public ISet<EntityHitBox> VisibleHitBoxes = new SortedSet<EntityHitBox>(new Kutie.ObjectComparer());

    void Awake()
    {
        collider = GetComponent<SphereCollider>();
        collider.radius = range;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<EntityHitBox>() is EntityHitBox hitBox)
        {
            if(hitBox.Model.Entity.Team != model.Entity.Team)
            {
                inRangeHitBoxes.Add(hitBox);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<EntityHitBox>() is EntityHitBox hitBox)
        {
            inRangeHitBoxes.Remove(hitBox);
            if(VisibleHitBoxes.Remove(hitBox)){
                HitBoxInvisibleEvent.Invoke(hitBox);
            }
        }
    }

    void Update()
    {
        // Recalculate visible hitboxes
        foreach(var hitBox in inRangeHitBoxes)
        {
            bool visible = true;
            // check that the center of the HB is within FOV
            var dir = (hitBox.transform.position - transform.position).normalized;
            var angle = Vector3.Angle(dir, transform.forward);
            if(angle > fov)
            {
                visible = false;
            }
            else {
                // check that there is nothing in the way of the hitbox
                var result = Physics.Raycast(
                    transform.position,
                    dir,
                    out var hit,
                    float.PositiveInfinity,
                    collisionLayerMask
                );
                if(!result || hit.collider.gameObject != hitBox.gameObject)
                {
                    visible = false;
                }
            }

            if(visible){
                // Add to visible hitboxes
                // If it wasn't in there already
                // invoke event
                if(VisibleHitBoxes.Add(hitBox)) {
                    NewHitBoxVisibleEvent.Invoke(hitBox);
                }
            }
            else {
                // Remove from visible hitboxes
                // If it was removed,
                // invoke event
                if(VisibleHitBoxes.Remove(hitBox)){
                    HitBoxInvisibleEvent.Invoke(hitBox);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Vector3[] points = new Vector3[]
        {
            transform.position,
            transform.position + Quaternion.AngleAxis(fov, Vector3.up) * transform.forward * range,
            transform.position + Quaternion.AngleAxis(fov, Vector3.up) * transform.forward * range,
            transform.position + Quaternion.AngleAxis(-fov, Vector3.up) * transform.forward * range,
            transform.position + Quaternion.AngleAxis(-fov, Vector3.up) * transform.forward * range,
            transform.position,
        };
        Gizmos.color = (
            VisibleHitBoxes.Count > 0
            ? Color.red
            : Color.white
        );
        Gizmos.DrawLineList(
            points
        );
    }
}
