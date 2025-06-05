using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using UnityEngine.Serialization;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ScanEnemy", story: "[VisionCone] scans for [HitBox]", category: "Action", id: "9811a04f351bc1c18576854c14a25f3c")]
public partial class ScanEnemyAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyVisionCone> VisionCone;
    [SerializeReference] public BlackboardVariable<EntityHitBox> HitBox;

    private Status status = Status.Running;
    
    private readonly Dictionary<BodyPart, int> bodyPartPreferenceMap = new()
    {
        { BodyPart.Head, 2 },
        { BodyPart.Chest, 3 },
        { BodyPart.LeftArm, 1 },
        { BodyPart.RightArm, 1 },
        { BodyPart.LeftLeg, 0 },
        { BodyPart.RightLeg, 0 }
    };
    
    protected override Status OnStart()
    {
        VisionCone.Value.NewHitBoxVisibleEvent.AddListener(OnNewHitBoxVisible);
        VisionCone.Value.HitBoxInvisibleEvent.AddListener(OnHitBoxInvisible);
        status = Status.Running;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return status;
    }

    protected override void OnEnd()
    {
        VisionCone.Value.NewHitBoxVisibleEvent.RemoveListener(OnNewHitBoxVisible);
        VisionCone.Value.HitBoxInvisibleEvent.RemoveListener(OnHitBoxInvisible);
    }

    private void OnNewHitBoxVisible(EntityHitBox hitBox)
    {
        if (HitBox.Value != null)
        {
            var newPreference = bodyPartPreferenceMap[hitBox.BodyPart];
            var curPreference = bodyPartPreferenceMap[HitBox.Value.BodyPart];
            if (newPreference <= curPreference) return;
        }
        HitBox.Value = hitBox;
        status = Status.Success;
    }

    private void OnHitBoxInvisible(EntityHitBox hitBox)
    {
        if (HitBox.Value != hitBox) return;
        HitBox.Value = null;
        EntityHitBox newHitBox = null;
        // find a new hitbox
        foreach (var hb in VisionCone.Value.VisibleHitBoxes)
        {
            var oldPreference = newHitBox ? bodyPartPreferenceMap[newHitBox.BodyPart] : int.MinValue;
            var newPreference = bodyPartPreferenceMap[hb.BodyPart];
            if (newPreference > oldPreference)
            {
                newHitBox = hb;
            }
        }

        HitBox.Value = newHitBox;
        status = HitBox.Value == null ? Status.Failure : Status.Success;
    }
}
