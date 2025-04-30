using UnityEngine;

public class PillBehavior : MonoBehaviour {
	[System.NonSerialized] public Entity Entity;

	public virtual float EffectDuration {get;} = 0f;

	public virtual void OnUse(){

	}

	public virtual void OverTimeEffect(){

	}

	public virtual void OnEndEffect(){

	}

}