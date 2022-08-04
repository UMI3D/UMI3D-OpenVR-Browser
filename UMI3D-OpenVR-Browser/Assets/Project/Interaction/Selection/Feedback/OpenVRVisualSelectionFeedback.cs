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
using umi3d.cdk.interaction.selection;
using umi3d.cdk.interaction.selection.cursor;
using umi3d.cdk.interaction.selection.feedback;
using umi3dVRBrowsersBase.interactions.selection;
using UnityEngine;

namespace umi3dbrowser.openvr.interaction.selection.feedback
{
    /// <summary>
    /// A visual selection feedback for OpenVR devices
    /// Only used for Interactable as Selectable have their own system.
    /// </summary>
    public class OpenVRVisualSelectionFeedback : MonoBehaviour, IPersistentFeedback
    {
        [Header("Target object")]
        /// <summary>
        /// Glowing effect shader
        /// </summary>
        [SerializeField]
        private Shader pointedOutlineShader;

        /// <summary>
        /// Glowing effect shader
        /// </summary>
        [SerializeField]
        private Material pointedOutlineMaterial;

        /// <summary>
        /// Glowing effect shader
        /// </summary>
        [SerializeField]
        private Shader proximityOutlineShader;

        /// <summary>
        /// Glowing effect shader
        /// </summary>
        [SerializeField]
        private Material proximityOutlineMaterial;

        /// <summary>
        /// Stores shaders while the predicted object receives a glowing effect
        /// </summary>
        private Dictionary<int, Shader> cachedShaders = new Dictionary<int, Shader>();

        private AbstractCursor pointingCursor;

        private void Awake()
        {
            var selectionManager = GetComponentInParent<VRSelectionManager>();
            pointingCursor = selectionManager.pointingCursor;
        }


        public void Activate(AbstractSelectionData selectionData)
        {
            SelectionData<InteractableContainer> interactableSelectionData = selectionData as SelectionData<InteractableContainer>;
            if (interactableSelectionData == null)
                return;
            if (interactableSelectionData.detectionOrigin == DetectionOrigin.POINTING)
            {
                Outline(interactableSelectionData.selectedObject, pointedOutlineShader);
                pointingCursor.ChangeAccordingToSelection(selectionData);
            }
            else if (interactableSelectionData.detectionOrigin == DetectionOrigin.PROXIMITY)
            {
                Outline(interactableSelectionData.selectedObject, proximityOutlineShader);
            }               
        }

        public void Deactivate(AbstractSelectionData selectionData)
        {
            SelectionData<InteractableContainer> interactableSelectionData = selectionData as SelectionData<InteractableContainer>;
            if (interactableSelectionData == null)
                return;
            DisableOutline(interactableSelectionData.selectedObject);
            if (interactableSelectionData.detectionOrigin == DetectionOrigin.POINTING)
                pointingCursor.ChangeAccordingToSelection(null);
        }

        private void Outline(InteractableContainer interactable, Shader shader)
        {
            var renderer = interactable.gameObject.GetComponentInChildren<Renderer>();
            var id = interactable.GetInstanceID();
            if (renderer != null
                && renderer.material != null
                && !cachedShaders.ContainsKey(id)
                && renderer.material.shader != shader)
            {
                cachedShaders.Add(id, renderer.material.shader);
                renderer.material.shader = shader;
            }
        }

        private void DisableOutline(InteractableContainer interactable)
        {
            if (interactable != null)
            {
                var renderer = interactable.gameObject.GetComponentInChildren<Renderer>();
                var id = interactable.GetInstanceID();
                if (renderer != null && renderer.material != null && cachedShaders.ContainsKey(id))
                {
                    renderer.material.shader = cachedShaders[id];
                }
                cachedShaders.Remove(id);
            }
        }
    }
}