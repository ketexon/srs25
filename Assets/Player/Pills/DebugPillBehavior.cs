using UnityEngine;

public class DebugPillBehavior : PillBehavior
{
    public override void OnUse()
    {
        base.OnUse();
        Debug.Log("DebugPilllBehavior");
    }
}
