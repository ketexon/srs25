using System;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : Interactable
{
    private static readonly int OpenId = Animator.StringToHash("open");
    [SerializeField] public Transform Spawn;
    [SerializeField] private int targetFloor;
    [SerializeField] public Animator Animator;

    [System.NonSerialized] public Room5 Room;
    
    public override string InteractText => "Press elevator button";
    public override bool CanInteract(Entity e) => true;

    private Entity entity;
    private Elevator targetElevator;
    private Vector3 targetPos;
    
    private void Start()
    {
        Room.LevelGenerator.Elevators.Register(this);
    }

    public override void Interact(Entity entity, Interactable source = null)
    {
        base.Interact(entity, source);
        this.entity = entity;
        targetElevator =
            Room.LevelGenerator.Elevators.Get(targetFloor, Room.Position);
        
        targetElevator.Animator.SetBool(OpenId, false);
        Animator.SetBool(OpenId, false);
        
        var spawnOffset = entity.Movement.transform.position - Spawn.position;
        spawnOffset.y = 0;
        targetPos = targetElevator.Spawn.position + spawnOffset;
    }

    public void OnDoorsClosed()
    {
        Debug.Log($"Teleporting to {targetElevator.Room.Position} on floor {targetFloor} ({targetPos})");
        entity.Movement.Teleport(targetPos);
        
        // open doors
        targetElevator.Animator.SetBool(OpenId, true);
        Animator.SetBool(OpenId, true);
    }
}
