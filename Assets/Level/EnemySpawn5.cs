using System.ComponentModel.Design.Serialization;
using Kutie.Extensions;
using UnityEngine;

public class EnemySpawn5 : MonoBehaviour
{
    [SerializeField] private EnemyTypeSO enemyType;
    [SerializeField] private float spawnChance = 0.5f;
    [SerializeField] private LayerMask playerLayerMask;

    private void OnTriggerEnter(Collider other)
    {
        if (!playerLayerMask.Contains(other.gameObject.layer)) return;

        Spawn();
        Destroy(gameObject);
    }

    private void Spawn()
    {
        if (!(Random.value < spawnChance)) return;
        Debug.Log("Spawning enemy");
        var enemyPrefab = enemyType.EnemyPrefabs.Sample();
        var enemyGO = Instantiate(enemyPrefab);
        
        enemyGO.GetComponent<EntityMovement>().Teleport(transform.position);
    }
}
