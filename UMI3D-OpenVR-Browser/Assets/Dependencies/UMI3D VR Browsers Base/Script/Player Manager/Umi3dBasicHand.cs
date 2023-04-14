/*
Copyright 2019 - 2023 Inetum

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
using System.Collections;
using System.Collections.Generic;
using umi3d.browser.utils;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3dVRBrowsersBase.ikManagement
{
    [System.Serializable]
    public class Umi3dBasicHand: IUmi3dPlayerLife
    {
        [HideInInspector]
        public AvatarIKGoal Goal;

        #region GameObjects

        [HideInInspector]
        public GameObject BasicHand;

        [HideInInspector]
        public GameObject Sphere;

        #region Thumb

        [HideInInspector]
        public GameObject Thumb1;
        [HideInInspector]
        public GameObject Thumb2;
        [HideInInspector]
        public GameObject Thumb3;
        [HideInInspector]
        public GameObject Thumb4;

        #endregion

        #region Index

        [HideInInspector]
        public GameObject Index1;
        [HideInInspector]
        public GameObject Index2;
        [HideInInspector]
        public GameObject Index3;
        [HideInInspector]
        public GameObject Index4;

        #endregion

        #region Middle

        [HideInInspector]
        public GameObject Middle1;
        [HideInInspector]
        public GameObject Middle2;
        [HideInInspector]
        public GameObject Middle3;
        [HideInInspector]
        public GameObject Middle4;

        #endregion

        #region Ring

        [HideInInspector]
        public GameObject Ring1;
        [HideInInspector]
        public GameObject Ring2;
        [HideInInspector]
        public GameObject Ring3;
        [HideInInspector]
        public GameObject Ring4;

        #endregion

        #region Pinky

        [HideInInspector]
        public GameObject Pinky1;
        [HideInInspector]
        public GameObject Pinky2;
        [HideInInspector]
        public GameObject Pinky3;
        [HideInInspector]
        public GameObject Pinky4;

        #endregion

        #endregion

        #region Components

        [HideInInspector]
        public VirtualObjectBodyInteraction HandBodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionEndBone SphereBodyInteraction;

        #region Thumb

        [HideInInspector]
        public VirtualObjectBodyInteractionBone Thumb1BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionBone Thumb2BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionBone Thumb3BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionEndBone Thumb4BodyInteraction;

        #endregion

        #region Index

        [HideInInspector]
        public VirtualObjectBodyInteractionBone Index1BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionBone Index2BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionBone Index3BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionEndBone Index4BodyInteraction;

        #endregion

        #region Middle

        [HideInInspector]
        public VirtualObjectBodyInteractionBone Middle1BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionBone Middle2BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionBone Middle3BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionEndBone Middle4BodyInteraction;

        #endregion

        #region Ring

        [HideInInspector]
        public VirtualObjectBodyInteractionBone Ring1BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionBone Ring2BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionBone Ring3BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionEndBone Ring4BodyInteraction;

        #endregion

        #region Pinky

        [HideInInspector]
        public VirtualObjectBodyInteractionBone Pinky1BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionBone Pinky2BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionBone Pinky3BodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteractionEndBone Pinky4BodyInteraction;

        #endregion

        #endregion


        void IUmi3dPlayerLife.Create()
        {
            if (BasicHand == null) BasicHand = new GameObject($"Basic {Goal}");
            if (Sphere == null) Sphere = new GameObject("Sphere");
            {
                if (Thumb1 == null) Thumb1 = new GameObject("Thumb1");
                if (Thumb2 == null) Thumb2 = new GameObject("Thumb2");
                if (Thumb3 == null) Thumb3 = new GameObject("Thumb3");
                if (Thumb4 == null) Thumb4 = new GameObject("Thumb4");
            }
            {
                if (Index1 == null) Index1 = new GameObject("Index1");
                if (Index2 == null) Index2 = new GameObject("Index2");
                if (Index3 == null) Index3 = new GameObject("Index3");
                if (Index4 == null) Index4 = new GameObject("Index4");
            }
            {
                if (Middle1 == null) Middle1 = new GameObject("Middle1");
                if (Middle2 == null) Middle2 = new GameObject("Middle2");
                if (Middle3 == null) Middle3 = new GameObject("Middle3");
                if (Middle4 == null) Middle4 = new GameObject("Middle4");
            }
            {
                if (Ring1 == null) Ring1 = new GameObject("Ring1");
                if (Ring2 == null) Ring2 = new GameObject("Ring2");
                if (Ring3 == null) Ring3 = new GameObject("Ring3");
                if (Ring4 == null) Ring4 = new GameObject("Ring4");
            }
            {
                if (Pinky1 == null) Pinky1 = new GameObject("Pinky1");
                if (Pinky2 == null) Pinky2 = new GameObject("Pinky2");
                if (Pinky3 == null) Pinky3 = new GameObject("Pinky3");
                if (Pinky4 == null) Pinky4 = new GameObject("Pinky4");
            }
        }

        void IUmi3dPlayerLife.AddComponents()
        {
            BasicHand.GetOrAddComponent(out HandBodyInteraction);
            Sphere.GetOrAddComponent(out SphereBodyInteraction);

            Thumb1.GetOrAddComponent(out Thumb1BodyInteraction);
            Thumb2.GetOrAddComponent(out Thumb2BodyInteraction);
            Thumb3.GetOrAddComponent(out Thumb3BodyInteraction);
            Thumb4.GetOrAddComponent(out Thumb4BodyInteraction);

            Index1.GetOrAddComponent(out Index1BodyInteraction);
            Index2.GetOrAddComponent(out Index2BodyInteraction);
            Index3.GetOrAddComponent(out Index3BodyInteraction);
            Index4.GetOrAddComponent(out Index4BodyInteraction);

            Middle1.GetOrAddComponent(out Middle1BodyInteraction);
            Middle2.GetOrAddComponent(out Middle2BodyInteraction);
            Middle3.GetOrAddComponent(out Middle3BodyInteraction);
            Middle4.GetOrAddComponent(out Middle4BodyInteraction);

            Ring1.GetOrAddComponent(out Ring1BodyInteraction);
            Ring2.GetOrAddComponent(out Ring2BodyInteraction);
            Ring3.GetOrAddComponent(out Ring3BodyInteraction);
            Ring4.GetOrAddComponent(out Ring4BodyInteraction);

            Pinky1.GetOrAddComponent(out Pinky1BodyInteraction);
            Pinky2.GetOrAddComponent(out Pinky2BodyInteraction);
            Pinky3.GetOrAddComponent(out Pinky3BodyInteraction);
            Pinky4.GetOrAddComponent(out Pinky4BodyInteraction);
        }

        void IUmi3dPlayerLife.SetComponents()
        {
            HandBodyInteraction.goal = Goal;
            {
                Thumb1BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftThumbProximal : HumanBodyBones.RightThumbProximal;
                Thumb2BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftThumbIntermediate : HumanBodyBones.RightThumbIntermediate;
                Thumb3BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftThumbDistal : HumanBodyBones.RightThumbDistal;
            }
            {
                Index1BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftIndexProximal : HumanBodyBones.RightIndexProximal;
                Index2BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftIndexIntermediate : HumanBodyBones.RightIndexIntermediate;
                Index3BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftIndexDistal : HumanBodyBones.RightIndexDistal;
            }
            {
                Middle1BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal;
                Middle2BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftMiddleIntermediate : HumanBodyBones.RightMiddleIntermediate;
                Middle3BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftMiddleDistal : HumanBodyBones.RightMiddleDistal;
            }
            {
                Ring1BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftRingProximal : HumanBodyBones.RightRingProximal;
                Ring2BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftRingIntermediate : HumanBodyBones.RightRingIntermediate;
                Ring3BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftRingDistal : HumanBodyBones.RightRingDistal;
            }
            {
                Pinky1BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftLittleProximal : HumanBodyBones.RightLittleProximal;
                Pinky2BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftLittleIntermediate : HumanBodyBones.RightLittleIntermediate;
                Pinky3BodyInteraction.bone = Goal == AvatarIKGoal.LeftHand ? HumanBodyBones.LeftLittleDistal : HumanBodyBones.RightLittleDistal;
            }
        }

        void IUmi3dPlayerLife.SetHierarchy()
        {
            BasicHand.Add(Sphere);
            Sphere.Add(Thumb1);
            Sphere.Add(Index1);
            Sphere.Add(Middle1);
            Sphere.Add(Ring1);
            Sphere.Add(Pinky1);
            {
                Thumb1.Add(Thumb2);
                Thumb2.Add(Thumb3);
                Thumb3.Add(Thumb4);
            }
            {
                Index1.Add(Index2);
                Index2.Add(Index3);
                Index3.Add(Index4);
            }
            {
                Middle1.Add(Middle2);
                Middle2.Add(Middle3);
                Middle3.Add(Middle4);
            }
            {
                Ring1.Add(Ring2);
                Ring2.Add(Ring3);
                Ring3.Add(Ring4);
            }
            {
                Pinky1.Add(Pinky2);
                Pinky2.Add(Pinky3);
                Pinky3.Add(Pinky4);
            }

            BasicHand.transform.localPosition = new Vector3(0f, 0f, -.1f);
            BasicHand.transform.rotation = Quaternion.Euler
            (
                Goal == AvatarIKGoal.LeftHand
                ? new Vector3 (-90f, 90f, 0f) 
                : new Vector3 (-90f, -90f, 0f)
            );
            Sphere.transform.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
            Sphere.transform.localScale = Vector3.one * .02f;
        }

        void IUmi3dPlayerLife.Clear()
        {

        }
    }
}
