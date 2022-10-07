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
    /// Represents a favorite user server.
    /// </summary>
    public class FavoriteServerEntry : MonoBehaviour
    {
        #region Fields

        [Tooltip("Button to iniates the connection to the favorite server")]
        public Button button;

        [Tooltip("Label to display the name of the server or its ip")]
        public Text label;

        [Tooltip("Button to remove this server from favorite ones")]
        public Button deleteButton;

        #endregion

        #region Methods

        /// <summary>
        /// Sets up the UI with required information.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="url"></param>
        /// <param name="slider"></param>
        public void SetUp(string name, string url, SliderDisplayer slider)
        {
            label.text = name;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(async () =>
            {
                await ConnectionMenuManager.instance._Connect(url);
            });

            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() =>
            {
                DialogBox.Instance.Display("Remove " + name + " server", "Are you sure ?", "Yes", (b) =>
                {
                    if (b)
                    {
                        slider.RemoveElement(this.gameObject);
                        PlayerPrefsManager.RemoveServerFromFavorite(url);

                        if (slider.NbOfElements == 0)
                            ConnectionMenuManager.instance.DisplayHome();
                    }
                });
            });
        }

        #endregion
    }
}