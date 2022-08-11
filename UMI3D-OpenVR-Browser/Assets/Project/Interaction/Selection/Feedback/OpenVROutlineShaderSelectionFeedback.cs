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
    public class OpenVROutlineShaderSelectionFeedback : MonoBehaviour, IUpdatablePersistentFeedback
    {
        [Header("Target object")]
        /// <summary>
        /// Glowing effect shader
        /// </summary>
        [SerializeField]
        private Shader pointedOutlineShader;

        /// <summary>
        /// Stores shaders while the predicted object receives a glowing effect
        /// </summary>
        private Dictionary<int, OverridenRendererInfo> cachedRenderers = new Dictionary<int, OverridenRendererInfo>();


        protected class OverridenRendererInfo
        {
            public int objId;
            public Renderer overridenRenderer;
            public Dictionary<Material, Shader> overridenMaterials = new Dictionary<Material, Shader>();
        }

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
                Outline(interactableSelectionData.selectedObject, pointedOutlineShader);
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

        public void UpdateFeedback(AbstractSelectionData selectionData)
        {
            SelectionIntentData<InteractableContainer> interactableSelectionData = selectionData as SelectionIntentData<InteractableContainer>;
            if (interactableSelectionData == null)
                return;
            var id = interactableSelectionData.selectedObject.GetInstanceID();
            if (cachedRenderers.ContainsKey(id))
            {
                var renderer = cachedRenderers[id].overridenRenderer;
                if (!cachedRenderers[id].overridenMaterials.ContainsKey(renderer.material)) // happens when material is changed at runtime
                {
                    cachedRenderers[id].overridenMaterials.Add(renderer.material, renderer.material.shader);
                    cachedRenderers[id].overridenRenderer.material.shader = pointedOutlineShader;
                }
            }
        }

        /// <summary>
        /// Set up the outline shader on the material
        /// </summary>
        /// <param name="interactable"></param>
        /// <param name="shader"></param>
        private void Outline(InteractableContainer interactable, Shader shader)
        {
            var renderer = interactable.gameObject.GetComponentInChildren<Renderer>();
            var id = interactable.GetInstanceID();
            if (renderer != null
                && renderer.material != null
                && !cachedRenderers.ContainsKey(id)
                && renderer.material.shader != shader)
            {
                var savedRenderer = new OverridenRendererInfo()
                {
                    objId = id,
                    overridenRenderer = renderer
                };
                savedRenderer.overridenMaterials.Add(renderer.material, renderer.material.shader);
                cachedRenderers.Add(id, savedRenderer);
                renderer.material.shader = shader;
            }
        }

        /// <summary>
        /// Remove the outline shader from the material
        /// </summary>
        /// <param name="interactable"></param>
        /// <param name="shader"></param>
        private void DisableOutline(InteractableContainer interactable)
        {
            if (interactable != null)
            {
                var renderer = interactable.gameObject.GetComponentInChildren<Renderer>();
                var id = interactable.GetInstanceID();
                if (renderer != null && renderer.material != null && cachedRenderers.ContainsKey(id))
                {
                    foreach (var mat in renderer.materials)
                    {
                        if (cachedRenderers[id].overridenMaterials.ContainsKey(mat))
                            mat.shader = cachedRenderers[id].overridenMaterials[mat]; // remove all the material overrides that have been performed
                    }
                }
                cachedRenderers.Remove(id);
            }
        }
    }
}