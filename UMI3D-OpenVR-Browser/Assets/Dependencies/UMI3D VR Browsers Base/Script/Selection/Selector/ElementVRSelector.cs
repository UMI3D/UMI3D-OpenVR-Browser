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
using umi3d.cdk.interaction.selection.intentdetector;
using umi3d.cdk.interaction.selection.projector;
using umi3dVRBrowsersBase.ui;
using UnityEngine;

namespace umi3dVRBrowsersBase.interactions.selection
{
    /// <summary>
    /// Selector for <see cref="AbstractClientInteractableElement"/> objects on VR browsers
    /// </summary>
    public class ElementVRSelector : AbstractVRSelector<AbstractClientInteractableElement>
    {
        /// <summary>
        /// Selection Intent Detector (virtual pointing)
        /// </summary>
        public AbstractPointingElementDetector pointingDetector;

        /// <summary>
        /// Selection Intent Detector (virtual hand)
        /// </summary>
        public AbstractGrabElementDetector grabDetector;

        /// <summary>
        /// Previously detected objects for virtual hand
        /// </summary>
        [HideInInspector]
        public Cache<AbstractClientInteractableElement> detectionCacheProximity = new Cache<AbstractClientInteractableElement>();

        /// <summary>
        /// Previously detected objects from virtual pointing
        /// </summary>
        [HideInInspector]
        public Cache<AbstractClientInteractableElement> detectionCachePointing = new Cache<AbstractClientInteractableElement>();

        #region constructors

        private ElementVRSelector() : base()
        {
            projector = new ElementProjector();
        }

        /// <inheritdoc/>
        [HideInInspector]
        public class ElementSelectionData : SelectionIntentData<AbstractClientInteractableElement>
        {
            public ElementSelectionData() : base(SelectableObjectType.CLIENT_ELEMENT)
            {
            }
        }

        #endregion constructors

        #region lifecycle

        /// <inheritdoc/>
        protected override void ActivateInternal()
        {
            base.ActivateInternal();
            pointingDetector.Init(controller);
            grabDetector.Init(controller);
            projector = new ElementProjector();
        }

        /// <inheritdoc/>
        protected override void DeactivateInternal()
        {
            base.DeactivateInternal();
            pointingDetector.Reinit();
            grabDetector.Reinit();
        }

        protected void Update()
        {
            // look for interaction from the controller and send the right events
            // probably should not belong in that piece of code
            if (AbstractControllerInputManager.Instance.GetButtonDown(controller.type, ActionType.Trigger))
            {
                if (activated)
                {
                    VRInteractionMapper.lastControllerUsedToClick = controller.type;
                    OnPointerDown();
                }
            }
            if (AbstractControllerInputManager.Instance.GetButtonUp(controller.type, ActionType.Trigger))
            {
                if (activated)
                {
                    VRInteractionMapper.lastControllerUsedToClick = controller.type;
                    OnPointerUp();
                }
            }
        }

        /// <summary>
        /// Executed when the pointer transits to down state
        /// </summary>
        protected void OnPointerDown()
        {
            if (isSelecting)
            {
                LastSelected.selectedObject.Interact(controller);
            }
        }

        /// <summary>
        /// Executed when the pointer transits to up state
        /// </summary>
        protected void OnPointerUp()
        {
        }

        #endregion lifecycle

        #region selection

        /// <inheritdoc/>
        public override List<SelectionIntentData> GetIntentDetections()
        {
            var possibleSelection = new List<SelectionIntentData>();

            if (grabDetector.isRunning)
            {
                var interactableToSelectProximity = grabDetector.PredictTarget();

                var detectionInfo = new ElementSelectionData
                {
                    selectedObject = interactableToSelectProximity,
                    controller = controller,
                    detectionOrigin = DetectionOrigin.PROXIMITY,
                };
                detectionCacheProximity.Add(detectionInfo);
                if (CanSelect(interactableToSelectProximity))
                    possibleSelection.Add(detectionInfo);
            }

            if (pointingDetector.isRunning)
            {
                var interactableToSelectPointed = pointingDetector.PredictTarget();
                var detectionInfo = new ElementSelectionData
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

        #endregion selection
    }
}