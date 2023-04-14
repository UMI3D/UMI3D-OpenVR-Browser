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

namespace umi3dBrowsers.interaction.selection.feedback
{
    /// <summary>
    /// Abstract class for haptic selection feedback.
    /// </summary>
    public abstract class AbstractVRHapticSelectionFeedback : MonoBehaviour, IInstantaneousFeedback
    {
        /// <summary>
        /// Haptic settings for selection feedback.
        /// </summary>
        public HapticSettings settings;

        /// <summary>
        /// Container for haptic chain of pulses parameters.
        /// </summary>
        [System.Serializable]
        public struct HapticSettings
        {
            [Tooltip("Duration of the chain of pulses in seconds.")]
            public float duration;

            [Tooltip("Frequency of pulses in the chain.")]
            public float frequency;

            [Tooltip("Amplitude of the pulse. An higher value will result in a stronger vibration.")]
            public float amplitude;
        }

        protected umi3dVRBrowsersBase.interactions.VRController controller;

        private void Awake()
        {
            controller = GetComponentInParent<umi3dVRBrowsersBase.interactions.VRController>();
        }

        /// <summary>
        /// Trigger a pulse according to the haptic settings
        /// </summary>
        public abstract void Trigger();
    }
}