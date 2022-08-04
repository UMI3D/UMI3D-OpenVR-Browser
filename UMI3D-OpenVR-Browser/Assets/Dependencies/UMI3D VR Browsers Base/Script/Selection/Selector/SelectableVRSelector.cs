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
using umi3d.cdk.interaction.selection.intentdetector;
using umi3d.cdk.interaction.selection.projector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.interactions.selection
{
    /// <summary>
    /// Selector for <see cref="Selectable"/> objects (2D UI) on VR browsers
    /// </summary>
    public class SelectableVRSelector : AbstractVRSelector<Selectable>, IPointerUpHandler, IPointerDownHandler
    {
        /// <summary>
        /// Selection Intent Detector (virtual pointing)
        /// </summary>
        public AbstractPointingSelectableDetector pointingDetector;

        /// <summary>
        /// Selection Intent Detector (virtual hand)
        /// </summary>
        public AbstractGrabSelectableDetector grabDetector;

        SelectableVRSelector() : base()
        {
            supportedObjectType = SelectableObjectType.SELECTABLE;
            projector = new SelectableProjector();
        }

        /// <summary>
        /// Tag class for selection dta for selectable
        /// Mirror how interactable classes are conceived
        /// </summary>
        public class SelectableSelectionData : SelectionData<Selectable>
        {
            public SelectableSelectionData() : base(SelectableObjectType.SELECTABLE)
            {
            }
        }

        /// <summary>
        /// Previously detected objects for virtual hand
        /// </summary>
        [HideInInspector]
        public Cache<Selectable> detectionCacheProximity = new Cache<Selectable>();

        /// <summary>
        /// Previously detected objects from virtual pointing
        /// </summary>
        [HideInInspector]
        public Cache<Selectable> detectionCachePointing = new Cache<Selectable>();

        /// <inheritdoc/>
        protected override void ActivateInternal()
        {
            base.ActivateInternal();
            pointingDetector.Init(controller);
            grabDetector.Init(controller);
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
            if (AbstractControllerInputManager.Instance.GetButtonDown(controller.type, ActionType.Trigger))
            {
                if (activated)
                {
                    VRInteractionMapper.lastControllerUsedToClick = controller.type;
                    OnPointerDown(new PointerEventData(EventSystem.current) { clickCount = 1 });
                }
            }
            if (AbstractControllerInputManager.Instance.GetButtonUp(controller.type, ActionType.Trigger))
            {
                if (activated)
                {
                    VRInteractionMapper.lastControllerUsedToClick = controller.type;
                    OnPointerUp(new PointerEventData(EventSystem.current) { clickCount = 1 });
                }
            }
        }

        /// <summary>
        /// Checks if the selectable :
        ///     - exists
        ///     - is enabled
        ///     - is different from the previous one
        /// </summary>
        /// <param name="uiToSelect"></param>
        /// <returns></returns>
        protected override bool CanSelect(Selectable uiToSelect)
        {
            if (uiToSelect == null)
                return false;
            return uiToSelect.enabled && uiToSelect.interactable;
        }

        /// <inheritdoc/>
        [ContextMenu("Pick")] //?
        public override List<SelectionData> Detect()
        {
            var possibleSelection = new List<SelectionData>();

            if (grabDetector.isRunning)
            {
                var uiToSelectProximity = grabDetector.PredictTarget();
                var detectionInfo = new SelectableSelectionData
                {
                    selectedObject = uiToSelectProximity,
                    controller = controller,
                    detectionOrigin = DetectionOrigin.PROXIMITY,
                };
                detectionCacheProximity.Add(detectionInfo);
                if (CanSelect(uiToSelectProximity))
                    possibleSelection.Add(detectionInfo);
            }

            if (pointingDetector.isRunning)
            {
                var uiToSelectPointed = pointingDetector.PredictTarget();
                var detectionInfo = new SelectableSelectionData
                {
                    selectedObject = uiToSelectPointed,
                    controller = controller,
                    detectionOrigin = DetectionOrigin.POINTING,
                };
                detectionCachePointing.Add(detectionInfo);
                if (CanSelect(uiToSelectPointed))
                    possibleSelection.Add(detectionInfo);
            }

            foreach (var poss in possibleSelection)
                propositionSelectionCache.Add(poss);
            return possibleSelection;
        }

        /// <summary>
        /// Callback that trigger the interaction with selectables
        /// </summary>
        /// <param name="eventData"></param>
        [ContextMenu("Pick")]
        public void OnPointerUp(PointerEventData eventData)
        {
            var projector = this.projector as SelectableProjector;
            if (LastSelected != null && isSelecting )
            {
                projector.Pick(LastSelected.selectedObject, controller);
            }
            if (projector.currentlyPressedButton != null
                && LastSelected?.selectedObject != projector.currentlyPressedButton) //specific rule for button's focus
            {
                projector.currentlyPressedButton.OnPointerUp(new PointerEventData(EventSystem.current));
                projector.currentlyPressedButton = null;
            }
        }

        /// <summary>
        /// Callback that trigger the interaction with selectables
        /// </summary>
        /// <param name="eventData"></param>
        [ContextMenu("Press")]
        public void OnPointerDown(PointerEventData eventData)
        {
            if (LastSelected != null && isSelecting)
            {
                (projector as SelectableProjector).Press(LastSelected.selectedObject, controller);
            }
        }
    }
}