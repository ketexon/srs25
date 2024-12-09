using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Kutie.Extensions;
using System.Linq;




#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

[System.Serializable]
public class LevelDoor {
	public Vector3Int Position;
	public Vector3Int Face;
	public bool Required;
}

[System.Serializable]
public class LevelGridEntry {
	public Vector3Int Size;
	public Vector3 BottomLeftCornerOffset;
	public List<LevelDoor> Doors;
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelGenerator3))]
public class LevelGenerator3Editor : Editor {
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
		var generator = target as LevelGenerator3;

		var defaultInspector = new VisualElement();
		InspectorElement.FillDefaultInspector(defaultInspector, serializedObject, this);

		var cellSizeProperty = serializedObject.FindProperty("cellSize");
		defaultInspector.RegisterCallback<SerializedPropertyChangeEvent>(e => {
			LevelGenerator3.CellSize = cellSizeProperty.vector3Value;
		});

		root.Add(defaultInspector);

		var generateButton = new Button(() => {
			generator.Generate();
			serializedObject.ApplyModifiedProperties();
		});
		generateButton.Add(new Label("Generate"));

		root.Add(generateButton);

		return root;
    }

	void OnSceneGUI(){
		var generator = target as LevelGenerator3;
		if(generator == null){
			return;
		}

		Handles.BeginGUI();

		EditorGUI.BeginChangeCheck();

		var labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.normal.textColor = Color.white;
		var width = GUILayout.Width(100);

		var gridOpacityProperty = serializedObject.FindProperty("gridOpacity");
		GUILayout.Label("Grid Opacity", labelStyle);
		var newGridOpacity = GUILayout.HorizontalSlider(
			gridOpacityProperty.floatValue,
			0, 1,
			width
		);
		if(newGridOpacity != gridOpacityProperty.floatValue){
			gridOpacityProperty.floatValue = newGridOpacity;
		}

		var cellOpacityProperty = serializedObject.FindProperty("cellOpacity");
		GUILayout.Label("Cell Opacity", labelStyle);
		var newCellOpacity = GUILayout.HorizontalSlider(
			cellOpacityProperty.floatValue,
			0, 1,
			width
		);
		if(newCellOpacity != cellOpacityProperty.floatValue){
			cellOpacityProperty.floatValue = newCellOpacity;
		}

		var boundsOpacityProperty = serializedObject.FindProperty("boundsOpacity");
		GUILayout.Label("Bounds Opacity", labelStyle);
		float newBoundsOpactiy = GUILayout.HorizontalSlider(
			boundsOpacityProperty.floatValue,
			0, 1,
			width
		);
		if(newBoundsOpactiy != boundsOpacityProperty.floatValue){
			boundsOpacityProperty.floatValue = newBoundsOpactiy;
		}

		if(EditorGUI.EndChangeCheck()){
			serializedObject.ApplyModifiedProperties();
		}

		Handles.EndGUI();
	}
}
#endif

public class LevelGenerator3 : MonoBehaviour {
	[System.Serializable]
	enum CellType {
		Empty,
		Room,
		Pathway,
		OutOfBounds,
	};

	public static Vector3 CellSize = new(2, 3, 2);

	/// <summary>
	/// RENDERING PARAMETERS
	/// </summary>
	[SerializeField, HideInInspector] float gridOpacity = 0.10f;
	[SerializeField, HideInInspector] float cellOpacity = 0.10f;
	[SerializeField, HideInInspector] float boundsOpacity = 0.10f;

	[SerializeField] Vector3 cellSize = new(2, 2, 2);
	[SerializeField] Vector3Int gridSize = new(40, 40, 40);
	[SerializeField] Kutie.IntRange roomCount = new(10, 20);
	[SerializeField] Kutie.IntRange roomLength = new(4, 8);
	[SerializeField] Kutie.IntRange roomHeight = new(2, 4);
	[SerializeField] int roomPaddingHorizontal = 1;
	[SerializeField] int roomPaddingVertical = 1;
	// these are the areas in which rooms will be spawned
	// these are not necessarily the exact size of rooms,
	// but just upper bounds
	[SerializeField] List<BoundsInt> spacePartitions = new();
	[SerializeField] List<Room> spawnedRooms = new();

