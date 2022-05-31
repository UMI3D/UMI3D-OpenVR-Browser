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
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// This class is responsible for displaying a message if an error happens while users try to connect.
    /// </summary>
    public class ConnectionErrorPanel : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        [Tooltip("Main gameObject of the panel")]
        private GameObject panel;

        [SerializeField]
        private Button callBackButton;

        [SerializeField]
        [Tooltip("Text label associated to callbackButton")]
        private Text buttonLabel;

        [SerializeField]
        [Tooltip("Label used to display error message")]
        private Text errorMessageLabel;

        #endregion

        #region Methods

        /// <summary>
        /// Displays a panel to tell users an error occured while they were trying to join an environment.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="buttonMessage"></param>
        /// <param name="buttonCallBack"></param>
        public void DisplayError(string errorMessage, string buttonMessage, System.Action buttonCallBack)
        {
            callBackButton.onClick.RemoveAllListeners();
            callBackButton.onClick.AddListener(() =>
            {
                Hide();
                buttonCallBack?.Invoke();
            });

            errorMessageLabel.text = errorMessage;
            buttonLabel.text = buttonMessage;

            panel.SetActive(true);
        }

        /// <summary>
        /// Hides panel.
        /// </summary>
        public void Hide()
        {
            panel.SetActive(false);
            callBackButton.onClick.RemoveAllListeners();
            errorMessageLabel.text = string.Empty;
            buttonLabel.text = string.Empty;
        }

        #endregion
    }
}