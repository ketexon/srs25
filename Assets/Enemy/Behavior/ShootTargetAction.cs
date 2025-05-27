using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ShootTarget", story: "[Agent] shoots [Target]", category: "Action", id: "7d8a4098b205d8dc682e330e68fff761")]
public partial class ShootTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    [SerializeReference] float maxRecoil = -0.2f;
    [SerializeReference] float minRecoil = -1f;

    bool recoilCooldown = false;

    private Entity entity;
    private EnemyVisionCone vision;

    private Vector3 m_LastTargetPosition;
    private Vector3 m_ColliderAdjustedTargetPosition;

    protected override Status OnStart()
    {
        entity = Agent.Value.GetComponent<Entity>();
        vision = Agent.Value.GetComponentInChildren<EnemyVisionCone>();
        entity.Gun.ShootStraight = true;

        m_LastTargetPosition = Target.Value.transform.position;
        m_ColliderAdjustedTargetPosition = GetPositionColliderAdjusted();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // Check if the target position has changed.
        bool boolUpdateTargetPosition = !Mathf.Approximately(m_LastTargetPosition.x, Target.Value.transform.position.x) || !Mathf.Approximately(m_LastTargetPosition.y, Target.Value.transform.position.y) || !Mathf.Approximately(m_LastTargetPosition.z, Target.Value.transform.position.z);
        if (boolUpdateTargetPosition)
        {
            m_LastTargetPosition = Target.Value.transform.position;
            m_ColliderAdjustedTargetPosition = GetPositionColliderAdjusted();
        }

        Vector3 agentPosition = Agent.Value.transform.position;
        Vector3 toDestination = m_ColliderAdjustedTargetPosition - agentPosition;
        toDestination.y = 0.0f;
        toDestination.Normalize();

        // Look at the target.
        Agent.Value.transform.forward = toDestination;

        entity.Movement.LookAt(Target.Value.transform.position);
        entity.Gun.vTargetRotation = entity.Movement.Pitch;
        vision.transform.localRotation = Quaternion.AngleAxis(entity.Movement.Pitch, Vector3.right);

        Debug.Log("recoilCooldown: " + recoilCooldown);
        Debug.Log("recoil: " + entity.Gun.CurrentRecoil);

        if (recoilCooldown)
        {
            if (entity.Gun.CurrentRecoil > maxRecoil)
            {
                Debug.Log("Shoot 1");
                entity.Gun.Shooting = true;
                recoilCooldown = false;

                Debug.Log("Shoot Success 1");
            }
        }
        else
        {
            if (!entity.Gun.Shooting)
            {
                Debug.Log("Shoot 2");
                entity.Gun.Shooting = true;

                Debug.Log("Shoot Success 2");
            }
            if (entity.Gun.CurrentRecoil < minRecoil)
            {
                Debug.Log("Shoot 3");
                entity.Gun.Shooting = false;
                recoilCooldown = true;

                Debug.Log("Shoot Success 3");
                return Status.Success;
            }
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }

    private Vector3 GetPositionColliderAdjusted()
    {
        Collider targetCollider = Target.Value.GetComponentInChildren<Collider>();
        if (targetCollider != null)
        {
            return targetCollider.ClosestPoint(Agent.Value.transform.position);
        }
        return Target.Value.transform.position;
    }
}

