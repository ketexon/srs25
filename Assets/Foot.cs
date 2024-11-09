using UnityEngine;

[ExecuteInEditMode]
public class Foot : MonoBehaviour
{
    [SerializeField] Transform ikTarget;
    [SerializeField] Transform goal;
    [SerializeField] float interpolateSpeed = 1.0f;

    void Update()
    {
        if(goal.position != ikTarget.position)
        {
            var dir = (goal.position - ikTarget.position).normalized;
            var newPoint = ikTarget.position + interpolateSpeed * Time.deltaTime * dir;
            Debug.Log($"{newPoint} {Vector3.Dot(newPoint - goal.position, dir)}");
            if (Vector3.Dot(newPoint - goal.position, dir) > 0) {
                ikTarget.position = goal.position;
            }
            else
            {
                ikTarget.position = newPoint;
            }
        }
    }
}
