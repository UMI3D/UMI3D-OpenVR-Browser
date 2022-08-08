using umi3dVRBrowsersBase.interactions;
using umi3dVRBrowsersBase.ui;

namespace umi3d.cdk.interaction.selection.projector
{
    public class ElementProjector : IProjector<AbstractClientInteractableElement>
    {
        /// <inheritdoc/>
        public void Project(AbstractClientInteractableElement objToProjec, AbstractController controller)
        {
            objToProjec.Select(controller as VRController);
        }

        /// <inheritdoc/>
        public void Release(AbstractClientInteractableElement objToRelease, AbstractController controller)
        {
            objToRelease.Deselect(controller as VRController);
        }
    }
}