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
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.displayers
{
    /// <summary>
    /// 2D displayer for <see cref="BooleanInputMenuItem"/>.
    /// </summary>
    public class ToggleMenuInputDisplayer : AbstractBooleanInputDisplayer, IDisplayerUIGUI
    {
        #region Fields

        /// <summary>
        /// Toggle to edit associated value.
        /// </summary>
        public Toggle toggle;

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
            toggle.isOn = GetValue();
            toggle.onValueChanged.AddListener(NotifyValueChange);
            label.text = this.menuItem.Name;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Hide()
        {
            toggle.onValueChanged.RemoveListener(NotifyValueChange);
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public override int IsSuitableFor(AbstractMenuItem menu) => (menu is BooleanInputMenuItem) ? 2 : 0;

        #endregion
    }
}