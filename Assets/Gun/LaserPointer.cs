using UnityEngine;

public class LaserPointer : MonoBehaviour
{
    [SerializeField] Transform source;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float length = 100;

    Vector3[] points = new Vector3[2];

    // late update bc transforms are manually synced for entities
    void LateUpdate(){
        points[0] = source.position;
        if(Physics.Raycast(transform.position, transform.forward, out var hit, float.PositiveInfinity, layerMask, QueryTriggerInteraction.Ignore)){
            points[1] = hit.point;
        }
        else {
            points[1] = source.position + source.forward * length;
        }
        lineRenderer.SetPositions(points);
    }
}
