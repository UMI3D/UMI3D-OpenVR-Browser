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

using umi3dVRBrowsersBase.interactions;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.ui
{
    /// <summary>
    /// <see cref="IClientElement"/> that can be selected
    /// </summary>
    internal interface ISelectableElement : IClientElement
    {
        /// <summary>
        /// Event raised when selection is detected.
        /// </summary>
        UnityEvent OnSelected { get; }

        /// <summary>
        /// Event raised when selection is no longer detected.
        /// </summary>
        UnityEvent OnDeselected { get; }

        /// <summary>
        /// Selects the object
        /// </summary>
        /// <param name="controller">Controller used for selection</param>
        void Select(VRController controller);

        /// <summary>
        /// Deselects the object
        /// </summary>
        /// <param name="controller">Controller that was used for selection</param>
        void Deselect(VRController controller);

        /// <summary>
        /// Returns true i the object is currenlty selected
        /// </summary>
        /// <returns></returns>
        bool IsSelected();
    }
}