using System.Collections.Generic;
using Kutie.Extensions;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using Unity.AI.Navigation.Editor;
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
	[SerializeField] public List<LevelGenerator> LevelGenerators;
	[SerializeField] public EntityMovement Player;
	[SerializeField]
	[Tooltip("Seed for the level generator. If negative, a random seed will be used.")]
	public int Seed;
	[SerializeField] NavMeshSurface surface;

	private void Awake()
	{
		Clear();
	}

	void Start(){
		Generate();
		BuildNavMesh();
		Player.Teleport(PlayerSpawn.position);
	}

	public void Generate()
	{
		var seed = Seed < 0
			? Random.Range(0, int.MaxValue)
			: Seed;
        foreach(var generator in LevelGenerators)
        {
            generator.Generate(seed);
        }
	}

	public void Clear()
	{
        foreach(var generator in LevelGenerators)
        {
            generator.Clear();
        }
	}

	void BuildNavMesh(){
#if UNITY_EDITOR
		NavMeshAssetManager.instance.StartBakingSurfaces(new Object[]{ surface });
#else
		surface.AddData();
        surface.UpdateNavMesh(surface.navMeshData);
        surface.BuildNavMesh();
#endif
	}
    
    #if UNITY_EDITOR
    public void UpdateSerializedObject()
    {
        foreach(var generator in LevelGenerators)
        {
            var serializedObject = new SerializedObject(generator);
            serializedObject.Update();
        }
    }
    #endif
}