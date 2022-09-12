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
using umi3dBrowsers.interaction.selection.intentdetector;
using umi3dBrowsers.interaction.selection.projector;
using UnityEngine;

namespace umi3dVRBrowsersBase.interactions.selection.selector
{
    /// <summary>
    /// Selector for <see cref="InteractableContainer"/> objects on VR browsers
    /// </summary>
    public class InteractableVRSelector : AbstractVRSelector<InteractableContainer>
    {
        /// <summary>
        /// Selection Intent Detector (virtual pointing)
        /// </summary>
        [Tooltip("Selection Intent Detector for virtual pointing.")]
        public AbstractPointingInteractableDetector pointingDetector;

        /// <summary>
        /// Optional selection Intent Detector (virtual pointing)
        /// </summary>
        [Tooltip("Optional second delection Intent Detector for virtual pointing.")]
        public AbstractPointingInteractableDetector pointingSecondaryDetector;

        /// <summary>
        /// Selection Intent Detector (virtual hand)
        /// </summary>
        [Tooltip("Selection Intent Detector for virtual hand (grab).")]
        public AbstractGrabInteractableDetector grabDetector;

        /// <summary>
        /// Previously detected objects for virtual hand
        /// </summary>
        [HideInInspector]
        public Cache<InteractableContainer> detectionCacheProximity = new Cache<InteractableContainer>();

        /// <summary>
        /// Previously detected objects from virtual pointing
        /// </summary>
        [HideInInspector]
        public Cache<InteractableContainer> detectionCachePointing = new Cache<InteractableContainer>();

        #region constructors

        private InteractableVRSelector() : base()
        {
            projector = new InteractableProjector();
        }

        /// <inheritdoc/>
        [HideInInspector]
        public class InteractableSelectionData : SelectionIntentData<InteractableContainer>
        {
            /// <summary>
            /// Tool associated to the Interactable
            /// </summary>
            public AbstractTool tool;
        }

        #endregion constructors

        #region lifecycle

        /// <inheritdoc/>
        protected override void ActivateInternal()
        {
            base.ActivateInternal();
            pointingDetector.Init(controller);
            grabDetector.Init(controller);
            projector = new InteractableProjector();
        }

        /// <inheritdoc/>
        protected override void DeactivateInternal()
        {
            base.DeactivateInternal();
            pointingDetector.Reinit();
            grabDetector.Reinit();
        }

        #endregion lifecycle

        #region selection

        /// <summary>
        /// Checks if the interactable :
        ///     - exists
        ///     - has at least one associated interaction
        ///     - has not been seen its tool projected yet
        /// </summary>
        /// <param name="icToSelect"></param>
        /// <returns></returns>
        protected override bool CanSelect(InteractableContainer icToSelect)
        {
            return icToSelect != null
                    && icToSelect.Interactable.dto.interactions != null
                    && icToSelect.Interactable.dto.interactions.Count > 0
                    && !InteractionMapper.Instance.IsToolSelected(icToSelect.Interactable.dto.id);
        }

        /// <summary>
        /// Is the passed object currently slected by this selector?
        /// </summary>
        /// <param name="ic"></param>
        /// <returns></returns>
        private bool IsObjectSelected(InteractableContainer ic)
        {
            return ic == LastSelected?.selectedObject
                    || InteractionMapper.Instance.IsToolSelected(ic.Interactable.dto.id);
        }

        /// <inheritdoc/>
        public override List<SelectionIntentData> GetIntentDetections()
        {
            var possibleSelection = new List<SelectionIntentData>();

            if (grabDetector.isRunning)
            {
                var interactableToSelectProximity = grabDetector.PredictTarget();
                var detectionInfo = new InteractableSelectionData
                {
                    selectedObject = interactableToSelectProximity,
                    controller = controller,
                    detectionOrigin = DetectionOrigin.PROXIMITY,
                };
                detectionCacheProximity.Add(detectionInfo as SelectionIntentData<InteractableContainer>);
                if (CanSelect(interactableToSelectProximity))
                    possibleSelection.Add(detectionInfo);
            }

            if (pointingDetector.isRunning)
            {
                var interactableToSelectPointed = pointingDetector.PredictTarget();
                var detectionInfo = new InteractableSelectionData
                {
                    selectedObject = interactableToSelectPointed,
                    controller = controller,
                    detectionOrigin = DetectionOrigin.POINTING,
                };
                detectionCachePointing.Add(detectionInfo);
                if (CanSelect(interactableToSelectPointed))
                    possibleSelection.Add(detectionInfo);
            }

            if (pointingSecondaryDetector != null && pointingSecondaryDetector.isRunning)
            {
                var interactableToSelectPointed = pointingDetector.PredictTarget();
                var detectionInfo = new InteractableSelectionData
                {
                    selectedObject = interactableToSelectPointed,
                    controller = controller,
                    detectionOrigin = DetectionOrigin.POINTING,
                };
                detectionCachePointing.Add(detectionInfo);
                if (CanSelect(interactableToSelectPointed))
                    possibleSelection.Add(detectionInfo);
            }

            foreach (var poss in possibleSelection)
                propositionSelectionCache.Add(poss);
            return possibleSelection;
        }

        /// <summary>
        /// Deselect object and remove feedback
        /// </summary>
        /// <param name="interactableToDeselectInfo"></param>
        protected override void Deselect(SelectionIntentData<InteractableContainer> interactableToDeselectInfo)
        {
            var icToDeselectinfo = interactableToDeselectInfo as InteractableSelectionData;

            if (icToDeselectinfo.tool != null)
                (projector as InteractableProjector)?.Release(icToDeselectinfo.tool, controller);
            //! there is a case where the tool is not released and the material is not changed
            isSelecting = false;
            deselectionEvent.Invoke(interactableToDeselectInfo);

            pointingDetector.Reinit();
            grabDetector.Reinit();
        }

        /// <summary>
        /// Select an interactable and provides its infos
        /// </summary>
        /// <param name="selectionInfo"></param>
        /// <param name="selectionInfo"></param>
        protected override void Select(SelectionIntentData<InteractableContainer> selectionInfo)
        {
            if (isSelecting)
            {
                if (selectionInfo == null && LastSelected != null) //the selector was selecting something before and should remember it choose to select nothing this time
                {
                    Deselect(LastSelected);
                    LastSelected = null;
                    return;
                }
                else if (selectionInfo == null)
                    throw new System.ArgumentNullException("Argument should be null only if moving outside of an object");
                else if (IsObjectSelected(selectionInfo.selectedObject)) //  the selector was selecting the same target before
                    return;
            }

            var interactionTool = AbstractInteractionMapper.Instance.GetTool(selectionInfo.selectedObject.Interactable.dto.id);
            if (selectionInfo is InteractableSelectionData)
                (selectionInfo as InteractableSelectionData).tool = interactionTool;

            if (isSelecting
                && (LastSelected != null || (!controller.IsAvailableFor(interactionTool) && controller.IsCompatibleWith(interactionTool))))
                // happens when an object is destroyed but the tool is not released
                Deselect(LastSelected);

            if (controller.IsAvailableFor(interactionTool) && controller.IsCompatibleWith(interactionTool))
            {
                projector.Project(selectionInfo.selectedObject, controller);
                selectionInfo.hasBeenSelected = true;
                LastSelected = selectionInfo;
                isSelecting = true;
                selectionEvent.Invoke(selectionInfo);
                pointingDetector.Reinit();
                grabDetector.Reinit();
            }
        }

        #endregion selection
    }
}