using UnityEngine;

public class IncreaseDarkness : PillBehavior
{
    public override void Use()
    {
		  GetComponentInParent<PlayerStats>().ChangeStat(PlayerStats.StatType.Speed, 5);
    }
}

