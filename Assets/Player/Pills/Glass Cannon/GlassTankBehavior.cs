using UnityEngine;

public class GlassTankBehavior : PillBehavior
{
    EntityStats entityStats;

    public override float EffectDuration => 60f;

    float speedTarget, shakinessTarget, painToleranceTarget;
    float speedChangeRate, shakinessChangeRate, painToleranceChangeRate;

    public override void OnUse()
    {
        entityStats = Entity.GetComponent<EntityStats>();
        if (entityStats == null) return;

        speedTarget = entityStats.GetStat(EntityStats.StatType.Speed) / 2f;
        shakinessTarget = entityStats.GetStat(EntityStats.StatType.Shakiness) / 2f;
        painToleranceTarget = entityStats.GetStat(EntityStats.StatType.PainTolerance) * 2f;

        speedChangeRate = (entityStats.GetStat(EntityStats.StatType.Speed) - speedTarget) / EffectDuration;
        shakinessChangeRate = (entityStats.GetStat(EntityStats.StatType.Shakiness) - shakinessTarget) / EffectDuration;
        painToleranceChangeRate = (entityStats.GetStat(EntityStats.StatType.PainTolerance) - painToleranceTarget) / EffectDuration;

        entityStats.ChangeStat(EntityStats.StatType.Speed, entityStats.GetStat(EntityStats.StatType.Speed) * 2f);
        entityStats.ChangeStat(EntityStats.StatType.Shakiness, entityStats.GetStat(EntityStats.StatType.Shakiness) * 2f);
        entityStats.ChangeStat(EntityStats.StatType.PainTolerance, entityStats.GetStat(EntityStats.StatType.PainTolerance) * 0.5f);
    }

    public override void OverTimeEffect()
    {
        entityStats.ChangeStat(EntityStats.StatType.Speed, -speedChangeRate * Time.deltaTime);
        entityStats.ChangeStat(EntityStats.StatType.Shakiness, -shakinessChangeRate * Time.deltaTime);
        entityStats.ChangeStat(EntityStats.StatType.PainTolerance, -painToleranceChangeRate * Time.deltaTime);
    }

    public override void OnEndEffect()
    {
        entityStats.SetStat(EntityStats.StatType.Speed, speedTarget);
        entityStats.SetStat(EntityStats.StatType.Shakiness, shakinessTarget);
        entityStats.SetStat(EntityStats.StatType.PainTolerance, painToleranceTarget);
    }
}
