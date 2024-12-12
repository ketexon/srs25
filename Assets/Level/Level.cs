using UnityEngine;

public class Level : MonoBehaviour {
	[SerializeField] public LevelGenerator4 LevelGenerator;
	[SerializeField] public EntityMovement Player;

	void Start(){
		LevelGenerator.Generate();
		Player.Teleport(LevelGenerator.GetPlayerSpawnPoint());
		Debug.Log(LevelGenerator.GetPlayerSpawnPoint());
	}
}