public class EnemyStateIdle : EnemyState {
	override public void Enter(){
		Vision.NewHitBoxVisibleEvent.AddListener(OnHitBoxVisible);
	}

	override public void Exit(){
		Vision.NewHitBoxVisibleEvent.RemoveListener(OnHitBoxVisible);
	}

	void OnHitBoxVisible(EntityHitBox _){
		Controller.TransitionTo<EnemyStateAggro>();
	}
}