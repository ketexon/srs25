using System;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : Interactable
{
    [SerializeField] public Transform Spawn;
    [SerializeField] private int targetFloor;

    [System.NonSerialized] public Room5 Room;
    
    public override string InteractText => "Press elevator button";
    public override bool CanInteract(Entity e) => true;
    
    private void Start()
    {
        Room.LevelGenerator.Elevators.Register(this);
    }

    public override void Interact(Entity entity, Interactable source = null)
    {
        base.Interact(entity, source);
        var spawnOffset = entity.Movement.transform.position - Spawn.position;
        spawnOffset.y = 0;
        var targetElevator =
            Room.LevelGenerator.Elevators.Get(targetFloor, Room.Position);
        var targetPos = targetElevator.Spawn.position + spawnOffset;
        Debug.Log($"Teleporting to {targetElevator.Room.Position} on floor {targetFloor} ({targetPos})");
        entity.Movement.Teleport(targetPos);
    }
}
