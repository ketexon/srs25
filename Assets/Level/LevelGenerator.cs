using UnityEngine;

public abstract class LevelGenerator : MonoBehaviour {
	public abstract void Generate(int seed);
	public abstract void Clear();
	public abstract Vector3 GetPlayerSpawnPoint();
}