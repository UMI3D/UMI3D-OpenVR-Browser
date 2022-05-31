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

using inetum.unityUtils;
using umi3dVRBrowsersBase.ui.keyboard;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// Class responsible of asking users a login and or a password if required by the environment.
    /// </summary>
    public class LoginPasswordAsker : SingleBehaviour<LoginPasswordAsker>
    {
        #region Fields

        [SerializeField]
        [Tooltip("Main gameobject of the panel")]
        private GameObject panel;

        [SerializeField]
        [Tooltip("Input to set environment login")]
        private CustomInputWithKeyboard loginField;

        [SerializeField]
        [Tooltip("Input to set environment password")]
        private CustomInputWithKeyboard passwordField;

        [SerializeField]
        [Tooltip("Button to submit login/password")]
        private Button submitButton;

        [SerializeField]
        [Tooltip("Reference to panel's RectTransform")]
        private RectTransform loginPanel;

        [Tooltip("When the password is played, yLoginOffset is applied to thge login panel to position it above the password input")]
        [SerializeField]
        private int yLoginOffset = 130;

        [SerializeField]
        [Tooltip("Reference to password's panel's RectTransform")]
        private RectTransform passwordPanel;

        #endregion

        #region Methods

        private void Start()
        {
            loginField.SetKeyboard(ConnectionMenuManager.instance.keyboard);
            passwordField.SetKeyboard(ConnectionMenuManager.instance.keyboard);
        }

        /// <summary>
        /// Sets submission callback.
        /// </summary>
        /// <param name="callback"></param>
        public void Register(System.Action<string, string> callback)
        {
            submitButton.onClick.AddListener(() => callback(loginField.text, passwordField.text));
        }

        /// <summary>
        /// Unregisters all events associated to submitButton.
        /// </summary>
        public void UnregisterAll()
        {
            submitButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Displays panel
        /// </summary>
        /// <param name="displayLoginInput"></param>
        public void Display(bool displayLoginInput = true)
        {
            panel.SetActive(true);

            if (displayLoginInput)
            {
                loginPanel.gameObject.SetActive(true);
                passwordPanel.localPosition = new Vector3(loginPanel.localPosition.x, yLoginOffset, loginPanel.localPosition.z);
            }
            else
            {
                loginPanel.gameObject.SetActive(false);
                passwordPanel.localPosition = new Vector3(loginPanel.localPosition.x, 0, loginPanel.localPosition.z);
            }
        }

        /// <summary>
        /// Hides panel.
        /// </summary>
        public void Hide()
        {
            panel.SetActive(false);
        }

        #endregion
    }
}