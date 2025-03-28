using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// This class is a pseudo interactable that forwards
/// the interaction to another interactable.
/// </summary>
public class PseudoInteractable : Interactable
{
    [SerializeField] private Interactable interactable;

    public Interactable Interactable => interactable;
    
    public override void Interact(Entity entity, Interactable source = null)
    {
        interactable.Interact(entity, this);
    }

    public override bool CanInteract(Entity entity) => interactable.CanInteract(entity);
    
    public override void OnHoverEnter() => interactable.OnHoverEnter();

    public override void OnHoverExit() => interactable.OnHoverExit();
}
