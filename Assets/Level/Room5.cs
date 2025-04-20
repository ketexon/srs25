using System;
using System.Collections.Generic;
using Kutie;
using Kutie.Extensions;
using UnityEngine;

public class Room5 : MonoBehaviour
{
    [System.Serializable]
    public class DoorSetEntry : LevelGenerator5.RoomDoor
    {
        public Door5Set DoorSet;
    }
    
    [SerializeField] public Vector2Int Size = Vector2Int.one;
    [SerializeField] public List<LevelGenerator5.RoomDoor> Doors = new();
    [SerializeField] public List<DoorSetEntry> DoorSets = new();
    [Tooltip("The player spawn point in this room. Doesn't matter if the player does not spawn in this room.")]
    [SerializeField] public Vector3 PlayerSpawnPoint;
    
    [SerializeField] public List<GameObject> DoorPrefabs = new();
    [SerializeField] private List<Elevator> elevators = new();

    [SerializeField, Kutie.Inspector.ReadOnly]
    public Vector2Int Position;

    [SerializeField, Kutie.Inspector.ReadOnly]
    public int Rotation;

    [SerializeField, Kutie.Inspector.ReadOnly]
    public LevelGenerator5 LevelGenerator;

    [SerializeField, Kutie.Inspector.ReadOnly]
    public LevelGrid5 Grid;

    [SerializeField, Kutie.Inspector.ReadOnly]
    public List<LevelGenerator5.RoomDoor> OpenDoors = new();

    // called by LevelGenerator5
    public void SpawnDoors()
    {
        foreach (var doorSetEntry in DoorSets)
        {
            var blocked = true;
            foreach(var openDoor in OpenDoors){
                if(openDoor.Position == doorSetEntry.Position
                   && openDoor.Direction == doorSetEntry.Direction
               ){
                    blocked = false;
                    break;
                }
            }

            if (!blocked)
            {
                // disable this door if it is left or down
                var globalDir = doorSetEntry.Direction.Rotate90(Rotation);
                if (globalDir == DirectionMask.Left || globalDir == DirectionMask.Down)
                {
                    doorSetEntry.DoorSet.gameObject.SetActive(false);
                    continue;
                }
            }
            doorSetEntry.DoorSet.Blocked = blocked;
        }
        // we only spawn doors if we are the most positive room touching
        // that door
        // ie. only doors that are facing (globally) left or down
        // foreach (var door in OpenDoors)
        // {
        //     var globalDir = door.Direction.Rotate90(Rotation);
        //     if (globalDir == DirectionMask.Right || globalDir == DirectionMask.Up)
        //     {
        //         continue;
        //     }
        //
        //     var doorPrefab = DoorPrefabs.Sample();
        //     var doorGO = KPrefabUtility.InstantiatePrefab(
        //         doorPrefab,
        //         parent: transform);
        //
        //     var offset = door.Direction.ToVector2Int();
        //     doorGO.transform.localPosition = new Vector3(
        //         door.Position.x + 0.5f + offset.x/2.0f,
        //         0,
        //         door.Position.y + 0.5f + offset.y/2.0f
        //     ) * Grid.CellLength;
        //
        //     doorGO.transform.localRotation = door.Direction.ToQuaternion(
        //         Vector3.up, DirectionMask.Up);
        // }
    }

    private void Awake()
    {
        foreach (var elevator in elevators)
        {
            elevator.Room = this;
        }
    }

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

            foreach(var openDoor in OpenDoors){
                var doorPosition = openDoor.Position;
                var doorDirection = openDoor.Direction;
                var dirVector = doorDirection.ToVector2Int();
                var doorCenter = transform.position
                    + transform.rotation * (new Vector3(
                        doorPosition.x + 0.5f + dirVector.x/3.0f,
                        0,
                        doorPosition.y + 0.5f + dirVector.y/3.0f
                    ) * Grid.CellLength);

                Gizmos.matrix = Matrix4x4.TRS(
                    doorCenter,
                    transform.rotation,
                    Vector3.one
                );
                Gizmos.color = Color.green.WithA(0.5f);
                Gizmos.DrawCube(
                    Vector3.zero,
                    Vector3.one * Grid.CellLength/8
                );
            }
        }
    }
}
