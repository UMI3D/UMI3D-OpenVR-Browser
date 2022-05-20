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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.interactions
{
    /// <summary>
    /// Observer for VR controller buttons.
    /// </summary>
    public class VRInputObserver : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Input acyion type.
        /// </summary>
        public ActionType action;

        /// <summary>
        /// Type of controller.
        /// </summary>
        public ControllerType controller;

        /// <summary>
        /// Event raised when the input is pressed.
        /// </summary>
        public UnityEvent onActionDown;

        /// <summary>
        /// Event raised while the input is pressed.
        /// </summary>
        public UnityEvent onAction;

        /// <summary>
        /// Event raised when the input is released.
        /// </summary>
        public UnityEvent onActionUp;

        /// <summary>
        /// List of suscribers to <see cref="onActionUp"/>.
        /// </summary>
        private List<System.Action> subscribersUp = new List<System.Action>();

        /// <summary>
        /// List of suscribers to <see cref="onActionDown"/>.
        /// </summary>
        private List<System.Action> subscribersDown = new List<System.Action>();

        /// <summary>
        /// List of suscribers to <see cref="onAction"/>.
        /// </summary>
        private List<System.Action> subscribers = new List<System.Action>();

        #endregion

        #region Methods

        /// <summary>
        /// Adds a suscriber to <see cref="onActionUp"/>.
        /// </summary>
        /// <param name="callback"></param>
        public void AddOnStateUpListener(System.Action callback)
        {
            subscribersUp.Add(callback);
        }

        /// <summary>
        /// Adds a suscriber to <see cref="onActionDown"/>.
        /// </summary>
        /// <param name="callback"></param>
        public void AddOnStateDownListener(System.Action callback)
        {
            subscribersDown.Add(callback);
        }

        /// <summary>
        /// Adds a suscriber to <see cref="onAction"/>.
        /// </summary>
        /// <param name="callback"></param>
        public void AddOnStateListener(System.Action callback)
        {
            subscribers.Add(callback);
        }

        /// <summary>
        /// Removes a suscriber from <see cref="onActionUp"/>.
        /// </summary>
        /// <param name="callback"></param>
        public void RemoveOnStateUpListener(System.Action callback)
        {
            subscribersUp.Remove(callback);
        }

        /// <summary>
        /// Removes a suscriber from <see cref="onActionDown"/>.
        /// </summary>
        /// <param name="callback"></param>
        public void RemoveOnStateDownListener(System.Action callback)
        {
            subscribersDown.Remove(callback);
        }

        /// <summary>
        /// Removes a suscriber from <see cref="onAction"/>.
        /// </summary>
        /// <param name="callback"></param>
        public void RemoveOnStateListener(System.Action callback)
        {
            subscribers.Remove(callback);
        }

        private void Update()
        {
            if (AbstractControllerInputManager.Instance.GetButtonUp(controller, action))
            {
                onActionUp.Invoke();
                foreach (System.Action action in subscribersUp)
                    action.Invoke();
            }
            else if (AbstractControllerInputManager.Instance.GetButtonDown(controller, action))
            {
                onActionDown.Invoke();
                foreach (System.Action action in subscribersDown)
                    action.Invoke();
            }
            else if (AbstractControllerInputManager.Instance.GetButton(controller, action))
            {
                onAction.Invoke();
                foreach (System.Action action in subscribers)
                    action.Invoke();
            }
        }

        #endregion
    }
}