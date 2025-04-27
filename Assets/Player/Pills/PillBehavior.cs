using UnityEngine;

public class PillBehavior : MonoBehaviour {
	[System.NonSerialized] public Entity Entity;

	public float EffectDuration = 0f;

	public virtual void OnUse(){

	}

	public virtual void OverTimeEffect(){

	}

	public virtual void OnEndEffect(){

	}

}