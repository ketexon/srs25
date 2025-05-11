using UnityEngine;

public class DebugStatusEffect : StatusEffectBase
{
    override protected float Duration => 5f; // Duration of the effect in seconds

    protected override void PerFrameEffect()
    {
        Debug.Log("Time since start: " + timeSinceStart);
    }
}
