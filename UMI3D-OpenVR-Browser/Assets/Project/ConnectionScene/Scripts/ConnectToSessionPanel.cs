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
using MainThreadDispatcher;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the ConnectToSessionPanel i.e. getting the pin server entered by users, displaying all sessions related to this pin 
/// and starting the connection to the selected session.
/// </summary>
public class ConnectToSessionPanel : MonoBehaviour
{
    #region Fields

    [SerializeField]
    GameObject mainPanel;

    [SerializeField]
    GameObject sessionPinPanel;

    [SerializeField]
    InputField sessionPinInput;

    [SerializeField]
    Button confirmPinButton;

    [SerializeField]
    GameObject sessionListPanel;

    [SerializeField]
    SliderDisplayer sessionListSlider;

    LaucherOnMasterServer launcher = new LaucherOnMasterServer();

    ServerSessionEntry currentSelectedEntry;

    [SerializeField]
    [Tooltip("In seconds, after this time, if no connection was established, display an error message.")]
    float maxConnectionTime = 5;

    #endregion

    #region Methods

    Coroutine waitForGettingSessions;

    private void Awake()
    {
        confirmPinButton.onClick.AddListener(() =>
        {
            string pin = sessionPinInput.text.Trim();
            if (string.IsNullOrEmpty(pin))
                return;

            sessionListSlider.Clear();
            launcher.SendDataSession(pin, DisplaySession);

            LoadingScreen.Instance.Display("Connecting ...");

            waitForGettingSessions = StartCoroutine(WaitForGettingSessions());
        });
    }

    Coroutine waitForMasterServerConnection;

    /// <summary>
    /// Start the connection to a server.
    /// </summary>
    /// <param name="url"></param>
    public void ConnectToMasterServer(string url)
    {
        mainPanel.SetActive(true);
        sessionPinPanel.SetActive(false);
        sessionListPanel.SetActive(false);
        currentSelectedEntry = null;

        LoadingScreen.Instance.Display("Connecting ...");

        ConnectionMenuManager.instance.HideNextNavigationButton();
        ConnectionMenuManager.instance.ShowPreviousNavigationButton(() =>
        {
            if (waitForMasterServerConnection != null)
            {
                StopCoroutine(waitForMasterServerConnection);
            }

            if (waitForGettingSessions != null)
                StopCoroutine(waitForGettingSessions);

            LoadingScreen.Instance.Hide();
            mainPanel.SetActive(false);
            sessionPinInput.text = string.Empty;
            ConnectionMenuManager.instance.DisplayHome();
        });

        waitForMasterServerConnection = StartCoroutine(WaitForMasterServerConnection(url));
        launcher.ConnectToMasterServer(OnConnectToMasterServerSucceded, url);
    }

    IEnumerator WaitForMasterServerConnection(string url)
    {
        float t = Time.time;

        while (Time.time < t + maxConnectionTime)
            yield return null;

        LoadingScreen.Instance.Hide();
        mainPanel.SetActive(false);
        sessionPinInput.text = string.Empty;

        ConnectionMenuManager.instance.DisplayError("Cannot connect to destination host : " + url, "Go back to home", () =>
        {
            ConnectionMenuManager.instance.DisplayHome();
        });
    }

    IEnumerator WaitForGettingSessions()
    {
        float t = Time.time;

        while (Time.time < t + maxConnectionTime)
            yield return null;

        LoadingScreen.Instance.Hide();
        mainPanel.SetActive(false);
        sessionPinInput.text = string.Empty;

        ConnectionMenuManager.instance.DisplayError("Cannot find sessions with this pin.", "Go back to home", () =>
        {
            ConnectionMenuManager.instance.DisplayHome();
        });
    }

    private bool onConnectToMasterServerSucceded = false;
    private void OnConnectToMasterServerSucceded()
    {
        if (waitForMasterServerConnection != null)
        {
            onConnectToMasterServerSucceded = true;
        }
    }

    private void Update()
    {
        if (onConnectToMasterServerSucceded)
        {
            onConnectToMasterServerSucceded = false;

            LoadingScreen.Instance.Hide();
            mainPanel.SetActive(true);
            sessionPinPanel.SetActive(true);

            if(waitForMasterServerConnection != null)
            {
                StopCoroutine(waitForMasterServerConnection);
            }
        }
    }

    [ContextMenu("Debug entry")]
    private void DebugEntry()
    {
        LoadingScreen.Instance.Hide();

        for (int i = 0; i < 6; i++)
        {
            DisplaySession(new BeardedManStudios.Forge.Networking.MasterServerResponse.Server { Name = "Session fake " + i.ToString(), PlayerCount = Random.Range(0, 10) });
        }
    }

    /// <summary>
    /// Asks to display information about a session to <see cref="DisplaySessionCoroutine(BeardedManStudios.Forge.Networking.MasterServerResponse.Server)"/>.
    /// </summary>
    /// <param name="response"></param>
    private void DisplaySession(BeardedManStudios.Forge.Networking.MasterServerResponse.Server response)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(DisplaySessionCoroutine(response));
    }

    /// <summary>
    /// Displays an UI to show information about a session.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    private IEnumerator DisplaySessionCoroutine(BeardedManStudios.Forge.Networking.MasterServerResponse.Server response)
    {
        if (waitForGettingSessions != null)
            StopCoroutine(waitForGettingSessions);

        yield return null;

        LoadingScreen.Instance.Hide();

        sessionPinPanel.SetActive(false);
        sessionListPanel.SetActive(true);

        GameObject go = Instantiate(sessionListSlider.baseElement, sessionListSlider.Container.transform);
        ServerSessionEntry entry = go.GetComponent<ServerSessionEntry>();
        Debug.Assert(entry != null);
        entry.Setup(response, this);

        sessionListSlider.AddElement(go);
    }

    /// <summary>
    /// Called when users change their session selection.
    /// </summary>
    /// <param name="entry"></param>
    public void OnSelectionChanged(ServerSessionEntry entry)
    {
        LoadingScreen.Instance.Hide();

        if (currentSelectedEntry == null)
        {
            ConnectionMenuManager.instance.ShowNextNavigationButton(() =>
            {
                ConnectionMenuManager.instance.ConnectToUmi3DEnvironement(currentSelectedEntry.SessionIp, currentSelectedEntry.SessionPort);
                mainPanel.SetActive(false);
            });
            currentSelectedEntry = entry;
        }
        else
        {
            currentSelectedEntry.ToggleSelect();
            if (currentSelectedEntry == entry)
            {
                ConnectionMenuManager.instance.HideNextNavigationButton();
                currentSelectedEntry = null;
            }
            else
            {
                currentSelectedEntry = entry;
            }
        }

    }

    #endregion
}

