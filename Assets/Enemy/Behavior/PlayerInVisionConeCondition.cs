using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "PlayerInVisionCone", story: "Player in [VisionCone]", category: "Variable Conditions", id: "889de639b579e1c69809d1002b271976")]
public partial class PlayerInVisionConeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyVisionCone> VisionCone;

    public override bool IsTrue()
    {
        return VisionCone.Value.VisibleHitBoxes.Count > 0;
    }
}
