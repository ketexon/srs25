using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.AI.Navigation;
using Kutie.Extensions;


/// <summary>
/// This level generator is based on 2 steps: generating non-overlapping rooms and then connecting them.
/// </summary>
public class LevelGenerator2 : MonoBehaviour {
	[SerializeField] int roomsToSpawn = 10;
	[SerializeField] Bounds bounds = new(){
		center = Vector3.zero,
		size = new Vector3(40, 40, 40),
	};

	[SerializeField] float layerHeight = 5.0f;
	[SerializeField] int layers = 3;
	[SerializeField] float padding = 5.0f;

	[SerializeField] RoomPoolSO roomPool;
	[SerializeField] LayerMask boundsLayerMask;
	[SerializeField] NavMeshSurface surface;

	List<Room> instantiatedRooms = new();
	Dictionary<Collider, Room> boundsRoomMap = new();

	void Start() {
		GenerateRooms();
		surface.AddData();
		surface.UpdateNavMesh(surface.navMeshData);
		surface.BuildNavMesh();
	}

	Room AddRoom(Room room, Vector3 pos, Quaternion rot) {
		var go = Instantiate(room.gameObject, pos, rot);
		go.name = $"Room {instantiatedRooms.Count} ({room.gameObject.name})";
		var instantiatedRoom = go.GetComponent<Room>();
		instantiatedRooms.Add(instantiatedRoom);
		return instantiatedRoom;
	}

	void GenerateRooms(){
		/// step 1: spawn rooms at random locations
		for(int i = 0; i < roomsToSpawn; i++){
			var roomPrefab = GetRandomRoomPrefab();
			var pos = Kutie.KRandom.Range(bounds.min, bounds.max);
			pos.y = Random.Range(0, layers) * layerHeight;

			AddRoom(roomPrefab, pos);
		}

		/// step 2: move rooms so they no longer overlaps
		Collider[] colliders = new Collider[10];
		for(int i = 0; i < 10; ++i){
			bool overlaps = false;
			foreach(var room in instantiatedRooms){
				int nOverlapping = Physics.OverlapBoxNonAlloc(
					room.Bounds.center,
					room.Bounds.size + Vector3.one * padding,
					colliders,
					room.Bounds.transform.rotation,
					boundsLayerMask,
					QueryTriggerInteraction.Collide
				);
				overlaps = overlaps || nOverlapping > 1;
				/// move every overlapping room so that it is within 1 unit of padding
				/// of the bounds
				for(int j = 0; j < nOverlapping; ++j){
					var otherCollider = colliders[j];
					var otherRoom = boundsRoomMap[otherCollider];
					if(otherRoom == room) continue;
					var moveDir = otherRoom.transform.position - room.transform.position;
					moveDir = moveDir.normalized;;
					otherRoom.transform.position += moveDir;
				}
			}
			Physics.SyncTransforms();
			if(!overlaps) break;
		}

		Debug.Log("DONE GENERATING ROOMS");
	}

	Room AddRoom(GameObject roomPrefab, Vector3 pos){
		var roomGO = Instantiate(roomPrefab, pos, Quaternion.identity);
		var room = roomGO.GetComponent<Room>();
		instantiatedRooms.Add(room);

		boundsRoomMap.Add(room.Bounds, room);
		return room;
	}

	GameObject GetRandomRoomPrefab(){
		return roomPool.Rooms.Sample();
	}
}