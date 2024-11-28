using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] RoomPoolSO roomPool;
    [SerializeField] LayerMask boundsLayerMask;
    [SerializeField] NavMeshSurface surface;
    [SerializeField] List<RoomTypeConnection> validConnections;

    List<Room> instantiatedRooms = new();
    List<Room> availableRooms = new();

    void Start(){
        Generate();
        surface.AddData();
        surface.UpdateNavMesh(surface.navMeshData);
        surface.BuildNavMesh();
    }

    Room AddRoom(Room room, Vector3 pos, Quaternion rot){
        var go = Instantiate(room.gameObject, pos, rot);
        go.name = $"Room {instantiatedRooms.Count} ({room.gameObject.name})";
        var instantiatedRoom = go.GetComponent<Room>();
        instantiatedRooms.Add(instantiatedRoom);
        return instantiatedRoom;
    }

    void Generate(){
        int iterationCount = 0;
        while(instantiatedRooms.Count < 25 && iterationCount++ < 50) {
            Room prefabRoom = GetRandomRoom();
            if(instantiatedRooms.Count == 0){
                var room = AddRoom(prefabRoom, Vector3.zero, Quaternion.identity);
                availableRooms.Add(room);
            }
            else {
                Room targetRoom = null;
                Transform targetEntrance = null;
                int sourceEntranceIndex = 0;
                Vector3 deltaPos = Vector3.zero;
                Quaternion rot = Quaternion.identity;
                bool overlaps = false;
                // find an instantiated room
                // that can connect to the new room
                foreach(var availableRoom in availableRooms){
                    if(!validConnections.Contains(new RoomTypeConnection(){
                        A = prefabRoom.Type,
                        B = availableRoom.Type,
                    })){
                        continue;
                    }
                    foreach(var availableEntrance in availableRoom.UnconnectedEntrances){
                        for(sourceEntranceIndex = 0; sourceEntranceIndex < prefabRoom.Entrances.Count; ++sourceEntranceIndex){
                            var entrance = prefabRoom.Entrances[sourceEntranceIndex];
                            rot = (
                                Quaternion.AngleAxis(180, Vector3.up)
                                * availableEntrance.rotation
                                * Quaternion.Inverse(entrance.rotation)
                            );
                            deltaPos = availableEntrance.position - rot * entrance.localPosition - transform.position;
                            var check = Physics.CheckBox(
                                prefabRoom.Bounds.transform.position + deltaPos
                                + prefabRoom.Bounds.center,
                                prefabRoom.Bounds.size / 2,
                                rot * prefabRoom.Bounds.transform.rotation,
                                boundsLayerMask,
                                QueryTriggerInteraction.Collide
                            );
                            if(check){
                                continue;
                            }
                            targetRoom = availableRoom;
                            targetEntrance = availableEntrance;
                            break;
                        }
                        if(targetRoom != null){
                            break;
                        }
                    }
                    if(targetRoom != null){
                        break;
                    }
                }
                if(targetRoom != null){
                    var sourceRoom = AddRoom(prefabRoom, deltaPos, rot);
                    var sourceEntrance = sourceRoom.Entrances[sourceEntranceIndex];
                    sourceRoom.Connect(targetRoom, sourceEntrance);

                    targetRoom.Connect(sourceRoom, targetEntrance);
                    sourceRoom.Connect(targetRoom, sourceEntrance);

                    if(targetRoom.UnconnectedEntrances.Count == 0){
                        availableRooms.Remove(targetRoom);
                    }
                    if(sourceRoom.UnconnectedEntrances.Count > 0){
                        availableRooms.Add(sourceRoom);
                    }
                }
                if(overlaps) return;
            }
        }
    }

    Room GetRandomRoom() {
        return roomPool.Rooms[Random.Range(0, roomPool.Rooms.Count)].GetComponent<Room>();
    }
}
