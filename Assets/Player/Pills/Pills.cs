using System.Collections.Generic;
using UnityEngine;

public class Pills : MonoBehaviour
{
    [SerializeField] public Entity Entity;
    [SerializeField] List<PillBehavior> pillBehaviors = new();

    void Reset() {
        pillBehaviors = new(GetComponents<PillBehavior>());
    }

    public void Use(){
        pillBehaviors.ForEach(pill => {
            pill.Entity = Entity;
            pill.Use();
        });
    }
}
