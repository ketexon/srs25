using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Kutie.Extensions;
using System.Linq;
using Delauney.Utils.Triangulation;





#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

[System.Serializable]
public class LevelDoor2 {
	public Transform Transform;
	public bool Required;
}

[System.Serializable]
public class LevelGridEntry2 {
	public Vector3 Size;
	public Vector3 BottomLeftCornerOffset;
	public List<LevelDoor2> Doors;
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelGenerator4))]
public class LevelGenerator4Editor : Editor {
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
		var generator = target as LevelGenerator4;

		var defaultInspector = new VisualElement();
		InspectorElement.FillDefaultInspector(defaultInspector, serializedObject, this);

		root.Add(defaultInspector);

		var generateButton = new Button(() => {
			generator.Generate();
			serializedObject.ApplyModifiedProperties();
		});
		generateButton.Add(new Label("Generate"));

		root.Add(generateButton);

		return root;
    }
}
#endif

public class LevelGenerator4 : MonoBehaviour {
	[System.Serializable]
	enum CellType {
		Empty,
		Room,
		Pathway,
		OutOfBounds,
	};

	[System.Serializable]
	class Door {
		public Vector2Int Cell;
		public Vector2Int Face;
		public bool Required = false;
	}

	[SerializeField] Vector2 cellSize = new(2, 2);
	[SerializeField] Vector2Int gridSize = new(20, 20);
	[SerializeField] Kutie.IntRange roomCount = new(10, 20);
	[SerializeField] Kutie.IntRange roomLength = new(4, 8);
	[SerializeField] int roomGap = 1;
	[SerializeField] int roomMargin = 1;
	// these are the areas in which rooms will be spawned
	// these are not necessarily the exact size of rooms,
	// but just upper bounds
	[SerializeField] List<RectInt> allSpacePartitions = new();
	[SerializeField] List<RectInt> spacePartitions = new();
	[SerializeField, HideInInspector] List<List<int>> adjacencies = new();
	[SerializeField, HideInInspector] List<List<int>> spanningAdjacencies = new();
	[SerializeField] List<Room> spawnedRooms = new();
	[SerializeField] int maxIters = 2;

	[Header("Prefabs")]
	[SerializeField] List<GameObject> roomPrefabs = new();
	[SerializeField, HideInInspector] List<Room> roomPrefabRooms = new();
	[SerializeField, HideInInspector] List<List<Door>> roomPrefabDoors = new();
	[SerializeField, HideInInspector] List<Vector2Int> roomSizes = new();
	[SerializeField, HideInInspector] List<CellType> grid = new();
	[SerializeField, HideInInspector] List<Vector2Int> doorCells = new();

	int GetCellIndex(Vector2Int cell) => cell.x + cell.y * gridSize.x;
	bool CellInBounds(Vector2Int cell) => CellInBounds(GetCellIndex(cell));
	bool CellInBounds(int idx) => idx < grid.Count && idx >= 0;

	void Start() {
		PartitionSpace(new RectInt(Vector2Int.zero, gridSize));
	}

	public void Generate(){
		Clean();
		PartitionSpace(new RectInt(Vector2Int.zero, gridSize));
		BakeRoomCoordinates();
		SpawnRooms();
		Triangulate();
	}

	void PartitionSpace(RectInt bounds){
		Queue<RectInt> queue = new();
		// add padding on all sides
		// so that rooms don't touch the walls
		bounds = new RectInt(
			bounds.min + Vector2Int.one * roomMargin,
			bounds.size - Vector2Int.one * 2 * roomMargin
		);
		queue.Enqueue(bounds);

		List<RectInt> candidates = new();

		while(queue.Count > 0 && spacePartitions.Count < roomCount.Max){
			var top = queue.Dequeue();
			if(top.size.x < roomLength.Min || top.size.y < roomLength.Min){
				allSpacePartitions.Add(top);
				continue;
			}
			if(top.size.x <= roomLength.Max && top.size.y <= roomLength.Max){
				candidates.Add(top);
				allSpacePartitions.Add(top);
				continue;
			}
			RectInt a, b;
			bool mustSplitX = top.size.x > roomLength.Max && top.size.y <= roomLength.Max;
			bool mustSplitY = top.size.y > roomLength.Max && top.size.x <= roomLength.Max;
			// x
			if(!mustSplitY && (mustSplitX || Random.value > 0.5f)){
				var splitPoint = Random.Range(roomLength.Min, top.size.x - roomLength.Min + 1 - roomGap);
				a = new RectInt(
					top.min,
					new Vector2Int(splitPoint, top.size.y)
				);
				b = new RectInt(
					top.min + (splitPoint + roomGap) * Vector2Int.right,
					new Vector2Int(top.size.x - splitPoint - roomGap, top.size.y)
				);
			}
			// y
			else {
				var splitPoint = Random.Range(roomLength.Min, top.size.y - roomLength.Min + 1 - roomGap);
				// var splitPoint = MinRoomLength;
				a = new RectInt(
					top.min,
					new Vector2Int(top.size.x, splitPoint)
				);
				b = new RectInt(
					top.min + (splitPoint + roomGap) * Vector2Int.up,
					new Vector2Int(top.size.x, top.size.y - splitPoint - roomGap)
				);
			}

			if(Random.value > 0.5f){
				(a,b) = (b,a);
			}

			queue.Enqueue(a);
			queue.Enqueue(b);
		}

		foreach(var partition in queue){
			allSpacePartitions.Add(partition);
		}

		if(candidates.Count < roomCount.Min){
			PartitionSpace(bounds);
			return;
		}

		spacePartitions = candidates.Sample(
			Random.Range(
				roomCount.Min,
				System.Math.Min(roomCount.Max + 1, candidates.Count + 1)
			)
		);
	}

