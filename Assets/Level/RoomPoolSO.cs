using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RoomPoolSO : ScriptableObject {
	[SerializeField] public List<GameObject> Rooms;
}