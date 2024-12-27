using UnityEngine;

public interface IHandIKTarget
{
	Transform LeftHandTarget { get; }
	Transform RightHandTarget { get; }
}