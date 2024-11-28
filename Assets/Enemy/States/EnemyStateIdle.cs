public class EnemyStateIdle : EnemyAIState {
	override public void Enter(){
		base.Enter();
		Vision.NewHitBoxVisibleEvent.AddListener(OnHitBoxVisible);
	}

	override public void Exit(){
		base.Exit();
		Vision.NewHitBoxVisibleEvent.RemoveListener(OnHitBoxVisible);
	}

	void OnHitBoxVisible(EntityHitBox _){
		Controller.TransitionTo<EnemyStateAggro>();
	}
}