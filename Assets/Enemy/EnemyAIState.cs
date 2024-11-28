using UnityEngine;

public class EnemyAIState : MonoBehaviour {
	[System.NonSerialized] public EnemyAIController Controller;

	// Utility properties
	protected EnemyVisionCone Vision => Controller.Vision;
	protected Entity Entity => Controller.Entity;
	protected Gun Gun => Entity.Gun;
	protected HumanModel HumanModel => Entity.HumanModel;

	virtual protected void Awake(){
		enabled = false;
	}

	virtual protected void Start(){
		HumanModel.DeathEvent.AddListener(OnDeath);
	}

	public virtual void Enter(){
		enabled = true;
	}

	public virtual void Exit(){
		enabled = false;
	}

	virtual protected void OnDeath(){
		if(enabled){
			Controller.Stop();
		}
	}
}