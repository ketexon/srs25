using UnityEngine;

public class ElevatorModel : MonoBehaviour
{
    [SerializeField] private Elevator elevator;
    
    public void DoorsOpened()
    {
        Debug.Log("Elevator Doors Opened");
    }
    
    public void DoorsClosed()
    {
        elevator.OnDoorsClosed();
    }
}