	[Header("Prefabs")]
	[SerializeField] List<GameObject> roomPrefabs = new();

	[SerializeField, HideInInspector]
	List<CellType> grid = new();

	[SerializeField, HideInInspector]
	List<Vector3Int> doorCells = new();

	int GetGridIndex(Vector3Int position){
		return position.x + position.y * gridSize.x + position.z * gridSize.x * gridSize.y;
	}

	bool InBounds(Vector3Int position){
		var idx = GetGridIndex(position);
		return idx >= 0 && idx < grid.Count;
	}

	CellType GridGet(Vector3Int position){
		var idx = GetGridIndex(position);
		if(idx < 0 || idx >= grid.Count){
			return CellType.OutOfBounds;
		}
		return grid[GetGridIndex(position)];
	}

	void GridSet(Vector3Int position, CellType value){
		grid[GetGridIndex(position)] = value;
	}

	int MinRoomLength => roomLength.Min;
	int MaxRoomLength => roomLength.Max;
	int MinRoomHeight => roomHeight.Min;
	int MaxRoomHeight => roomHeight.Max;

	void Start() {
		PartitionSpace(new BoundsInt(Vector3Int.zero, gridSize));
	}

	public void Generate(){
		spacePartitions.Clear();
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

		grid = new(gridSize.Volume());
		for(int i = 0; i < gridSize.Volume(); ++i){
			grid.Add(CellType.Empty);
		}

		doorCells.Clear();

		PartitionSpace(new BoundsInt(Vector3Int.zero, gridSize));
		SpawnRooms();
		GeneratePathways();
	}

	void PartitionSpace(BoundsInt bounds){
		Queue<BoundsInt> queue = new();
		queue.Enqueue(bounds);
		while(queue.Count > 0 && spacePartitions.Count < roomCount.Max){
			var top = queue.Dequeue();
			if(top.size.x < MinRoomLength || top.size.y < MinRoomHeight || top.size.z < MinRoomLength){
				continue;
			}
			if(top.size.x <= MaxRoomLength && top.size.y <= MaxRoomHeight && top.size.z <= MaxRoomLength){
				top.size -= new Vector3Int(
					roomPaddingHorizontal * 2,
					roomPaddingVertical * 2,
					roomPaddingHorizontal * 2
				);
				spacePartitions.Add(top);
				continue;
			}
			var splitAxis = Random.Range(0, 3);
			// x
			if(splitAxis == 0){
				var splitPoint = Random.Range(MinRoomLength, top.size.x - MinRoomLength);
				// var splitPoint = MinRoomLength;
				queue.Enqueue(new BoundsInt(top.min, new Vector3Int(splitPoint, top.size.y, top.size.z)));
				queue.Enqueue(new BoundsInt(
					new Vector3Int(top.min.x + splitPoint, top.min.y, top.min.z),
					new Vector3Int(top.size.x - splitPoint, top.size.y, top.size.z)
				));
			}
			// y
			else if(splitAxis == 1){
				var splitPoint = Random.Range(MinRoomHeight, top.size.y - MinRoomHeight);
				// var splitPoint = MinRoomHeight;
				queue.Enqueue(new BoundsInt(top.min, new Vector3Int(top.size.x, splitPoint, top.size.z)));
				queue.Enqueue(new BoundsInt(
					new Vector3Int(top.min.x, top.min.y + splitPoint, top.min.z),
					new Vector3Int(top.size.x, top.size.y - splitPoint, top.size.z)
				));
			}
			// z
			else {
				var splitPoint = Random.Range(MinRoomLength, top.size.z - MinRoomLength);
				// var splitPoint = MinRoomLength;
				queue.Enqueue(new BoundsInt(
					top.min,
					new Vector3Int(top.size.x, top.size.y, splitPoint)
				));
				queue.Enqueue(new BoundsInt(
					new Vector3Int(top.min.x, top.min.y, top.min.z + splitPoint),
					new Vector3Int(top.size.x, top.size.y, top.size.z - splitPoint)
				));
			}
		}
		if(spacePartitions.Count < roomCount.Min){
			spacePartitions.Clear();
			PartitionSpace(bounds);
		}
	}


