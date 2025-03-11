using System.Collections.Generic;
using Kutie.Extensions;
using UnityEngine;

public class Room5 : MonoBehaviour
{
    [SerializeField] public Vector2Int Size = Vector2Int.one;
    [SerializeField] public List<LevelGenerator5.RoomDoor> Doors = new();
    [Tooltip("The player spawn point in this room. Doesn't matter if the player does not spawn in this room.")]
    [SerializeField] public Vector3 PlayerSpawnPoint;

    [SerializeField, Kutie.Inspector.ReadOnly]
    public Vector2Int Position;

    [SerializeField, Kutie.Inspector.ReadOnly]
    public LevelGenerator5 LevelGenerator;

    [SerializeField, Kutie.Inspector.ReadOnly]
    public LevelGrid5 Grid;

    [SerializeField, Kutie.Inspector.ReadOnly]
    public List<LevelGenerator5.RoomDoor> OpenDoors = new();

    void OnDrawGizmos()
    {
        if(Grid){
            var roomSize = new Vector3(Size.x, 0, Size.y)
                * Grid.CellLength;
            var roomCenter = transform.position
                + transform.rotation
                * (roomSize/2);

            Gizmos.matrix = Matrix4x4.TRS(
                roomCenter,
                transform.rotation,
                Vector3.one
            );
            Gizmos.color = Color.green.WithA(0.2f);
            Gizmos.DrawCube(
                Vector3.zero,
                roomSize
            );
        }
    }
}
