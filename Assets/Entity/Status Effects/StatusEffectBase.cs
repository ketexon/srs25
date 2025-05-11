using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffectBase : MonoBehaviour
{
    public Entity Entity { get; set; }

    protected float timeSinceStart;

    /// <summary>
    /// The duration of the status effect. If set to -1, the effect will last indefinitely.
    /// </summary>
    protected abstract float Duration { get; }


    protected virtual void OnUse() { }
    protected virtual void PerFrameEffect() { }
    protected virtual void OnEndEffect() { }

    void Start()
    {
        timeSinceStart = 0f;
        OnUse();
    }

    void Update()
    {
        if (Entity == null) return;

        if (Duration != -1 && timeSinceStart >= Duration)
        {
            OnEndEffect();
            Destroy(gameObject);
            return;
        }

        PerFrameEffect();

        timeSinceStart += Time.deltaTime;
    }
}