	void SpawnRooms(){
		List<Room> roomPrefabRooms = new();
		foreach(var roomPrefab in roomPrefabs){
			if(roomPrefab.TryGetComponent<Room>(out var room)){
				roomPrefabRooms.Add(room);
			}
			else {
				Debug.LogError($"Prefab {roomPrefab.name} does not have a Room component");
			}
		}

		// find the room of largest area
		// that fits inside each space partition
		foreach(var partition in spacePartitions){
			Room roomPrefab = null;
			int rotation = 0;
			foreach(var candidate in roomPrefabRooms){
				// check if it fits within partition
				var size = candidate.LevelEntry.Size;

				// check if it fits within the partition
				// and rotate it if necessary
				bool fits = false;

				// since we only need to check 2 rotations (0 and 90),
				// and checking 0 and 90 is the same as checking 180 and 270
				// we can check a random rotation within 0 and 270
				// and then check that rotation +90 or -90
				int candidateRot = Random.Range(0, 4) * 90;
				for(int i = 0; i < 2; ++i){
					var sizeRotated = Quaternion.Euler(0, candidateRot, 0) * size;
					if(size.x <= partition.size.x && size.y <= partition.size.y && size.z <= partition.size.z){
						fits = true;
						break;
					}
					if(Random.value > 0.5f){
						candidateRot += 90;
					}
					else {
						candidateRot -= 90;
					}
				}

				if(!fits){
					continue;
				}

				if(roomPrefab == null || size.sqrMagnitude > roomPrefab.Bounds.size.sqrMagnitude){
					roomPrefab = candidate;
					rotation = candidateRot;
				}
			}
			if(roomPrefab != null){
				var rotationQuat = Quaternion.Euler(0, rotation, 0);
				var bottomLeftCell = partition.min
						+ (partition.size - roomPrefab.LevelEntry.Size) / 2;
				var roomGO = Instantiate(
					roomPrefab.gameObject,
					bottomLeftCell.Hammard(cellSize)
					- roomPrefab.LevelEntry.BottomLeftCornerOffset,
					rotationQuat
				);
				var room = roomGO.GetComponent<Room>();
				spawnedRooms.Add(room);

				// mark grid cells as occupied
				var rotatedSize = Vector3Int.RoundToInt(
					rotationQuat * room.LevelEntry.Size
				);
				for(int x = 0; x < rotatedSize.x; x++){
					for(int y = 0; y < rotatedSize.y; y++){
						for(int z = 0; z < rotatedSize.z; z++){
							GridSet(
								bottomLeftCell + new Vector3Int(x, y, z),
								CellType.Room
							);
						}
					}
				}
				// add door cells as targets
				foreach(var door in room.LevelEntry.Doors){
					var doorCell = Vector3Int.RoundToInt(
						rotationQuat * (door.Position + door.Face)
						+ bottomLeftCell
					);
					if(InBounds(doorCell)){
						doorCells.Add(doorCell);
					}
				}
			}
		}
	}

