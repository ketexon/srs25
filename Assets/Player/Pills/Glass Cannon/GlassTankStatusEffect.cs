using UnityEngine;

public class GlassTankStatusEffect : StatusEffectBase
{
    protected override float Duration => 60f;

    internal float baseSpeed, baseShakiness, basePainTolerance;

    EntityStats entityStats;

    override protected void OnUse()
    {
        if (Entity == null) return;

        entityStats = Entity.GetComponent<EntityStats>();
        if (entityStats == null) return;

        baseSpeed = entityStats.GetStat(EntityStats.StatType.Speed);
        baseShakiness = entityStats.GetStat(EntityStats.StatType.Shakiness);
        basePainTolerance = entityStats.GetStat(EntityStats.StatType.PainTolerance);
    }

    override protected void PerFrameEffect()
    {
        float multiplier = 2f - (timeSinceStart / Duration);

        if (Entity == null) return;
        if (entityStats == null) return;

        entityStats.SetStat(EntityStats.StatType.Speed, baseSpeed * multiplier);
        entityStats.SetStat(EntityStats.StatType.Shakiness, baseShakiness * multiplier);
        entityStats.SetStat(EntityStats.StatType.PainTolerance, basePainTolerance / multiplier);
    }

    override protected void OnEndEffect()
    {
        if (Entity == null) return;
        if (entityStats == null) return;

        entityStats.SetStat(EntityStats.StatType.Speed, baseSpeed);
        entityStats.SetStat(EntityStats.StatType.Shakiness, baseShakiness);
        entityStats.SetStat(EntityStats.StatType.PainTolerance, basePainTolerance);
    }
}
