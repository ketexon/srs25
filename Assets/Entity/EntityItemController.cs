using System.Collections.Generic;
using Kutie;
using UnityEngine;

public class EntityItemController : MonoBehaviour
{
	[SerializeField] public Entity Entity;
	[SerializeField] public List<EntityItem> Items = new();

	[SerializeField] public int ActiveItemIndex = 0;
	public EntityItem ActiveItem => Items[ActiveItemIndex];

	virtual protected void Start()
	{
		for(int i = 0; i < Items.Count; i++) {
			Items[i].gameObject.SetActive(i == ActiveItemIndex);
		}
		SetItem(ActiveItemIndex, 1);
	}

	public void Cycle(int dir, bool wrap = false, bool subItem = true) {
        // try to cycle the item itself (if it has sub-items)
        if(subItem && Items[ActiveItemIndex].Cycle(dir, wrap)) {
            return;
        }
        // otherwise, cycle the item controller
        else {
            int newIndex = KMath.Rem(ActiveItemIndex + dir, Items.Count);
            while(newIndex != ActiveItemIndex) {
                if(Items[newIndex].CanEquip) {
                    SetItem(newIndex, dir);
                    break;
                };
                newIndex = KMath.Rem(newIndex + dir, Items.Count);
            }
        }
    }

	public void SetItem(int index, int cycleDir) {
        if(ActiveItemIndex != index) {
            ActiveItem.gameObject.SetActive(false);
            ActiveItemIndex = index;
            ActiveItem.CycleDir = cycleDir;
            ActiveItem.gameObject.SetActive(true);
        }

        Entity.HumanModel.HumanIK.SetHandIKTargets(
            ActiveItem.HandIKTargets
        );
    }
}