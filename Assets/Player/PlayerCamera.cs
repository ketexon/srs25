using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] Entity entity;
    [SerializeField] EntityMovement movement;
    [SerializeField] Transform aliveSocket;
    [SerializeField] Transform deadSocket;
    [SerializeField] CinemachineCamera vcam;

    bool Dead => entity.HumanModel.Health.IsDead;

    void Awake(){
        entity.HumanModel.DeathEvent.AddListener(OnDeath);
        vcam.Target.TrackingTarget = aliveSocket;
    }

    void Update()
    {
        if(Dead){
            transform.rotation = deadSocket.rotation;
        }
        else {
            var pitchQuat = Quaternion.AngleAxis(
                movement.Pitch,
                Vector3.right
            );
            var yawQuat = movement.TargetTransform.rotation;
            transform.rotation = yawQuat * pitchQuat;
        }
    }

    void OnDeath(){
        vcam.Target.TrackingTarget = deadSocket;
    }
}