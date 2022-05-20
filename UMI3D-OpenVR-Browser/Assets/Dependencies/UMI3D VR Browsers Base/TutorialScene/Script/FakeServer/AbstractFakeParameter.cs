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
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.tutorial.fakeServer
{
    /// <summary>
    /// This class is a local imitation of a UMI3D Parameter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractFakeParameter<T> : AbstractFakeParameter
    {
        [SerializeField]
        [Tooltip("Associated value")]
        protected T value;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            return value;
        }

        [System.Serializable]
        public class ParameterEvent<V> : UnityEvent<T>
        {

        }
    }

    /// <summary>
    /// TThis class is a local imitation of a UMI3D Parameter.
    /// </summary>
    public abstract class AbstractFakeParameter : AbstractFakeInteraction
    {
        /// <summary>
        /// Returns value associated to the parameter.
        /// </summary>
        /// <returns></returns>
        public abstract object GetValue();

        /// <summary>
        /// Sets value associated to the parameter
        /// </summary>
        /// <param name="val"></param>
        public abstract void SetValue(object val);
    }
}