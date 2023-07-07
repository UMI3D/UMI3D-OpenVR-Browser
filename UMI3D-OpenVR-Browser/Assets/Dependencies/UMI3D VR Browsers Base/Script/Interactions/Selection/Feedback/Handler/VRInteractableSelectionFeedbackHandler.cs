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

using umi3d.cdk.interaction;
using umi3dBrowsers.interaction.selection.cursor;
using umi3dVRBrowsersBase.interactions.selection;
using UnityEngine;

namespace umi3dBrowsers.interaction.selection.feedback
{
    /// <summary>
    /// Feedback handler for interactable selection feedback
    /// Mostly useful for Unity's serialization
    /// </summary>
    public class VRInteractableSelectionFeedbackHandler : AbstractSelectionFeedbackHandler<InteractableContainer>
    {
        [SerializeField]
        private AbstractVRHapticSelectionFeedback hapticFeedback;

        [SerializeField]
        private VROutlineRenderSelectionFeedback highlightFeedback;

        private AbstractPointingCursor pointingCursor;
        private AbstractCursor grabCursor;

        private DetectionOrigin lastOrigin;

        private void Awake()
        {
            var selectionManager = GetComponentInParent<VRSelectionManager>();
            pointingCursor = selectionManager.pointingCursor;
            grabCursor = selectionManager.grabCursor;
        }

        /// <inheritdoc/>
        public override void StartFeedback(AbstractSelectionData selectionData)
        {
            if (!isRunning)
            {
                var iSelectionData = selectionData as SelectionIntentData;
                hapticFeedback.Trigger();
                highlightFeedback.Activate(iSelectionData);
                lastOrigin = iSelectionData.detectionOrigin;
                switch (lastOrigin)
                {
                    case DetectionOrigin.POINTING:
                        pointingCursor.Display();
                        pointingCursor.ChangeAccordingToSelection(iSelectionData);
                        break;

                    case DetectionOrigin.PROXIMITY:
                        pointingCursor.Hide();
                        grabCursor.Display();
                        grabCursor.ChangeAccordingToSelection(iSelectionData);
                        break;
                }
                isRunning = true;
            }
        }

        /// <inheritdoc/>
        public override void UpdateFeedback(AbstractSelectionData selectionData)
        {
            var iSelectionData = selectionData as SelectionIntentData;
            if (lastOrigin == DetectionOrigin.POINTING && iSelectionData.detectionOrigin == DetectionOrigin.PROXIMITY)
            {
                pointingCursor.Hide();
                pointingCursor.ChangeAccordingToSelection(null);
                grabCursor.Display();
                grabCursor.ChangeAccordingToSelection(iSelectionData);
                lastOrigin = DetectionOrigin.PROXIMITY;
            }
            else if (lastOrigin == DetectionOrigin.PROXIMITY && iSelectionData.detectionOrigin == DetectionOrigin.POINTING)
            {
                grabCursor.Hide();
                grabCursor.ChangeAccordingToSelection(null);
                pointingCursor.Display();
                pointingCursor.ChangeAccordingToSelection(iSelectionData);
                lastOrigin = DetectionOrigin.POINTING;
            }
        }

        /// <inheritdoc/>
        public override void EndFeedback(AbstractSelectionData selectionData)
        {
            if (isRunning)
            {
                highlightFeedback.Deactivate(selectionData);
                pointingCursor.Display();
                pointingCursor.ChangeAccordingToSelection(null);
                grabCursor.Hide();
                grabCursor.ChangeAccordingToSelection(null);
                isRunning = false;
            }
        }
    }
}