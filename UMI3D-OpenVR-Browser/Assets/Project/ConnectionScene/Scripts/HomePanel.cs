/*
Copyright 2019 - 2021 Inetum
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

public class HomePanel : MonoBehaviour
{
    [SerializeField]
    GameObject panel;

    [SerializeField]
    Button advancedConnectionButton;

    [Header("Add a new server panel")]
    [SerializeField]
    GameObject newServerPanel;
    [SerializeField]
    Text noFavoriteServerLabel;
    [SerializeField]
    InputField addNewServerInput;
    [SerializeField]
    Toggle rememberServerToggle;
    [SerializeField]
    Button connectToServerBtn;

    [Header("Favorite servers panel")]
    [SerializeField]
    GameObject favoriteServerPanel;
    [SerializeField]
    Button connectToANewServerBtn;
    [SerializeField]
    SliderDisplayer favoriteServersSlider;

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

        bool displayFavoriteServers = PlayerPrefsManager.HasFavoriteServersStored() && !forceDisplayAddNewServer;

        newServerPanel.SetActive(!displayFavoriteServers);
        favoriteServerPanel.SetActive(displayFavoriteServers);

        if (displayFavoriteServers)
        {
            favoriteServersSlider.Clear();
            var favoriteServers = PlayerPrefsManager.GetFavoriteServers();
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
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private void BindUI()
    {
        advancedConnectionButton.onClick.AddListener(() =>
        {
            Hide();
            ConnectionMenuManager.instance.SwitchToAdvancedConnectionPanel();
        });

        connectToServerBtn.onClick.AddListener(() =>
        {
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
}
