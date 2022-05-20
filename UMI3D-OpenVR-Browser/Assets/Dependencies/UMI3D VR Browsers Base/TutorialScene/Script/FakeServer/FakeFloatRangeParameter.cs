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
    /// Local imitation of UMI3DFloatRange parameter.
    /// </summary>
    public class FakeFloatRangeParameter : AbstractFakeParameter<float>
    {
        #region Fields

        [SerializeField]
        [Tooltip("Event raised when this associated value changes")]
        private FloatEvent onValueChanged = new FloatEvent();

        [SerializeField]
        [Tooltip("Minimul value of associated value")]
        private float min = 0;

        [SerializeField]
        [Tooltip("Maximum value of associated value")]
        private float max = 10;

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override AbstractInteractionDto GetDto()
        {
            return new FloatRangeParameterDto
            {
                id = Id,
                name = displayName,
                value = value,
                min = min,
                max = max
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="val"></param>
        public override void SetValue(object val)
        {
            var dto = (val as FloatRangeParameterDto);

            if (dto != null && !value.Equals(dto.value))
            {
                value = dto.value;
                Debug.Log("VALUE " + value);
                onValueChanged?.Invoke(value);
            }
        }

        #endregion
    }

    /// <summary>
    /// Defines an event which returns a float value.
    /// </summary>
    [System.Serializable]
    public class FloatEvent : UnityEvent<float>
    {

    }
}