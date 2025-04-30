using System.Collections.Generic;
using Kutie;
using Kutie.Extensions;
using UnityEngine;

public class PlayerPills : EntityItem
{
    [SerializeField] private List<GameObject> pillPrefabs;
    [SerializeField] private Transform target;

    int selectedIndex = 0;

    public override bool CanEquip => availablePills.Count > 0;

    List<Pills> availablePills = new();
    List<Pills> activePills = new();

    Pills SelectedPills => selectedIndex >= 0 && selectedIndex < availablePills.Count
        ? availablePills[selectedIndex]
        : null;

    private void Awake(){
        foreach(var pillPrefab in pillPrefabs) {
            var pillGO = Instantiate(
                pillPrefab,
                transform
            );
            var pills = pillGO.GetComponent<Pills>();
            availablePills.Add(pills);
            pills.Entity = entity;
            pills.HidePill();
        }
    }

    private void OnEnable()
    {
        if (availablePills.Count <= 0) return;
        selectedIndex = CycleDir > 0
            ? 0
            : availablePills.Count - 1;
        if (!SelectedPills) return;
        SelectedPills.ShowPill();
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
            SelectedPills.HidePill();
        }
    }

    public override bool Cycle(int dir, bool wrap = false)
    {
        if(SelectedPills){
            SelectedPills.HidePill();
        }
        selectedIndex += dir;
        return ActivatePillsAtCurIndex();
    }

    bool ActivatePillsAtCurIndex(bool wrap = false)
    {
        if(availablePills.Count == 0) return false;
        if(wrap){
            selectedIndex = KMath.Rem(selectedIndex, availablePills.Count);
        }

        if (!SelectedPills) return false;
        SelectedPills.ShowPill();
        return true;
    }

    public override void Use(bool start = true)
    {
        if(!start) return;

        if(SelectedPills is Pills pills) {
            pills.Use();
            activePills.Add(pills);
            pills.onPillEffectOver.AddListener(HandlePillEffectOver);
            availablePills.Remove(pills);
            
            if(!ActivatePillsAtCurIndex()){
                entity.ItemController.Cycle(1,subItem: true, wrap: true);
            }
        }
    }

    void HandlePillEffectOver(Pills pills)
    {
        activePills.Remove(pills);
        pills.onPillEffectOver.RemoveListener(HandlePillEffectOver);
    }
}
