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

namespace umi3d.cdk.interaction.selection.projector
{
    /// <summary>
    /// Projector for Interactable
    /// </summary>
    public class InteractableProjector : IProjector<InteractableContainer>
    {
        /// <summary>
        /// Checks whether an interctable has already projected tools
        /// </summary>
        /// <param name="interactable"></param>
        /// <returns></returns>
        public bool IsProjected(InteractableContainer interactable, AbstractController controller)
        {
            return InteractionMapper.Instance.IsToolSelected(interactable.Interactable.dto.id);
        }

        /// <summary>
        /// Project an interactable that possesses a tool on a controller
        /// </summary>
        /// <param name="interactable"></param>
        /// <param name="controller"></param>
        public void Project(InteractableContainer interactable, AbstractController controller)
        {
            var interactionTool = AbstractInteractionMapper.Instance.GetTool(interactable.Interactable.dto.id);
            Project(interactionTool, interactable.Interactable.dto.id, controller);
        }

        /// <summary>
        /// Project a given tool on a controller
        /// </summary>
        /// <param name="interactionTool"></param>
        /// <param name="selectedObjectId"></param>
        /// <param name="controller"></param>
        public void Project(AbstractTool interactionTool, ulong selectedObjectId, AbstractController controller)
        {
            controller.Project(interactionTool, true, new RequestedUsingSelector<AbstractSelector>() { controller = controller }, selectedObjectId);
        }

        /// <summary>
        /// Release (deproject) a tool from a controller
        /// </summary>
        /// <param name="interactionTool"></param>
        /// <param name="controller"></param>
        public void Release(AbstractTool interactionTool, AbstractController controller)
        {
            controller.Release(interactionTool, new RequestedUsingSelector<AbstractSelector>() { controller = controller });
        }

        /// <inheritdoc/>
        public void Release(InteractableContainer interactable, AbstractController controller)
        {
            controller.Release(AbstractInteractionMapper.Instance.GetTool(interactable.Interactable.dto.id), new RequestedUsingSelector<AbstractSelector>() { controller = controller });
        }
    }
}