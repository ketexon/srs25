using System.Numerics;
using UnityEngine;
using Kutie.Extensions;

[System.Serializable]
public enum Alignment {
	Left,
	Center,
	Right,
}

[System.Serializable]
public enum Alignment2D {
	TopLeft,
	TopCenter,
	TopRight,
	MiddleLeft,
	MiddleCenter,
	MiddleRight,
	BottomLeft,
	BottomCenter,
	BottomRight,
}

[System.Serializable, System.Flags]
public enum DirectionMask {
	None = 0,
	Right = 1,
	Up = 2,
	Left = 4,
	Down = 8,
	All = Right | Up | Left | Down,
}

public static partial class Util {
	public static DirectionMask Rotate90(this DirectionMask mask, int count) {
		count = (count % 4 + 4) % 4;
		if (count == 0) return mask;
		if(mask == DirectionMask.None) return DirectionMask.None;
		var maskInt = (int)mask;
		maskInt <<= count;
		if (maskInt > (int)DirectionMask.All) {
			maskInt |= maskInt / 16;
			maskInt &= (int)DirectionMask.All;
		}
		return (DirectionMask)maskInt;
	}

	public static DirectionMask ToDirection(this Vector2Int dir){
		return dir switch {
			(1, 0) => DirectionMask.Right,
			(0, 1) => DirectionMask.Up,
			(-1, 0) => DirectionMask.Left,
			(0, -1) => DirectionMask.Down,
			_ => DirectionMask.None,
		};
	}

	public static Vector2Int ToVector2Int(this DirectionMask dir){
		return dir switch {
			DirectionMask.Right => new(1, 0),
			DirectionMask.Up => new(0, 1),
			DirectionMask.Left => new(-1, 0),
			DirectionMask.Down => new(0, -1),
			_ => Vector2Int.zero,
		};
	}

	public static Vector2Int Rotate90(this Vector2Int v, int count) {
		count = (count % 4 + 4) % 4;
		if (count == 0) return v;
		var x = v.x;
		var y = v.y;
		for (int i = 0; i < count; i++) {
			(x,y) = (-y, x);
		}
		return new(x, y);
	}
}