using Kutie.Extensions;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Kutie.SpringParameters p;
    Kutie.SpringQuaternion v;

    Kutie.PIDFloatController pid;

    void Start()
    {
        v = new(transform.rotation, p);
    }

    void Update()
    {
        v.TargetValue = target.rotation;
        transform.rotation = v.CurrentValue;

        v.Update(Time.deltaTime);
    }
}
