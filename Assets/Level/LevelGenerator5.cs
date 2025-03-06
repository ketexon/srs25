using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator5 : LevelGenerator {
	[System.Serializable]
	class RoomSizeEntry {
		public Vector2Int Size;
		public int Weight;
	}


	[System.Serializable]
	public class RoomDoor {
		public Vector2Int Position;
		public DirectionMask Direction;
	}

	[System.Serializable]
	class FixedRoomEntry {
		public Alignment2D Alignment;
		public Vector2Int Size;
		public Vector2Int Offset;
		public GameObject Prefab;
		public Color GizmosColorOverride = Color.clear;
		public List<RoomDoor> Doors = new();
	}

	[System.Serializable]
	class RequiredPathEntry {
		public Alignment2D StartAlignment;
		public Vector2Int StartOffset;
		public Alignment2D EndAlignment;
		public Vector2Int EndOffset;
	}

	[SerializeField] LevelGrid5 grid;
	[SerializeField] int officeWidth = 5;
	[SerializeField] int parkLength = 12;
	[SerializeField] int maxAttempts = 10;

	[SerializeField] List<FixedRoomEntry> fixedRooms = new();
	[SerializeField] List<GameObject> roomPrefabs = new();
	[SerializeField] List<RoomSizeEntry> roomSizes = new();

	/// <summary>
	/// For every path in requiredPaths, there must be a path
	/// between the start and end room
	/// </summary>
	[SerializeField] List<RequiredPathEntry> requiredPaths = new();

	[SerializeField, HideInInspector] List<RectInt> roomRects = new();
	// if we have 4 rooms:
	// 0 1
	// 2 3
	// a door between 0 and 1 is horizontalDoors[0] = horizontalDoors[1-1]
	// a door between 1 and 3 is verticalDoors[]
	[SerializeField, HideInInspector] List<bool> horizontalDoors = new();
	[SerializeField, HideInInspector] List<bool> verticalDoors = new();
	[SerializeField, HideInInspector] List<List<Vector2Int>> paths = new();

	Queue<Vector2Int> spawnRoomQueue = new();

	Vector2Int maxRoomSize;

	class RegenerateException : System.Exception {}

    public override Vector3 GetPlayerSpawnPoint()
    {
        return Vector3.zero;
    }

    void Reset()
    {
        grid = GetComponent<LevelGrid5>();
		grid.Size = Vector2Int.one * (2 * officeWidth + parkLength);
    }

	void Clear(){
		grid.Clear();
		roomRects.Clear();
	}

    public override void Generate()
    {
		grid.Size = grid.Size = Vector2Int.one * (2 * officeWidth + parkLength);
		int attempts = 0;
		while(attempts < maxAttempts){
			try {
				Clear();
				CalculateRoomSizes();
				InitializeGrid();
				InitializeSpawning();
				AddFixedRooms();
				PartitionGrid();
				FindPaths();

				return;
			} catch (RegenerateException) {
				++attempts;
			}
		}
    }

	void CalculateRoomSizes(){
		maxRoomSize = Vector2Int.zero;
		foreach(var sizeEntry in roomSizes){
			maxRoomSize.x = System.Math.Max(maxRoomSize.x, sizeEntry.Size.x);
			maxRoomSize.y = System.Math.Max(maxRoomSize.y, sizeEntry.Size.y);
		}
	}

	void InitializeGrid(){
		for(int x = officeWidth; x < officeWidth + parkLength; ++x){
			for(int y = officeWidth; y < officeWidth + parkLength; ++y){
				grid.SetCellType(new Vector2Int(x, y), CellType5.Blocked);
			}
		}
	}

	void InitializeSpawning(){
		spawnRoomQueue = new();
	}

	void AddFixedRooms(){
		foreach(var entry in fixedRooms){
			// calculate room position
			Vector2Int roomSize = entry.Size;
			Vector2Int roomPosition = grid.AlignmentOffsetRectToCell(
				entry.Alignment,
				entry.Offset,
				roomSize
			);

			// place room
			RectInt roomRect = new(
				roomPosition.x, roomPosition.y,
				roomSize.x, roomSize.y
			);
			PlaceRoom(roomRect, entry.GizmosColorOverride);
			// set the doors
			foreach(var doorEntry in entry.Doors){
				Vector2Int doorPosition = doorEntry.Position;
				grid.GetCell(roomPosition + doorPosition).DoorDirections = doorEntry.Direction;
			}
		}
	}

	void PlaceRoom(RectInt chosenRoomRect, CellType5 cellType = CellType5.Occupied){
		PlaceRoom(chosenRoomRect, Color.clear, cellType);
	}

	void PlaceRoom(RectInt chosenRoomRect, Color colorOverride, CellType5 cellType = CellType5.Occupied){
		for (int x = chosenRoomRect.xMin; x < chosenRoomRect.xMax; ++x) {
			for (int y = chosenRoomRect.yMin; y < chosenRoomRect.yMax; ++y) {
				var cell = grid.GetCell(
					x, y
				);
				cell.Type = cellType;
				cell.RoomIndex = roomRects.Count;
				cell.ColorOverride = colorOverride;
			}
		}

		roomRects.Add(chosenRoomRect);

		// add all cells surrounding the room rect to the queue
		for (int x = chosenRoomRect.xMin - 1; x < chosenRoomRect.xMax + 1; ++x) {
			spawnRoomQueue.Enqueue(new Vector2Int(x, chosenRoomRect.yMin - 1));
			spawnRoomQueue.Enqueue(new Vector2Int(x, chosenRoomRect.yMax));
		}
		for (int y = chosenRoomRect.yMin; y < chosenRoomRect.yMax; ++y) {
			spawnRoomQueue.Enqueue(new Vector2Int(chosenRoomRect.xMin - 1, y));
			spawnRoomQueue.Enqueue(new Vector2Int(chosenRoomRect.xMax, y));
		}
	}

	void PartitionGrid() {
		if(spawnRoomQueue.Count == 0){
			spawnRoomQueue.Enqueue(new Vector2Int(0, 0));
		}
		while (spawnRoomQueue.Count > 0) {
			var cell = spawnRoomQueue.Dequeue();
			if (!grid.CellInBounds(cell) || grid.GetCellType(cell) != CellType5.Empty) {
				continue;
			}

			bool CanFit(RectInt rect){
				for (int x = rect.xMin; x < rect.xMax; ++x) {
					for (int y = rect.yMin; y < rect.yMax; ++y) {
						if (
							!grid.CellInBounds(x, y)
							|| grid.GetCellType(x, y) != CellType5.Empty
						) {
							return false;
						}
					}
				}
				return true;
			}

			List<(RectInt, int)> viableRoomPositions = new();
			foreach (var roomSizeEntry in roomSizes){
				var roomSize = roomSizeEntry.Size;
				RectInt[] roomRects = {
					new(cell.x, cell.y, roomSize.x, roomSize.y),
					new(cell.x, cell.y, roomSize.y, roomSize.x),

					new(cell.x - roomSize.x + 1, cell.y, roomSize.x, roomSize.y),
					new(cell.x - roomSize.y + 1, cell.y, roomSize.y, roomSize.x),

					new(cell.x, cell.y - roomSize.x + 1, roomSize.x, roomSize.y),
					new(cell.x, cell.y - roomSize.y + 1, roomSize.y, roomSize.x),

					new(cell.x - roomSize.x + 1, cell.y - roomSize.y + 1, roomSize.x, roomSize.y),
					new(cell.x - roomSize.y + 1, cell.y - roomSize.x + 1, roomSize.y, roomSize.x),
				};

				foreach (var roomRect in roomRects){
					if (CanFit(roomRect)){
						viableRoomPositions.Add((roomRect, roomSizeEntry.Weight));
					}
				}
			}

			if (viableRoomPositions.Count == 0){
				Debug.LogError("NOOOOOOOO :((((");
				throw new RegenerateException();
			}

			// weight by room size to make larger rooms more likely
			int totalWeight = 0;
			foreach (var (roomRect, weight) in viableRoomPositions){
				totalWeight += weight;
			}

			int choice = Random.Range(0, totalWeight);
			RectInt chosenRoomRect = new();
			foreach (var (roomRect, weight) in viableRoomPositions){
				choice -= weight;
				if (choice <= 0){
					chosenRoomRect = roomRect;
					break;
				}
			}

			PlaceRoom(chosenRoomRect);
		}
	}



	void FindPaths(){
		List<(Vector2Int, Vector2Int)> startEnds = new();
		foreach(var requiredPath in requiredPaths){
			Vector2Int start = grid.AlignmentOffsetToCell(
				requiredPath.StartAlignment,
				requiredPath.StartOffset
			);
			Vector2Int end = grid.AlignmentOffsetToCell(
				requiredPath.EndAlignment,
				requiredPath.EndOffset
			);
			startEnds.Add((start, end));
		}

		HashSet<Vector2Int> reachableCells = new();
		foreach(var (start, end) in startEnds){
			// do A* to find path and place doors
			// mark cells as reachable
			// so that we can break if a path is already connected
			var path = Kutie.Algorithm.AStar(
				start,
				heuristic: cell => Vector2Int.Distance(cell, end),
				isTarget: cell => cell == end
				|| reachableCells.Contains(cell),
				isWalkable: cell => (
					grid.CellInBounds(cell)
					&& grid.GetCellType(cell) != CellType5.Blocked
				)
			) ?? throw new RegenerateException();

            foreach (var cell in path){
				reachableCells.Add(cell);
			}
			paths.Add(path);
		}
	}

	void OnDrawGizmos(){
		foreach(var roomRect in roomRects){
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(
				transform.position + new Vector3(
					roomRect.x + (float) roomRect.width / 2,
					0,
					roomRect.y + (float) roomRect.height / 2
				) * grid.CellLength,
				new Vector3(roomRect.width, 0, roomRect.height) * grid.CellLength
			);
		}

		for (int x = 0; x < grid.Size.x; ++x)
		{
			for(int y = 0; y < grid.Size.y; ++y){
				var cell = grid.GetCell(new Vector2Int(x, y));
				if(cell.DoorDirections != DirectionMask.None){
					Gizmos.color = Color.blue;
					var center = transform.position + new Vector3(x + 0.5f, 0, y + 0.5f) * grid.CellLength;
					(DirectionMask, Vector3)[] doorDirections = {
						(DirectionMask.Up, Vector3.forward),
						(DirectionMask.Down, Vector3.back),
						(DirectionMask.Left, Vector3.left),
						(DirectionMask.Right, Vector3.right),
					};

					foreach(var (direction, directionVector) in doorDirections){
						if((cell.DoorDirections & direction) != 0){
							Gizmos.DrawRay(
								center,
								directionVector * grid.CellLength
							);
						}
					}
				}
			}
		}
		foreach(var requiredPath in requiredPaths){
			Gizmos.color = Color.red;
			var startCenter = grid.AlignmentOffsetToCell(
				requiredPath.StartAlignment,
				requiredPath.StartOffset
			) + Vector2.one / 2;
			var endCenter = grid.AlignmentOffsetToCell(
				requiredPath.EndAlignment,
				requiredPath.EndOffset
			) + Vector2.one / 2;
			Gizmos.DrawLine(
				transform.position + new Vector3(startCenter.x, 0, startCenter.y) * grid.CellLength,
				transform.position + new Vector3(endCenter.x, 0, endCenter.y) * grid.CellLength
			);
		}

		// draw paths
		foreach(var path in paths){
			Gizmos.color = Color.red;
			for(int i = 0; i < path.Count - 1; ++i){
				var start = path[i];
				var end = path[i + 1];
				Gizmos.DrawLine(
					transform.position + new Vector3(start.x + 0.5f, 0, start.y + 0.5f) * grid.CellLength,
					transform.position + new Vector3(end.x + 0.5f, 0, end.y + 0.5f) * grid.CellLength
				);
			}
		}
	}
}