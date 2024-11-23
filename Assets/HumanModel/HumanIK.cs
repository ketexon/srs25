using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanIK : MonoBehaviour
{
    [SerializeField] Animator animator;

    [SerializeField] Transform leftHandTarget;
    [SerializeField] Transform rightHandTarget;
    [SerializeField] Transform leftFootTarget;
    [SerializeField] Transform rightFootTarget;

    void OnAnimatorIK(int layerIndex) {
        if(!(leftHandTarget && rightHandTarget && leftFootTarget && rightFootTarget))
        {
            return;
        }
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);

        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootTarget.position);
        animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootTarget.position);

        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootTarget.rotation);
        animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootTarget.rotation);
    }
}
