using UnityEngine;

public class Tracer : MonoBehaviour
{
    [SerializeField] float length = 3;
    [SerializeField] float speed = 100;
    [SerializeField] GameObject bulletHitEffectPrefab;

    /// <summary>
    /// If true, the tracer will spawn a BulletHitEffect when done
    /// </summary>
    [System.NonSerialized] public bool Hit = false;
    [System.NonSerialized] public Vector3 EndPoint;
    [System.NonSerialized] public Vector3 EndNormal;

    float distance;
    bool finished = false;

    void Start()
    {
        distance = Vector3.Distance(EndPoint, transform.position);
        distance -= length;
    }

    void Update()
    {
        UpdateDistance(Time.deltaTime);
    }

    void UpdateDistance(float dt)
    {
        var delta = dt * speed;
        if (delta > distance)
        {
            delta = distance;
            finished = true;
        }
        else
        {
            distance -= delta;
        }

        transform.position += transform.forward * delta;

        if (finished)
        {
            if (Hit)
            {
                Instantiate(bulletHitEffectPrefab, EndPoint, Quaternion.LookRotation(EndNormal));
            }
            Destroy(gameObject);
        }
    }
}
