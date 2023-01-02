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
using System.Collections;
using umi3d.cdk;
using umi3dVRBrowsersBase.rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// Manages loading environment screen.
    /// </summary>
    public class LoadingPanel : SingleBehaviour<LoadingPanel>
    {
        #region Fields

        [Tooltip("Main gameobject of this panel")]
        public GameObject panel;

        [Tooltip("Label to diplay a loading message")]
        public Text message;

        /// <summary>
        /// Objects to hide while environment is loading.
        /// </summary>
        public GameObject[] objectsToHide;

        /// <summary>
        /// Event called when the loading of the environment starts.
        /// </summary>
        public static UnityEvent OnLoadingEnvironmentStart = new UnityEvent();

        /// <summary>
        /// Event called when the loading og the environment ends.
        /// </summary>
        public static UnityEvent OnLoadingEnvironmentFinish = new UnityEvent();

        #endregion

        private void Start()
        {
            StartCoroutine(WaitForLoader());
            Hide();
        }

        /// <summary>
        /// Waits for <see cref="UMI3DEnvironmentLoader"/> existence before listening to its events.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForLoader()
        {
            while (!UMI3DEnvironmentLoader.Exists)
                yield return null;

            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(HideLoadingScreen);
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(() =>
            {
                OnLoadingEnvironmentFinish?.Invoke();
                AudioListener.volume = 1.0f;
            });
            //UMI3DEnvironmentLoader.Instance.onProgressChange.AddListener(v =>
            //{
            //    if (v == 0)
            //    {
            //        OnLoadingEnvironmentStart?.Invoke();
            //        AudioListener.volume = 0.0f;
            //    }
            //});
        }

        /// <summary>
        /// Sets up loading screen.
        /// </summary>
        public void SetLoadingScreen()
        {
            ConnectionMenuManager.instance.HideNextNavigationButton();
            ConnectionMenuManager.instance.HidePreviousNavigationButton();

            SetLightningSettings.ResetLightningSettings();
            LoadingScreenDisplayer.Instance.Display();
            foreach (GameObject o in objectsToHide)
                o.SetActive(false);
        }

        public void DisplayObjectHidden()
        {
            foreach (GameObject o in objectsToHide)
                o.SetActive(true);
        }

        /// <summary>
        /// Hides panel
        /// </summary>
        public void HideLoadingScreen()
        {
            LoadingScreenDisplayer.Instance.Hide();
        }

        /// <summary>
        /// Displays a loading string with a message.
        /// </summary>
        /// <param name="message"></param>
        public void Display(string message)
        {
            panel?.SetActive(true);
            if (this.message != null)
                this.message.text = message;
        }

        /// <summary>
        /// Hides the loading screen.
        /// </summary>
        public void Hide()
        {
            panel?.SetActive(false);
        }
    }
}