

using umi3d.cdk.interaction;
using umi3d.cdk.interaction.selection;
using umi3d.cdk.interaction.selection.cursor;
using umi3d.cdk.interaction.selection.feedback;
using UnityEngine;

namespace umi3dbrowser.openvr.interaction.selection.feedback
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
        private OpenVRVisualSelectionFeedback highlightFeedback;

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
