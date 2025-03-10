using System.Collections.Generic;
using Kutie.Extensions;
using UnityEngine;

public class Room5 : MonoBehaviour
{
    [SerializeField] public Vector2Int Size = Vector2Int.one;
    [SerializeField] public List<LevelGenerator5.RoomDoor> Doors = new();

    [SerializeField, Kutie.Inspector.ReadOnly]
    public Vector2Int Position;

    [SerializeField, Kutie.Inspector.ReadOnly]
    public LevelGenerator5 LevelGenerator;

    [SerializeField, Kutie.Inspector.ReadOnly]
    public LevelGrid5 Grid;

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
