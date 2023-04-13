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
using umi3d.browser.utils;
using umi3d.cdk.collaboration;
using umi3d.cdk.userCapture;
using umi3d.cdk.volumes;
using umi3d.common.userCapture;
using umi3dVRBrowsersBase.connection;
using UnityEngine;

namespace umi3dVRBrowsersBase.ikManagement
{
    [System.Serializable]
    public class Umi3dIkManager: IUmi3dPlayer, IUmi3dPlayerLife
    {
        #region Children

        public GameObject Avatar;
        public GameObject Skeleton;
        public GameObject Ybot;
        public Umi3dMixamorigManager Mixamorig;
        public GameObject Feet;
        public GameObject LeftFoot;
        public GameObject RightFoot;

        #endregion

        #region Components

        [HideInInspector]
        public SetUpAvatarHeight AvatarHeight;
        [HideInInspector]
        public UMI3DCollaborationClientUserTracking CollaborationTracking;
        [HideInInspector]
        public AdaptHandPose HandPose;
        [HideInInspector]
        public UMI3DClientUserTrackingBone SkeletonTracking;
        [HideInInspector]
        public Animator Animator;
        [HideInInspector]
        public IKControl IkControl;
        [HideInInspector]
        public PlayerMovement Movement;
        [HideInInspector]
        public FootTargetBehavior FootBehaviour;
        [HideInInspector]
        public VirtualObjectBodyInteraction LeftFootBodyInteraction;
        [HideInInspector]
        public VirtualObjectBodyInteraction RightFootBodyInteraction;

        #endregion

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayerLife.Create()
        {
            Umi3dPlayerManager.Instance.gameObject.FindOrCreate($"Avatar", out Avatar);

            Avatar.FindOrCreate($"Skeleton", out Skeleton);
            Avatar.FindOrCreate($"Feet", out Feet);

            Skeleton.FindOrCreatePrefab($"Unit ybot(Clone)", out Ybot, Umi3dPlayerManager.Instance.PrefabUnityYBot);
            if (Mixamorig == null) Mixamorig = new Umi3dMixamorigManager();
            if (Ybot != null) Mixamorig.CreateMixamorigHierarchy();

            Feet.FindOrCreate($"LeftFoot", out LeftFoot);
            Feet.FindOrCreate($"RightFoot", out RightFoot);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayerLife.SetHierarchy()
        {
            Avatar.Add(Skeleton);
            Skeleton.Add(Ybot);
            Avatar.Add(Feet);
            Feet.Add(LeftFoot);
            Feet.Add(RightFoot);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayerLife.AddComponents()
        {
            Avatar.GetOrAddComponent(out AvatarHeight);
            Avatar.GetOrAddComponent(out CollaborationTracking);
            Avatar.GetOrAddComponent(out HandPose);

            Skeleton.GetOrAddComponent(out SkeletonTracking);

            if (Ybot != null)
            {
                Ybot.GetOrAddComponent(out Animator);
                Ybot.GetOrAddComponent(out IkControl);
                Ybot.GetOrAddComponent(out Movement);
            }

            Feet.GetOrAddComponent(out FootBehaviour);

            LeftFoot.GetOrAddComponent(out LeftFootBodyInteraction);

            RightFoot.GetOrAddComponent(out RightFootBodyInteraction);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayerLife.SetComponents()
        {   
            SkeletonTracking.boneType = BoneType.CenterFeet;

            LeftFootBodyInteraction.goal = AvatarIKGoal.LeftFoot;
            RightFootBodyInteraction.goal = AvatarIKGoal.RightFoot;

            AvatarHeight.skeletonContainer = Skeleton.transform;
            AvatarHeight.FootTargetBehavior = FootBehaviour;

            CollaborationTracking.skeletonContainer = Skeleton.transform;
            CollaborationTracking.viewpointBonetype = BoneType.Head;

            FootBehaviour.FollowedAvatarNode = Avatar.transform;
            FootBehaviour.SkeletonAnimator = Animator;
            FootBehaviour.LeftTarget = LeftFootBodyInteraction;
            FootBehaviour.RightTarget = RightFootBodyInteraction;

            LeftFoot.transform.localScale = new Vector3(.01f, .01f, .01f);
            RightFoot.transform.localScale = new Vector3(.01f, .01f, .01f);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayerLife.Clear()
        {

        }

        #region IUmi3dPlayer

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnLeftHandFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.LeftHand == null) return;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnMainCameraFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.MainCamera == null) return;

            if (AvatarHeight != null) AvatarHeight.OVRAnchor = Umi3dPlayerManager.Instance.MainCamera.transform;
            if (CollaborationTracking != null) CollaborationTracking.viewpoint = Umi3dPlayerManager.Instance.MainCamera.transform;
            if (Umi3dPlayerManager.Instance.MainCamera.gameObject.GetComponent<BasicAllVolumesTracker>() == null) Umi3dPlayerManager.Instance.MainCamera.gameObject.AddComponent<BasicAllVolumesTracker>();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnPlayerFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.Player == null) return;

            if (FootBehaviour != null) FootBehaviour.OVRRig = Umi3dPlayerManager.Instance.Player.transform;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnRightHandFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.RightHand == null) return;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnPrefabYBotFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.PrefabUnityYBot == null) return;

