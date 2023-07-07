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
using umi3dBrowsers.interaction.selection;
using umi3dBrowsers.interaction.selection.intentdetector;
using umi3dVRBrowsersBase.interactions.selection.intentdetector;
using umi3dVRBrowsersBase.interactions.selection.projector;
using umi3dVRBrowsersBase.ui;
using UnityEngine;

namespace umi3dVRBrowsersBase.interactions.selection.selector
{
    /// <summary>
    /// Selector for <see cref="AbstractClientInteractableElement"/> objects on VR browsers
    /// </summary>
    public class ElementVRSelector : AbstractVRSelector<AbstractClientInteractableElement>
    {
        /// <summary>
        /// Selection Intent Detectors (virtual pointing). In order of decreasing priority.
        /// </summary>
        [SerializeField, Tooltip("Selection Intent Detector for virtual pointing. In order of decreasing priority.")]
        private List<AbstractPointingElementDetector> pointingDetectors;

        /// <summary>
        /// Selection Intent Detector (virtual hand). In order of decreasing priority.
        /// </summary>
        [SerializeField, Tooltip("Selection Intent Detector for virtual hand (grab). In order of decreasing priority.")]
        private List<AbstractGrabElementDetector> proximityDetectors;

        #region constructors

        private ElementVRSelector() : base()
        {
            projector = new ElementProjector();
        }

        /// <inheritdoc/>
        [HideInInspector]
        public class ElementSelectionData : SelectionIntentData<AbstractClientInteractableElement>
        { }

        #endregion constructors

        #region lifecycle

        protected override void Update()
        {
            base.Update();
            // look for interaction from the controller and send the right events
            // probably should not belong in that piece of code
            if (AbstractControllerInputManager.Instance.GetButtonDown(controller.type, ActionType.Trigger))
            {
                if (activated)
                {
                    VRInteractionMapper.lastControllerUsedToClick = controller.type;
                    OnPointerDown();
                    LockedSelector = true;
                }
            }
            if (AbstractControllerInputManager.Instance.GetButtonUp(controller.type, ActionType.Trigger))
            {
                if (activated)
                {
                    VRInteractionMapper.lastControllerUsedToClick = controller.type;
                    OnPointerUp();
                    LockedSelector = false;
                }
            }
            if (AbstractControllerInputManager.Instance.GetButton(controller.type, ActionType.Trigger))
            {
                if (activated)
                    OnPointerPressed();
            }
        }

        /// <summary>
        /// Executed while the pointer is pressed
        /// </summary>
        private void OnPointerPressed()
        {
            if (isSelecting)
            {
                if (LastSelected.selectedObject is IPressableElement)
                    (LastSelected.selectedObject as IPressableElement).PressStay(controller.type);
            }
        }

        /// <summary>
        /// Executed when the pointer transits to down state
        /// </summary>
        protected void OnPointerDown()
        {
            if (isSelecting)
            {
                if (LastSelected.selectedObject is IPressableElement)
                    (LastSelected.selectedObject as IPressableElement).PressDown(controller.type);
                if (LastSelected.selectedObject is ITriggerableElement)
                    (LastSelected.selectedObject as ITriggerableElement).Trigger(controller.type);
            }
        }

        /// <summary>
        /// Executed when the pointer transits to up state
        /// </summary>
        protected void OnPointerUp()
        {
            if (isSelecting)
            {
                if (LastSelected.selectedObject is IPressableElement)
                    (LastSelected.selectedObject as IPressableElement).PressUp(controller.type);
            }
        }

        #endregion lifecycle

        #region detectors
        /// <inheritdoc/>
        public override List<AbstractDetector<AbstractClientInteractableElement>> GetProximityDetectors()
        {
            var l = new List<AbstractDetector<AbstractClientInteractableElement>>();
            foreach (var detector in proximityDetectors)
                l.Add(detector);
            return l;
        }

        /// <inheritdoc/>
        public override List<AbstractDetector<AbstractClientInteractableElement>> GetPointingDetectors()
        {
            var l = new List<AbstractDetector<AbstractClientInteractableElement>>();
            foreach (var detector in pointingDetectors)
                l.Add(detector);
            return l;
        }
        #endregion

        #region selection

        /// <inheritdoc/>
        public override SelectionIntentData CreateSelectionIntentData(AbstractClientInteractableElement obj, DetectionOrigin origin)
        {
            return new ElementSelectionData()
            {
                selectedObject = obj,
                controller = controller,
                detectionOrigin = origin
            };
        }

        /// <summary>
        /// Checks if the selectable: <br/>
        ///     - exists <br/>
        ///     - is enabled <br/>
        /// </summary>
        /// <param name="elToSelect"></param>
        /// <returns>True if the object could be selected by the selector</returns>
        protected override bool CanSelect(AbstractClientInteractableElement elToSelect)
        {
            return elToSelect != null && elToSelect.enabled;
        }

        #endregion selection
    }
}