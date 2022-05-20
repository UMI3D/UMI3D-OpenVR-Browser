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

using umi3d.cdk.menu;
using umi3d.cdk.menu.view;
using umi3dVRBrowsersBase.ui.keyboard;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.displayers
{
    /// <summary>
    /// 2D Displayer for <see cref="TextInputMenuItem"/> in a UMI3D environment.
    /// </summary>
    public class StringInputMenuItemDisplayerEnvironment : AbstractTextInputDisplayer, IDisplayerUIGUI
    {
        #region Fields

        /// <summary>
        /// Input to edit associated text.
        /// </summary>
        public CustomInputWithKeyboardEnvironment inputField;

        /// <summary>
        /// Label to display associated item name.
        /// </summary>
        public Text label;

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Display(bool forceUpdate = false)
        {
            this.gameObject.SetActive(true);
            inputField.text = menuItem.GetValue();
            label.text = menuItem.Name;

            inputField.SetEditionCallback((res) => { NotifyValueChange(res); inputField.SetTextWithoutNotify(res); });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Hide()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public override int IsSuitableFor(AbstractMenuItem menu) => (menu is TextInputMenuItem) ? 2 : 0;

        #endregion
    }
}