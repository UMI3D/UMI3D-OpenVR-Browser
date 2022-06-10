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
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// Manages home menu of the connection scene.
    /// </summary>
    public class HomePanel : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        [Tooltip("Main gameobject of the panel")]
        private GameObject panel;

        [SerializeField]
        [Tooltip("Button to open advanced connection panel")]
        private Button advancedConnectionButton;

        [Header("Add a new server panel")]

        [SerializeField]
        [Tooltip("Main gameobject of add new server panel")]
        private GameObject newServerPanel;

        [SerializeField]
        [Tooltip("Label to display that no server was set as favorite")]
        private Text noFavoriteServerLabel;

        [SerializeField]
        [Tooltip("Input to enter a new server ip")]
        private CustomInputWithKeyboard addNewServerInput;

        [SerializeField]
        [Tooltip("Toogle to set new server as a favorite")]
        private Toggle rememberServerToggle;

        [SerializeField]
        [Tooltip("Button to initiate the connection to a UMI3D server")]
        private Button connectToServerBtn;

        [Header("Favorite servers panel")]

        [SerializeField]
        [Tooltip("Gameobject which displays all favorite servers")]
        private GameObject favoriteServerPanel;

        [SerializeField]
        [Tooltip("Button to initiate the connection to a new server")]
        private Button connectToANewServerBtn;

        [SerializeField]
        [Tooltip("Carousel which displays all favorite servers")]
        private SliderDisplayer favoriteServersSlider;

        #endregion

        #region Methods

        private void Awake()
        {
            BindUI();
            Hide();
        }

        /// <summary>
        /// Display the home panel (with favorites servers if they exist, inputs to add a new server otherwise.
        /// </summary>
        /// <param name="forceDisplayAddNewServer"></param>
        public void Display(bool forceDisplayAddNewServer = false)
        {
            panel.SetActive(true);
            LoadingPanel.Instance.Hide();

            bool displayFavoriteServers = PlayerPrefsManager.HasFavoriteServersStored() && !forceDisplayAddNewServer;

            newServerPanel.SetActive(!displayFavoriteServers);
            favoriteServerPanel.SetActive(displayFavoriteServers);

            if (displayFavoriteServers)
            {
                favoriteServersSlider.Clear();
                System.Collections.Generic.List<PlayerPrefsManager.FavoriteServerData> favoriteServers = PlayerPrefsManager.GetFavoriteServers();
                foreach (PlayerPrefsManager.FavoriteServerData data in favoriteServers)
                {
                    GameObject go = Instantiate(favoriteServersSlider.baseElement, favoriteServersSlider.Container.transform);
                    FavoriteServerEntry entry = go.GetComponent<FavoriteServerEntry>();
                    Debug.Assert(entry != null);
                    entry.SetUp(data.serverName, data.serverUrl, favoriteServersSlider);
                    favoriteServersSlider.AddElement(go);
                }

            }
            else
            {
                noFavoriteServerLabel.gameObject.SetActive(!forceDisplayAddNewServer);

                if (forceDisplayAddNewServer)
                {
                    ConnectionMenuManager.instance.ShowPreviousNavigationButton(() =>
                    {
                        ConnectionMenuManager.instance.HidePreviousNavigationButton();
                        Display();
                    });
                }
            }

            ConnectionMenuManager.instance.keyboard.Hide();
        }

        /// <summary>
        /// Hides panel
        /// </summary>
        public void Hide()
        {
            panel.SetActive(false);
        }

        /// <summary>
        /// Binds all UI elements to their actions
        /// </summary>
        private void BindUI()
        {
            addNewServerInput.SetKeyboard(ConnectionMenuManager.instance.keyboard);

            advancedConnectionButton.onClick.AddListener(() =>
            {
                Hide();
                ConnectionMenuManager.instance.SwitchToAdvancedConnectionPanel();
            });

            connectToServerBtn.onClick.AddListener(() =>
            {
                ConnectionMenuManager.instance.keyboard.Hide();

                string url = addNewServerInput.text.Trim();

                if (string.IsNullOrEmpty(url))
                    return;

                if (rememberServerToggle.isOn)
                {
                    PlayerPrefsManager.AddServerToFavorite(url, url);
                }

                ConnectionMenuManager.instance.ConnectToMasterServer(url);
            });

            connectToANewServerBtn.onClick.AddListener(() =>
            {
                Display(true);
            });
        }

        #endregion
    }
}