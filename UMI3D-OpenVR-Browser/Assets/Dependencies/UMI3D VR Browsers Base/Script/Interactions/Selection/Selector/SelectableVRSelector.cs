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

using System.Collections.Generic;
using umi3dBrowsers.interaction.selection;
using umi3dBrowsers.interaction.selection.intentdetector;
using umi3dBrowsers.interaction.selection.projector;
using umi3dBrowsers.interaction.selection.zoneselection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.interactions.selection.selector
{
    /// <summary>
    /// Selector for <see cref="Selectable"/> objects (2D UI) on VR browsers
    /// </summary>
    public class SelectableVRSelector : AbstractVRSelector<Selectable>, IPointerUpHandler, IPointerDownHandler
    {
        /// <summary>
        /// Selection Intent Detectors (virtual pointing). In order of decreasing priority.
        /// </summary>
        [SerializeField, Tooltip("Selection Intent Detector for virtual pointing. In order of decreasing priority.")]
        private List<AbstractPointingSelectableDetector> pointingDetectors;

        /// <summary>
        /// Selection Intent Detector (virtual hand). In order of decreasing priority.
        /// </summary>
        [SerializeField, Tooltip("Selection Intent Detector for virtual hand (grab). In order of decreasing priority.")]
        private List<AbstractGrabSelectableDetector> proximityDetectors;

        /// <summary>
        /// Helper to raycast selectable.
        /// </summary>
        private RaySelectionZone<Selectable> raycastHelper = new(Vector3.zero, Vector3.zero);

        private PointerEventData hoverEventData;

        #region constructors

        private SelectableVRSelector() : base()
        {
            projector = new SelectableProjector();

            hoverEventData = new PointerEventData(EventSystem.current) { clickCount = 1 };
        }

        /// <summary>
        /// Tag class for selection dta for selectable
        /// Mirror how interactable classes are conceived
        /// </summary>
        public class SelectableSelectionData : SelectionIntentData<Selectable>
        {
        }

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
                    OnPointerDown(new PointerEventData(EventSystem.current) { clickCount = 1 });
                    LockedSelector = true;
                }
            }
            else if (AbstractControllerInputManager.Instance.GetButtonUp(controller.type, ActionType.Trigger))
            {
                if (activated)
                {
                    VRInteractionMapper.lastControllerUsedToClick = controller.type;
                    OnPointerUp(new PointerEventData(EventSystem.current) { clickCount = 1 });
                    LockedSelector = false;
                }
            } else if (activated && LastSelected != null)
            {
                raycastHelper.origin = controller.transform.position;
                raycastHelper.direction = controller.transform.forward;

                var closestAndRaycastHit = raycastHelper.GetClosestAndRaycastHit();

                if (closestAndRaycastHit.obj != null)
                {
                    hoverEventData.pointerCurrentRaycast = new RaycastResult { worldPosition = closestAndRaycastHit.raycastHit.point };
                    ExecuteEvents.Execute(LastSelected.selectedObject.gameObject, hoverEventData, ExecuteEvents.pointerMoveHandler);
                }
            }
        }

        /// <summary>
        /// Callback that trigger the interaction with selectables
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            var projector = this.projector as SelectableProjector;
            if (LastSelected != null && isSelecting)
            {
                projector.Pick(LastSelected.selectedObject, controller, eventData);
            }
            if (projector.currentlyPressedButton != null
                && LastSelected?.selectedObject != projector.currentlyPressedButton) //specific rule for button's focus
            {
                projector.currentlyPressedButton.OnPointerUp(eventData);
                projector.currentlyPressedButton = null;
            }
        }

        /// <summary>
        /// Callback that trigger the interaction with selectables
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (isSelecting && LastSelected != null)
            {
                (projector as SelectableProjector).PressDown(LastSelected.selectedObject, controller, eventData);
            }
        }

        #endregion lifecycle

        #region detectors
        /// <inheritdoc/>
        public override List<AbstractDetector<Selectable>> GetProximityDetectors()
        {
            var l = new List<AbstractDetector<Selectable>>();
            foreach (var detector in proximityDetectors)
                l.Add(detector);
            return l;
        }

        /// <inheritdoc/>
        public override List<AbstractDetector<Selectable>> GetPointingDetectors()
        {
            var l = new List<AbstractDetector<Selectable>>();
            foreach (var detector in pointingDetectors)
                l.Add(detector);
            return l;
        }
        #endregion

        #region selection

        /// <summary>
        /// Checks if the selectable: <br/>
        ///     - exists <br/>
        ///     - is enabled <br/>
        ///     - can be interacted with <br/>
        /// </summary>
        /// <param name="uiToSelect"></param>
        /// <returns></returns>
        protected override bool CanSelect(Selectable uiToSelect)
        {
            return uiToSelect != null 
                && uiToSelect.enabled 
                && uiToSelect.interactable;
        }

        /// <inheritdoc/>
        public override SelectionIntentData CreateSelectionIntentData(Selectable obj, DetectionOrigin origin)
        {
            return new SelectableSelectionData()
            {
                selectedObject = obj,
                controller = controller,
                detectionOrigin = origin
            };
        }

        #endregion selection
    }
}