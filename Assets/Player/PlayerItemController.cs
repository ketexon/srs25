using System.Collections.Generic;
using Kutie;
using Kutie.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItemController : MonoBehaviour
{
    [SerializeField] List<GameObject> items;
    [SerializeField] Entity entity;
    List<HandIKTarget> handIKTargets = new();

    int activeItemIndex = 0;

    void Reset() {
        entity = GetComponent<Entity>();
    }

    void Start()
    {
        for(int i = 0; i < items.Count; i++) {
            items[i].SetActive(i == activeItemIndex);
            handIKTargets.Add(items[i].GetComponent<HandIKTarget>());
        }
        SetItem(activeItemIndex);
    }

    void OnCycle(InputValue inputValue) {
        float dirFloat = inputValue.Get<float>();
        if(dirFloat == 0) return;
        int dir = dirFloat > 0 ? 1 : -1;
        SetItem(KMath.Rem(activeItemIndex + dir, items.Count));
    }

    public void SetItem(int index) {
        if(activeItemIndex != index) {
            items[activeItemIndex].SetActive(false);
            activeItemIndex = index;
            items[activeItemIndex].SetActive(true);
        }

        entity.HumanModel.HumanIK.SetHandIKTargets(
            handIKTargets[activeItemIndex]
        );
    }
}
