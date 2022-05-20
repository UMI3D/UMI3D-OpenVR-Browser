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
using umi3dVRBrowsersBase.ui;
using umi3dVRBrowsersBase.ui.keyboard;
using umi3dVRBrowsersBase.ui.playerMenu;
using umi3dVRBrowsersBase.ui.watchMenu;

namespace umi3dVRBrowsersBase.selection
{
    /// <summary>
    /// Ray selector for VR controllers.
    /// 
    /// </summary>
    public class VRInteractableRaySelector : InteractableRaySelector
    {
        /// <summary>
        /// Action type to perform a selection.
        /// </summary>
        public ActionType action;

        /// <summary>
        /// If exists, <see cref="WatchMenu"/> associated to <see cref="controller"/>.
        /// </summary>
        WatchMenu associatedWatch;

        #region Fields

        protected override void Start()
        {
            base.Start();

            associatedWatch = WatchMenu.FindInstanceAssociatedToController((controller as VRController).type);
        }

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
            if ((controller as VRController).IsInputPressed || ParameterGear.Instance.IsHovered ||
                (PlayerMenuManager.Exists && PlayerMenuManager.Instance.IsOpen) || (Keyboard.Instance?.IsOpen ?? false)
                || VRClickableElementSelector.IsElementHovered()
                || (associatedWatch != null && associatedWatch.IsObjectInPlayerFieldOfView()))
                return;

            base.Update();
        }

        #endregion
    }
}