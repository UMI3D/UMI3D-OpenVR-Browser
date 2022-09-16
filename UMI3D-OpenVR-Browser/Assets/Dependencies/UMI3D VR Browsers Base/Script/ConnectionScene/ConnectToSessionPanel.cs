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

using MainThreadDispatcher;
using System.Collections;
using umi3d.cdk.collaboration;
using umi3dVRBrowsersBase.ui.keyboard;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// Manages the ConnectToSessionPanel i.e. getting the pin server entered by users, displaying all sessions related to this pin 
    /// and starting the connection to the selected session.
    /// </summary>
    public class ConnectToSessionPanel : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        [Tooltip("Main gameObject of the panel")]
        private GameObject mainPanel;

        [SerializeField]
        [Tooltip("Panel to ask users their session pin")]
        private GameObject sessionPinPanel;

        [SerializeField]
        [Tooltip("Input to enter session pin")]
        private CustomInputWithKeyboard sessionPinInput;

        [SerializeField]
        [Tooltip("Button to confirm session pin")]
        private Button confirmPinButton;

        [SerializeField]
        [Tooltip("GameObject which displays the list of all sessions found")]
        private GameObject sessionListPanel;

        [SerializeField]
        [Tooltip("Dislays all sessions in a carousel")]
        private SliderDisplayer sessionListSlider;

        /// <summary>
        /// Object which handles the connection to a master server.
        /// </summary>
        private LaucherOnMasterServer launcher = new LaucherOnMasterServer();

        /// <summary>
        /// Stores current selected session.
        /// </summary>
        private ServerSessionEntry currentSelectedEntry;

        [SerializeField]
        [Tooltip("In seconds, after this time, if no connection was established, display an error message.")]
        private float maxConnectionTime = 5;

        #endregion

        #region Methods

        /// <summary>
        /// Coroutine which waits for sessions to be found.
        /// </summary>
        private Coroutine waitForGettingSessions;

        private void Awake()
        {
            sessionPinInput.SetKeyboard(ConnectionMenuManager.instance.keyboard);
            confirmPinButton.onClick.AddListener(() =>
            {
                string pin = sessionPinInput.text.Trim();
                if (string.IsNullOrEmpty(pin))
                    return;

                sessionListSlider.Clear();
                launcher.SendDataSession(pin, DisplaySession);

                LoadingPanel.Instance.Display("Connecting ...");

                waitForGettingSessions = StartCoroutine(WaitForGettingSessions());
            });
        }

        /// <summary>
        /// Coroutine which waits for master server connection.
        /// </summary>
        private Coroutine waitForMasterServerConnection;

        /// <summary>
        /// Start the connection to a master server.
        /// </summary>
        /// <param name="url"></param>
        public void ConnectToMasterServer(string url)
        {
            mainPanel.SetActive(true);
            sessionPinPanel.SetActive(false);
            sessionListPanel.SetActive(false);
            currentSelectedEntry = null;

            LoadingPanel.Instance.Display("Connecting ...");

            ConnectionMenuManager.instance.HideNextNavigationButton();
            ConnectionMenuManager.instance.ShowPreviousNavigationButton(() =>
            {
                if (waitForMasterServerConnection != null)
                {
                    StopCoroutine(waitForMasterServerConnection);
                }

                if (waitForGettingSessions != null)
                    StopCoroutine(waitForGettingSessions);

                LoadingPanel.Instance.Hide();
                mainPanel.SetActive(false);
                sessionPinInput.text = string.Empty;
                ConnectionMenuManager.instance.DisplayHome();
            });
            waitForMasterServerConnection = StartCoroutine(WaitForMasterServerConnection(url));
            launcher.ConnectToMasterServer(OnConnectToMasterServerSucceded, url, () =>
            {
                ConnectionMenuManager.instance.DisplayError("Cannot connect to master server : " + url, "Go back to home", () =>
                {
                    ConnectionMenuManager.instance.DisplayHome();
                });
            });
        }

        /// <summary>
        /// Waits for the connection with a master server to be done.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private IEnumerator WaitForMasterServerConnection(string url)
        {
            float t = Time.time;

            while (Time.time < t + maxConnectionTime)
                yield return null;

            LoadingPanel.Instance.Hide();
            mainPanel.SetActive(false);
            sessionPinInput.text = string.Empty;

            ConnectionMenuManager.instance.DisplayError("Cannot connect to destination host : " + url, "Go back to home", () =>
            {
                ConnectionMenuManager.instance.DisplayHome();
            });
        }

        /// <summary>
        /// Wait for sessions to be found.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForGettingSessions()
        {
            float t = Time.time;

            while (Time.time < t + maxConnectionTime)
                yield return null;

            LoadingPanel.Instance.Hide();
            mainPanel.SetActive(false);
            sessionPinInput.text = string.Empty;

            ConnectionMenuManager.instance.DisplayError("Cannot find sessions with this pin.", "Go back to home", () =>
            {
                ConnectionMenuManager.instance.DisplayHome();
            });
        }

        private bool onConnectToMasterServerSucceded = false;

        /// <summary>
        /// Resets status when connection to a master server is done.
        /// </summary>
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

                LoadingPanel.Instance.Hide();
                mainPanel.SetActive(true);
                sessionPinPanel.SetActive(true);

                if (waitForMasterServerConnection != null)
                {
                    StopCoroutine(waitForMasterServerConnection);
                }
            }
        }

        [ContextMenu("Debug entry")]
        private void DebugEntry()
        {
            LoadingPanel.Instance.Hide();

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

            LoadingPanel.Instance.Hide();

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
            LoadingPanel.Instance.Hide();

            if (currentSelectedEntry == null)
            {
                ConnectionMenuManager.instance.ShowNextNavigationButton(() =>
                {
                    ConnectionMenuManager.instance.ConnectToUmi3DEnvironement(currentSelectedEntry.SessionIp, currentSelectedEntry.SessionPort);
                    ConnectionMenuManager.instance.HideNextNavigationButton();
                    mainPanel.SetActive(false);
                });
                currentSelectedEntry = entry;
                entry.Select();
            }
            else
            {
                currentSelectedEntry.UnSelect();
                if (currentSelectedEntry == entry)
                {
                    ConnectionMenuManager.instance.HideNextNavigationButton();
                    currentSelectedEntry = null;
                }
                else
                {
                    currentSelectedEntry = entry;
                    entry.Select();
                }
            }

        }

        #endregion
    }
}