using UnityEngine;

public class Interactable : MonoBehaviour
{
    public virtual string InteractText => null;
    
    /// <summary>
    /// Interact with this object.
    /// </summary>
    /// <param name="entity">
    /// the entity that is interacting with this object
    /// </param>
    /// <param name="source">
    /// the interactable that initiated the interaction
    /// is null if the interaction was initiated by the entity
    /// </param>
    public virtual void Interact(Entity entity, Interactable source = null)
    { }
    
    public virtual bool CanInteract(Entity entity)
    {
        return true;
    }
    
    public virtual void OnHoverEnter()
    { }
    
    public virtual void OnHoverExit()
    { }
}