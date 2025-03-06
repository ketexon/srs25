using System.Collections.Generic;
using UnityEngine;

public class Room5 : MonoBehaviour
{
    [SerializeField] public Vector2Int Size = Vector2Int.one;
    [SerializeField] public List<LevelGenerator5.RoomDoor> Doors = new();
}
