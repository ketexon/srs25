using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DestroyOnDeath", story: "Destroy [Entity] on death", category: "Action", id: "destroy_on_death_action")]
public partial class DestroyOnDeathAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Entity;

    private Entity entity;
    private bool destroyed = false;
    
    protected override Status OnStart()
    {
        entity = Entity.Value.GetComponent<Entity>();
        
        entity.HumanModel.DeathEvent.AddListener(OnDeath);
        
        return Status.Running;
    }

    void OnDeath()
    {
        Object.Destroy(GameObject.GetComponent<BehaviorGraphAgent>());
    }

    protected override void OnEnd()
    {
        if (entity && entity.HumanModel)
        {
            entity.HumanModel.DeathEvent.RemoveListener(OnDeath);
        }
        base.OnEnd();
    }

    protected override Status OnUpdate() => destroyed ? Status.Success : Status.Running;
}