	// this calculates the cells for each door
	// based on their world position
	void BakeRoomCoordinates(){
		foreach(var roomPrefab in roomPrefabs){
			var room = roomPrefab.GetComponent<Room>();
			roomPrefabRooms.Add(room);

			roomSizes.Add(room.LevelEntry.Size.XZ().Divide(cellSize).RoundToInt());

			// Get cell and face for each door
			List<Door> doors = new();
			foreach(var door in room.LevelEntry.Doors){
				var doorXform = door.Transform;
				var forward = doorXform.forward.XZ();

				typeof(Room).GetHashCode();

				var relativePos =
					(doorXform.position - room.LevelEntry.BottomLeftCornerOffset)
					.XZ()
					.Divide(cellSize)
				// the door transform is at an edge
				// so move inward half a unit
					+ forward / 2
				// move to bottom left corner
					- Vector2.one / 2;
				var doorCell = relativePos.RoundToInt();
				if(
					doorCell - relativePos != Vector2.zero
				){
					Debug.LogWarning($"Entrance {doorXform.name} of room {gameObject.name} is not aligned to the grid");
				}
				doors.Add(new Door(){
					Cell = doorCell,
					Face =-forward.RoundToInt(),
					Required = door.Required,
				});
			}
			roomPrefabDoors.Add(doors);
		}
	}

	void SpawnRooms(){
		// find the room of largest area
		// that fits inside each space partition
		List<RectInt> usedSpacedPartitions = new();
		foreach(var partition in spacePartitions){
			Room roomPrefab = null;
			int roomPrefabIndex = 0;
			//TODO: Also consider rotation
			foreach(var (i, candidate) in roomPrefabRooms.ZipIndex()){
				// check if it fits within partition
				var roomSize = roomSizes[i];
				if(roomSize.x > partition.size.x || roomSize.y > partition.size.y){
					continue;
				}
				if(roomPrefab == null || roomSize.sqrMagnitude > roomPrefab.Bounds.size.sqrMagnitude){
					roomPrefab = candidate;
					roomPrefabIndex = i;
				}
			}

			var size = roomSizes[roomPrefabIndex];

			if(roomPrefab != null){
				usedSpacedPartitions.Add(partition);

				var bottomLeftCell = partition.min + (partition.size - size) / 2;
				var roomGO = Instantiate(
					roomPrefab.gameObject,
					bottomLeftCell.Hammard(cellSize).WithZ(0).XZY()
					- roomPrefab.LevelEntry.BottomLeftCornerOffset,
					Quaternion.identity
				);
				var room = roomGO.GetComponent<Room>();
				spawnedRooms.Add(room);

				// mark grid cells as occupied
				for(int x = 0; x < size.x; x++){
					for(int y = 0; y < size.y; y++){
						int idx = GetCellIndex(bottomLeftCell + new Vector2Int(x, y));
						if(CellInBounds(idx)){
							grid[idx] = CellType.Room;
						}
					}
				}

				// add door cells as targets
				var doors = roomPrefabDoors[roomPrefabIndex];
				foreach(var door in doors){
					var doorCell = bottomLeftCell + door.Cell;
					var pathwayCell = doorCell + door.Face;
					int idx = GetCellIndex(pathwayCell);
					if(CellInBounds(idx)){
						grid[idx] = CellType.Pathway;
						doorCells.Add(pathwayCell);
					}
				}
			}
		}

		// remove unused space partitions
		spacePartitions = usedSpacedPartitions;
	}

