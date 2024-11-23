using System.Collections.Generic;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [SerializeField] HumanModel model;
    [SerializeField] float fov = 45.0f;
    [SerializeField] float range = 10.0f;
    [SerializeField] LayerMask entityLayerMask;

    new SphereCollider collider;

    // sorted set for fast iteration
    // could, in theory, use hashset
    readonly ISet<EntityHitBox> inRangeHitBoxes = new SortedSet<EntityHitBox>(new Kutie.ObjectComparer());
    [System.NonSerialized] readonly public List<EntityHitBox> VisibleHitBoxes = new();

    void Awake()
    {
        collider = GetComponent<SphereCollider>();
        collider.radius = range;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<EntityHitBox>() is EntityHitBox hitBox)
        {
            if(hitBox.Model != model)
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
        }
    }

    void Update()
    {
        // Recalculate visible hitboxes
        VisibleHitBoxes.Clear();
        foreach(var hitBox in inRangeHitBoxes)
        {
            // check that the center of the HB is within FOV
            var dir = (hitBox.transform.position - transform.position).normalized;
            var angle = Vector3.Angle(dir, transform.forward);
            if(angle > fov)
            {
                continue;
            }

            // check that there is nothing in the way of the hitbox
            var result = Physics.Raycast(
                transform.position,
                dir,
                out var hit,
                float.PositiveInfinity,
                entityLayerMask
            );
            if (result)
            {
                if(hit.collider.gameObject == hitBox.gameObject)
                {
                    VisibleHitBoxes.Add(hitBox);
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
