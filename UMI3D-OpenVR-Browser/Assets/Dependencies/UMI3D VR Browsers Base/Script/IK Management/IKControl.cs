/*
Copyright 2019 - 2022 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/


using System;
using System.Collections.Generic;
using UnityEngine;

namespace umi3dVRBrowsersBase.ikManagement
{
    [RequireComponent(typeof(Animator))]

    public class IKControl : MonoBehaviour
    {
        protected Animator animator;

        //public bool leftIkActive = false;
        //public bool rightIkActive = false;

        //public bool controllerIkActive = false;
        //public bool headIkActive = false;
        public bool feetIkActive = false;

        //public Transform lookObj = null;

        public float animationSpeed = 1f;
        //public VirtualObjectBodyInteraction LeftBodyRestPose;
        //public VirtualObjectBodyInteraction LeftBodyInteraction;
        //public VirtualObjectBodyInteraction RightBodyRestPose;
        //public VirtualObjectBodyInteraction RightBodyInteraction;

        //public VirtualObjectBodyInteraction LeftHand;
        //public VirtualObjectBodyInteraction RightHand;

        public VirtualObjectBodyInteraction LeftFoot;
        public VirtualObjectBodyInteraction RightFoot;

        private BodyInteractionHelper rightRestPoseHelper;
        private BodyInteractionHelper rightBodyInteractionHelper;
        private BodyInteractionHelper leftRestPoseHelper;
        private BodyInteractionHelper leftBodyInteractionHelper;

        private float rightAnimationDuration = 1f;
        private float leftAnimationDuration = 1f;

        private float rightAnimation = 0f;
        private float leftAnimation = 0f;

        public class BodyInteractionHelper
        {
            public class PoseSyncData
            {
                public Transform bone;
                public Transform targetBone;
                public bool active = false;
                public Quaternion memory;
            }

            private Animator animator;
            private VirtualObjectBodyInteraction bodyInteraction;
            public VirtualObjectBodyInteraction origin = null;

            public Vector3 ikOriginalPosition;
            public Quaternion ikOriginalRotation;

            [NonSerialized] public Dictionary<HumanBodyBones, PoseSyncData> poses = new Dictionary<HumanBodyBones, PoseSyncData>();
            [NonSerialized] public Dictionary<HumanBodyBones, PoseSyncData> originalPoses = new Dictionary<HumanBodyBones, PoseSyncData>();

            public BodyInteractionHelper(VirtualObjectBodyInteraction bodyInteraction, Animator animator)
            {
                this.animator = animator;
                this.bodyInteraction = bodyInteraction;

                foreach (VirtualObjectBodyInteractionBone ibone in bodyInteraction.GetComponentsInChildren<VirtualObjectBodyInteractionBone>())
                {
                    poses.Add(ibone.bone, new PoseSyncData()
                    {
                        bone = animator.GetBoneTransform(ibone.bone),
                        targetBone = ibone.transform
                    });
                }
            }

            public void SyncRightIk(float weight)
            {
                if (origin == null)
                {
                    animator.SetIKPositionWeight(bodyInteraction.goal, weight);
                    animator.SetIKRotationWeight(bodyInteraction.goal, weight);
                    animator.SetIKPosition(bodyInteraction.goal, bodyInteraction.transform.position);
                    animator.SetIKRotation(bodyInteraction.goal, bodyInteraction.transform.rotation * Quaternion.Euler(0, 90, 0));
                }
                else
                {
                    animator.SetIKPositionWeight(bodyInteraction.goal, 1);
                    animator.SetIKRotationWeight(bodyInteraction.goal, 1);
                    var position = Vector3.Lerp(origin.transform.position, bodyInteraction.transform.position, weight);
                    var rotation = Quaternion.Slerp(origin.transform.rotation, bodyInteraction.transform.rotation, weight);
                    animator.SetIKPosition(bodyInteraction.goal, position);
                    animator.SetIKRotation(bodyInteraction.goal, rotation * Quaternion.Euler(0, 90, 0));
                }
            }

            public void SyncLeftIk(float weight)
            {
                if (origin == null)
                {
                    animator.SetIKPositionWeight(bodyInteraction.goal, weight);
                    animator.SetIKRotationWeight(bodyInteraction.goal, weight);
                    animator.SetIKPosition(bodyInteraction.goal, bodyInteraction.transform.position);
                    animator.SetIKRotation(bodyInteraction.goal, bodyInteraction.transform.rotation * Quaternion.Euler(0, -90, 0));
                }
                else
                {
                    animator.SetIKPositionWeight(bodyInteraction.goal, 1);
                    animator.SetIKRotationWeight(bodyInteraction.goal, 1);
                    var position = Vector3.Lerp(origin.transform.position, bodyInteraction.transform.position, weight);
                    var rotation = Quaternion.Slerp(origin.transform.rotation, bodyInteraction.transform.rotation, weight);
                    animator.SetIKPosition(bodyInteraction.goal, position);
                    animator.SetIKRotation(bodyInteraction.goal, rotation * Quaternion.Euler(0, -90, 0));
                }
            }

            public void UnsyncIK()
            {
                animator.SetIKPositionWeight(bodyInteraction.goal, 0);
                animator.SetIKRotationWeight(bodyInteraction.goal, 0);
            }

            public void SyncPoses(float animation)
            {
                foreach (KeyValuePair<HumanBodyBones, PoseSyncData> pair in poses)
                {
                    PoseSyncData pose = pair.Value;
                    if (!pose.active)
                    {
                        pose.memory = pose.bone.localRotation;
                        pose.active = true;
                    }
                    if (originalPoses != null && originalPoses.ContainsKey(pair.Key))
                    {
                        pose.bone.localRotation = Quaternion.Slerp(originalPoses[pair.Key].targetBone.localRotation, pose.targetBone.localRotation, animation);
                    }
                    else
                    {
                        pose.bone.localRotation = Quaternion.Slerp(pose.memory, pose.targetBone.localRotation, animation);
                    }
                }
            }

            public void UnsyncPoses(bool reset = true)
            {
                foreach (PoseSyncData pose in poses.Values)
                {
                    if (pose.active)
                    {
                        if (reset)
                            pose.bone.localRotation = pose.memory;
                        pose.active = false;
                    }
                }
            }
        }

        private float rightInitDist;

        private void Start()
        {
            animator = GetComponent<Animator>();

            //rightInitDist = Vector3.Distance(RightBodyInteraction.transform.position, animator.GetBoneTransform(HumanBodyBones.RightShoulder).position);

            //MouseAndKeyboardController.HoverEnter.AddListener((id, v) => {
            //    Vector3 point = (UMI3DEnvironmentLoader.GetEntity(id) as UMI3DNodeInstance).transform.TransformPoint(v);
            //    if (Vector3.Distance(point, animator.GetBoneTransform(HumanBodyBones.RightShoulder).position) < 1.5f) {
            //        float f = Mathf.Min(rightInitDist, Vector3.Distance(point, RightBodyInteraction.transform.position));
            //        Vector3 dir = (point - RightBodyInteraction.transform.position).normalized;
            //        RightBodyInteraction.transform.position = animator.GetBoneTransform(HumanBodyBones.RightShoulder).position + dir * f;
            //        RightBodyInteraction.transform.LookAt(point);
            //        RightBodyInteraction.transform.rotation *= Quaternion.Euler(-90, -90, 0);
            //        rightIkActive = true;  } 
            //});

            //MouseAndKeyboardController.HoverUpdate.AddListener((id, v) =>
            //{
            //    if (rightIkActive)
            //    {
            //        Vector3 point = (UMI3DEnvironmentLoader.GetEntity(id) as UMI3DNodeInstance).transform.TransformPoint(v);
            //        if (Vector3.Distance(point, animator.GetBoneTransform(HumanBodyBones.RightShoulder).position) < 1.5f)
            //        {
            //            float f = Mathf.Min(rightInitDist, Vector3.Distance(point, RightBodyInteraction.transform.position));
            //            Vector3 dir = (point - RightBodyInteraction.transform.position).normalized;
            //            Vector3 newPos = animator.GetBoneTransform(HumanBodyBones.RightShoulder).position + dir * f;
            //            if (Vector3.Distance(newPos, RightBodyInteraction.transform.position) > 0.1)
            //            {
            //                RightBodyInteraction.transform.position = newPos;
            //                RightBodyInteraction.transform.LookAt(point);
            //                RightBodyInteraction.transform.rotation *= Quaternion.Euler(-90, -90, 0);
            //            }
            //        }
            //        else
            //            rightIkActive = false;
            //    }
            //    else
            //    {
            //        Vector3 point = (UMI3DEnvironmentLoader.GetEntity(id) as UMI3DNodeInstance).transform.TransformPoint(v);
            //        if (Vector3.Distance(point, animator.GetBoneTransform(HumanBodyBones.RightShoulder).position) < 1.5f)
            //        {
            //            float f = Mathf.Min(rightInitDist, Vector3.Distance(point, RightBodyInteraction.transform.position));
            //            Vector3 dir = (point - RightBodyInteraction.transform.position).normalized;
            //            RightBodyInteraction.transform.position = animator.GetBoneTransform(HumanBodyBones.RightShoulder).position + dir * f;
            //            RightBodyInteraction.transform.LookAt(point);
            //            RightBodyInteraction.transform.rotation *= Quaternion.Euler(-90, -90, 0);
            //            rightIkActive = true;
            //        }
            //    }

            //});

            //MouseAndKeyboardController.HoverExit.AddListener(() => rightIkActive = false);

            //rightRestPoseHelper = new BodyInteractionHelper(RightBodyRestPose, animator);
            //rightBodyInteractionHelper = new BodyInteractionHelper(RightBodyInteraction, animator);
            //rightRestPoseHelper.origin = RightBodyInteraction;
            //rightBodyInteractionHelper.origin = RightBodyRestPose;
            //rightRestPoseHelper.originalPoses = rightBodyInteractionHelper.poses;
            //rightBodyInteractionHelper.originalPoses = rightRestPoseHelper.poses;
            //rightAnimationDuration = Vector3.Distance(RightBodyRestPose.transform.position, RightBodyInteraction.transform.position) / animationSpeed;

            //leftRestPoseHelper = new BodyInteractionHelper(LeftBodyRestPose, animator);
            //leftBodyInteractionHelper = new BodyInteractionHelper(LeftBodyInteraction, animator);
            //leftRestPoseHelper.origin = LeftBodyInteraction;
            //leftBodyInteractionHelper.origin = LeftBodyRestPose;
            //leftRestPoseHelper.originalPoses = leftBodyInteractionHelper.poses;
            //leftBodyInteractionHelper.originalPoses = leftRestPoseHelper.poses;
            //leftAnimationDuration = Vector3.Distance(LeftBodyRestPose.transform.position, LeftBodyInteraction.transform.position) / animationSpeed;
        }


        private void OnAnimatorIK()
        {
            if (animator)
            {
                ////if the IK is active, set the position and rotation directly to the goal. 
                //if (rightIkActive || rightAnimation > 0f)
                //{
                //    float rightDelta = Time.deltaTime / rightAnimationDuration;

                //    if (!rightIkActive && rightAnimation > 0f)
                //        rightDelta *= -1f;

                //    rightAnimation += rightDelta;
                //    rightAnimation = Mathf.Clamp01(rightAnimation);

                //    // Set the look target position, if one has been assigned

                //    // other method depending on the grabbing hand
                //}

                //if (leftIkActive || leftAnimation > 0f)
                //{
                //    float leftDelta = Time.deltaTime / leftAnimationDuration;

                //    if (!leftIkActive && leftAnimation > 0f)
                //        leftDelta *= -1f;

                //    leftAnimation += leftDelta;
                //    leftAnimation = Mathf.Clamp01(leftAnimation);
                //}

                //if (lookObj != null && lookObj)
                //{
                //    animator.SetLookAtWeight(1);
                //    animator.SetLookAtPosition(lookObj.position);
                //}

                //if (rightIkActive)
                //{
                //    rightRestPoseHelper.UnsyncPoses(false);
                //    rightBodyInteractionHelper.SyncRightIk(rightAnimation);
                //}
                //else
                //{
                //    rightBodyInteractionHelper.UnsyncPoses(false);
                //    rightRestPoseHelper.SyncRightIk(1f - rightAnimation);
                //}


                //if (leftIkActive)
                //{
                //    leftRestPoseHelper.UnsyncPoses(false);
                //    leftBodyInteractionHelper.SyncLeftIk(leftAnimation);
                //}
                //else
                //{
                //    leftBodyInteractionHelper.UnsyncPoses(false);
                //    leftRestPoseHelper.SyncLeftIk(1f - leftAnimation);
                //}

                //if (controllerIkActive)
                //{
                //    animator.SetIKPositionWeight(LeftHand.goal, 1);
                //    animator.SetIKRotationWeight(LeftHand.goal, 1);
                //    animator.SetIKPosition(LeftHand.goal, LeftHand.transform.position);
                //    animator.SetIKRotation(LeftHand.goal, LeftHand.transform.rotation);

                //    animator.SetIKPositionWeight(RightHand.goal, 1);
                //    animator.SetIKRotationWeight(RightHand.goal, 1);
                //    animator.SetIKPosition(RightHand.goal, RightHand.transform.position);
                //    animator.SetIKRotation(RightHand.goal, RightHand.transform.rotation);
                //}
                //else
                //{
                //    animator.SetIKPositionWeight(LeftHand.goal, 0);
                //    animator.SetIKRotationWeight(LeftHand.goal, 0);
                //    animator.SetIKPositionWeight(RightHand.goal, 0);
                //    animator.SetIKRotationWeight(RightHand.goal, 0);
                //}

                if (feetIkActive)
                {
                    animator.SetIKPositionWeight(LeftFoot.goal, 1);
                    animator.SetIKRotationWeight(LeftFoot.goal, 1);
                    animator.SetIKPosition(LeftFoot.goal, LeftFoot.transform.position);
                    animator.SetIKRotation(LeftFoot.goal, LeftFoot.transform.rotation);

                    animator.SetIKPositionWeight(RightFoot.goal, 1);
                    animator.SetIKRotationWeight(RightFoot.goal, 1);
                    animator.SetIKPosition(RightFoot.goal, RightFoot.transform.position);
                    animator.SetIKRotation(RightFoot.goal, RightFoot.transform.rotation);
                }
                else
                {
                    animator.SetIKPositionWeight(LeftFoot.goal, 0);
                    animator.SetIKRotationWeight(LeftFoot.goal, 0);
                    animator.SetIKPositionWeight(RightFoot.goal, 0);
                    animator.SetIKRotationWeight(RightFoot.goal, 0);
                }
            }
        }

        //private void LateUpdate()
        //{
        //    if (animator)
        //    {
        //        Transform leftKnee = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        //        Transform rightKnee = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        //        leftKnee.localRotation = rightKnee.localRotation;
        //    }

        //    if (animator)
        //    {
        //        if (rightIkActive)
        //        {
        //            rightRestPoseHelper.UnsyncPoses(false);
        //            rightBodyInteractionHelper.SyncPoses(rightAnimation);
        //        }
        //        else
        //        {
        //            rightBodyInteractionHelper.UnsyncPoses(false);
        //            rightRestPoseHelper.SyncPoses(1f - rightAnimation);
        //        }

        //        if (leftIkActive)
        //        {
        //            leftRestPoseHelper.UnsyncPoses(false);
        //            leftBodyInteractionHelper.SyncPoses(leftAnimation);
        //        }
        //        else
        //        {
        //            leftBodyInteractionHelper.UnsyncPoses(false);
        //            leftRestPoseHelper.SyncPoses(1f - leftAnimation);
        //        }
        //    }
        //}
    }
}