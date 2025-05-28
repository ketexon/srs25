using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Serialization;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AimTowardsTarget", story: "[Entity] aims towards [Target]", category: "Action", id: "aim_towards_target_action")]
public partial class AimTowardsTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Entity;
    
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    [SerializeReference] private float rotationEpsilon = 5f;

    private Entity entity;
    private EntityHitBox target;

    protected override Status OnStart()
    {
        entity = Entity.Value.GetComponent<Entity>();
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
    
    protected override Status OnUpdate(){
        if (Target.Value == null)
        {
            Debug.Log("Lost target, stopping shooting.");
            return Status.Failure;
        }
        
        entity.Movement.LookAt(Target.Value.transform.position);
        entity.Gun.vTargetRotation = entity.Movement.Pitch;

        return Mathf.Abs(entity.Gun.vRotation - entity.Gun.vTargetRotation) <
               rotationEpsilon ? Status.Success : Status.Running;
    }
}

