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

using umi3d.cdk.userCapture;

namespace umi3dVRBrowsersBase.settings
{
    /// <summary>
    /// Defines if avatar's tracking are sent to the server or not.
    /// </summary>
    public class AvatarSetting : IEnvironmentSetting
    {
        #region Fields

        /// <summary>
        /// Are avatar's trackings sent to server ?
        /// </summary>
        public bool IsOn { get; private set; } = true;

        /// <summary>
        /// Event raised when avatar status changes.
        /// </summary>
        public BoolEvent OnValueChanged { get; private set; } = new BoolEvent();

        #endregion

        #region Methods

        /// <summary>
        /// Sets if avatar's tracking are sent or not to the server.
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(bool value)
        {
            if (!EnvironmentSettings.Instance.IsEnvironmentLoaded)
                return;

            if (value != IsOn && UMI3DClientUserTracking.Exists)
            {
                IsOn = value;
                UMI3DClientUserTracking.Instance.setTrackingSending(IsOn);
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