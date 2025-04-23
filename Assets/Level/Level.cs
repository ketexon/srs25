using System.Collections.Generic;
using Kutie.Extensions;
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
            level.UpdateSerializedObject();
		}

		if (GUILayout.Button("Clear"))
		{
			level.Clear();
            level.UpdateSerializedObject();
		}
	}
}
#endif

public class Level : MonoBehaviour {
    [SerializeField] public Transform PlayerSpawn;
	[SerializeField] public List<LevelGenerator> LevelGenerator;
	[SerializeField] public EntityMovement Player;
	[SerializeField]
	[Tooltip("Seed for the level generator. If negative, a random seed will be used.")]
	public int Seed;

	void Start(){
		Generate();
		Player.Teleport(PlayerSpawn.position);
	}

	public void Generate()
	{
		var seed = Seed < 0
			? Random.Range(0, int.MaxValue)
			: Seed;
        foreach(var generator in LevelGenerator)
        {
            generator.Generate(seed);
        }
	}

	public void Clear()
	{
        foreach(var generator in LevelGenerator)
        {
            generator.Clear();
        }
	}
    
    #if UNITY_EDITOR
    public void UpdateSerializedObject()
    {
        foreach(var generator in LevelGenerator)
        {
            var serializedObject = new SerializedObject(generator);
            serializedObject.Update();
        }
    }
    #endif
}