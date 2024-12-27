using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Kutie.Extensions;
using Kutie.Collections;
using Delauney.Utils.Triangulation;
using Kutie;
using UnityEngine.Serialization;
using Unity.AI.Navigation;
using Unity.AI.Navigation.Editor;
using Kutie.Editor;





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
		var grid = generator.Grid;

		SerializedObject gridSerializedObject = null;
		if(grid){
			gridSerializedObject = new SerializedObject(grid);
		}

		var defaultInspector = new VisualElement();
		InspectorElement.FillDefaultInspector(defaultInspector, serializedObject, this);

		root.Add(defaultInspector);

		var generateButton = new Button(() => {
			if(!grid) return;
			Undo.RegisterFullObjectHierarchyUndo(
				generator.gameObject,
				"Generate Level"
			);

			generator.Generate();
			serializedObject.ApplyModifiedProperties();
			if(grid){
				gridSerializedObject.ApplyModifiedProperties();
			}
		});
		generateButton.Add(new Label("Generate"));

		root.Add(generateButton);

		return root;
    }
}
#endif

public class LevelGenerator4 : MonoBehaviour {
	class RegenerateException : System.Exception {}

	[System.Serializable]
	class Door {
		public Vector2Int Cell;
		public Vector2Int Face;
		public bool Required = false;
	}

	[Header("References")]
	[SerializeField] public LevelGrid Grid;
	[SerializeField] NavMeshSurface surface;

	[Header("Parameters")]
	[SerializeField] int seed = 42;
	[SerializeField] IntRange roomCount = new(10, 20);
	[SerializeField] IntRange roomLength = new(4, 8);
	[SerializeField] IntRange nAdditionalEdges = new(5, 10);
	[SerializeField] int roomGap = 1;
	[SerializeField] int roomMargin = 1;
	// these are the areas in which rooms will be spawned
	// these are not necessarily the exact size of rooms,
	// but just upper bounds
	[SerializeField] List<RectInt> allSpacePartitions = new();
	[SerializeField] List<RectInt> spacePartitions = new();
	[SerializeField] List<Vector2Int> edges = new();
	[SerializeField] List<Vector2Int> spanningEdges = new();
	[SerializeField] List<Vector2Int> additionalEdges = new();
	[SerializeField, FormerlySerializedAs("spawnedRooms")]
	public List<Room> SpawnedRooms = new();
	[SerializeField]
	public List<Hallway> SpawnedHallways = new();

	[SerializeField]
	int startRoomIndex = 0;

	public Room StartRoom => SpawnedRooms[startRoomIndex];

	[SerializeField] int maxIters = 2;

	[Header("Prefabs")]
	[SerializeField] List<GameObject> roomPrefabs = new();
	[SerializeField] GameObject hallwayPrefab;
	[SerializeField, HideInInspector] List<Room> roomPrefabRooms = new();
	List<List<Door>> roomPrefabDoors = new();
	[SerializeField, HideInInspector] List<Vector2Int> roomSizes = new();
	Dictionary<int, List<Vector2Int>> doorPathwayCells = new();
	Dictionary<int, List<Vector2Int>> requiredDoorPathwayCells = new();
	Dictionary<int, List<Vector2Int>> optionalDoorPathwayCells = new();
	HashSet<Vector2Int> requiredPathwayCells = new();

	void Awake(){
		this.ClearChildren();
	}

