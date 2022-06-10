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

using umi3dVRBrowsersBase.ui.keyboard;
using UnityEngine;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// Manages the panel which enabes user to join an environement directly with an ip and a port.
    /// </summary>
    public class AdvancedConnectionPanel : MonoBehaviour
    {
        #region Methods

        /// <summary>
        /// Main gameobject of the panel.
        /// </summary>
        public GameObject panel;

        /// <summary>
        /// Input to set environment ip.
        /// </summary>
        public CustomInputWithKeyboard ip;

        /// <summary>
        /// Input to set environment port.
        /// </summary>
        public CustomInputWithKeyboard port;

        /// <summary>
        /// Class which stores all data required to join an environment.
        /// </summary>
        public class Data
        {
            public string ip;
            public string port;
        }

        #endregion

        #region Methods

        private void Start()
        {
            ip.text = PlayerPrefsManager.GetUmi3dIp();
            port.text = PlayerPrefsManager.GetUmi3DPort();

            ip.SetKeyboard(ConnectionMenuManager.instance.keyboard);

            port.SetKeyboard(ConnectionMenuManager.instance.keyboard);

            Hide();
        }

        /// <summary>
        /// Initiates the connection to an UMI3D environment.
        /// </summary>
        public void Run()
        {
            LoadingPanel.Instance.Display("Connection ...");

            if ((ip != null) && (ip.text != null) && (ip.text != ""))
            {
                PlayerPrefsManager.SaveUmi3dIp(ip.text);
            }
            if ((port != null) && (port.text != null) && (port.text != ""))
            {
                PlayerPrefsManager.SaveUmi3dPort(port.text);
            }

            Hide();

            ConnectionMenuManager.instance.ConnectToUmi3DEnvironement(ip.text, port.text);
        }

        /// <summary>
        /// Displays the panel to join an UMI3D environment.
        /// </summary>
        public void Display()
        {
            panel.SetActive(true);
        }

        /// <summary>
        /// Hides the panel.
        /// </summary>
        public void Hide()
        {
            panel.SetActive(false);
        }

        #endregion
    }
}