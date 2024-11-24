using UnityEngine;

public class BasicEnemyAI : MonoBehaviour {
	[SerializeField] EnemyVision enemyVision;

	void Reset(){
		enemyVision = GetComponentInChildren<EnemyVision>();
	}
}