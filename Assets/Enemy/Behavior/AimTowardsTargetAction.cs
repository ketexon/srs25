using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Serialization;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AimTowardsTarget", story: "[Entity] aims towards [TargetHitBox]", category: "Action", id: "aim_towards_target_action")]
public partial class AimTowardsTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Entity;
    
    [SerializeReference] public BlackboardVariable<EntityHitBox> TargetHitBox;

    private Entity entity;
    private EntityAINavigation entityAINavigation;

    protected override Status OnStart()
    {
        entity = Entity.Value.GetComponent<Entity>();
        entityAINavigation = entity.GetComponent<EntityAINavigation>();
        entityAINavigation.LookForward = false;
        return Status.Running;
    }

    protected override void OnEnd()
    {
        base.OnEnd();

        entityAINavigation.LookForward = true;
    }

    protected override Status OnUpdate(){
        if (TargetHitBox.Value == null)
        {
            return Status.Failure;
        }
        
        entity.Movement.LookAt(TargetHitBox.Value.transform.position);
        entity.Gun.vTargetRotation = entity.Movement.Pitch;
        
        return Status.Running;
    }
}

