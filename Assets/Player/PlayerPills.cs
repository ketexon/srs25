using System.Collections.Generic;
using Kutie;
using Kutie.Extensions;
using UnityEngine;

public class PlayerPills : EntityItem
{
    [SerializeField] List<GameObject> pillPrefabs;
    [SerializeField] Transform target;

    int selectedIndex = 0;

    public override bool CanEquip => pillPrefabs.Count > 0;

    List<Pills> spawnedPills = new();

    Pills SelectedPills => selectedIndex >= 0 && selectedIndex < spawnedPills.Count
        ? spawnedPills[selectedIndex]
        : null;

    void Awake(){
        foreach(var (i, pillPrefab) in pillPrefabs.ZipIndex()) {
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

    void OnEnable() {
        if(pillPrefabs.Count > 0) {
            selectedIndex = CycleDir > 0
                ? 0
                : pillPrefabs.Count - 1;
            SelectedPills.gameObject.SetActive(true);
        }
    }

    void Update(){
        if(SelectedPills is Pills pills) {
            pills.transform.SetPositionAndRotation(
                target.position,
                target.rotation
            );
        }
    }

    void OnDisable(){
        if(SelectedPills is Pills pills) {
            pills.gameObject.SetActive(false);
        }
    }

    public override bool Cycle(int dir, bool wrap = false)
    {
        if(SelectedPills is Pills oldSelected){
            oldSelected.gameObject.SetActive(false);
        }
        selectedIndex += dir;
        if(wrap){
            selectedIndex = KMath.Rem(selectedIndex, pillPrefabs.Count);
        }

        if(SelectedPills is Pills newSelected){
            newSelected.gameObject.SetActive(true);
            return true;
        }
        else {
            return false;
        }
    }

    public override void Use(bool start = true)
    {
        if(SelectedPills is Pills pills) {
            pills.Use();
            Destroy(pills.gameObject);

            if(spawnedPills.Count > 1){
                Cycle(1, wrap: true);
            }
            spawnedPills.RemoveAt(selectedIndex);
            if(spawnedPills.Count == 0){
                entity.ItemController.Cycle(1);
            }
        }
    }
}
