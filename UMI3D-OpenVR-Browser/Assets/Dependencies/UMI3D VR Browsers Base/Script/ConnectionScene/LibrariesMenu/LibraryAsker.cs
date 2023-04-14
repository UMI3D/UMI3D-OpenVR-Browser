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
using System;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// This class is responsible for asking users if they authorize required library downloading when they join an environment.
    /// </summary>
    public class LibraryAsker : SingleBehaviour<LibraryAsker>
    {
        #region Fields

        /// <summary>
        /// Pop up panel.
        /// </summary>
        public GameObject panel;

        /// <summary>
        /// Text used to explain what is asked to users.
        /// </summary>
        public Text info;

        /// <summary>
        /// Button to accept.
        /// </summary>
        public Button accept;

        /// <summary>
        /// Button to refuse.
        /// </summary>
        public Button deny;

        #endregion

        #region Methods

        /// <summary>
        /// Displays a pop up to accept or refuse the downloading.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="callback"></param>
        public void AskForDownload(int count, Action<bool> callback)
        {
            LoadingPanel.Instance.HideLoadingScreen();
            LoadingPanel.Instance.Hide();
            // Fix for Laval
            UnityEngine.Debug.Log("<color=red>For Laval: </color>" + $"To be updated");
            callback(true);
            return;


            LoadingPanel.Instance.DisplayObjectHidden();
            panel.SetActive(true);
            accept.onClick.RemoveAllListeners();
            deny.onClick.RemoveAllListeners();

            info.text = count + " Libraries are required to join the environment.";
            accept.onClick.AddListener(() =>
            {
                callback(true);
                LoadingPanel.Instance?.Display("Downloading libraries ...");
                Hide();
            });
            deny.onClick.AddListener(() =>
            {
                callback(false);
                Hide();
            });
        }

        /// <summary>
        /// Hides the pop opened by <see cref="AskForDownload(int, Action{bool})"/>.
        /// </summary>
        public void Hide()
        {
            panel.SetActive(false);
        }

        #endregion
    }
}


