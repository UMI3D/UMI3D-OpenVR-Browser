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
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk.menu;
using umi3dVRBrowsersBase.interactions.input;
using UnityEngine;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    public partial class PlayerToolboxMenu
    {
        /// <summary>
        /// Toggles the display of the menu with all pinned items.
        /// </summary>
        public void ToggleDisplayMenu()
        {
            if (IsOpen)
                Close();
            else
                Open();
        }

        /// <summary>
        /// Navigates to the [menu].
        /// </summary>
        /// <param name="menu"></param>
        public void NavigateTo(AbstractMenu menu)
            => menuDisplayManager.Navigate(menu);
    }

    public partial class PlayerToolboxMenu : AbstractMenuManager
    {
        private void Start()
        {
            Close();
        }

        /// <summary>
        /// Open the toolboxes player menu.
        /// </summary>
        [ContextMenu("Open player menu")]
        public override void Open()
        {
            base.Open();

            PlayerMenuManager.Instance.CtrlToolMenu.Hide();

            menuDisplayManager.Display(true);
        }

        /// <summary>
        /// Closes the toolboxes player menu.
        /// </summary>
        public override void Close()
        {
            base.Close();
            menuDisplayManager.Hide(false);
        }
    }
}
