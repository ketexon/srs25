using UnityEngine;

[System.Serializable]
public enum CellType5 {
	Empty,
	Occupied,
	Blocked,
	Fixed,
}

[System.Serializable]
public class LevelCell5 : LevelCell<CellType5> {
	public int RoomIndex = -1;

	public override Color DebugColor => Type switch {
		CellType5.Empty => Color.white,
		CellType5.Occupied => Color.blue,
		CellType5.Blocked => Color.red,
		_ => Color.black,
	};
}

public class LevelGrid5 : LevelGridBase<CellType5, LevelCell5> {}
