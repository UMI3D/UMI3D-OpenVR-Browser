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

namespace umi3dVRBrowsersBase.settings
{
    /// <summary>
    /// Defines audio is enabled or not.
    /// </summary>
    public class AudioSetting : IEnvironmentSetting
    {
        #region Fields

        /// <summary>
        /// Is audio enabled ?
        /// </summary>
        public bool IsOn { get; private set; } = true;

        /// <summary>
        /// Event raised when audio status changes
        /// </summary>
        public BoolEvent OnValueChanged { get; private set; } = new BoolEvent();

        #endregion

        #region Methods

        /// <summary>
        /// Sets if audio is enabled or not.
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(bool value)
        {
            if (IsOn != value)
            {
                IsOn = value;
                AudioListener.volume = IsOn ? 1 : 0;
                OnValueChanged?.Invoke(IsOn);
            }
        }


        /// <summary>
        /// Toggles <see cref="IsOn"/>.
        /// </summary>
        public void Toggle()
        {
            SetValue(!IsOn);
        }

        #endregion
    }
}