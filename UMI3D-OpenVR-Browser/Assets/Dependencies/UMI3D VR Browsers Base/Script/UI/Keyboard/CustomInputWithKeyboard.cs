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

using UnityEngine;
using UnityEngine.EventSystems;

namespace umi3dVRBrowsersBase.ui.keyboard
{
    /// <summary>
    /// Custom <see cref="UnityEngine.UI.InputField"/> which works with <see cref="Keyboard"/> (both displayed at the same time).
    /// If the keyboard must hide the inputfield to edit it, <see cref="CustomInputWithKeyboardEnvironment"/>.
    /// </summary>
    public class CustomInputWithKeyboard : CustomInputSelectable
    {
        /// <summary>
        /// Keyboard to edit this inputfield.
        /// </summary>
        protected Keyboard keyboard;

        /// <summary>
        /// Setter for <see cref="keyboard"/>.
        /// </summary>
        /// <param name="keyboard"></param>
        public void SetKeyboard(Keyboard keyboard)
        {
            this.keyboard = keyboard;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            if (keyboard != null)
            {
                keyboard.OpenKeyboard(this, res =>
                {
                    text = res;
                });
            }
            else
            {
                Debug.Log("Set keyboard of this input before enabling users to use it.");
            }
        }
    }
}