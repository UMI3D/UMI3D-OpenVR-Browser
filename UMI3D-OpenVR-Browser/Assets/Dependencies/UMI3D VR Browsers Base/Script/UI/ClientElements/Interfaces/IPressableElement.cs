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
    internal interface IPressableElement : IClientElement
    {
        /// <summary>
        /// Event raised when <see cref="Click"/> is called.
        /// </summary>
        UnityEvent OnPressedDown { get; }

        /// <summary>
        /// Event raised when <see cref="Click"/> is called.
        /// </summary>
        UnityEvent OnPressedUp { get; }

        /// <summary>
        /// Raises an event when this element is pressed down.
        /// </summary>
        /// <param name="controller">Controller used to click</param>
        void PressDown(ControllerType controller);

        /// <summary>
        /// Raises an event when this element is pressed up.
        /// </summary>
        /// <param name="controller">Controller used to click</param>
        void PressUp(ControllerType controller);

        /// <summary>
        /// Returns true if the controller is currently pressing the element
        /// </summary>
        /// <param name="controller"></param>
        bool IsPressed(ControllerType controller);
        void PressStay(ControllerType type);
    }
}