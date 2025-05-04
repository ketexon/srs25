using System;
using System.Collections.Generic;
using Kutie.Extensions;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    private static readonly int Open = Animator.StringToHash("open");
    [SerializeField] private LayerMask entityLayerMask;
    [SerializeField] private Animator animator;
    
    HashSet<Collider> collidersInTrigger = new();
    
    private void OnTriggerEnter(Collider other)
    {
        if (entityLayerMask.Contains(other.gameObject.layer))
        {
            collidersInTrigger.Add(other);
            UpdateAnimator();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        collidersInTrigger.Remove(other);
        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        animator.SetBool(Open, collidersInTrigger.Count > 0);
    }
}
