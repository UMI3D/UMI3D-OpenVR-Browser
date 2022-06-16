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

using umi3d.cdk.interaction;
using umi3d.cdk.interaction.selection.feedback;
using umi3dVRBrowsersBase.interactions;
using UnityEngine;
using Valve.VR;

namespace umi3dbrowser.openvr.interaction.selection.feedback
{
    /// <summary>
    /// Hatic feedback for OpenVR devices
    /// </summary>
    public class OpenVRHapticSelectionFeedback : MonoBehaviour, IInstantaneousFeedback
    {
        public HapticSettings settings;

        [System.Serializable]
        public struct HapticSettings
        {
            public float duration;
            public float frequency;
            public float amplitude;
        }
        public SteamVR_Action_Vibration hapticAction;

        private VRController controller;

        private void Awake()
        {
            controller = GetComponentInParent<VRController>();
        }

        public void Trigger()
        {
            var input = controller.type == ControllerType.LeftHandController ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
            hapticAction.Execute(0, settings.duration, settings.frequency, settings.amplitude, input);
        }
    }
}