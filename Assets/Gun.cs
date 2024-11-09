using UnityEngine;
using UnityEngine.UIElements;

public class Gun : MonoBehaviour
{
    [SerializeField] Entity entity;
    [SerializeField] float minAngle = -60;
    [SerializeField] float maxAngle = 80;
    [SerializeField] Transform tip;
    [SerializeField] float shotInterval = 1.0f;
    [SerializeField] GameObject bulletPrefab;

    [System.NonSerialized] public bool Shooting = false;
    float lastShotTime = float.NegativeInfinity;

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

            Instantiate(bulletPrefab, tip.transform.position, tip.transform.rotation);
        }
    }
}
