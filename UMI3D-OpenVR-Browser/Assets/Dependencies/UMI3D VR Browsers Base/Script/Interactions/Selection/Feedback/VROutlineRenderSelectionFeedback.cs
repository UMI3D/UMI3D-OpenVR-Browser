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

using umi3d.cdk.interaction;
using umi3dBrowsers.interaction.selection.cursor;
using umi3dVRBrowsersBase.interactions.selection;
using UnityEngine;

namespace umi3dBrowsers.interaction.selection.feedback
{
    /// <summary>
    /// A visual selection feedback for VR devices
    /// Only used for Interactable as Selectable have their own system.
    /// </summary>
    public class VROutlineRenderSelectionFeedback : MonoBehaviour, IPersistentFeedback
    {
        /// <summary>
        /// Outlined target info
        /// </summary>
        private TargetOutlineInfo targetInfo;

        public class TargetOutlineInfo
        {
            public GameObject targetObject;
            public LayerMask originalLayer;
            public Renderer renderer;
        }

        [Tooltip("Layer associated with selection outline in URP settings")]
        public LayerMask selectionOutlineLayer;



        /// <inheritdoc/>
        public void Activate(AbstractSelectionData selectionData)
        {
            SelectionIntentData<InteractableContainer> interactableSelectionData = selectionData as SelectionIntentData<InteractableContainer>;
            if (interactableSelectionData == null)
                return;
            if (interactableSelectionData.detectionOrigin == DetectionOrigin.POINTING)
                Outline(interactableSelectionData.selectedObject);
        }

        /// <inheritdoc/>
        public void Deactivate(AbstractSelectionData selectionData)
        {
            SelectionIntentData<InteractableContainer> interactableSelectionData = selectionData as SelectionIntentData<InteractableContainer>;
            if (interactableSelectionData == null)
                return;
            DisableOutline(interactableSelectionData.selectedObject);
        }

        /// <summary>
        /// Outline the object by placing them on the right layer
        /// </summary>
        /// <param name="ic"></param>
        public void Outline(InteractableContainer ic)
        {
            var renderer = ic.GetComponentInChildren<Renderer>();
            targetInfo = new TargetOutlineInfo()
            {
                targetObject = ic.gameObject,
                renderer = renderer,
                originalLayer = ic.gameObject.layer
            };
            renderer.gameObject.layer = (int)Mathf.Log(selectionOutlineLayer, 2); // strange but necessary to convert
        }

        /// <summary>
        /// Disable outline by replacing the object on its previous layer
        /// </summary>
        /// <param name="ic"></param>
        private void DisableOutline(InteractableContainer ic)
        {
            if (targetInfo?.renderer == null)
                return;
            targetInfo.renderer.gameObject.layer = targetInfo.originalLayer;
            targetInfo = null;
        }
    }
}