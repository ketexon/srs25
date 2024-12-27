using UnityEngine;

public class HandIKTarget : MonoBehaviour, IHandIKTarget
{
	[SerializeField] Transform leftHandTarget;
	[SerializeField] Transform rightHandTarget;

	public Transform LeftHandTarget => leftHandTarget;
	public Transform RightHandTarget => rightHandTarget;
}