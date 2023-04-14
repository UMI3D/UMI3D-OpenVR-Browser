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
#if UNITY_2021_3_15_OR_NEWER
using Unity.VisualScripting;
#endif
using inetum.unityUtils;
using System.Collections.Generic;
using umi3d.browser.utils;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using umi3dVRBrowsersBase.connection;
using umi3dVRBrowsersBase.interactions;
using umi3dVRBrowsersBase.navigation;
using UnityEngine;
using UnityEngine.UIElements;

namespace umi3dVRBrowsersBase.ikManagement
{
    public interface IUmi3dPlayer
    {
        void OnValidate()
        {
            OnPlayerFieldUpdate();
            OnMainCameraFieldUpdate();
            OnLeftHandFieldUpdate();
            OnRightHandFieldUpdate();
            OnPrefabYBotFieldUpdate();
            OnPrefabInvisibleSkeletonFieldUpdate();
            OnPrefabArcImpactNotPossibleFieldUpdate();
            OnPrefabArcImpactFieldUpdate();
            OnPrefabArcStepDisplayerFieldUpdate();
            OnPrefabSelectorFieldUpdate();
        }
        void OnPlayerFieldUpdate();
        void OnMainCameraFieldUpdate();
        void OnLeftHandFieldUpdate();
        void OnRightHandFieldUpdate();

        void OnPrefabYBotFieldUpdate();
        void OnPrefabInvisibleSkeletonFieldUpdate();
        void OnPrefabArcImpactNotPossibleFieldUpdate();
        void OnPrefabArcImpactFieldUpdate();
        void OnPrefabArcStepDisplayerFieldUpdate();
        void OnPrefabSelectorFieldUpdate();
    }

    public interface IUmi3dPlayerLife
    {
        void Create();
        void AddComponents();
        void SetComponents();
        void SetHierarchy();
        void Clear();
    }

    public partial class Umi3dPlayerManager : PersistentSingleBehaviour<Umi3dPlayerManager>, IUmi3dPlayer, IUmi3dPlayerLife
    {
        [Header("Player Extern SDK")]
        [Tooltip("Player, the XR root")]
        public GameObject Player;
        [Tooltip("Main camera")]
        public Camera MainCamera;
        [Tooltip("Player left hand")]
        public GameObject LeftHand;
        [Tooltip("player right hand")]
        public GameObject RightHand;

        [Header("Player Umi3d SDK")]
        [Tooltip("Prefab for the YBot")]
        public GameObject PrefabUnityYBot;
        [Tooltip("Prefab for the invisible unit skeleton")]
        public GameObject PrefabInvisibleUnitSkeleton;
        [Tooltip("Prefab for the arc impact not possible")]
        public GameObject PrefabArcImpactNotPossible;
        [Tooltip("Prefab for the arc impact")]
        public GameObject PrefabArcImpact;
        [Tooltip("Prefab for the arc step displayer")]
        public GameObject PrefabArcStepDisplayer;
        [Tooltip("Prefab for the selector")]
        public GameObject PrefabSelector;

        #region Components

        [HideInInspector]
        public umi3dVRBrowsersBase.navigation.UMI3DNavigation VRNavigation;
        [HideInInspector]
        public umi3d.cdk.UMI3DNavigation Navigation;
        [HideInInspector]
        public WaitForServer WaitForServer;
        [HideInInspector]
        public VRInteractionMapper InteractionMapper;
        [HideInInspector]
        public SnapTurn SnapTurn;
        [HideInInspector]
        public UMI3DClientUserTrackingBone CameraTracking;

        #endregion

        #region Sub manager class

        [Header("Avatar")]
        public Umi3dIkManager IkManager;
        [HideInInspector]
        public Umi3dHandManager HandManager;

        #endregion

