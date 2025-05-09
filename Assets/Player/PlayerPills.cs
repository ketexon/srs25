using System.Collections.Generic;
using Kutie;
using Kutie.Extensions;
using UnityEngine;

public class PlayerPills : EntityItem
{
    [SerializeField] private List<GameObject> pillPrefabs;
    [SerializeField] private Transform target;

    int selectedIndex = 0;

    public override bool CanEquip => spawnedPills.Count > 0;

    List<Pills> spawnedPills = new();

    private Pills SelectedPills => selectedIndex >= 0 && selectedIndex < spawnedPills.Count
        ? spawnedPills[selectedIndex]
        : null;

    private void Awake(){
        foreach(var pillPrefab in pillPrefabs) {
            var pillGO = Instantiate(
                pillPrefab,
                transform
            );
            var pills = pillGO.GetComponent<Pills>();
            spawnedPills.Add(pills);
            pills.Entity = entity;

            pillGO.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (spawnedPills.Count <= 0) return;
        selectedIndex = CycleDir > 0
            ? 0
            : spawnedPills.Count - 1;
        if (!SelectedPills) return;
        SelectedPills.gameObject.SetActive(true);
    }

    private void Update(){
        if(SelectedPills) {
            SelectedPills.transform.SetPositionAndRotation(
                target.position,
                target.rotation
            );
        }
    }

    private void OnDisable(){
        if(SelectedPills) {
            SelectedPills.gameObject.SetActive(false);
        }
    }

    public override bool Cycle(int dir, bool wrap = false)
    {
        if(SelectedPills){
            SelectedPills.gameObject.SetActive(false);
        }
        selectedIndex += dir;
        return ActivatePillsAtCurIndex();
    }

    bool ActivatePillsAtCurIndex(bool wrap = false)
    {
        if(spawnedPills.Count == 0) return false;
        if(wrap){
            selectedIndex = KMath.Rem(selectedIndex, spawnedPills.Count);
        }

        if (!SelectedPills) return false;
        SelectedPills.gameObject.SetActive(true);
        return true;
    }

    public override void Use(bool start = true)
    {
        if (!start) return;
        if (!SelectedPills) return;
        SelectedPills.Use();
        Destroy(SelectedPills.gameObject);
        spawnedPills.RemoveAt(selectedIndex);
        if(selectedIndex > spawnedPills.Count)
        {
            selectedIndex = 0;
        }
        
        if(!ActivatePillsAtCurIndex(true)){
            entity.ItemController.Cycle(1);
        }
    }
}
