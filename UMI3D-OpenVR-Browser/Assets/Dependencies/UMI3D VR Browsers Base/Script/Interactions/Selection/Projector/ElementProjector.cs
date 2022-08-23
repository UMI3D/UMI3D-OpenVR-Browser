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
using umi3dBrowsers.interaction.selection.projector;
using umi3dVRBrowsersBase.ui;

namespace umi3dVRBrowsersBase.interactions.selection.projector
{
    /// <summary>
    /// Projector for AbstractClientElement
    /// </summary>
    /// Does not really projects the UMI3D interactions but act like it is projecting the client own's interactions.
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