	void GeneratePathways(){
		// mark each door cell pathway
		foreach(var door in doorCells){
			GridSet(door, CellType.Pathway);
		}

		// store which cells are reachable using
		// union find

		UnionFind.DisjointSet<Vector3Int> disjointSet = new();

		return;
		// for each door
		// find the closest door
		// do a* until it reaches that door
		// mark the path as a door
		// repeat until all doors are connected
		foreach(var door in doorCells){
			disjointSet.MakeSet(door);
			var path = FindPath(door);
			Debug.Log($"Path from {door} to {path.Last()}");
			foreach(var cell in path){
				Debug.Log(cell);
				GridSet(cell, CellType.Pathway);
				disjointSet.MakeSet(cell);
				disjointSet.Union(door, cell);
			}
		}
	}

	List<Vector3Int> FindPath(Vector3Int start) {
		Queue<Vector3Int> queue = new();
		queue.Enqueue(start);

		Dictionary<Vector3Int, Vector3Int> cameFrom = new();
		cameFrom[start] = start;

		bool found = false;
		Vector3Int end = Vector3Int.zero;
		while(queue.Count > 0){
			var cell = queue.Dequeue();

			// check each orthogonal direction
			foreach(var direction in new Vector3Int[]{
				Vector3Int.right,
				Vector3Int.left,
				Vector3Int.up,
				Vector3Int.down,
				Vector3Int.forward,
				Vector3Int.back,
			}){
				var next = cell + direction;

				// if we've already visited this cell, skip it
				if(cameFrom.ContainsKey(next)){
					continue;
				}
				cameFrom[next] = cell;

				// if the cell is empty, add it to the queue
				var contents = GridGet(next);
				if(contents == CellType.Empty){
					queue.Enqueue(next);
				}
				else if(contents == CellType.Pathway){
					found = true;
					end = next;
					break;
				}
			}

			if(found){
				break;
			}
		}

		// construct path
		if(found){
			List<Vector3Int> path = new();
			var current = end;
			while(current != start){
				path.Add(current);
				current = cameFrom[current];
			}
			path.Add(current);
			path.Reverse();
			return path;
		}

		return null;
	}

	void OnDrawGizmos(){
		if(!enabled) return;
		// draw grid
		var color = Color.white;
		color.a = gridOpacity;
		Gizmos.color = color;
		for(int x = 0; x < gridSize.x; x++){
			for(int y = 0; y < gridSize.y; y++){
				for(int z = 0; z < gridSize.z; z++){
					// +0.5f because each point represents the bottom left corner of a cell
					Gizmos.DrawWireCube(
						new Vector3(
							(x + 0.5f) * cellSize.x,
							(y + 0.5f) * cellSize.y,
							(z + 0.5f) * cellSize.z
						),
						cellSize
					);
				}
			}
		}

		color = Color.green;
		color.a = boundsOpacity;
		Gizmos.color = color;
		if(spacePartitions != null){
			foreach(var partition in spacePartitions){
				Gizmos.DrawWireCube(
					new Vector3(
						partition.center.x * cellSize.x,
						partition.center.y * cellSize.y,
						partition.center.z * cellSize.z
					),
					new Vector3(
						partition.size.x * cellSize.x,
						partition.size.y * cellSize.y,
						partition.size.z * cellSize.z
					)
				);
			}
		}

		// draw grid
		if(grid.Count == gridSize.Volume()){
			for(int x = 0; x < gridSize.x; x++){
				for(int y = 0; y < gridSize.y; y++){
					for(int z = 0; z < gridSize.z; z++){
						var contents = GridGet(new Vector3Int(x, y, z));

						if (contents == CellType.Pathway){
							color = Color.green;
						}
						else if (contents == CellType.Room){
							color = Color.red;
						}
						color.a = cellOpacity;
						Gizmos.color = color;

						if(contents == CellType.Empty){
							color = Color.white;
							color.a = 0.05f;
							Gizmos.color = color;
						}

						// +0.5f because each point represents the bottom left corner of a cell
						Gizmos.DrawWireCube(
							new Vector3(
								(x + 0.5f) * cellSize.x,
								(y + 0.5f) * cellSize.y,
								(z + 0.5f) * cellSize.z
							),
							cellSize / 2
						);
					}
				}
			}
		}
	}
}