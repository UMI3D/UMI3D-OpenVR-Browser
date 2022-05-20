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

using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.tutorial.fakeServer
{
    /// <summary>
    /// Local imitation of UMI3D string parameter.
    /// </summary>
    public class FakeStringParameter : AbstractFakeParameter<string>
    {
        [SerializeField]
        [Tooltip("Event raised when this associated value changes")]
        private StringEvent onValueChanged = new StringEvent();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override AbstractInteractionDto GetDto()
        {
            return new StringParameterDto
            {
                id = Id,
                name = displayName,
                value = value
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="val"></param>
        public override void SetValue(object val)
        {
            if (!value.Equals(val))
            {
                value = (string)val;
                onValueChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Defines an event which returns a string value.
        /// </summary>
        [System.Serializable]
        public class StringEvent : UnityEvent<string>
        {

        }
    }
}