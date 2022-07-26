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

using umi3d.cdk.interaction.selection.feedback;
using umi3d.cdk.interaction.selection.intentdetector;
using umi3d.cdk.interaction.selection.projector;
using umi3dVRBrowsersBase.interactions;
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

        /// <summary>
        /// Tag class for selection dta for selectable
        /// Mirror how interactable classes are conceived
        /// </summary>
        public class SelectableSelectionData : SelectionData<Selectable>
        { }

        /// <summary>
        /// Previously selected objects
        /// </summary>
        [HideInInspector]
        public SelectionCache<SelectableSelectionData> selectionCache = new SelectionCache<SelectableSelectionData>();

        /// <summary>
        /// Manages projection on the controller
        /// </summary>
        [HideInInspector]
        public SelectableProjector projector = new SelectableProjector();

        /// <summary>
        /// Shortcut for last selected object info
        /// </summary>
        public SelectableSelectionData LastSelected => selectionCache.Objects.Last?.Value;

        /// <summary>
        /// Manage feedback when an object is selected
        /// </summary>
        public AbstractSelectionFeedbackHandler<Selectable> selectionFeedbackHandler;

        protected override void ActivateInternal()
        {
            base.ActivateInternal();
            pointingDetector.Init(controller);
            grabDetector.Init(controller);
            selectionEvent.AddListener(OnSelection);
            deselectionEvent.AddListener(OnDeselection);
        }

        protected override void DeactivateInternal()
        {
            base.DeactivateInternal();
            pointingDetector.Reinit();
            grabDetector.Reinit();
            selectionEvent.RemoveAllListeners();
            deselectionEvent.RemoveAllListeners();
        }

        protected override void Update()
        {
            base.Update();
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
        private bool CanSelect(Selectable uiToSelect)
        {
            if (uiToSelect == null)
                return false;
            return uiToSelect.enabled
                && (LastSelected == null || uiToSelect != LastSelected.selectedObject);
        }

        /// <summary>
        /// Select an object according to the current context
        /// </summary>
        [ContextMenu("Pick")] //?
        public override void Select()
        {
            Selectable uiToSelectProximity = null;
            Selectable uiToSelectPointed = null;

            //priority for proximity selection
            if (grabDetector.isRunning)
            {
                uiToSelectProximity = grabDetector.PredictTarget();
                detectionCacheProximity.Add(uiToSelectProximity);
                if (CanSelect(uiToSelectProximity))
                {
                    Select(uiToSelectProximity, out SelectableSelectionData selectionData);
                    selectionData.detectionOrigin = SelectionData<Selectable>.DetectionOrigin.PROXIMITY;
                    selectionCache.Add(selectionData);
                    selectionEvent.Invoke(selectionData);
                    return;
                }
            }

            if (pointingDetector.isRunning)
            {
                uiToSelectPointed = pointingDetector.PredictTarget();
                detectionCachePointing.Add(uiToSelectPointed);
                if (CanSelect(uiToSelectPointed))
                {
                    Select(uiToSelectPointed, out SelectableSelectionData selectionData);
                    selectionData.detectionOrigin = SelectionData<Selectable>.DetectionOrigin.POINTING;
                    selectionCache.Add(selectionData);
                    selectionEvent.Invoke(selectionData);
                    return;
                }
            }
            if (uiToSelectProximity == null
                    && uiToSelectPointed == null
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
        /// <param name="uiToDeselectInfo"></param>
        private void Deselect(SelectableSelectionData uiToDeselectInfo)
        {
            projector.Release(uiToDeselectInfo.selectedObject);
            deselectionEvent.Invoke(uiToDeselectInfo);
        }

        /// <summary>
        /// Select a selectable and provides its infos
        /// </summary>
        /// <param name="uiToSelect"></param>
        /// <param name="selectionInfo"></param>
        private void Select(Selectable uiToSelect, out SelectableSelectionData selectionInfo)
        {
            selectionInfo = new SelectableSelectionData() { selectedObject = uiToSelect };

            if (LastSelected != null)
                Deselect(LastSelected);

            selectionFeedbackHandler.StartFeedback(selectionInfo);
            projector.Project(uiToSelect, controller);
        }

        /// <summary>
        /// Triggered when an selectable is selected
        /// </summary>
        /// <param name="deselectionData"></param>
        protected override void OnSelection(SelectionData<Selectable> selectionData)
        {
            selectionFeedbackHandler.StartFeedback(selectionData);
        }

        /// <summary>
        /// Triggered when an selectable is deselected
        /// </summary>
        /// <param name="deselectionData"></param>
        protected override void OnDeselection(SelectionData<Selectable> deselectionData)
        {
            selectionFeedbackHandler.EndFeedback(deselectionData);
        }

        /// <summary>
        /// Callback that trigger the interaction with selectables
        /// </summary>
        /// <param name="eventData"></param>
        [ContextMenu("Pick")]
        public void OnPointerUp(PointerEventData eventData)
        {
            if (LastSelected != null)
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
            if (LastSelected != null)
            {
                projector.Press(LastSelected.selectedObject, controller);
            }
        }
    }
}