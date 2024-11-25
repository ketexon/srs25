using UnityEngine;

public class EnemyState : MonoBehaviour {
	[System.NonSerialized] public EnemyAIController Controller;

	// Utility properties
	protected EnemyVisionCone Vision => Controller.Vision;
	protected Entity Entity => Controller.Entity;
	protected Gun Gun => Entity.Gun;
	protected HumanModel HumanModel => Entity.HumanModel;

	public virtual void Enter(){
		enabled = true;
	}

	public virtual void Exit(){
		enabled = false;
	}
}