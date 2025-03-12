using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class LevelCell<T> {
	[SerializeField]
	public T Type;

	[SerializeField]
	public Color ColorOverride = Color.clear;

	[SerializeField]
	public DirectionMask DoorDirections;

	public virtual Color DebugColor => Color.white;
}

public class LevelGridBase<TCellType, TCell> : MonoBehaviour
	where TCell : LevelCell<TCellType>, new()
{
	public TCellType DefaultCellType = default;
	public Vector2Int Size = new(20, 20);
	public float CellLength = 3;
	[SerializeField, HideInInspector] List<TCell> grid = new();

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
			grid.Add(new TCell() {
				Type = DefaultCellType
			});
		}
	}

	int GetCellIndex(Vector2Int cell) => GetCellIndex(cell.x, cell.y);
	int GetCellIndex(int x, int y) => x + y * Size.x;
	public bool CellInBounds(Vector2Int cell) => CellInBounds(cell.x, cell.y);

	public bool CellInBounds(int x, int y) => (
		x >= 0 && x < Size.x
		&& y >= 0 && y < Size.y
	);
	bool CellInBounds(int idx) => idx < grid.Count && idx >= 0;

	public TCellType GetCellType(int x, int y) => GetCellType(new Vector2Int(x, y));

	public TCell GetCell(Vector2Int cell) {
		return grid[GetCellIndex(cell)];
	}

	public TCell GetCell(int x, int y) {
		return GetCell(new Vector2Int(x, y));
	}

	public TCellType GetCellType(Vector2Int cell) {
		return GetCell(cell).Type;
	}

	public bool SetCellType(int x, int y, TCellType type) {
		return SetCellType(new Vector2Int(x, y), type);
	}

	public bool SetCellType(Vector2Int cell, TCellType type) {
		var i = GetCellIndex(cell);
		if(CellInBounds(i)){
			grid[i].Type = type;
			return true;
		}
		return false;
	}

	public bool SetCell(int x, int y, TCell cell) {
		return SetCell(new Vector2Int(x, y), cell);
	}

	public bool SetCell(Vector2Int cell, TCell newCell) {
		var i = GetCellIndex(cell);
		if(CellInBounds(i)){
			grid[i] = newCell;
			return true;
		}
		return false;
	}

	public Vector2Int AlignmentOffsetToCell(
		Alignment2D alignment,
		Vector2Int offset
	) => AlignmentOffsetRectToCell(alignment, offset, Vector2Int.one);

	public Vector2Int AlignmentOffsetRectToCell(
		Alignment2D alignment,
		Vector2Int offset,
		Vector2Int size
	) => alignment switch {
		Alignment2D.BottomLeft => new Vector2Int(0, 0),
		Alignment2D.BottomCenter => new Vector2Int(Size.x / 2 - size.x / 2, 0),
		Alignment2D.BottomRight => new Vector2Int(Size.x - size.x, 0),
		Alignment2D.MiddleLeft => new Vector2Int(0, Size.y / 2 - size.y / 2),
		Alignment2D.MiddleCenter => new Vector2Int(Size.x / 2 - size.x / 2, Size.y / 2 - size.y / 2),
		Alignment2D.MiddleRight => new Vector2Int(Size.x - size.x, Size.y / 2 - size.y / 2),
		Alignment2D.TopLeft => new Vector2Int(0, Size.y - size.y),
		Alignment2D.TopCenter => new Vector2Int(Size.x / 2 - size.x / 2, Size.y - size.y),
		Alignment2D.TopRight => new Vector2Int(Size.x - size.x, Size.y - size.y),
		_ => throw new System.NotImplementedException(),
	} + offset;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
		// draw bounds
		Gizmos.DrawWireCube(
			transform.position
			+ new Vector3(Size.x * CellLength / 2, 0, Size.y * CellLength / 2),
			new Vector3(Size.x * CellLength, 0, Size.y * CellLength)
		);

		for (int x = 0; x < Size.x; x++)
		{
			for (int y = 0; y < Size.y; y++)
			{
				var coord = new Vector2Int(x, y);
				var cell = GetCell(coord);
				if(cell == null) continue;

				Vector3 center = transform.position
					+ new Vector3(
						x * CellLength + CellLength / 2,
						0,
						y * CellLength + CellLength / 2
					);

				Gizmos.color = cell.ColorOverride.a > 0
					? cell.ColorOverride
					: cell.DebugColor;
				Gizmos.DrawCube(
					center,
					new Vector3(CellLength, 0, CellLength) / 4
				);
			}
		}
    }
}
