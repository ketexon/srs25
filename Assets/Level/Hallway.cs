using System.Collections.Generic;
using Kutie;
using Kutie.Inspector;
using UnityEngine;
public class Hallway : MonoBehaviour
{
    [SerializeField, HideInInspector] public LevelGenerator4 LevelGenerator;
    [SerializeField, ReadOnly] public Vector2Int StartCell;
    [SerializeField] GameObject hallwaySegmentPrefab;

    LevelGrid Grid => LevelGenerator.Grid;

    public void Generate(){
        HashSet<Vector2Int> visited = new() {
            StartCell
        };

        Vector2Int currentCell = StartCell;
        Queue<Vector2Int> queue = new();

        queue.Enqueue(currentCell);

        while(queue.Count > 0){
            currentCell = queue.Dequeue();
            SpawnHallwaySegment(currentCell);

            foreach(var dir in KMath.Directions4){
                var neighbor = currentCell + dir;
                if(!Grid.CellInBounds(neighbor) || visited.Contains(neighbor)){
                    continue;
                }

                if(Grid.GetCellType(neighbor) == CellType.Pathway){
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }
    }

    void SpawnHallwaySegment(Vector2Int cell){
        var pos = Grid.CellToWorld(cell + new Vector2(0.5f, 0.5f));
        KPrefabUtility.InstantiatePrefab(
            hallwaySegmentPrefab,
            pos,
            Quaternion.identity,
            transform
        );
    }
}
