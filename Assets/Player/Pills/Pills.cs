using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Pills : MonoBehaviour
{
    [System.NonSerialized] public Entity Entity;
    [SerializeField] List<PillBehavior> pillBehaviors = new();
    [SerializeField] private TMPro.TMP_Text pillTMP;
    [SerializeField] private string pillText = "Pill";

    //Trigger event when no more behaviors are left, player pills subs and removes that pill when done
    public UnityEvent<Pills> onIsDone = new();


    bool used = false;
    float timeSinceUse = 0f;
    

    void Reset()
    {
        pillBehaviors = new(GetComponents<PillBehavior>());
    }

    private void Awake()
    {
        pillTMP.text = pillText;
    }

    public void Use(){
        pillBehaviors.ForEach(pill => {
            pill.Entity = Entity;
            pill.OnUse();
            if (pill.EffectDuration == 0f){
                pillBehaviors.Remove(pill);
            }
        });
    }

    void Update()
    {
        if (!used) return;

        timeSinceUse += Time.deltaTime;

        pillBehaviors.ForEach(pill =>
        {
            if(timeSinceUse >= pill.EffectDuration)
            {
                pill.OnEndEffect();
                pillBehaviors.Remove(pill);
            } else {
                pill.OverTimeEffect();
            }
        });


    }
}
