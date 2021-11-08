using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using umi3d.common;
using umi3d.cdk;
using System;

public class AdaptHandPose : MonoBehaviour
{
    public IKControl IKControl;

    public VirtualObjectBodyInteraction RightHand;
    public VirtualObjectBodyInteraction LeftHand;

    UMI3DHandPoseDto currentRightPose = null;
    UMI3DHandPoseDto currentLeftPose = null;

    IEnumerator rightHandPlacement = null;
    IEnumerator leftHandPlacement = null;

    ulong currentRightHoverId = 0;
    UMI3DHandPoseDto passiveRightHoverPose;

    ulong currentLeftHoverId = 0;
    UMI3DHandPoseDto passiveLeftHoverPose;

    Dictionary<uint, Quaternion> rightPhalanxRotations;
    Dictionary<uint, Quaternion> leftPhalanxRotations;

    uint lastBoneId = 0;

    // Start is called before the first frame update
    void Start()
    {
        UMI3DClientUserTracking.Instance.handPoseEvent.AddListener((dto) => SetupHandPose(dto, lastBoneId));
        InteractableRaySelector.HoverEnter.AddListener((hoverId, boneId) =>
        {
            lastBoneId = boneId;
            if (boneId.Equals(BoneType.RightHand))
                currentRightHoverId = hoverId;
            else
                currentLeftHoverId = hoverId;
        });
        InteractableRaySelector.HoverExit.AddListener((hoverId, boneId) =>
        {
            lastBoneId = boneId;
            if (boneId.Equals(BoneType.RightHand))
                if (currentRightHoverId != 0 && hoverId.Equals(currentRightHoverId))
                {
                    currentRightHoverId = 0;
                    passiveRightHoverPose = null;
                }
                else
                if (currentLeftHoverId != 0 && hoverId.Equals(currentLeftHoverId))
                {
                    currentLeftHoverId = 0;
                    passiveLeftHoverPose = null;
                }
        });
        BooleanInput.BooleanEvent.AddListener(boneId => lastBoneId = boneId);
    }

    IEnumerator LerpRightPhalanxQuaternion(float startTime, Transform transform = null)
    {
        float elapsedTime = startTime;

        while (transform != null)
        {
            elapsedTime = elapsedTime + Time.deltaTime;

            foreach (var ibone in RightHand.GetComponentsInChildren<VirtualObjectBodyInteractionBone>())
                ibone.transform.localRotation = Quaternion.Slerp(ibone.transform.localRotation, rightPhalanxRotations[ibone.bone.Convert()], elapsedTime);

            yield return null;
        }

        currentLeftPose = null;
        IKControl.leftIkActive = false;
    }

    IEnumerator LerpLeftPhalanxQuaternion(float startTime, Transform transform = null)
    {
        float elapsedTime = startTime;

        while (transform != null)
        {
            elapsedTime = elapsedTime + Time.deltaTime;

            foreach (var ibone in LeftHand.GetComponentsInChildren<VirtualObjectBodyInteractionBone>())
                ibone.transform.localRotation = Quaternion.Slerp(ibone.transform.localRotation, leftPhalanxRotations[ibone.bone.Convert()], elapsedTime);

            yield return null;
        }

        currentRightPose = null;
        IKControl.rightIkActive = false;
    }

