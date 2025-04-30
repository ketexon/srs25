using System;
using UnityEngine;

public class StatChangePillBehavior : PillBehavior
{
    [SerializeField] EntityStats.StatType statType;
    [SerializeField] private float statDelta = 0;
    [SerializeField] private float statMult = 1;

    public override void Use()
    {
        var entityStats = Entity.GetComponent<EntityStats>();
        entityStats[statType] = entityStats[statType] * statMult + statDelta;
    }
}
