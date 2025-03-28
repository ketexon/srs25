using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor {
	public override void OnInspectorGUI(){
		var level = (Level)target;
		DrawDefaultInspector();
		if(GUILayout.Button("Generate")){
			level.Generate();
			var levelGeneratorSO = new SerializedObject(((Level)target).LevelGenerator);
			levelGeneratorSO.Update();
		}
	}
}
#endif

public class Level : MonoBehaviour {
	[SerializeField] public LevelGenerator LevelGenerator;
	[SerializeField] public EntityMovement Player;
	[SerializeField]
	[Tooltip("Seed for the level generator. If negative, a random seed will be used.")]
	public int Seed;

	void Start(){
		Generate();
		Player.Teleport(LevelGenerator.GetPlayerSpawnPoint());
		// Debug.Log(LevelGenerator.GetPlayerSpawnPoint());
	}

	public void Generate()
	{
		var seed = Seed < 0
			? Random.Range(0, int.MaxValue)
			: Seed;
		LevelGenerator.Generate(seed);
	}
}