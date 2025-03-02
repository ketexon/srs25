using UnityEngine;

public class IncreaseDarkness : PillBehavior
{
    public override void Use()
    {
		  GetComponentInParent<EntityStats>().ChangeStat(EntityStats.StatType.Speed, 5);
    }
}

