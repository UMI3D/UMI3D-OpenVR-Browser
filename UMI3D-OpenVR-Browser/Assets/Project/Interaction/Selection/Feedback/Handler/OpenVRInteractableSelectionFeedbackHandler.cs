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
using umi3dBrowsers.interaction.selection;
using umi3dBrowsers.interaction.selection.feedback;
using UnityEngine;

namespace umi3dBrowserOpenVR.interaction.selection.feedback
{
    /// <summary>
    /// Feedback handler for interactable selection feedback
    /// Mostly useful for Unity's serialization
    /// </summary>
    public class OpenVRInteractableSelectionFeedbackHandler : AbstractSelectionFeedbackHandler<InteractableContainer>
    {
        [SerializeField]
        private OpenVRHapticSelectionFeedback hapticFeedback;
        [SerializeField]
        private OpenVROutlineRenderSelectionFeedback highlightFeedback;

        /// <inheritdoc/>
        public override void StartFeedback(AbstractSelectionData selectionData)
        {
            if (!isRunning)
            {
                hapticFeedback.Trigger();
                highlightFeedback.Activate(selectionData);
                isRunning = true;
            }
        }

        /// <inheritdoc/>
        public override void UpdateFeedback(AbstractSelectionData selectionData)
        {
            
        }

        /// <inheritdoc/>
        public override void EndFeedback(AbstractSelectionData selectionData)
        {
            if (isRunning)
            {
                highlightFeedback.Deactivate(selectionData);
                isRunning = false;
            }
        }
    }
}