    public void SetupHandPose(UMI3DHandPoseDto dto, uint boneId)
    {
        if (dto.IsActive)
        {
            if (boneId.Equals(BoneType.RightHand))
                if (currentRightPose != null && currentRightPose.HoverPose == false && dto.HoverPose == true)
                {
                    passiveRightHoverPose = dto;
                    return;
                }
                else
                if (currentLeftPose != null && currentLeftPose.HoverPose == false && dto.HoverPose == true)
                {
                    passiveLeftHoverPose = dto;
                    return;
                }

            if (boneId.Equals(BoneType.RightHand))
                if (currentRightPose != null && currentRightPose.HoverPose == true && dto.HoverPose == false)
                {
                    passiveRightHoverPose = currentRightPose;
                }
                else
                if (currentLeftPose != null && currentLeftPose.HoverPose == true && dto.HoverPose == false)
                {
                    passiveLeftHoverPose = currentLeftPose;
                }

            if (boneId.Equals(BoneType.RightHand))
            {
                if (rightHandPlacement != null)
                    StopCoroutine(rightHandPlacement);

                if (currentRightPose != null)
                    (UMI3DEnvironmentLoader.GetEntity(currentRightPose.id).dto as UMI3DHandPoseDto).IsActive = false;

                currentRightPose = dto;

                rightPhalanxRotations = GetHandedRotations(dto.PhalanxRotations, boneId);

                Transform relativeNode = (currentRightHoverId != 0 && dto.isRelativeToNode) ? (UMI3DEnvironmentLoader.GetEntity(currentRightHoverId) as UMI3DNodeInstance).transform : null;

                IKControl.rightIkActive = true;

                rightHandPlacement = LerpRightPhalanxQuaternion(0, relativeNode);
                StartCoroutine(rightHandPlacement);
            }
            else
            {
                if (leftHandPlacement != null)
                    StopCoroutine(leftHandPlacement);

                if (currentLeftPose != null)
                    (UMI3DEnvironmentLoader.GetEntity(currentLeftPose.id).dto as UMI3DHandPoseDto).IsActive = false;

                currentLeftPose = dto;

                leftPhalanxRotations = GetHandedRotations(dto.PhalanxRotations, boneId);

                Transform relativeNode = (currentLeftHoverId != 0 && dto.isRelativeToNode) ? (UMI3DEnvironmentLoader.GetEntity(currentLeftHoverId) as UMI3DNodeInstance).transform : null;

                IKControl.leftIkActive = true;

                leftHandPlacement = LerpLeftPhalanxQuaternion(0, relativeNode);
                StartCoroutine(leftHandPlacement);
            }
        }
        else
        {
            if (boneId.Equals(BoneType.RightHand))
            {
                if (currentRightPose != null && dto.id.Equals(currentRightPose.id))
                {
                    if (passiveRightHoverPose != null)
                    {
                        StopCoroutine(rightHandPlacement);
                        currentRightPose = passiveRightHoverPose;

                        rightPhalanxRotations = GetHandedRotations(currentRightPose.PhalanxRotations, boneId);

                        Transform relativeNode = (currentRightHoverId != 0 && currentRightPose.isRelativeToNode) ? (UMI3DEnvironmentLoader.GetEntity(currentRightHoverId) as UMI3DNodeInstance).transform : null;

                        rightHandPlacement = LerpRightPhalanxQuaternion(0, relativeNode);
                        StartCoroutine(rightHandPlacement);
                    }
                    else
                    {
                        currentRightPose = null;
                        IKControl.rightIkActive = false;
                    }
                }
            }
            else
            {
                if (currentLeftPose != null && dto.id.Equals(currentLeftPose.id))
                {
                    if (passiveLeftHoverPose != null)
                    {
                        StopCoroutine(rightHandPlacement);
                        currentLeftPose = passiveLeftHoverPose;

                        leftPhalanxRotations = GetHandedRotations(currentLeftPose.PhalanxRotations, boneId);

                        Transform relativeNode = (currentLeftHoverId != 0 && currentLeftPose.isRelativeToNode) ? (UMI3DEnvironmentLoader.GetEntity(currentLeftHoverId) as UMI3DNodeInstance).transform : null;

                        leftHandPlacement = LerpLeftPhalanxQuaternion(0, relativeNode);
                        StartCoroutine(leftHandPlacement);
                    }
                    else
                    {
                        currentLeftPose = null;
                        IKControl.leftIkActive = false;
                    }
                }
            }
        }
    }

