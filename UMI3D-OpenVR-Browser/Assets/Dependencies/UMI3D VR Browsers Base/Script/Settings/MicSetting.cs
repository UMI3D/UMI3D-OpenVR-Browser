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

using umi3d.cdk.collaboration;

namespace umi3dVRBrowsersBase.settings
{
    /// <summary>
    /// Defines if user's microphone is enabled or not.
    /// </summary>
    public class MicSetting : IEnvironmentSetting
    {
        #region Fields

        /// <summary>
        /// Is user's microphone enabled ?
        /// </summary>
        public bool IsOn
        {
            get => !MicrophoneListener.IsMute;
            set
            {
                if (value == MicrophoneListener.IsMute)
                {
                    MicrophoneListener.IsMute = !value;
                    OnValueChanged?.Invoke(!MicrophoneListener.IsMute);
                }
            }
        }

        /// <summary>
        /// Event raised when microphone status changes.
        /// </summary>
        public BoolEvent OnValueChanged { get; private set; } = new BoolEvent();

        #endregion

        #region Methods

        /// <summary>
        /// Sets if microphone is enabled or disabled ?
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(bool value)
        {
            IsOn = value;
        }

        /// <summary>
        /// Toogles microphone status.
        /// </summary>
        public void Toggle()
        {
            IsOn = !IsOn;
        }

        #endregion
    }
}