using UnityEngine;

public abstract class LevelGenerator : MonoBehaviour {
	public abstract void Generate();
	public abstract Vector3 GetPlayerSpawnPoint();
}