    private Dictionary<uint, Quaternion> GetHandedRotations(Dictionary<uint, SerializableVector3> PhalanxRotations, uint boneId)
    {
        Dictionary<uint, Quaternion> handedPhalanxRotations = new Dictionary<uint, Quaternion>();

        if (boneId.Equals(BoneType.RightHand))
        {
            handedPhalanxRotations.Add((BoneType.RightThumbProximal), Quaternion.Euler(PhalanxRotations[BoneType.RightThumbProximal]));
            handedPhalanxRotations.Add((BoneType.RightThumbIntermediate), Quaternion.Euler(PhalanxRotations[BoneType.RightThumbIntermediate]));
            handedPhalanxRotations.Add((BoneType.RightThumbDistal), Quaternion.Euler(PhalanxRotations[BoneType.RightThumbDistal]));

            handedPhalanxRotations.Add((BoneType.RightIndexProximal), Quaternion.Euler(PhalanxRotations[BoneType.RightIndexProximal]));
            handedPhalanxRotations.Add((BoneType.RightIndexIntermediate), Quaternion.Euler(PhalanxRotations[BoneType.RightIndexIntermediate]));
            handedPhalanxRotations.Add((BoneType.RightIndexDistal), Quaternion.Euler(PhalanxRotations[BoneType.RightIndexDistal]));

            handedPhalanxRotations.Add((BoneType.RightMiddleProximal), Quaternion.Euler(PhalanxRotations[BoneType.RightMiddleProximal]));
            handedPhalanxRotations.Add((BoneType.RightMiddleIntermediate), Quaternion.Euler(PhalanxRotations[BoneType.RightMiddleIntermediate]));
            handedPhalanxRotations.Add((BoneType.RightMiddleDistal), Quaternion.Euler(PhalanxRotations[BoneType.RightMiddleDistal]));

            handedPhalanxRotations.Add((BoneType.RightRingProximal), Quaternion.Euler(PhalanxRotations[BoneType.RightRingProximal]));
            handedPhalanxRotations.Add((BoneType.RightRingIntermediate), Quaternion.Euler(PhalanxRotations[BoneType.RightRingIntermediate]));
            handedPhalanxRotations.Add((BoneType.RightRingDistal), Quaternion.Euler(PhalanxRotations[BoneType.RightRingDistal]));

            handedPhalanxRotations.Add((BoneType.RightLittleProximal), Quaternion.Euler(PhalanxRotations[BoneType.RightLittleProximal]));
            handedPhalanxRotations.Add((BoneType.RightLittleIntermediate), Quaternion.Euler(PhalanxRotations[BoneType.RightLittleIntermediate]));
            handedPhalanxRotations.Add((BoneType.RightLittleDistal), Quaternion.Euler(PhalanxRotations[BoneType.RightLittleDistal]));
        }
        else
        {
            handedPhalanxRotations.Add((BoneType.LeftThumbProximal), Quaternion.Euler(PhalanxRotations[BoneType.LeftThumbProximal]));
            handedPhalanxRotations.Add((BoneType.LeftThumbIntermediate), Quaternion.Euler(PhalanxRotations[BoneType.LeftThumbIntermediate]));
            handedPhalanxRotations.Add((BoneType.LeftThumbDistal), Quaternion.Euler(PhalanxRotations[BoneType.LeftThumbDistal]));

            handedPhalanxRotations.Add((BoneType.LeftIndexProximal), Quaternion.Euler(PhalanxRotations[BoneType.LeftIndexProximal]));
            handedPhalanxRotations.Add((BoneType.LeftIndexIntermediate), Quaternion.Euler(PhalanxRotations[BoneType.LeftIndexIntermediate]));
            handedPhalanxRotations.Add((BoneType.LeftIndexDistal), Quaternion.Euler(PhalanxRotations[BoneType.LeftIndexDistal]));

            handedPhalanxRotations.Add((BoneType.LeftMiddleProximal), Quaternion.Euler(PhalanxRotations[BoneType.LeftMiddleProximal]));
            handedPhalanxRotations.Add((BoneType.LeftMiddleIntermediate), Quaternion.Euler(PhalanxRotations[BoneType.LeftMiddleIntermediate]));
            handedPhalanxRotations.Add((BoneType.LeftMiddleDistal), Quaternion.Euler(PhalanxRotations[BoneType.LeftMiddleDistal]));

            handedPhalanxRotations.Add((BoneType.LeftRingProximal), Quaternion.Euler(PhalanxRotations[BoneType.LeftRingProximal]));
            handedPhalanxRotations.Add((BoneType.LeftRingIntermediate), Quaternion.Euler(PhalanxRotations[BoneType.LeftRingIntermediate]));
            handedPhalanxRotations.Add((BoneType.LeftRingDistal), Quaternion.Euler(PhalanxRotations[BoneType.LeftRingDistal]));

            handedPhalanxRotations.Add((BoneType.LeftLittleProximal), Quaternion.Euler(PhalanxRotations[BoneType.LeftLittleProximal]));
            handedPhalanxRotations.Add((BoneType.LeftLittleIntermediate), Quaternion.Euler(PhalanxRotations[BoneType.LeftLittleIntermediate]));
            handedPhalanxRotations.Add((BoneType.LeftLittleDistal), Quaternion.Euler(PhalanxRotations[BoneType.LeftLittleDistal]));
        }

        return handedPhalanxRotations;
    }
}
