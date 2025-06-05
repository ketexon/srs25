using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HumanIK : MonoBehaviour
{
    [SerializeField] bool runInEditor = false;
    [SerializeField] Animator animator;

    [SerializeField] Transform leftHandTarget;
    [SerializeField] Transform rightHandTarget;
    [SerializeField] Transform leftFootTarget;
    [SerializeField] Transform rightFootTarget;

    (AvatarIKGoal, Transform)[] IKTargets => new (AvatarIKGoal, Transform)[]{
        (AvatarIKGoal.LeftHand, leftHandTarget),
        (AvatarIKGoal.RightHand, rightHandTarget),
        (AvatarIKGoal.LeftFoot, leftFootTarget),
        (AvatarIKGoal.RightFoot, rightFootTarget)
    };

    public void SetHandIKTargets(IHandIKTarget handIKTarget)
    {
        if(handIKTarget != null){
            leftHandTarget = handIKTarget.LeftHandTarget;
            rightHandTarget = handIKTarget.RightHandTarget;
        }
        else {
            leftHandTarget = null;
            rightHandTarget = null;
        }
    }

    void Update(){
        if(runInEditor && !Application.isPlaying){
            animator.Update(0);
        }
    }

    void OnAnimatorIK(int layerIndex) {
        foreach(var (goal, target) in IKTargets)
        {
            if(target)
            {
                animator.SetIKPositionWeight(goal, 1);
                animator.SetIKRotationWeight(goal, 1);
                animator.SetIKPosition(goal, target.position);
                animator.SetIKRotation(goal, target.rotation);
            }
            else {
                animator.SetIKPositionWeight(goal, 0);
                animator.SetIKRotationWeight(goal, 0);
            }
        }
    }
}
