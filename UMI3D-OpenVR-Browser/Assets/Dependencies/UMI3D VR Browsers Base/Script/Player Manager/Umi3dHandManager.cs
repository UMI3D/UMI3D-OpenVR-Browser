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
using UnityEngine;

namespace umi3dVRBrowsersBase.ikManagement
{
    [System.Serializable]
    public class Umi3dHandManager: IUmi3dPlayer, IUmi3dPlayerLife
    {
        [HideInInspector]
        public Umi3dHandController LeftHand;
        [HideInInspector]
        public Umi3dHandController RightHand;

        #region IUmi3dPlayerLife

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayerLife.Create()
        {
            if (LeftHand == null) LeftHand = new Umi3dHandController { Goal = AvatarIKGoal.LeftHand };
            LeftHand.Goal = AvatarIKGoal.LeftHand;
            if (RightHand == null) RightHand = new Umi3dHandController { Goal = AvatarIKGoal.RightHand };
            RightHand.Goal = AvatarIKGoal.RightHand;

            (LeftHand as IUmi3dPlayerLife).Create();
            (RightHand as IUmi3dPlayerLife).Create();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayerLife.AddComponents()
        {
            (LeftHand as IUmi3dPlayerLife).AddComponents();
            (RightHand as IUmi3dPlayerLife).AddComponents();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayerLife.SetComponents()
        {
            (LeftHand as IUmi3dPlayerLife).SetComponents();
            (RightHand as IUmi3dPlayerLife).SetComponents();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayerLife.SetHierarchy()
        {
            (LeftHand as IUmi3dPlayerLife).SetHierarchy();
            (RightHand as IUmi3dPlayerLife).SetHierarchy();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayerLife.Clear()
        {
            (LeftHand as IUmi3dPlayerLife).Clear();
            (RightHand as IUmi3dPlayerLife).Clear();
        }

        #endregion

        #region IUmi3dPlayer

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnLeftHandFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.LeftHand == null) return;

            (LeftHand as IUmi3dPlayer).OnLeftHandFieldUpdate();
            (RightHand as IUmi3dPlayer).OnLeftHandFieldUpdate();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnMainCameraFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.MainCamera == null) return;

            (LeftHand as IUmi3dPlayer).OnMainCameraFieldUpdate();
            (RightHand as IUmi3dPlayer).OnMainCameraFieldUpdate();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnPlayerFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.Player == null) return;

            (LeftHand as IUmi3dPlayer).OnPlayerFieldUpdate();
            (RightHand as IUmi3dPlayer).OnPlayerFieldUpdate();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnRightHandFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.RightHand == null) return;

            (LeftHand as IUmi3dPlayer).OnRightHandFieldUpdate();
            (RightHand as IUmi3dPlayer).OnRightHandFieldUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        void IUmi3dPlayer.OnPrefabYBotFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.PrefabUnityYBot == null) return;

            (LeftHand as IUmi3dPlayer)?.OnPrefabYBotFieldUpdate();
            (RightHand as IUmi3dPlayer)?.OnPrefabYBotFieldUpdate();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnPrefabInvisibleSkeletonFieldUpdate()
        {

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnPrefabArcImpactFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.PrefabArcImpact == null) return;

            (LeftHand as IUmi3dPlayer).OnPrefabArcImpactFieldUpdate();
            (RightHand as IUmi3dPlayer).OnPrefabArcImpactFieldUpdate();
        }

        void IUmi3dPlayer.OnPrefabArcImpactNotPossibleFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.PrefabArcImpactNotPossible == null) return;

            (LeftHand as IUmi3dPlayer).OnPrefabArcImpactNotPossibleFieldUpdate();
            (RightHand as IUmi3dPlayer).OnPrefabArcImpactNotPossibleFieldUpdate();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IUmi3dPlayer.OnPrefabArcStepDisplayerFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.PrefabArcStepDisplayer == null) return;

            (LeftHand as IUmi3dPlayer).OnPrefabArcStepDisplayerFieldUpdate();
            (RightHand as IUmi3dPlayer).OnPrefabArcStepDisplayerFieldUpdate();
        }

        void IUmi3dPlayer.OnPrefabSelectorFieldUpdate()
        {
            if (Umi3dPlayerManager.Instance.PrefabSelector == null) return;

            (LeftHand as IUmi3dPlayer).OnPrefabSelectorFieldUpdate();
            (RightHand as IUmi3dPlayer).OnPrefabSelectorFieldUpdate();
        }

        #endregion
    }
}
