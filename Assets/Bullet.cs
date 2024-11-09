using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] LayerMask entityLayerMask;

    void Start()
    {
        var hit = Physics.Raycast(
            transform.position,
            transform.forward,
            out var hitInfo,
            float.PositiveInfinity,
            entityLayerMask,
            QueryTriggerInteraction.Collide
        );

        if (hit)
        {
            Debug.Log("HI");
        }
    }

    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 5);
    }
}
