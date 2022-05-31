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

using UnityEngine.Events;

namespace umi3dVRBrowsersBase.settings
{
    /// <summary>
    /// Handles a session setting, managed by <see cref="EnvironmentSettings"/>.
    /// </summary>
    public interface IEnvironmentSetting
    {
        /// <summary>
        /// Is associated setting on ?
        /// </summary>
        bool IsOn { get; }

        /// <summary>
        /// Event to raise when associated value changes.
        /// </summary>
        BoolEvent OnValueChanged { get; }

        /// <summary>
        /// Toggles associated value.
        /// </summary>
        void Toggle();

        /// <summary>
        /// Sets associated value.
        /// </summary>
        /// <param name="value"></param>
        void SetValue(bool value);
    }

    public class BoolEvent : UnityEvent<bool>
    {
    }
}