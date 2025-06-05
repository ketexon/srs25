using System.Collections.Generic;
using UnityEngine;

public class Elevators : MonoBehaviour
{
    Dictionary<(int, Vector2Int), Elevator> elevators = new();
    
    public void Register(Elevator elevator)
    {
        elevators.Add(
            (elevator.Room.LevelGenerator.Floor, elevator.Room.Position),
            elevator
        );
        
        // Debug.Log($"Registered elevator at {elevator.Room.Position} on floor {elevator.Room.LevelGenerator.Floor}");
    }

    public void Unregister(Elevator elevator)
    {
        elevators.Remove(
            (elevator.Room.LevelGenerator.Floor, elevator.Room.Position)
        );
        
        // Debug.Log($"Unregistered elevator at {elevator.Room.Position} on floor {elevator.Room.LevelGenerator.Floor}");
    }

    public Elevator Get(int floor, Vector2Int coord)
    {
        return elevators[(floor, coord)];
    }
}