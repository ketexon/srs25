using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemyStatePatrol : EnemyAIState {
	[SerializeField] List<Transform> patrolPoints;
	[SerializeField] float idleTime = 2.0f;

	int pointIndex = 0;

	bool idling = false;
	float idleStartTime;

	override public void Enter(){
		base.Enter();
		Controller.AINavigation.enabled = true;
		Controller.AINavigation.SetDestination(patrolPoints[pointIndex].position);

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
		pointIndex = (pointIndex + 1) % patrolPoints.Count;
		Controller.AINavigation.SetDestination(patrolPoints[pointIndex].position);
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