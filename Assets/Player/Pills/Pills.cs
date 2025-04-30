using System;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Events;

public class Pills : MonoBehaviour
{
    [System.NonSerialized] public Entity Entity;
    [SerializeField] List<PillBehavior> pillBehaviors = new();
    [SerializeField] private TMPro.TMP_Text pillTMP;
    [SerializeField] private string pillText = "Pill";

    //Trigger event when no more behaviors are left, player pills subs and removes that pill when done
    public readonly UnityEvent<Pills> onPillEffectOver = new();


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
        used = true;
        List<PillBehavior> newBehaviors = new(pillBehaviors);
        pillBehaviors.ForEach(behavior => {
            behavior.Entity = Entity;
            behavior.OnUse();
            if (behavior.EffectDuration == 0f){
                newBehaviors.Remove(behavior);
            }
        });
        pillBehaviors = newBehaviors;
    }

    void Update()
    {
        if (!used) return;

        timeSinceUse += Time.deltaTime;


        List<PillBehavior> newBehaviors = new(pillBehaviors);
        pillBehaviors.ForEach(pill =>
        {
            if(timeSinceUse >= pill.EffectDuration)
            {
                pill.OnEndEffect();
                newBehaviors.Remove(pill);
            } else {
                pill.OverTimeEffect();
            }
        });

        pillBehaviors = newBehaviors;


        if(pillBehaviors.Count == 0)
        {
            onPillEffectOver.Invoke(this);
            Destroy(gameObject);
        }
    }

    public void ShowPill(){
        if (pillTMP != null)
        {
            pillTMP.gameObject.SetActive(true);
        }
        if (GetComponent<MeshRenderer>() != null)
        {
            GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public void HidePill(){
        if(pillTMP != null)
        {
            pillTMP.gameObject.SetActive(false);
        }
        if (GetComponent<MeshRenderer>() != null)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