        protected override void Awake()
        {
            base.Awake();
            transform.parent = UMI3DCollaborationEnvironmentLoader.Instance.transform;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [ContextMenu("Create Player")]
        void IUmi3dPlayerLife.Create()
        {
            if (IkManager == null) IkManager = new Umi3dIkManager();
            if (HandManager == null) HandManager = new Umi3dHandManager();

            (IkManager as IUmi3dPlayerLife).Create();
            (HandManager as IUmi3dPlayerLife).Create();

            (this as IUmi3dPlayerLife).AddComponents();
            (this as IUmi3dPlayerLife).SetComponents();
            (this as IUmi3dPlayerLife).SetHierarchy();

            (this as IUmi3dPlayer).OnValidate();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayerLife.AddComponents()
        {
            this.GetOrAddComponent<umi3dVRBrowsersBase.navigation.UMI3DNavigation>(out VRNavigation);
            this.GetOrAddComponent<umi3d.cdk.UMI3DNavigation>(out Navigation);
            this.GetOrAddComponent<WaitForServer>(out WaitForServer);
            this.GetOrAddComponent<VRInteractionMapper>(out InteractionMapper);
            this.GetOrAddComponent<SnapTurn>(out SnapTurn);

            (IkManager as IUmi3dPlayerLife).AddComponents();
            (HandManager as IUmi3dPlayerLife).AddComponents();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayerLife.SetComponents()
        {
            if (Navigation.navigations == null) Navigation.navigations = new List<AbstractNavigation>();
            if (!Navigation.navigations.Contains(VRNavigation)) Navigation.navigations.Add(VRNavigation);

            InteractionMapper.shouldProjectHoldableEventOnSpecificInput = true;

            (IkManager as IUmi3dPlayerLife).SetComponents();
            (HandManager as IUmi3dPlayerLife).SetComponents();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayerLife.SetHierarchy() 
        {
            this.gameObject.Add(IkManager.Avatar);

            (IkManager as IUmi3dPlayerLife).SetHierarchy();
            (HandManager as IUmi3dPlayerLife).SetHierarchy();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [ContextMenu("Clear Player")]
        void IUmi3dPlayerLife.Clear()
        {
            (IkManager as IUmi3dPlayerLife).Clear();
            (HandManager as IUmi3dPlayerLife).Clear();

            IkManager = null;
            HandManager = null;
        }

        [ContextMenu("Validate")]
        void Validate()
        {
            var umi3dPlayer = this as IUmi3dPlayer;
            umi3dPlayer.OnValidate();
        }

        

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnLeftHandFieldUpdate()
        {
            if (LeftHand == null) return;

            (IkManager as IUmi3dPlayer)?.OnLeftHandFieldUpdate();
            (HandManager as IUmi3dPlayer)?.OnLeftHandFieldUpdate();

            LeftHand.Add(HandManager.LeftHand.RootHand);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnMainCameraFieldUpdate()
        {
            if (MainCamera == null) return;

            MainCamera.GetOrAddComponent(out CameraTracking);
            CameraTracking.boneType = BoneType.Viewpoint;
            CameraTracking.isTracked = true;

            (IkManager as IUmi3dPlayer)?.OnMainCameraFieldUpdate();
            (HandManager as IUmi3dPlayer)?.OnMainCameraFieldUpdate();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnPlayerFieldUpdate()
        {
            if (Player == null) return;

            this.gameObject.Add(Player);

            (IkManager as IUmi3dPlayer)?.OnPlayerFieldUpdate();
            (HandManager as IUmi3dPlayer)?.OnPlayerFieldUpdate();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnRightHandFieldUpdate()
        {
            if (RightHand == null) return;

            (IkManager as IUmi3dPlayer)?.OnRightHandFieldUpdate();
            (HandManager as IUmi3dPlayer)?.OnRightHandFieldUpdate();

            RightHand.Add(HandManager.RightHand.RootHand);
        }

        /// <summary>
        /// 
        /// </summary>
        void IUmi3dPlayer.OnPrefabYBotFieldUpdate()
        {
            if (PrefabUnityYBot == null) return;

            (IkManager as IUmi3dPlayer)?.OnPrefabYBotFieldUpdate();
            (HandManager as IUmi3dPlayer)?.OnPrefabYBotFieldUpdate();
        }

        void IUmi3dPlayer.OnPrefabInvisibleSkeletonFieldUpdate()
        {
            if (PrefabInvisibleUnitSkeleton == null) return;

            (IkManager as IUmi3dPlayer)?.OnPrefabInvisibleSkeletonFieldUpdate();
            (HandManager as IUmi3dPlayer)?.OnPrefabInvisibleSkeletonFieldUpdate();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnPrefabArcImpactFieldUpdate()
        {
            if (PrefabArcImpact == null) return;

            (IkManager as IUmi3dPlayer)?.OnPrefabArcImpactFieldUpdate();
            (HandManager as IUmi3dPlayer)?.OnPrefabArcImpactFieldUpdate();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnPrefabArcImpactNotPossibleFieldUpdate()
        {
            if (PrefabArcImpactNotPossible == null) return;

            (IkManager as IUmi3dPlayer)?.OnPrefabArcImpactNotPossibleFieldUpdate();
            (HandManager as IUmi3dPlayer)?.OnPrefabArcImpactNotPossibleFieldUpdate();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnPrefabArcStepDisplayerFieldUpdate()
        {
            if (PrefabArcStepDisplayer == null) return;

            (IkManager as IUmi3dPlayer)?.OnPrefabArcStepDisplayerFieldUpdate();
            (HandManager as IUmi3dPlayer)?.OnPrefabArcStepDisplayerFieldUpdate();
        }

        void IUmi3dPlayer.OnPrefabSelectorFieldUpdate()
        {
            if (PrefabSelector == null) return;

            (IkManager as IUmi3dPlayer)?.OnPrefabSelectorFieldUpdate();
            (HandManager as IUmi3dPlayer)?.OnPrefabSelectorFieldUpdate();
        }
    }

    public partial class Umi3dPlayerManager : ITeleportable
    {
        public bool IsTeleportationAllow { get; set; } = true;

        /// <summary>
        /// Teleport the player at <paramref name="target"/>.
        /// </summary>
        /// <param name="target"></param>
        public void Teleport(Vector3 target)
        {
            Vector3 offset = this.transform.rotation * MainCamera.transform.localPosition;
            offset *= -1;
            offset.y = 0f;
            (this as ITeleportable).TeleportWithOffset(target, offset);
        }

        /// <summary>
        /// Teleports the player at the <paramref name="arc"/> impact point.
        /// </summary>
        protected void Teleport(TeleportArc arc)
        {
            Vector3? position = arc.GetPointedPoint();

            if (position.HasValue) Teleport(position.Value);
        }

        /// <summary>
        /// Teleport the player at the <see cref="Umi3dHandManager.LeftHand"/> arc impact point.
        /// </summary>
        [ContextMenu("Teleport left")]
        public void TeleportLeft() => Teleport(HandManager.LeftHand.ArcController);
        /// <summary>
        /// Teleport the player at the <see cref="Umi3dHandManager.RightHand*"/> arc impact point.
        /// </summary>
        [ContextMenu("Teleport right")]
        public void TeleportRight() => Teleport(HandManager.RightHand.ArcController);
    }
}
