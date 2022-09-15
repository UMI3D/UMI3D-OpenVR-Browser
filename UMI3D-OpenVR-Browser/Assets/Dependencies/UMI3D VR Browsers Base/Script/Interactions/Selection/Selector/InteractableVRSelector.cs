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

using System;
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
        /// Selection Intent Detectors (virtual pointing). In order of decreasing priority.
        /// </summary>
        [SerializeField, Tooltip("Selection Intent Detector for virtual pointing. In order of decreasing priority. Cannot be changed at runtime.")]
        private List<AbstractPointingInteractableDetector> pointingDetectors;

        /// <summary>
        /// Selection Intent Detector (virtual hand). In order of decreasing priority.
        /// </summary>
        [SerializeField, Tooltip("Selection Intent Detector for virtual hand (grab). In order of decreasing priority. Cannot be changed at runtime.")]
        private List<AbstractGrabInteractableDetector> proximityDetectors;

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

        protected override void Update()
        {
            base.Update();
            // look for interaction from the controller and send the right events
            // probably should not belong in that piece of code
            if (AbstractControllerInputManager.Instance.GetButtonDown(controller.type, ActionType.Trigger) && activated)
            {
                VRInteractionMapper.lastControllerUsedToClick = controller.type;
                OnPointerDown();
                LockedSelector = true;
            }
            if (AbstractControllerInputManager.Instance.GetButtonUp(controller.type, ActionType.Trigger) && activated)
            {
                VRInteractionMapper.lastControllerUsedToClick = controller.type;
                OnPointerUp();
                LockedSelector = false;
            }
        }

        private void OnPointerUp()
        {
            
        }

        private void OnPointerDown()
        {
            
        }

        #endregion lifecycle

        #region detectors
        /// <inheritdoc/>
        public override List<AbstractDetector<InteractableContainer>> GetProximityDetectors()
        {
            var l = new List<AbstractDetector<InteractableContainer>>();
            foreach (var detector in proximityDetectors)
                l.Add(detector);
            return l;
        }

        /// <inheritdoc/>
        public override List<AbstractDetector<InteractableContainer>> GetPointingDetectors()
        {
            var l = new List<AbstractDetector<InteractableContainer>>();
            foreach (var detector in pointingDetectors)
                l.Add(detector);
            return l;
        }
        #endregion

        #region selection

        /// <summary>
        /// Checks if the interactable :
        ///     - exists
        ///     - has at least one associated interaction
        ///     - is compatible with this controller
        /// </summary>
        /// <param name="icToSelect"></param>
        /// <returns></returns>
        protected override bool CanSelect(InteractableContainer icToSelect)
        {
            return icToSelect != null
                    && icToSelect.Interactable != null
                    && icToSelect.Interactable.dto.interactions != null
                    && icToSelect.Interactable.dto.interactions.Count > 0
                    && controller.IsCompatibleWith(icToSelect.Interactable)
                    && (!InteractionMapper.Instance.IsToolSelected(icToSelect.Interactable.id) 
                        || (LastSelected?.selectedObject.Interactable == icToSelect.Interactable));
        }

        /// <summary>
        /// Is the passed object currently selected by this selector?
        /// </summary>
        /// <param name="ic"></param>
        /// <returns></returns>
        private bool IsObjectSelected(InteractableContainer ic)
        {
            return ic == LastSelected?.selectedObject;
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

            foreach (var detector in pointingDetectors)
                detector.Reinit();
            foreach (var detector in proximityDetectors)
                detector.Reinit();
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
                    throw new ArgumentNullException("Argument should be null only if moving outside of an object");
                else if (IsObjectSelected(selectionInfo.selectedObject)) //  the selector was selecting the same target before
                {
                    if (LastSelected != null && selectionInfo.detectionOrigin != LastSelected.detectionOrigin)
                        selectionFeedbackHandler.UpdateFeedback(selectionInfo);
                    return;
                }
            }

            var interactionTool = AbstractInteractionMapper.Instance.GetTool(selectionInfo.selectedObject.Interactable.dto.id);
            if (selectionInfo is InteractableSelectionData)
                (selectionInfo as InteractableSelectionData).tool = interactionTool;

            if (isSelecting
                && (LastSelected != null || (!controller.IsAvailableFor(interactionTool) && InteractionMapper.Instance.IsToolSelected(interactionTool.id)))) // second case happens when an object is destroyed but the tool is not released
                Deselect(LastSelected);

            if (controller.IsAvailableFor(interactionTool))
            {
                projector.Project(selectionInfo.selectedObject, controller);
                selectionInfo.hasBeenSelected = true;
                LastSelected = selectionInfo;
                isSelecting = true;
                selectionEvent.Invoke(selectionInfo);
                foreach (var detector in pointingDetectors)
                    detector.Reinit();
                foreach (var detector in proximityDetectors)
                    detector.Reinit();
            }
        }

        /// <inheritdoc/>
        public override SelectionIntentData CreateSelectionIntentData(InteractableContainer obj, DetectionOrigin origin)
        {
            return new InteractableSelectionData()
            {
                selectedObject = obj,
                controller = controller,
                detectionOrigin = origin,
                tool = obj == null ? null : obj.Interactable
            };
        }
        #endregion selection
    }
}