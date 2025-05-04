using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerInteractController : MonoBehaviour
{
    [SerializeField] private Entity entity;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private Transform head;

    public UnityEvent<bool> InteractableChangedEvent = new();
    public Interactable HoveredInteractable { get; private set; } = null;

    private void Reset()
    {
        entity = GetComponent<Entity>();
        interactableLayer = LayerMask.GetMask("Default");
    }

    private void Update()
    {
        var hitSuccess = Physics.Raycast(
                head.position,
                head.forward,
                out var hit,
                interactDistance,
                interactableLayer);

        if (
            hitSuccess
            && hit.collider.TryGetComponent(out Interactable interactable)
            && interactable.CanInteract(entity)
        )
        {
            var oldInteractable = HoveredInteractable;
            HoveredInteractable = interactable;
            if (oldInteractable != HoveredInteractable && oldInteractable)
            {
                oldInteractable.OnHoverExit();
            }
            HoveredInteractable.OnHoverEnter();
            if (oldInteractable != HoveredInteractable)
            {
                InteractableChangedEvent.Invoke(true);
            }
        }
        else if (HoveredInteractable)
        {
            HoveredInteractable.OnHoverExit();
            HoveredInteractable = null;
            InteractableChangedEvent.Invoke(false);
        }
    }

    private void OnInteract()
    {
        Debug.Log("Interact");
        if (HoveredInteractable)
        {
            Debug.Log($"Interact with: {HoveredInteractable}");
            HoveredInteractable.Interact(entity);
        }
    }
}
