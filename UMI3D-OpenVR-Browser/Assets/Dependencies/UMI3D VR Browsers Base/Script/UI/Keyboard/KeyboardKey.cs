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
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.keyboard
{
    /// <summary>
    /// Base class for a <see cref="Keyboard"/> key.
    /// </summary>
    public class KeyboardKey : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Event raised when the key is pressed.
        /// </summary>
        [HideInInspector]
        public UnityEvent onPressed;

        /// <summary>
        /// Event raised when a user starts hovering the key.
        /// </summary>
        [HideInInspector]
        public UnityEvent onHoverEnter;

        /// <summary>
        /// Event raised when a user stops hovering the key.
        /// </summary>
        [HideInInspector]
        public UnityEvent onHoverExit;

        /// <summary>
        /// Button to interact with the key
        /// </summary>
        public Button button;

        #endregion

        #region Methods

        protected virtual void Start()
        {
            button.onClick.AddListener(() => onPressed?.Invoke());
        }

        /// <summary>
        /// Raises <see cref="onHoverEnter"/>.
        /// </summary>
        public void HoverEnter()
        {
            onHoverEnter?.Invoke();
        }

        /// <summary>
        /// Raises <see cref="onHoverExit"/>.
        /// </summary>
        public void HoverExit()
        {
            onHoverExit?.Invoke();
        }

        #endregion
    }
}
