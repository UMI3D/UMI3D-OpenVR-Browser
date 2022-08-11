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

using System.Collections.Generic;
using umi3d.cdk.interaction;
using umi3dBrowsers.interaction.selection;
using umi3dBrowsers.interaction.selection.cursor;
using umi3dBrowsers.interaction.selection.feedback;
using umi3dVRBrowsersBase.interactions.selection;
using UnityEngine;

namespace umi3dBrowserOpenVR.interaction.selection.feedback
{
    /// <summary>
    /// A visual selection feedback for OpenVR devices
    /// Only used for Interactable as Selectable have their own system.
    /// </summary>
    public class OpenVROutlineRenderSelectionFeedback : MonoBehaviour, IPersistentFeedback
    {
        private GameObject targetObject;
        private LayerMask targetCachedLayer;

        [Tooltip("Layer associated with selection outline in URP settings")]
        public LayerMask selectionOutlineLayer;

        private AbstractCursor pointingCursor;
        private AbstractCursor grabCursor;

        private void Awake()
        {
            var selectionManager = GetComponentInParent<VRSelectionManager>();
            pointingCursor = selectionManager.pointingCursor;
            grabCursor = selectionManager.grabCursor;
        }

        /// <inheritdoc/>
        public void Activate(AbstractSelectionData selectionData)
        {
            SelectionIntentData<InteractableContainer> interactableSelectionData = selectionData as SelectionIntentData<InteractableContainer>;
            if (interactableSelectionData == null)
                return;
            if (interactableSelectionData.detectionOrigin == DetectionOrigin.POINTING)
            {
                Outline(interactableSelectionData.selectedObject);
                pointingCursor.ChangeAccordingToSelection(selectionData);
            }
            else if (interactableSelectionData.detectionOrigin == DetectionOrigin.PROXIMITY)
            {
                grabCursor.ChangeAccordingToSelection(selectionData);
            } 
        }

        /// <inheritdoc/>
        public void Deactivate(AbstractSelectionData selectionData)
        {
            SelectionIntentData<InteractableContainer> interactableSelectionData = selectionData as SelectionIntentData<InteractableContainer>;
            if (interactableSelectionData == null)
                return;
            DisableOutline(interactableSelectionData.selectedObject);
            if (interactableSelectionData.detectionOrigin == DetectionOrigin.POINTING)
                pointingCursor.ChangeAccordingToSelection(null);
            else if (interactableSelectionData.detectionOrigin == DetectionOrigin.PROXIMITY)
                grabCursor.ChangeAccordingToSelection(null);
        }

        public void Outline(InteractableContainer ic)
        {
            targetCachedLayer = ic.gameObject.layer;
            ic.gameObject.layer = selectionOutlineLayer;
            targetObject = ic.gameObject;
        }

        private void DisableOutline(InteractableContainer ic)
        {
            targetObject.layer = targetCachedLayer;
            targetObject = null;
        }
    }
}