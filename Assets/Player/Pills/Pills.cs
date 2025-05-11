using System;
using System.Collections.Generic;
using UnityEngine;

public class Pills : MonoBehaviour
{
    [NonSerialized] public Entity Entity;
    List<StatusEffectBase> statusEffects = new();


    [SerializeField] private TMPro.TMP_Text pillTMP;
    [SerializeField] private string pillText = "Pill";

    void Reset()
    {
        statusEffects = new(GetComponentsInChildren<StatusEffectBase>());
        statusEffects.ForEach(statusEffect =>
        {
            statusEffect.gameObject.SetActive(false);
        });
    }

    private void Awake()
    {
        pillTMP.text = pillText;
        Reset();
    }

    public void Use()
    {

        statusEffects.ForEach(statusEffect =>
        {
            statusEffect.transform.SetParent(Entity.transform);
            statusEffect.Entity = Entity;
            statusEffect.gameObject.SetActive(true);
        });
    }
}
