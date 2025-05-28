using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ShootTarget", story: "[Agent] shoots [TargetHitBox]", category: "Action", id: "7d8a4098b205d8dc682e330e68fff761")]
public partial class ShootTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> TargetHitBox;

    [SerializeReference] private float maxRecoil = 1f;
    [SerializeReference] private float minRecoil = 0.2f;
    [SerializeReference] private float recoilCooldownDuration = 0.25f;
    
    private float recoilCooldownEndTime = float.NegativeInfinity;

    private Entity entity;

    protected override Status OnStart()
    {
        entity = Agent.Value.GetComponent<Entity>();
        entity.Gun.ShootStraight = true;
        return Status.Running;
    }

    protected override void OnEnd()
    {
        entity.Gun.Shooting = false;
    }
    
    protected override Status OnUpdate(){
        if (TargetHitBox.Value == null)
        {
            return Status.Failure;
        }
        
        if(Time.time < recoilCooldownEndTime)
        {
            return Status.Running;
        }
        
        if(!entity.Gun.Shooting){
            entity.Gun.Shooting = true;
        }
        if(entity.Gun.CurrentRecoil < maxRecoil)
        {
            return Status.Running;
        }
        
        entity.Gun.Shooting = false;
        recoilCooldownEndTime = Time.time + recoilCooldownDuration;
        return Status.Running;
    }
}

