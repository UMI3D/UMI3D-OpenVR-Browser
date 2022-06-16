using umi3d.cdk.interaction;
using umi3d.cdk.interaction.selection.feedback;
using UnityEngine.UI;
using UnityEngine;
using umi3d.cdk.interaction.selection.cursor;
using umi3d.cdk.interaction.selection;

namespace umi3dbrowser.openvr.interaction.selection.feedback
{
    /// <summary>
    /// Feedback handler for interactable selection feedback
    /// Mostly useful for Unity's serialization
    /// </summary>
    public class OpenVRSelectableSelectionFeedbackHandler : AbstractSelectionFeedbackHandler<Selectable>
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
                var selectionDataTyped = selectionData as SelectionData<Selectable>;
                hapticFeedback.Trigger();
                if (selectionDataTyped.detectionOrigin == SelectionData<Selectable>.DetectionOrigin.POINTING)
                    pointingCursor.ChangeAccordingToSelection(selectionData);
                isRunning = true;
            }
        }

        /// <inheritdoc/>
        public override void EndFeedback(AbstractSelectionData selectionData)
        {
            if (isRunning)
            {
                var selectionDataTyped = selectionData as SelectionData<Selectable>;
                if (selectionDataTyped.detectionOrigin == SelectionData<Selectable>.DetectionOrigin.POINTING)
                    pointingCursor.ChangeAccordingToSelection(null);
                isRunning = false;
            }
        }
    }


}