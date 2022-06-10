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

using System.Collections;
using umi3d.cdk.menu;
using umi3d.cdk.menu.interaction;
using umi3dVRBrowsersBase.interactions.input;
using umi3dVRBrowsersBase.ui.watchMenu;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.tutorial
{
    /// <summary>
    /// Object which has a secret text that can be revealed.
    /// </summary>
    public class Relica : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        [Tooltip("Secret text to reveal")]
        private Text translationTxt;

        /// <summary>
        /// Is the secret text revealed ?
        /// </summary>
        private bool isRevealed = false;

        #endregion

        #region Methods

        public void Start()
        {
            var tool = new Menu { Name = "Translation Tool" };

            var button = new EventMenuItem
            {
                Name = "Translate"
            };

            button.Subscribe(() =>
            {
                if (!isRevealed)
                {
                    isRevealed = true;
                    StartCoroutine(TextAppear());
                }
            });

            tool.Add(button);

            WatchMenu.PinMenu(tool);
        }

        /// <summary>
        /// Performs a smooth text reveal.
        /// </summary>
        /// <returns></returns>
        private IEnumerator TextAppear()
        {
            for (int i = 0; i < 50; i++)
            {
                translationTxt.color = new Color(1, 1, 1, (i + 1) * 1 / 50f);
                yield return new WaitForSeconds(.1f);
            }
        }

        #endregion
    }
}