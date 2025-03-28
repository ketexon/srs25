using UnityEngine;

public abstract class LevelGenerator : MonoBehaviour {
	public abstract void Generate(int seed);
	public abstract Vector3 GetPlayerSpawnPoint();
}