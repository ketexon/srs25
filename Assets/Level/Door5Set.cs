using System;
using UnityEngine;

public class Door5Set : MonoBehaviour
{
    [Tooltip("For two doors in the same spot, the one with the higher priority will be used.")]
    [SerializeField] public float Priority = 0;
    [SerializeField] private Door5 unblockedDoor;
    [SerializeField] private GameObject blockedDoor;

    [SerializeField]
    private bool blocked = false;
    public bool Blocked
    {
        get => blocked;
        set
        {
            blocked = value;
            blockedDoor.SetActive(blocked);
            unblockedDoor.gameObject.SetActive(!blocked);
        }
    }

    private void Start()
    {
        Blocked = blocked;
    }
}
