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
	Up = 1,
	Down = 2,
	Left = 4,
	Right = 8,
}