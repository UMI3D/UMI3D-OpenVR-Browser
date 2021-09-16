using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootTargetBehavior : MonoBehaviour
{
    public Transform FollowedAvatarNode;
    public Transform VRRig;

    public Animator SkeletonAnimator;

    public VirtualObjectBodyInteraction LeftTarget;
    public VirtualObjectBodyInteraction RightTarget;

    public IKControl IKControl;

    void Update()
    {
        this.transform.position = new Vector3(FollowedAvatarNode.position.x, VRRig.position.y, FollowedAvatarNode.position.z);
        this.transform.rotation = FollowedAvatarNode.rotation;
    }

    public void SetFootTargets()
    {
        LeftTarget.transform.position = SkeletonAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
        RightTarget.transform.position = SkeletonAnimator.GetBoneTransform(HumanBodyBones.RightFoot).position;

        IKControl.LeftFoot = LeftTarget;
        IKControl.RightFoot = RightTarget;

        IKControl.feetIkActive = true;
    }
}
