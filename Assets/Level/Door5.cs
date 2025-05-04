using System;
using UnityEngine;

public class Door5 : Interactable
{
    private static readonly int OpenHash = Animator.StringToHash("Open");
    
    [SerializeField] private Animator animator;
    [SerializeField] private new Collider collider;

    public bool Opened { get; private set; } = false;

    public override bool CanInteract(Entity entity) => !Opened;

    public override void Interact(Entity entity, Interactable source = null)
    {
        base.Interact(entity, source);

        if (Opened) return;
        Opened = true;
        animator.SetTrigger(OpenHash);
        collider.enabled = false;
    }
}