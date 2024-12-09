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
public class LevelDoor2 {
	public Vector2Int Position;
	public Vector2Int Face;
	public bool Required;
}

[System.Serializable]
public class LevelGridEntry2 {
	public Vector3Int Size;
	public Vector3 BottomLeftCornerOffset;
	public List<LevelDoor> Doors;
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

	[SerializeField] Vector2 cellSize = new(2, 2);
	[SerializeField] Vector2Int gridSize = new(20, 20);
	[SerializeField] Kutie.IntRange roomCount = new(10, 20);
	[SerializeField] Kutie.IntRange roomLength = new(4, 8);
	[SerializeField] int roomMargin = 1;
	// these are the areas in which rooms will be spawned
	// these are not necessarily the exact size of rooms,
	// but just upper bounds
	[SerializeField] List<RectInt> allSpacePartitions = new();
	[SerializeField] List<RectInt> spacePartitions = new();
	[SerializeField] List<Room> spawnedRooms = new();
	[SerializeField] int maxIters = 2;

	// [Header("Prefabs")]
	// [SerializeField] List<GameObject> roomPrefabs = new();

	// [SerializeField, HideInInspector]
	// List<CellType> grid = new();

	// [SerializeField, HideInInspector]
	// List<Vector3Int> doorCells = new();

	// int GetGridIndex(Vector3Int position){
	// 	return position.x + position.y * gridSize.x + position.z * gridSize.x * gridSize.y;
	// }

	// bool InBounds(Vector3Int position){
	// 	var idx = GetGridIndex(position);
	// 	return idx >= 0 && idx < grid.Count;
	// }

	// CellType GridGet(Vector3Int position){
	// 	var idx = GetGridIndex(position);
	// 	if(idx < 0 || idx >= grid.Count){
	// 		return CellType.OutOfBounds;
	// 	}
	// 	return grid[GetGridIndex(position)];
	// }

	// void GridSet(Vector3Int position, CellType value){
	// 	grid[GetGridIndex(position)] = value;
	// }

	void Start() {
		PartitionSpace(new RectInt(Vector2Int.zero, gridSize));
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
		// spawnedRooms.Clear();

		// grid = new(gridSize.Volume());
		// for(int i = 0; i < gridSize.Volume(); ++i){
		// 	grid.Add(CellType.Empty);
		// }

		// doorCells.Clear();

		PartitionSpace(new RectInt(Vector2Int.zero, gridSize));
		// SpawnRooms();
		// GeneratePathways();
	}

	void PartitionSpace(RectInt bounds){
		spacePartitions.Clear();
		allSpacePartitions.Clear();

		Queue<RectInt> queue = new();
		// add padding on all sides
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
				var splitPoint = Random.Range(roomLength.Min, top.size.x - roomLength.Min);
				// var splitPoint = MinRoomLength;
				a = new RectInt(
					top.min,
					new Vector2Int(splitPoint, top.size.y)
				);
				b = new RectInt(
					top.min + (splitPoint + 1) * Vector2Int.right,
					new Vector2Int(top.size.x - splitPoint - 1, top.size.y)
				);
			}
			// y
			else {
				var splitPoint = Random.Range(roomLength.Min, top.size.y - roomLength.Min);
				// var splitPoint = MinRoomLength;
				a = new RectInt(
					top.min,
					new Vector2Int(top.size.x, splitPoint)
				);
				b = new RectInt(
					top.min + (splitPoint + 1) * Vector2Int.up,
					new Vector2Int(top.size.x, top.size.y - splitPoint - 1)
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

	void OnDrawGizmos(){
		foreach(var partition in allSpacePartitions){
			if(spacePartitions.Contains(partition)){
				Gizmos.color = Color.green.WithA(0.1f);
			}
			else {
				Gizmos.color = Color.red.WithA(0.1f);
			}
			Gizmos.DrawWireCube(
				new Vector3(partition.center.x, 0, partition.center.y),
				new Vector3(partition.size.x, 0, partition.size.y)
			);
			// draw center
			Gizmos.color = Gizmos.color.WithA(0.4f);
			Gizmos.DrawCube(
				new Vector3(partition.center.x, 0, partition.center.y),
				Vector3.one * 0.1f
			);
		}
	}
}