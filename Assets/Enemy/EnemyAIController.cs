using System.Collections.Generic;
using UnityEngine;

public class EnemyAIController : MonoBehaviour {
	[SerializeField] public EnemyVisionCone Vision;
	[SerializeField] public Entity Entity;
	[SerializeField] EnemyState defaultState;

	[System.NonSerialized] public EnemyState ActiveState;

	void Reset(){
		Vision = GetComponentInChildren<EnemyVisionCone>();
		Entity = GetComponent<Entity>();
	}

	void Start(){
		TransitionTo(defaultState);
	}

	public void TransitionTo<T>() where T : EnemyState {
		TransitionTo(GetComponent<T>());
	}

	public void TransitionTo(EnemyState state){
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
}