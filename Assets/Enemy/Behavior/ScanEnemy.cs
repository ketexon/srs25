using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ScanEnemy", story: "[Agent] scans for [HitBox] [a]", category: "Action", id: "9811a04f351bc1c18576854c14a25f3c")]
public partial class ScanEnemyAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyVisionCone> Agent;
    [SerializeReference] public BlackboardVariable<EntityHitBox> HitBox;
    [SerializeReference] public BlackboardVariable<List<BodyPart>> BodyPartPreferences;

    bool finished = false;

    protected override Status OnStart()
    {
        finished = false;
        Agent.Value.NewHitBoxVisibleEvent.AddListener(OnNewHitBoxVisible);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return finished ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        Agent.Value.NewHitBoxVisibleEvent.RemoveListener(OnNewHitBoxVisible);
    }

    void OnNewHitBoxVisible(EntityHitBox hitBox)
    {
        HitBox.Value = hitBox;
        finished = true;
    }
}