	public void Generate(){
		if(seed >= 0){
			Random.InitState(seed);
		}
		bool success = false;
		int nAttempts = 0;
		while (!success && nAttempts++ < maxIters) {
			try {
				Clean();
				PartitionSpace(new RectInt(Vector2Int.zero, Grid.Size));
				SelectStartRoom();
				BakeRoomCoordinates();
				SpawnRooms();
				Triangulate();
				FindSpanningTree();
				ChooseAdditionalEdges();
				ConnectRooms();
				ConnectRequiredDoors();
				SpawnHallways();
				BuildNavMesh();
				success = true;
			}
			catch (RegenerateException) {
				Debug.LogWarning($"Caught RegenerationException. Regenerating...");
			}
		}

		if(!success){
			Debug.LogError("Could not generate level.");
		}
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

	void SelectStartRoom(){
		// select start room as top-leftmost room
		startRoomIndex = 0;
		for(int i = 0; i < spacePartitions.Count; i++){
			bool sameLeft = (
				spacePartitions[i].center.x
				== spacePartitions[startRoomIndex].center.x
			);
			bool moreLeft = (
				spacePartitions[i].center.x
				< spacePartitions[startRoomIndex].center.x
			);
			bool moreUp = (
				spacePartitions[i].center.y
				> spacePartitions[startRoomIndex].center.y
			);
			if(moreLeft || (sameLeft && moreUp)){
				startRoomIndex = i;
			}
		}
	}

	// this calculates the cells for each door
	// based on their world position
	void BakeRoomCoordinates(){
		roomSizes.Clear();
		foreach(var roomPrefab in roomPrefabs){
			var room = roomPrefab.GetComponent<Room>();
			roomPrefabRooms.Add(room);

			roomSizes.Add(
				room.LevelEntry.Size.XZ()
				.Divide(Grid.CellSize)
				.RoundToInt()
			);

			// Get cell and face for each door
			List<Door> doors = new();
			foreach(var door in room.LevelEntry.Doors){
				var doorXform = door.Transform;
				var forward = doorXform.forward.XZ();

				var relativePos =
					(doorXform.position - room.LevelEntry.BottomLeftCornerOffset)
					.XZ()
					.Divide(Grid.CellSize)
				// the door transform is at an edge
				// so move inward half a unit
					+ forward / 2
				// move to bottom left corner
					- Vector2.one / 2;
				var doorCell = relativePos.RoundToInt();
				if(
					doorCell - relativePos != Vector2.zero
				){
					Debug.LogWarning($"Entrance {doorXform.name} of room {gameObject.name} is not aligned to the grid ({doorCell} vs {relativePos})");
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
		int spacePartitionIndex = 0;
		foreach(var (partitionIndex, partition) in spacePartitions.ZipIndex()){
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
				var roomPosition =
					Grid.CellToWorld(bottomLeftCell)
					- roomPrefab.LevelEntry.BottomLeftCornerOffset;

#if UNITY_EDITOR
				// this spawns it as a prefab (rather
				// than ap refab clone), so that changes
				// can be observed when using in edit mode
				var roomGO = KPrefabUtility.InstantiatePrefab(
					roomPrefab.gameObject,
					roomPosition,
					Quaternion.identity
				);
#else
				var roomGO = Instantiate(
					roomPrefab.gameObject,
					roomPosition,
					Quaternion.identity
				);
#endif

				roomGO.transform.SetParent(transform);
				var room = roomGO.GetComponent<Room>();
				SpawnedRooms.Add(room);

				if(spacePartitionIndex == startRoomIndex){
					room.IsStartRoom = true;
				}

				// mark grid cells as occupied
				for(int x = 0; x < size.x; x++){
					for(int y = 0; y < size.y; y++){
						Grid.SetCellType(
							bottomLeftCell + new Vector2Int(x, y),
							CellType.Room
						);
					}
				}

				// add door cells as targets
				var doors = roomPrefabDoors[roomPrefabIndex];
				doorPathwayCells[spacePartitionIndex] = new();
				optionalDoorPathwayCells[spacePartitionIndex] = new();
				requiredDoorPathwayCells[spacePartitionIndex] = new();
				foreach(var door in doors){
					var doorCell = bottomLeftCell + door.Cell;
					Grid.SetCellType(doorCell, CellType.Door);

					var pathwayCell = doorCell + door.Face;
					if(Grid.CellInBounds(pathwayCell)){
						doorPathwayCells[spacePartitionIndex].Add(pathwayCell);
						if (door.Required){
							requiredDoorPathwayCells[spacePartitionIndex].Add(pathwayCell);
							requiredPathwayCells.Add(pathwayCell);
						}
						else {
							optionalDoorPathwayCells[spacePartitionIndex].Add(pathwayCell);
						}
					}
				}
				spacePartitionIndex++;
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
		edges = new();

		// for each triangle, add its
		// edges to the adjacency list
		// skip first triangle since this is the outer triangle
		// subtract 3 from each index since we ignore the first triangle
		HashSet<int> spacePartitionsAdded = new();
		for(int i = 0; i < triangulation.TriangleSet.TriangleCount; i++){
			var triangle = triangulation.TriangleSet.GetTrianglePoints(i);

			for(int j = 0; j < 3; j++){
				var a = triangle[j];
				var b = triangle[(j + 1) % 3];
				if(
					spacePartitionCenterIndexMap.TryGetValue(a, out var aIndex)
					&& spacePartitionCenterIndexMap.TryGetValue(b, out var bIndex)
				){
					edges.Add(new Vector2Int(aIndex, bIndex));
					spacePartitionsAdded.Add(aIndex);
					spacePartitionsAdded.Add(bIndex);
				}
			}
		}

		// check that all rooms have an edge
		for(int i = 0; i < spacePartitions.Count; i++){
			if(!spacePartitionsAdded.Contains(i)){
				Debug.LogWarning($"Room {i} has no adjacent rooms.");
				throw new RegenerateException();
			}
		}
	}

	void FindSpanningTree(){
		// cache adjacency list
		var adj = new Dictionary<int, List<int>>();
		foreach(var edge in edges){
			if(!adj.ContainsKey(edge.x)){
				adj[edge.x] = new();
			}
			if(!adj.ContainsKey(edge.y)){
				adj[edge.y] = new();
			}
			adj[edge.x].Add(edge.y);
			adj[edge.y].Add(edge.x);
		}

		// Prim's algorithm
		spanningEdges.Clear();
		HashSet<int> added = new();
		KPriorityQueue<Vector2Int, float> pq = new();

		added.Add(startRoomIndex);
		foreach(var neighbor in adj[startRoomIndex]){
			var squareDistance = (
				spacePartitions[startRoomIndex].center
				- spacePartitions[neighbor].center
			).sqrMagnitude;
			// add noise to encourage randomness
			squareDistance += Random.value * Grid.CellSize.sqrMagnitude * 3;
			pq.Enqueue(
				new Vector2Int(startRoomIndex, neighbor),
				squareDistance
			);
		}

		while(pq.Count > 0){
			var edge = pq.Dequeue();
			if(added.Contains(edge.y)){
				continue;
			}
			added.Add(edge.y);
			spanningEdges.Add(edge);
			foreach(var neighbor in adj[edge.y]){
				if(!added.Contains(neighbor)){
					var squareDistance = (
						spacePartitions[edge.y].center
						- spacePartitions[neighbor].center
					).sqrMagnitude;
					// add noise to encourage randomness
					squareDistance += Random.value * Grid.CellSize.sqrMagnitude * 3;
					pq.Enqueue(
						new Vector2Int(edge.y, neighbor),
						squareDistance
					);
				}
			}
		}
	}

	void ChooseAdditionalEdges(){
		var unusedEdges = new List<Vector2Int>();
		foreach(var edge in edges){
			var reverse = new Vector2Int(edge.y, edge.x);
			if(unusedEdges.Contains(reverse)){
				continue;
			}
			else if(spanningEdges.Contains(edge) || spanningEdges.Contains(reverse)){
				continue;
			}
			unusedEdges.Add(edge);
		}

		additionalEdges = unusedEdges.Sample(nAdditionalEdges.Random());
	}

	void ConnectRooms(){
		void Connect2Rooms(
			Vector2Int edge,
			bool stopOnHitPathway
		){
			// find closest doors
			var aDoorCells = doorPathwayCells[edge.x];
			var bDoorCells = doorPathwayCells[edge.y];

			var (aMinDoorCell, bMinDoorCell) = aDoorCells
				.Cartesian(bDoorCells)
				.MinBy(
					(pair) => (pair.Item1 - pair.Item2).sqrMagnitude
				);

			var path = Algorithm.AStar(
				aMinDoorCell,
				heuristic: (cell) => (cell - bMinDoorCell).sqrMagnitude,
				isTarget: (cell) =>
					(
						stopOnHitPathway
						&& cell != aMinDoorCell
						&& Grid.GetCellType(cell) == CellType.Pathway
						|| bDoorCells.Contains(cell)
					),
				isWalkable: (cell) => {
					var type = Grid.GetCellType(cell);
					return type == CellType.Empty || type == CellType.Pathway;
				}
			);

			if(path == null){
				Debug.LogWarning($"Could not connect rooms {edge.x} and {edge.y}");
				throw new RegenerateException();
			}

			PostProcessPathwayPath(path);

			foreach(var cell in path){
				Grid.SetCellType(cell, CellType.Pathway);
			}
		}

		// use A* to connect rooms
		foreach(var edge in spanningEdges){
			Connect2Rooms(edge, stopOnHitPathway: false);
		}
		foreach(var edge in additionalEdges){
			Connect2Rooms(edge, stopOnHitPathway: true);
		}
	}

	void PostProcessPathwayPath(List<Vector2Int> path){
		// prevent many bends
		for(int i = 0; i < path.Count; i++){
			// if cell is in a square
			// ..
			// x.
			// remove it

			// check each dir
			bool removed = false;
			for(int dx = -1; dx <= 1; dx += 2){
				for(int dy = -1; dy <= 1; dy += 2){
					var a = path[i];
					var b = a + new Vector2Int(dx, dy);
					var c = a + new Vector2Int(dx, 0);
					var d = a + new Vector2Int(0, dy);
					if(
						Grid.GetCellType(a) == CellType.Pathway
						&& Grid.GetCellType(b) == CellType.Pathway
						&& Grid.GetCellType(c) == CellType.Pathway
						&& Grid.GetCellType(d) == CellType.Pathway
					){
						path.RemoveAt(i);
						i--;
						removed = true;
						break;
					}
				}
			}
			if(removed) {
				continue;
			}


			if(i + 3 < path.Count){
				var a = path[i];
				var b = path[i + 1];
				var c = path[i + 2];
				var d = path[i + 3];

				var abDir = b - a;
				var bcDir = c - b;
				var cdDir = d - c;

				// ..
				//  ..
				// to
				// ...	or	.
				//   .		...
				if(abDir == cdDir && abDir != bcDir){
					// fail chance
					if(Random.value < 0.25f) continue;

					// try to move c to b + abDir
					var newC = b + abDir;
					if(Grid.GetCellType(newC) != CellType.Room){
						path[i + 2] = newC;
						continue;
					}

					// try to move b to c - abDir
					var newB = c - abDir;
					if(Grid.GetCellType(newB) != CellType.Room){
						path[i + 1] = newB;
						continue;
					}
				}
			}
		}
	}

	void ConnectRequiredDoors(){
		foreach(var (spacePartitionIndex, doorCells) in requiredDoorPathwayCells){
			foreach(var doorCell in doorCells){
				// if already in pathway, skip
				if(Grid.GetCellType(doorCell) == CellType.Pathway){
					continue;
				}

				// connect them
				var path = Algorithm.BFS(
					doorCell,
					isTarget: cell => Grid.GetCellType(cell) == CellType.Pathway,
					isWalkable: cell => {
						var type = Grid.GetCellType(cell);
						return type == CellType.Empty || type == CellType.Pathway;
					}
				);
				if(path == null){
					Debug.LogWarning($"Could not connect required door {doorCell} to pathway");
					throw new RegenerateException();
				}

				PostProcessPathwayPath(path);

				foreach(var cell in path){
					Grid.SetCellType(cell, CellType.Pathway);
				}
			}
		}
	}

	void SpawnHallways(){
		// DFS to find connected components
		// so we only spawn 1 hallway per connected
		// component
		var visited = new HashSet<Vector2Int>();

		void DFS(Vector2Int start){
			foreach(var dir in KMath.Directions4){
				var neighbor = start + dir;
				if(
					Grid.CellInBounds(neighbor)
					&& Grid.GetCellType(neighbor) == CellType.Pathway
					&& !visited.Contains(neighbor)
				){
					visited.Add(neighbor);
					DFS(neighbor);
				}
			}
		}

		// iterate over all door cells
		foreach(var (_, doorCells) in doorPathwayCells){
			foreach(var doorCell in doorCells){
				if(
					Grid.GetCellType(doorCell) != CellType.Pathway ||
					visited.Contains(doorCell)
				){
					continue;
				}
				var hallwayPosition = Grid.CellToWorld(
					(Vector2) doorCell + Vector2.one * 0.5f
				);

#if UNITY_EDITOR
				var hallwayGO = KPrefabUtility.InstantiatePrefab(
					hallwayPrefab,
					hallwayPosition,
					Quaternion.identity
				);
#else
				var hallwayGO = Instantiate(
					hallwayPrefab,
					hallwayPosition,
					Quaternion.identity
				);
#endif
				hallwayGO.transform.SetParent(transform);

				var hallway = hallwayGO.GetComponent<Hallway>();
				hallway.LevelGenerator = this;
				hallway.StartCell = doorCell;

				hallway.Generate();

				SpawnedHallways.Add(hallway);

				DFS(doorCell);
			}
		}
	}

	void Clean(){
		spacePartitions.Clear();
		allSpacePartitions.Clear();

		roomPrefabRooms.Clear();
		roomPrefabDoors.Clear();

		edges.Clear();
		spanningEdges.Clear();

		doorPathwayCells.Clear();
		requiredDoorPathwayCells.Clear();
		optionalDoorPathwayCells.Clear();

		foreach(var room in SpawnedRooms){
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
		SpawnedRooms.Clear();

		foreach(var hallway in SpawnedHallways){
			if(hallway){
#if UNITY_EDITOR
				if(EditorApplication.isPlaying){
					Destroy(hallway.gameObject);
				}
				else {
					DestroyImmediate(hallway.gameObject);
				}
#else
				Destroy(hallway.gameObject);
#endif
			}
		}
		SpawnedHallways.Clear();

		Grid.Clear();
	}

	void BuildNavMesh(){
#if UNITY_EDITOR
		NavMeshAssetManager.instance.StartBakingSurfaces(new Object[]{ surface });
#else
		surface.AddData();
        surface.UpdateNavMesh(surface.navMeshData);
        surface.BuildNavMesh();
#endif
	}

	public Vector3 GetPlayerSpawnPoint(){
		return StartRoom.transform.position;
	}

	void OnDrawGizmos(){
#if UNITY_EDITOR
		if(!enabled || !Grid) return;

		// draw space partitions
		foreach(var partition in allSpacePartitions){
			if(spacePartitions.Contains(partition)){
				Gizmos.color = Color.green.WithA(0.1f);
			}
			else {
				Gizmos.color = Color.red.WithA(0.1f);
			}
			var center = Grid.CellToWorld(partition.center);

			Gizmos.DrawWireCube(
				center,
				Grid.SizeToWorld(partition.size)
			);

			// draw center
			Gizmos.color = Gizmos.color.WithA(0.4f);
			Gizmos.DrawCube(
				center,
				Vector3.one * 0.1f
			);
		}

		// draw room indices
		foreach(var (i, spacePartition) in spacePartitions.ZipIndex()){
			Handles.Label(
				Grid.CellToWorld(spacePartitions[i].center),
				i.ToString()
			);
		}

		// draw lines between adjacent rooms
		if(edges != null){
			foreach(var edge in edges) {
				var a = Grid.CellToWorld(spacePartitions[edge.x].center);
				var b = Grid.CellToWorld(spacePartitions[edge.y].center);

				if(spanningEdges.Contains(edge)){
					Gizmos.color = Color.green.WithA(0.5f);
				}
				else if (additionalEdges.Contains(edge)){
					Gizmos.color = new Color(1, 0.8f, 0).WithA(0.1f);
				}
				else {
					Gizmos.color = Color.red.WithA(0.1f);
				}
				Gizmos.DrawLine(a, b);
			}
		}
#endif
	}
}