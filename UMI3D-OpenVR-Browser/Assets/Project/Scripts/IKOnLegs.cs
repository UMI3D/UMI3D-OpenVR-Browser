using UnityEngine;

public class IKOnLegs : MonoBehaviour
{
    public LegsMover legsmover;
    private Animator animator;

    private void Start()
    {
        legsmover = FindObjectOfType<LegsMover>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Order in which to apply foward kinematics
    /// </summary>
    private readonly HumanBodyBones[] orderToApplyFK = new HumanBodyBones[8]
    {
        HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, HumanBodyBones.RightToes,
        HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes,
    };

    private void OnAnimatorIK(int layerIndex)
    {
        if (legsmover.legsRotPrediction.Count == 0)
            return;

        // apply global positoon and hips offset (forward kinematics)
        foreach (var joint in orderToApplyFK)
            legsmover.jointReferences[joint].transform.localRotation = legsmover.legsRotPrediction[joint];

        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        animator.SetIKPosition(AvatarIKGoal.RightFoot, legsmover.jointReferences[HumanBodyBones.RightToes].transform.position);
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, legsmover.jointReferences[HumanBodyBones.LeftToes].transform.position);
    }
}