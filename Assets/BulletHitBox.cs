using UnityEngine;

public class BulletHitBox : MonoBehaviour {
	[SerializeField] public float EnergyPenalty = 1;

	public virtual void OnHit(float damage) {}
}