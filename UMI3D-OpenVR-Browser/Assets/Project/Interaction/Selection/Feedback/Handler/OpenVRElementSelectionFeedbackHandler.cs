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

using umi3d.cdk.interaction.selection.feedback;
using UnityEngine.UI;
using UnityEngine;
using umi3d.cdk.interaction.selection.cursor;
using umi3d.cdk.interaction.selection;
using umi3dVRBrowsersBase.interactions.selection;

namespace umi3dbrowser.openvr.interaction.selection.feedback
{
    /// <summary>
    /// Feedback handler for interactable selection feedback
    /// Mostly useful for Unity's serialization
    /// </summary>
    public class OpenVRElementSelectionFeedbackHandler : AbstractSelectionFeedbackHandler<Selectable>
    {
        [SerializeField]
        private OpenVRHapticSelectionFeedback hapticFeedback;

        private AbstractCursor pointingCursor;

        private void Awake()
        {
            pointingCursor = GetComponentInParent<VRSelectionManager>().pointingCursor;
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
                isRunning = true;
            }
        }

        /// <inheritdoc/>
        public override void EndFeedback(AbstractSelectionData selectionData)
        {
            if (isRunning)
            {
                var selectionDataTyped = selectionData as SelectionIntentData<Selectable>;
                if (selectionDataTyped.detectionOrigin == DetectionOrigin.POINTING)
                    pointingCursor.ChangeAccordingToSelection(null);
                isRunning = false;
            }
        }
    }
}