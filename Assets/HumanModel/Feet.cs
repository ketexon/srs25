using UnityEngine;

public class Feet : MonoBehaviour
{

    [SerializeField] Foot left;
    [SerializeField] Foot right;
    [SerializeField] Entity entity;

    Vector3 delta;

    void Start()
    {
        transform.SetParent(null);
        delta = transform.position - entity.transform.position;
    }

    void Update()
    {
        transform.position = entity.transform.position + delta;
    }
}
