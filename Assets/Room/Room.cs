using System.Collections.Generic;
using UnityEngine;
using Kutie.Extensions;
using Kutie.Inspector;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class Room : MonoBehaviour
{
    [SerializeField] public LevelGridEntry2 LevelEntry;
    [SerializeField] public BoxCollider Bounds;
    [SerializeField] public List<Transform> Entrances;
    [SerializeField] public List<EnemySpawn> enemySpawns;

    [SerializeField, ReadOnly] public bool IsStartRoom = false;

    [System.NonSerialized] public Dictionary<Transform, Room> ConnectedRooms = new();
    [System.NonSerialized] public List<Transform> UnconnectedEntrances = new();

    [System.NonSerialized] public List<Entity> Enemies = new();

    void Awake(){
        UnconnectedEntrances = new(Entrances);
    }

    void Reset(){
#if UNITY_EDITOR
        var boundsGO = transform.Find("Bounds");
        if(boundsGO){
            Bounds = boundsGO.GetComponent<BoxCollider>();
        }
        if(Bounds is BoxCollider boxCollider){
            boxCollider.center = Vector3.zero;
            boxCollider.size = Vector3.zero;

            Bounds colliderBounds = new();
            bool hasCollider = false;
            var colliders = GetComponentsInChildren<Collider>();
            foreach(var collider in colliders){
                if(collider == Bounds){
                    continue;
                }
                if(!hasCollider){
                    colliderBounds = collider.bounds;
                    hasCollider = true;
                }
                else {
                    colliderBounds.Encapsulate(collider.bounds);
                }
            }

            Bounds rendererBounds = new();
            bool hasRenderer = false;
            var renderers = GetComponentsInChildren<Collider>();
            if (renderers.Length > 0){
                rendererBounds = renderers[0].bounds;
                hasRenderer = true;
                for (int i = 1; i < renderers.Length; i++){
                    rendererBounds.Encapsulate(renderers[i].bounds);
                }
            }

            Bounds bounds = new();
            if (hasCollider && hasRenderer){
                bounds = colliderBounds;
                bounds.Encapsulate(rendererBounds);
            }
            else if (hasCollider){
                bounds = colliderBounds;
            }
            else if (hasRenderer){
                bounds = rendererBounds;
            }

            // note: we cannot use boxCollider.size or boxCollider.center
            // as then it won't be serialized
            var boxColliderSerializedObject = new SerializedObject(boxCollider);
            var sizeProp = boxColliderSerializedObject.FindProperty("m_Size");
            var centerProp = boxColliderSerializedObject.FindProperty("m_Center");
            centerProp.vector3Value = bounds.center - transform.position;
            sizeProp.vector3Value = bounds.size * 0.95f;
            boxColliderSerializedObject.ApplyModifiedProperties();

            LevelEntry.Size = bounds.size;
            LevelEntry.BottomLeftCornerOffset = bounds.min;
            LevelEntry.BottomLeftCornerOffset.y = 0;
        }

        Entrances = new();
        foreach(Transform child in transform){
            if(child.name.Contains("Entrance")){
                Entrances.Add(child);
            }
        }

        LevelEntry.Doors = new();
        foreach(var entrance in Entrances){
            LevelEntry.Doors.Add(new LevelDoor2(){
                Transform = entrance,
                Required = entrance.name.Contains("Required"),
            });
        }
#endif

        enemySpawns = new(GetComponentsInChildren<EnemySpawn>());
    }

    public void Connect(Room room, Transform entrance){
        ConnectedRooms[entrance] = room;
        UnconnectedEntrances.Remove(entrance);
    }

    void Start(){
        if(!IsStartRoom){
            SpawnEnemies();
        }
    }

    void SpawnEnemies(){
        foreach(var spawn in enemySpawns){
            if(spawn.Spawn(out var entity)) {
                Enemies.Add(entity);
            }
        }
    }
}
