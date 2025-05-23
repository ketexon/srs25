﻿using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] protected new CinemachineCamera camera;
    [SerializeField] protected Gun gun;
    [SerializeField] protected EntityMovement movement;
    [SerializeField] protected Entity entity;
    
    public bool InputEnabled { get; set; } = true;
    
    virtual protected void Reset()
    {
        gun = FindAnyObjectByType<Gun>();
        movement = GetComponent<EntityMovement>();
        entity = GetComponent<Entity>();

        entity.HumanModel.DeathEvent.AddListener(OnDeath);
    }

    virtual protected void OnEnable()
    {
        camera.Priority.Enabled = true;
    }

    virtual protected void OnDisable()
    {
        camera.Priority.Enabled = false;
        movement.MoveDir = Vector2.zero;
    }

    virtual protected void OnDeath(){
        enabled = false;
    }
}
