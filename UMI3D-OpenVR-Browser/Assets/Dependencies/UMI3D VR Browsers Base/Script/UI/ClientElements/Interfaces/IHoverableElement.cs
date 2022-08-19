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
    /// <see cref="IClientElement"/> that reacts to hover enter/exit
    /// </summary>
    internal interface IHoverableElement : IClientElement
    {
        /// <summary>
        /// Event raised when <see cref="Click"/> is called.
        /// </summary>
        UnityEvent OnHoverEnter { get; }

        /// <summary>
        /// Event raised when <see cref="Click"/> is called.
        /// </summary>
        UnityEvent OnHoverExit { get; }

        /// <summary>
        /// Called when the object is hovered / a raycast target
        /// </summary>
        /// <param name="controller"></param>
        void HoverEnter(ControllerType controller);
        /// <summary>
        /// Called when the object is no longer hovered / a raycast target
        /// </summary>
        /// <param name="controller"></param>
        void HoverExit(ControllerType controller);
        /// <summary>
        /// True when the object is hovered / a raycast target
        /// </summary>
        /// <param name="controller"></param>
        bool IsHovered(ControllerType controller);
    }
}