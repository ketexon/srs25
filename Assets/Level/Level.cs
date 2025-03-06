using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor {
	public override void OnInspectorGUI(){
		DrawDefaultInspector();
		if(GUILayout.Button("Generate")){
			((Level)target).LevelGenerator.Generate();
		}
	}
}
#endif

public class Level : MonoBehaviour {
	[SerializeField] public LevelGenerator LevelGenerator;
	[SerializeField] public EntityMovement Player;

	void Start(){
		LevelGenerator.Generate();
		Player.Teleport(LevelGenerator.GetPlayerSpawnPoint());
		Debug.Log(LevelGenerator.GetPlayerSpawnPoint());
	}
}