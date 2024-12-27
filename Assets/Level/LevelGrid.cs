using System.Collections.Generic;
using Kutie.Extensions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public enum CellType {
	Empty,
	Room,
	Pathway,
	Door,
	LockedDoor,
	OutOfBounds = -1,
};

public class LevelGrid : MonoBehaviour {
	public Vector2Int Size = new(20, 20);
	public Vector2 CellSize = new(2, 2);
	[SerializeField, HideInInspector] List<CellType> grid = new();

	void Reset(){
		Clear();
	}

	void OnValidate(){
		if(grid.Count != Size.x * Size.y){
			Clear();
		}
	}

	public void Clear(){
		grid.Clear();
		grid.Capacity = Size.x * Size.y;
		for(int i = 0; i < Size.x * Size.y; i++){
			grid.Add(CellType.Empty);
		}
	}

	int GetCellIndex(Vector2Int cell) => cell.x + cell.y * Size.x;
	public bool CellInBounds(Vector2Int cell) => (
		cell.x >= 0 && cell.x < Size.x
		&& cell.y >= 0 && cell.y < Size.y
	);
	bool CellInBounds(int idx) => idx < grid.Count && idx >= 0;

	public CellType GetCellType(int x, int y) => GetCellType(new Vector2Int(x, y));

	public CellType GetCellType(Vector2Int cell) {
		if(CellInBounds(cell)){
			return grid[GetCellIndex(cell)];
		}
		return CellType.OutOfBounds;
	}

	public bool SetCellType(Vector2Int cell, CellType type) {
		var i = GetCellIndex(cell);
		if(CellInBounds(i)){
			grid[i] = type;
			return true;
		}
		return false;
	}

	public Vector3 SizeToWorld(Vector2Int size) => (
		new Vector2(size.x, size.y)
			.Hammard(CellSize)
			.WithZ(0)
			.XZY()
	);

	public Vector3 SizeToWorld(Vector2 size) => (
		new Vector2(size.x, size.y)
			.Hammard(CellSize)
			.WithZ(0)
			.XZY()
	);

	public Vector3 CellToWorld(Vector2Int cell) => (
		new Vector3(cell.x, cell.y, 0)
			.Hammard(CellSize).XZY()
	);

	public Vector3 CellToWorld(Vector2 cell) => (
		new Vector3(cell.x, cell.y, 0)
			.Hammard(CellSize).XZY()
	);


	void OnDrawGizmos(){
#if UNITY_EDITOR
		// draw grid
		Gizmos.color = Color.white.WithA(0.01f);
		for(int x = 0; x < Size.x; x++){
			for(int y = 0; y < Size.y; y++){
				Gizmos.DrawWireCube(
					new Vector3(x + 0.5f, y + 0.5f, 0)
						.Hammard(CellSize).WithZ(0).XZY(),
					CellSize.WithZ(0).XZY()
				);
			}
		}

		// draw grid cells
		var sceneViewSize = SceneView.currentDrawingSceneView.size;
		var sceneViewSizeProp = sceneViewSize / CellSize.magnitude;
		var sceneCamera = SceneView.currentDrawingSceneView.camera;
		var isOrtho = sceneCamera.orthographic;

		for(int x = 0; x < Size.x; ++x){
			for(int y = 0; y < Size.y; ++y){
				var content = GetCellType(x, y);
				if(content == CellType.Room){
					Gizmos.color = Color.red.WithA(0.1f);
				}
				else if(content == CellType.Door){
					Gizmos.color = Color.red.WithA(0.3f);
				}
				else if(content == CellType.Pathway){
					Gizmos.color = Color.green.WithA(0.5f);
				}
				else {
					Gizmos.color = Color.white.WithA(0.1f);
				}

				var topLeft = (
					new Vector3(x, y + 1, 0)
					.Hammard(CellSize).XZY()
				);

				var center = (
					new Vector3(x + 0.5f, y + 0.5f, 0)
					.Hammard(CellSize).XZY()
				);

				Gizmos.DrawCube(
					center,
					Vector3.one.Hammard(CellSize).WithZ(0).XZY() * 0.25f
				);

				if (isOrtho && sceneViewSizeProp < 3) {
					var viewportPoint = sceneCamera.WorldToViewportPoint(topLeft);
					if(
						viewportPoint.x > 0 && viewportPoint.x < 1
						&& viewportPoint.y > 0 && viewportPoint.y < 1
						&& viewportPoint.z > 0
					){
						Handles.Label(
							topLeft,
							$"{x},{y}",
							new GUIStyle(){
								fontSize = 8,
								normal = new GUIStyleState(){
									textColor = Color.white
								}
							}
						);
					}
				}
			}
		}
#endif
	}
}