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

using System.Collections;
using umi3d;
using umi3d.cdk;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SignalingAsker;

/// <summary>
/// This class manages the home connection menu. It starts by displaying a menu controlled by <see cref="HomePanel"/> which enabled
/// users to connect to an UMI3D environement in two different ways :
///     - Advanced connection mode : managed by <see cref="SignalingAsker"/>.
///     - Master connection mode : managed by <see cref="ConnectToSessionPanel"/>.
///     
/// Once a connection to a server is established, <see cref="Connecting"/> is reponsible for asking users information required by the environement
/// and for starting loading it.
/// </summary>
public class ConnectionMenuManager : MonoBehaviour
{
    #region Fields

    public static ConnectionMenuManager instance;

    [SerializeField]
    Text Umi3DVersionLabel;

    [SerializeField]
    string connectionSceneName;

    public GameObject mainCanvas;

    [Header(" -- Panels -- ")]
    public HomePanel homePanel;
    public SignalingAsker advancedConnectionPanel;
    public ConnectToSessionPanel connectionToSessionPanel;
    public ConnectionErrorPanel errorPanel;
    public AvatarHeightPanel avatarHeightPanel;

    [Header("Navigation")]
    [SerializeField]
    Button previousButton;
    [SerializeField]
    Button nextButton;

    #endregion

    #region Methods

    #region Display Action

    private void Awake()
    {
        if (instance != null)
            Debug.LogError("There should be only on instance of ConnectionMenuManager in the scene");

        instance = this;
    }

    private void Start()
    {
        HidePreviousNavigationButton();
        HideNextNavigationButton();
        DisplayHome();

        Umi3DVersionLabel.text = UMI3DVersion.version;
    }

    /// <summary>
    /// Display the advanced connection panel.
    /// Warning : this method does not hide the previous panel displayed.
    /// </summary>
    public void SwitchToAdvancedConnectionPanel()
    {
        advancedConnectionPanel.Display();

        previousButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(false);

        previousButton.onClick.RemoveAllListeners();
        nextButton.onClick.RemoveAllListeners();

        previousButton.onClick.AddListener(() =>
        {
            previousButton.onClick.RemoveAllListeners();
            advancedConnectionPanel.Hide();
            previousButton.gameObject.SetActive(false);
            homePanel.Display();
        });
    }

    /// <summary>
    /// Show a button to enable users to go back.
    /// </summary>
    /// <param name="callback">Defines what to do when <see cref="previousButton"/> is clicked.
    public void ShowPreviousNavigationButton(System.Action callback)
    {
        previousButton.onClick.AddListener(() => callback?.Invoke());
        previousButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides <see cref="previousButton"/>.
    /// </summary>
    public void HidePreviousNavigationButton()
    {
        previousButton.onClick.RemoveAllListeners();
        previousButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Show a button to enable users to go next.
    /// </summary>
    /// <param name="callback">Defines what to do when <see cref="nextButton"/> is clicked.
    public void ShowNextNavigationButton(System.Action callback)
    {
        nextButton.onClick.AddListener(() => callback?.Invoke());
        nextButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides <see cref="nextButton"/>.
    /// </summary>
    public void HideNextNavigationButton()
    {
        nextButton.onClick.RemoveAllListeners();
        nextButton.gameObject.SetActive(false);
    }

    public void DisplayError(string error, string buttonMessage, System.Action buttonCallback)
    {
        errorPanel.DisplayError(error, buttonMessage, buttonCallback);
    }

    /// <summary>
    /// Displays home menu or the set up avatar height pop before is it has ne been set up already.
    /// </summary>
    public void DisplayHome()
    {
        HidePreviousNavigationButton();
        if (AvatarHeightPanel.isSetup)
        {
            homePanel.Display();
        }
        else
        {
            mainCanvas.SetActive(false);
            avatarHeightPanel.Display(() =>
            {
                mainCanvas.SetActive(true);
                homePanel.Display();
            });
        }

    }

    #endregion

    #endregion

    #region Connection Methods

    /// <summary>
    /// Starts the connection to a master server located at <paramref name="url"/>.
    /// </summary>
    /// <param name="url"></param>
    public void ConnectToMasterServer(string url)
    {
        homePanel.Hide();
        connectionToSessionPanel.ConnectToMasterServer(url);
    }

    /// <summary>
    /// Starts the connection to an Umi3DEnvironement.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="port"></param>
    public void ConnectToUmi3DEnvironement(string url, string port)
    {
        StartCoroutine(WaitReady(new Data() { ip = url, port = port }));
    }

    IEnumerator WaitReady(Data data)
    {
        while (!Connecting.Exists && !UMI3DEnvironmentLoader.Exists)
            yield return new WaitForEndOfFrame();

        Connecting.Instance.Connect(data);

        while (!UMI3DEnvironmentLoader.Exists)
            yield return new WaitForEndOfFrame();
        UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(() => SceneManager.UnloadSceneAsync(connectionSceneName));
    }

    #endregion
}
