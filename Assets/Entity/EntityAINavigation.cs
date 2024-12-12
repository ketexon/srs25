using UnityEngine;
using UnityEngine.AI;

public class EntityAINavigation : MonoBehaviour {
	[SerializeField] Entity entity;
	[SerializeField] NavMeshAgent agent;

	public bool HasReachedDestination => !agent.pathPending && agent.remainingDistance < 0.1f;

	void Start(){
		agent.transform.SetParent(null);
		agent.gameObject.name = $"{entity.name} NavMeshAgent";
		agent.speed = entity.Movement.MovementSpeed;
	}

	void OnDisable(){
		if(agent){
			ResetPath();
		}
	}

	public void SetDestination(Vector3 destination){
		agent.SetDestination(destination);
	}

	public void ResetPath(){
		agent.ResetPath();
	}

	void Update(){
		entity.Movement.LookDir(agent.transform.forward);
		entity.transform.position = agent.transform.position;
	}
}