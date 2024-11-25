using System.Collections.Generic;
using UnityEngine;

public class EnemyStateAggro : EnemyState {
	[SerializeField] List<BodyPart> bodyPartPreferences;

	readonly Dictionary<BodyPart, int> bodyPartPreferenceMap = new();

	EntityHitBox target;

	void Awake(){
		for (int i = 0; i < bodyPartPreferences.Count; i++){
			bodyPartPreferenceMap[bodyPartPreferences[i]] = i;
		}
	}

	void Start(){
		HumanModel.DeathEvent.AddListener(OnDeath);
		Gun.ShootStraight = true;
	}

	override public void Enter(){
		base.Enter();
		Vision.NewHitBoxVisibleEvent.AddListener(OnHitBoxVisible);
		Vision.HitBoxInvisibleEvent.AddListener(OnHitBoxInvisible);
		ChooseNewTarget();
	}

	override public void Exit(){
		base.Exit();
		Vision.NewHitBoxVisibleEvent.RemoveListener(OnHitBoxVisible);
		Vision.HitBoxInvisibleEvent.RemoveListener(OnHitBoxInvisible);
	}

	void OnHitBoxVisible(EntityHitBox hb){
		if(target == null || Prefers(hb.BodyPart, target.BodyPart)){
			target = hb;
		}
	}

	void OnHitBoxInvisible(EntityHitBox hb){
		if(target == hb){
			target = null;
			ChooseNewTarget();
		}
	}

	bool Prefers(BodyPart a, BodyPart to){
		return bodyPartPreferenceMap[a] < bodyPartPreferenceMap[to];
	}

	void ChooseNewTarget(){
		foreach(var hb in Controller.Vision.VisibleHitBoxes){
			Debug.Log(hb);
			if(target == null || Prefers(hb.BodyPart, target.BodyPart)){
				target = hb;
			}
		}
	}

	void Update(){
		if(target){
			Entity.Movement.LookAt(target.transform.position);
			Entity.Gun.TargetRotation = Entity.Movement.Pitch;
			Vision.transform.localRotation = Quaternion.AngleAxis(Entity.Movement.Pitch, Vector3.right);

			Entity.Gun.Shooting = true;
		}
		else {
			Entity.Gun.Shooting = false;
		}
	}

	void OnDeath(){
		enabled = false;
	}
}