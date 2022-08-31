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

using umi3dBrowsers.interaction.selection.cursor;
using umi3dVRBrowsersBase.interactions.selection;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dBrowsers.interaction.selection.feedback
{
    /// <summary>
    /// Feedback handler for interactable selection feedback
    /// Mostly useful for Unity's serialization
    /// </summary>
    public class VRSelectableSelectionFeedbackHandler : AbstractSelectionFeedbackHandler<Selectable>
    {
        [SerializeField]
        private AbstractVRHapticSelectionFeedback hapticFeedback;

        private AbstractCursor pointingCursor;

        private AbstractCursor grabCursor;

        private void Awake()
        {
            var manager = GetComponentInParent<VRSelectionManager>();
            pointingCursor = manager.pointingCursor;
            grabCursor = manager.grabCursor;
        }

        /// <inheritdoc/>
        /// <param name="selectionData"></param
        public override void StartFeedback(AbstractSelectionData selectionData)
        {
            if (!isRunning)
            {
                var selectionDataTyped = selectionData as SelectionIntentData<Selectable>;
                hapticFeedback.Trigger();
                if (selectionDataTyped.detectionOrigin == DetectionOrigin.POINTING)
                    pointingCursor.ChangeAccordingToSelection(selectionData);
                //else if (selectionDataTyped.detectionOrigin == DetectionOrigin.PROXIMITY)
                //    grabCursor.ChangeAccordingToSelection(selectionData);
                isRunning = true;
            }
        }

        /// <inheritdoc/>
        public override void UpdateFeedback(AbstractSelectionData selectionData)
        { }

        /// <inheritdoc/>
        public override void EndFeedback(AbstractSelectionData selectionData)
        {
            if (isRunning)
            {
                var selectionDataTyped = selectionData as SelectionIntentData<Selectable>;
                if (selectionDataTyped.detectionOrigin == DetectionOrigin.POINTING)
                    pointingCursor.ChangeAccordingToSelection(null);
                //else if (selectionDataTyped.detectionOrigin == DetectionOrigin.PROXIMITY)
                //    grabCursor.ChangeAccordingToSelection(null);
                isRunning = false;
            }
        }
    }


}