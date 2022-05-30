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

using umi3d.cdk.menu;
using umi3d.cdk.menu.interaction;
using umi3d.cdk.menu.view;
using umi3dVRBrowsersBase.interactions.input;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.displayers
{
    /// <summary>
    /// 2D Displayer for  <see cref="ButtonMenuItem"/>.
    /// </summary>
    public class ButtonInputDisplayer : AbstractInputMenuItemDisplayer<bool>, IDisplayerUIGUI
    {
        #region Fields

        /// <summary>
        /// Associated item.
        /// </summary>
        private EventMenuItem menuItem;

        /// <summary>
        /// Button to select the item.
        /// </summary>
        public Button button;

        /// <summary>
        /// Label to display <see cref="menuItem"/> name.
        /// </summary>
        public Text label;

        #endregion

        #region Methods

        /// <summary>
        /// Notifies <see cref="menuItem"/> that its value changes.
        /// </summary>
        public void NotifyPress()
        {
            menuItem.NotifyValueChange(true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="menu"></param>
        public override void SetMenuItem(AbstractMenuItem menu)
        {
            base.SetMenuItem(menu);
            menuItem = menu as EventMenuItem;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Display(bool forceUpdate = false)
        {
            this.gameObject.SetActive(true);
            label.text = menuItem.Name;
            button.onClick.AddListener(this.NotifyPress);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Hide()
        {
            this.gameObject.SetActive(false);
            button.onClick.RemoveListener(this.NotifyPress);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override int IsSuitableFor(AbstractMenuItem menu)
        {
            return (menu is EventMenuItem) ? 2 : 0;
        }

        #endregion
    }
}