	void Triangulate(){
		// does delauney triangulation on the rooms
		DelaunayTriangulation triangulation = new();
		var spacePartitionVertices = spacePartitions.Map(part => part.center).CollectList();
		triangulation.Triangulate(spacePartitionVertices);

		var spacePartitionCenterIndexMap = new Dictionary<Vector2, int>();
		for(int i = 0; i < spacePartitions.Count; i++){
			spacePartitionCenterIndexMap[spacePartitions[i].center] = i;
		}

		// initialize adjacencies
		adjacencies = new();
		foreach(var _ in spacePartitions){
			adjacencies.Add(new List<int>());
		}

		// for each triangle, add its
		// edges to the adjacency list
		// skip first triangle since this is the outer triangle
		// subtract 3 from each index since we ignore the first triangle
		for(int i = 0; i < triangulation.TriangleSet.TriangleCount; i++){
			var triangle = triangulation.TriangleSet.GetTrianglePoints(i);

			for(int j = 0; j < 3; j++){
				var a = triangle[j];
				var b = triangle[(j + 1) % 3];
				if(
					spacePartitionCenterIndexMap.TryGetValue(a, out var aIndex)
					&& spacePartitionCenterIndexMap.TryGetValue(b, out var bIndex)
				){
					adjacencies[aIndex].Add(bIndex);
					adjacencies[bIndex].Add(aIndex);
				}
			}
		}
	}

	void FindSpanningTree(){

	}

	void Clean(){
		spacePartitions.Clear();
		allSpacePartitions.Clear();

		roomPrefabRooms.Clear();
		roomPrefabDoors.Clear();

		adjacencies.Clear();

		foreach(var room in spawnedRooms){
			if(room){
#if UNITY_EDITOR
				if(EditorApplication.isPlaying){
					Destroy(room.gameObject);
				}
				else {
					DestroyImmediate(room.gameObject);
				}
#else
				Destroy(room.gameObject);
#endif
			}
		}
		spawnedRooms.Clear();

		grid = new(gridSize.x * gridSize.y);
		for(int i = 0; i < gridSize.x * gridSize.y; i++){
			grid.Add(CellType.Empty);
		}
	}

	void OnDrawGizmos(){
		if(!enabled) return;
		// draw grid
		Gizmos.color = Color.white.WithA(0.01f);
		for(int x = 0; x < gridSize.x; x++){
			for(int y = 0; y < gridSize.y; y++){
				Gizmos.DrawWireCube(
					new Vector3(x + 0.5f, y + 0.5f, 0).Hammard(cellSize).WithZ(0).XZY(),
					cellSize.WithZ(0).XZY()
				);
			}
		}

		// draw space partitions
		foreach(var partition in allSpacePartitions){
			if(spacePartitions.Contains(partition)){
				Gizmos.color = Color.green.WithA(0.1f);
			}
			else {
				Gizmos.color = Color.red.WithA(0.1f);
			}
			var center = partition.center.Hammard(cellSize).WithZ(0).XZY();

			Gizmos.DrawWireCube(
				center,
				partition.size.Hammard(cellSize).WithZ(0).XZY()
			);
			// draw center
			Gizmos.color = Gizmos.color.WithA(0.4f);
			Gizmos.DrawCube(
				center,
				Vector3.one * 0.1f
			);
		}

		// draw grid cells
		for(int x = 0; x < gridSize.x; ++x){
			for(int y = 0; y < gridSize.y; ++y){
				int idx = GetCellIndex(new Vector2Int(x, y));
				var content = grid[idx];
				if(content == CellType.Room){
					Gizmos.color = Color.red.WithA(0.1f);
				}
				else if(content == CellType.Pathway){
					Gizmos.color = Color.green.WithA(0.1f);
				}
				else {
					Gizmos.color = Color.white.WithA(0.1f);
				}
				Gizmos.DrawCube(
					new Vector3(x + 0.5f, y + 0.5f, 0).Hammard(cellSize).WithZ(0).XZY(),
					Vector3.one.Hammard(cellSize).WithZ(0).XZY() * 0.25f
				);
			}
		}

		// draw lines between adjacent rooms
		if(adjacencies != null){
			for(int i = 0; i < adjacencies.Count; i++){
				var a = spacePartitions[i].center.Hammard(cellSize).WithZ(0).XZY();
				foreach(var j in adjacencies[i]){
					var b = spacePartitions[j].center.Hammard(cellSize).WithZ(0).XZY();
					Gizmos.color = Color.blue.WithA(0.1f);
					Gizmos.DrawLine(a, b);
				}
			}
		}
	}
}