/*
Copyright 2019 - 2021 Inetum
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
using umi3d.cdk.interaction.selection.feedback;
using umi3d.cdk.interaction.selection.intentdetector;
using umi3d.cdk.interaction.selection.projector;
using UnityEngine;

namespace umi3dbrowser.openvr.interaction.selection
{
    /// <summary>
    /// Selector for <see cref="InteractableContainer"/> objects on VR browsers
    /// </summary>
    public class InteractableVRSelector : AbstractVRSelector<InteractableContainer>
    {
        /// <summary>
        /// Selection Intent Detector (virtual pointing)
        /// </summary>
        public AbstractPointingInteractableDetector pointingDetector;

        /// <summary>
        /// Selection Intent Detector (virtual hand)
        /// </summary>
        public AbstractGrabInteractableDetector grabDetector;


        [HideInInspector]
        public class InteractableSelectionData : SelectionData<InteractableContainer>
        {
            public AbstractTool tool;
        }

        /// <summary>
        /// Previously selected objects
        /// </summary>
        [HideInInspector]
        public SelectionCache<InteractableSelectionData> selectionCache = new SelectionCache<InteractableSelectionData>();

        /// <summary>
        /// Manages projection on the controller
        /// </summary>
        [HideInInspector]
        public InteractableProjector projector = new InteractableProjector();

        /// <summary>
        /// Shortcut for last selected object info
        /// </summary>
        public InteractableSelectionData LastSelected => selectionCache.Objects.Last?.Value;

        /// <summary>
        /// Manage feedback when an object is selected
        /// </summary>
        public AbstractSelectionFeedbackHandler<InteractableContainer> selectionFeedbackHandler;

        /// <inheritdoc/>
        protected override void ActivateInternal()
        {
            base.ActivateInternal();
            pointingDetector.Init(controller);
            grabDetector.Init(controller);
            selectionEvent.AddListener(OnSelection);
            deselectionEvent.AddListener(OnDeselection);
        }

        /// <inheritdoc/>
        protected override void DeactivateInternal()
        {
            base.DeactivateInternal();
            pointingDetector.Reinit();
            grabDetector.Reinit();
            selectionEvent.RemoveAllListeners();
            deselectionEvent.RemoveAllListeners();
        }

        /// <summary>
        /// Checks if the interactable :
        ///     - exists
        ///     - has at least one associated interaction
        ///     - has not been seen its tool projected yet
        /// </summary>
        /// <param name="icToSelect"></param>
        /// <returns></returns>
        private bool CanSelect(InteractableContainer icToSelect)
        {
            return icToSelect != null
                    && icToSelect != LastSelected?.selectedObject
                    && icToSelect.Interactable.dto.interactions != null
                    && icToSelect.Interactable.dto.interactions.Count > 0
                    && !InteractionMapper.Instance.IsToolSelected(icToSelect.Interactable.dto.id);
        }

        /// <summary>
        /// Select an object according to the current context
        /// </summary>
        public override void Select()
        {
            InteractableContainer interactableToSelectProximity = null;
            InteractableContainer interactableToSelectPointed = null;

            //priority for proximity selection
            if (grabDetector.isRunning)
            {
                interactableToSelectProximity = grabDetector.PredictTarget();
                detectionCacheProximity.Add(interactableToSelectProximity);
                if (CanSelect(interactableToSelectProximity))
                {
                    Select(interactableToSelectProximity, out InteractableSelectionData selectionData);
                    selectionData.detectionOrigin = SelectionData<InteractableContainer>.DetectionOrigin.PROXIMITY;
                    selectionCache.Add(selectionData);
                    selectionEvent.Invoke(selectionData);
                    return;
                }
            }

            if (pointingDetector.isRunning)
            {
                interactableToSelectPointed = pointingDetector.PredictTarget();
                detectionCachePointing.Add(interactableToSelectPointed);
                if (CanSelect(interactableToSelectPointed))
                {
                    Select(interactableToSelectPointed, out InteractableSelectionData selectionData);
                    selectionData.detectionOrigin = SelectionData<InteractableContainer>.DetectionOrigin.POINTING;
                    selectionCache.Add(selectionData);
                    selectionEvent.Invoke(selectionData);
                    return;
                }
            }

            if (interactableToSelectProximity == null
                    && interactableToSelectPointed == null
                    && LastSelected != null)
            {
                Deselect(LastSelected);
                selectionCache.Add(null);
                return;
            }
        }

        /// <summary>
        /// Deselect object and remove feedback
        /// </summary>
        /// <param name="interactableToDeselectInfo"></param>
        private void Deselect(InteractableSelectionData interactableToDeselectInfo)
        {
            if (LastSelected != null
                || (LastSelected == null && LastSelected.tool != null)) //destroyed object but tool still there
                projector.Release(LastSelected.tool, controller);
            deselectionEvent.Invoke(interactableToDeselectInfo);
        }

        /// <summary>
        /// Select an interactable and provides its infos
        /// </summary>
        /// <param name="icToSelect"></param>
        /// <param name="selectionInfo"></param>
        private void Select(InteractableContainer icToSelect, out InteractableSelectionData selectionInfo)
        {
            var interactionTool = AbstractInteractionMapper.Instance.GetTool(icToSelect.Interactable.dto.id);
            selectionInfo = new InteractableSelectionData() { selectedObject = icToSelect, tool = interactionTool };

            if (LastSelected != null
                || (!controller.IsAvailableFor(interactionTool) && controller.IsCompatibleWith(interactionTool)))
                // happens when an object is destroyed but the tool is not released
                Deselect(LastSelected);
            
            selectionFeedbackHandler.StartFeedback(selectionInfo);
            projector.Project(interactionTool, icToSelect.Interactable.dto.id, controller);
        }

        /// <summary>
        /// Triggered when an Interactable is selected
        /// </summary>
        /// <param name="deselectionData"></param>
        protected override void OnSelection(SelectionData<InteractableContainer> selectionData)
        {
            selectionFeedbackHandler.StartFeedback(selectionData);
        }

        /// <summary>
        /// Triggered when an Interactable is deselected
        /// </summary>
        /// <param name="deselectionData"></param>
        protected override void OnDeselection(SelectionData<InteractableContainer> deselectionData)
        {
            selectionFeedbackHandler.EndFeedback(deselectionData);
        }
    }
}