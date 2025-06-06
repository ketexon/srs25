using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Serialization;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ShootTarget", story: "[Agent] shoots [TargetHitBox]",
    category: "Action", id: "7d8a4098b205d8dc682e330e68fff761")]
public partial class ShootTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> TargetHitBox;

    [SerializeReference] public BlackboardVariable<float> MaxAngle = new(5.0f);

    [SerializeReference] public BlackboardVariable<float> MaxRecoil = new(1f);

    [SerializeReference]
    public BlackboardVariable<float> RecoilCooldownDuration = new(0.25f);

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
        if (entity && entity.Gun)
        {
            entity.Gun.Shooting = false;
        }
    }

    protected override Status OnUpdate()
    {
        if (TargetHitBox.Value == null)
        {
            return Status.Failure;
        }

        if (Time.time < recoilCooldownEndTime)
        {
            return Status.Running;
        }

        var angle = Vector3.Angle(
            entity.Gun.transform.forward,
            TargetHitBox.Value.transform.position -
            entity.Gun.transform.position
        );

        if (angle > MaxAngle.Value)
        {
            entity.Gun.Shooting = false;
            return Status.Running;
        }

        if (entity.Gun.CurrentRecoil > MaxRecoil.Value)
        {
            entity.Gun.Shooting = false;
            recoilCooldownEndTime = Time.time + RecoilCooldownDuration.Value;
            return Status.Running;
        }

        entity.Gun.Shooting = true;
        return Status.Running;
    }
}