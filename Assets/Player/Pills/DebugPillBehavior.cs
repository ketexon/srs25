using UnityEngine;

public class DebugPillBehavior : PillBehavior
{
    public override void Use()
    {
        base.Use();
        Debug.Log("DebugPilllBehavior");
    }
}