            Skeleton.FindOrCreatePrefab($"Unit ybot(Clone)", out Ybot, Umi3dPlayerManager.Instance.PrefabUnityYBot);
            if (Ybot != null)
            {
                Mixamorig.CreateMixamorigHierarchy();
                Ybot.GetOrAddComponent(out Animator);
                Ybot.GetOrAddComponent(out IkControl);
                Ybot.GetOrAddComponent(out Movement);

                AvatarHeight.IKControl = IkControl;
                AvatarHeight.Neck = Mixamorig.Neck.transform;

                HandPose.IKControl = IkControl;
                HandPose.RightHand = Umi3dPlayerManager.Instance.HandManager.RightHand.BasicHand.HandBodyInteraction;
                HandPose.LeftHand = Umi3dPlayerManager.Instance.HandManager.LeftHand.BasicHand.HandBodyInteraction;

                IkControl.LeftBodyRestPose = Mixamorig.LeftHandBodyInteraction;
                IkControl.RightBodyRestPose = Mixamorig.RightHandBodyInteraction;
                IkControl.LeftFoot = LeftFootBodyInteraction;
                IkControl.RightFoot = RightFootBodyInteraction;
                IkControl.LeftBodyInteraction = Umi3dPlayerManager.Instance.HandManager.LeftHand.BasicHand.HandBodyInteraction;
                IkControl.RightBodyInteraction = Umi3dPlayerManager.Instance.HandManager.RightHand.BasicHand.HandBodyInteraction;
                IkControl.LeftHand = Umi3dPlayerManager.Instance.HandManager.LeftHand.IkTargetBodyInteraction;
                IkControl.RightHand = Umi3dPlayerManager.Instance.HandManager.RightHand.IkTargetBodyInteraction;

                FootBehaviour.IKControl = IkControl;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnPrefabInvisibleSkeletonFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.PrefabInvisibleUnitSkeleton == null) return;

            CollaborationTracking.UnitSkeleton = Umi3dPlayerManager.Instance.PrefabInvisibleUnitSkeleton;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        void IUmi3dPlayer.OnPrefabArcImpactFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.PrefabArcImpact == null) return;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        void IUmi3dPlayer.OnPrefabArcImpactNotPossibleFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.PrefabArcImpactNotPossible == null) return;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnPrefabArcStepDisplayerFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.PrefabArcStepDisplayer == null) return;
        }

        void IUmi3dPlayer.OnPrefabSelectorFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.PrefabSelector == null) return;
        }

        #endregion
    }
}
