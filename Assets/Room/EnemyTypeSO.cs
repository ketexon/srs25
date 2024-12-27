using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Enemy Type", order = 1)]
public class EnemyTypeSO : ScriptableObject
{
	[SerializeField] public List<GameObject> EnemyPrefabs;
}