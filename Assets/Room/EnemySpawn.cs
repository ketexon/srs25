using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour {
	[SerializeField] public float Chance = 0.5f;
	[SerializeField] public EnemyTypeSO EnemyType;
	[SerializeField] List<Transform> patrolPoints;

	public bool Spawn(out Entity entity){
		if(Random.value < Chance){
			var enemyGO = Instantiate(EnemyType.EnemyPrefabs[Random.Range(0, EnemyType.EnemyPrefabs.Count)], transform.position, Quaternion.identity);
			var enemyPatrol = enemyGO.GetComponent<EnemyStatePatrol>();
			enemyPatrol.PatrolPoints = new(patrolPoints);

			entity = enemyGO.GetComponent<Entity>();
			return true;
		}
		entity = null;
		return false;
	}
}