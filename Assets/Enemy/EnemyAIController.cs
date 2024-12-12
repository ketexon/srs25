using System.Collections.Generic;
using UnityEngine;

public class EnemyAIController : MonoBehaviour {
	[SerializeField] public EnemyVisionCone Vision;
	[SerializeField] public Entity Entity;
	[SerializeField] public EntityAINavigation AINavigation;
	[SerializeField] EnemyAIState defaultState;

	[System.NonSerialized] public EnemyAIState ActiveState;

	void Reset(){
		Vision = GetComponentInChildren<EnemyVisionCone>();
		Entity = GetComponent<Entity>();
	}

	void Start(){
		TransitionTo(defaultState);
	}

	public void TransitionTo<T>() where T : EnemyAIState {
		TransitionTo(GetComponent<T>());
	}

	public void TransitionTo(EnemyAIState state){
		if(ActiveState){
			ActiveState.Exit();
		}
		ActiveState = state;
		if(ActiveState){
			ActiveState.Controller = this;
			ActiveState.Enter();
		}
	}

	public void Exit(){
		TransitionTo(defaultState);
	}

	public void Stop(){
		TransitionTo(null);
	}
}