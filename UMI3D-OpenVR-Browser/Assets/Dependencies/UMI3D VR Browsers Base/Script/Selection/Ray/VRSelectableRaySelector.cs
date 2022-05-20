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

using umi3dVRBrowsersBase.interactions;

namespace umi3dVRBrowsersBase.selection
{
    /// <summary>
    /// Selectable ray selector for VR controllers.
    /// </summary>
    public class VRSelectableRaySelector : SelectableRaySelector
    {
        #region Fields

        /// <summary>
        /// Associated controller.
        /// </summary>
        public ControllerType vrController;

        /// <summary>
        /// Action type to trigger selection.
        /// </summary>
        public ActionType action;

        #endregion

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void ActivateInternal()
        {
            if (!activated)
            {
                base.ActivateInternal();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void DeactivateInternal()
        {
            if (activated)
            {
                base.DeactivateInternal();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Update()
        {
            base.Update();
            if (AbstractControllerInputManager.Instance.GetButtonDown(vrController, action))
            {
                if (activated)
                {
                    VRInteractionMapper.lastControllerUsedToClick = vrController;
                    Select();
                }
            }
        }
    }
}