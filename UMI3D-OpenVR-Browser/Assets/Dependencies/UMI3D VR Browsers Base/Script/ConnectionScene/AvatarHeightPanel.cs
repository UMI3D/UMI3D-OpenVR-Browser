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

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// This class is responsible for asking users to set up their avatar heights.
    /// </summary>
    public class AvatarHeightPanel : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        [Tooltip("Main gameobject of the panel")]
        private GameObject panel;

        [SerializeField]
        [Tooltip("Button to confirm avatar's height")]
        private UnityEngine.UI.Button validateButton;

        /// <summary>
        /// List of <see cref="GameObject"/> to activate when avatar's height is set up.
        /// </summary>
        public List<GameObject> objectsToActivate;

        /// <summary>
        /// Has users set their height.
        /// </summary>
        public static bool isSetup = false;

        /// <summary>
        /// Defines what is done when avatar's height is set up.
        /// </summary>
        private System.Action validationCallBack;

        #endregion

        #region Methods

        private void Start()
        {
            validateButton.onClick.AddListener(ValidateButtonClicked);
        }

        [ContextMenu("Set Avatar Height")]
        void ValidateButtonClicked()
        {
            SetUpAvatarHeight setUp = GameObject.FindObjectOfType<SetUpAvatarHeight>();
            Debug.Assert(setUp != null, "No avatar found to set up height. Should not happen");
            setUp.objectsToActivate = objectsToActivate;
            StartCoroutine(setUp.SetUpAvatar());
            Hide();
            validationCallBack?.Invoke();
            isSetup = true;
        }

        /// <summary>
        /// Displays panel to set up avatar's height.
        /// </summary>
        /// <param name="validationCallBack"></param>
        public void Display(System.Action validationCallBack)
        {
            ConnectionMenuManager.instance.Library.SetActive(false);
            panel.SetActive(true);
            this.validationCallBack = validationCallBack;
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