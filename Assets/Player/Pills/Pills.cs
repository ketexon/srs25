using System;
using System.Collections.Generic;
using UnityEngine;

public class Pills : MonoBehaviour
{
    [System.NonSerialized] public Entity Entity;
    [SerializeField] List<PillBehavior> pillBehaviors = new();
    [SerializeField] private TMPro.TMP_Text pillTMP;
    [SerializeField] private string pillText = "Pill";

    void Reset() {
        pillBehaviors = new(GetComponents<PillBehavior>());
    }

    private void Awake()
    {
        pillTMP.text = pillText;
    }

    public void Use(){
        pillBehaviors.ForEach(pill => {
            pill.Entity = Entity;
            pill.Use();
        });
    }
}
