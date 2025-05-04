using System.Collections.Generic;
using UnityEngine;

public class EnemyStateAggro : EnemyAIState {
	[SerializeField] List<BodyPart> bodyPartPreferences;
	[SerializeField] float maxRecoil = 1f;
	[SerializeField] float minRecoil = 0.1f;

	readonly Dictionary<BodyPart, int> bodyPartPreferenceMap = new();

	EntityHitBox target;

	bool recoilCooldown = false;

	override protected void Awake(){
		base.Awake();
		for (int i = 0; i < bodyPartPreferences.Count; i++){
			bodyPartPreferenceMap[bodyPartPreferences[i]] = i;
		}
	}

	override protected void Start(){
		base.Start();
		HumanModel.DeathEvent.AddListener(OnDeath);
		Gun.ShootStraight = true;
	}

	override public void Enter(){
		base.Enter();
		recoilCooldown = false;
		Vision.NewHitBoxVisibleEvent.AddListener(OnHitBoxVisible);
		Vision.HitBoxInvisibleEvent.AddListener(OnHitBoxInvisible);
		ChooseNewTarget();
	}

	override public void Exit(){
		base.Exit();
		Gun.Shooting = false;
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

		if(target == null){
			Controller.TransitionTo<EnemyStatePatrol>();
		}
	}

	void Update(){
		if(target){
			Entity.Movement.LookAt(target.transform.position);
			Entity.Gun.vTargetRotation = Entity.Movement.Pitch;
			Vision.transform.localRotation = Quaternion.AngleAxis(Entity.Movement.Pitch, Vector3.right);

			if(recoilCooldown){
				if(Entity.Gun.CurrentRecoil < minRecoil){
					Entity.Gun.Shooting = true;
					recoilCooldown = false;
				}
			}
			else {
				if(!Entity.Gun.Shooting){
					Entity.Gun.Shooting = true;
				}
				if(Entity.Gun.CurrentRecoil > maxRecoil){
					Entity.Gun.Shooting = false;
					recoilCooldown = true;
				}
			}
		}
		else {
			Entity.Gun.Shooting = false;
		}
	}
}