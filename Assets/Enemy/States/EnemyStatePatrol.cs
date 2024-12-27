using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemyStatePatrol : EnemyAIState {
	[SerializeField] public List<Transform> PatrolPoints;
	[SerializeField] float idleTime = 2.0f;

	int pointIndex = 0;

	bool idling = false;
	float idleStartTime;

	override protected void Start(){
		base.Start();
		PatrolPoints.ForEach(p => p.SetParent(null));
	}

	override public void Enter(){
		base.Enter();
		Controller.AINavigation.enabled = true;
		Controller.AINavigation.SetDestination(PatrolPoints[pointIndex].position);

		Vision.NewHitBoxVisibleEvent.AddListener(OnHitBoxVisible);
	}

	void Update(){
		if(idling){
			if(Time.time > idleTime + idleStartTime){
				StopIdle();
			}
		}
		else if(Controller.AINavigation.HasReachedDestination){
			OnReachedDestination();
		}
	}

	void OnReachedDestination(){
		idling = true;
		idleStartTime = Time.time;
	}

	void StopIdle(){
		idling = false;
		pointIndex = (pointIndex + 1) % PatrolPoints.Count;
		Controller.AINavigation.SetDestination(PatrolPoints[pointIndex].position);
	}

	override public void Exit(){
		base.Exit();
		Controller.AINavigation.enabled = false;
		Vision.NewHitBoxVisibleEvent.RemoveListener(OnHitBoxVisible);
	}

	void OnHitBoxVisible(EntityHitBox hb){
		Controller.TransitionTo<EnemyStateAggro>();
	}
}