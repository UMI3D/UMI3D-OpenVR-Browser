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

using System;
using TMPro;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.common;
using umi3dVRBrowsersBase.connection;
using umi3dVRBrowsersBase.interactions;
using umi3dVRBrowsersBase.settings;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    public partial class PlayerMenuHeader
    {
        [Header("Session Information")]
        [SerializeField]
        private TextMeshProUGUI m_sesssionName = null;
        [SerializeField]
        private TextMeshProUGUI m_sessionTime = null;
        [SerializeField]
        private TextMeshProUGUI m_participantCount = null;

        [Header("Settings")]
        [SerializeField]
        private OnOffButton m_avatarButton = null;
        [SerializeField]
        private OnOffButton m_soundButton = null;
        [SerializeField]
        private OnOffButton m_micButton = null;

        [SerializeField]
        [Header("Buttons")]
        private Button m_toolbox = null;
        [SerializeField]
        private Button m_ctrlLeftBindings = null;
        [SerializeField]
        private TextMeshProUGUI m_ctrlLeftBindingsLabel = null;
        [SerializeField]
        private Button m_ctrlRightBindings = null;
        [SerializeField]
        private TextMeshProUGUI m_ctrlRightBindingsLabel = null;

        [Space]
        [SerializeField]
        Button m_closeButton;

        private DateTime m_startOfSession { get; set; } = default;
    }

    public partial class PlayerMenuHeader : MonoBehaviour
    {
        private void Start()
        {
            BindSettings();
            BindControllerButtons();
        }

        /// <summary>
        /// Set the initial status of UI and bind the environment loader event with their methods.
        /// </summary>
        public void Initialize()
        {
            SetEnvironmentName(null);
            m_sessionTime.gameObject.SetActive(false);
            m_participantCount.gameObject.SetActive(false);
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(() =>
            {
                m_sessionTime.gameObject.SetActive(true);
                m_participantCount.gameObject.SetActive(true);
                m_startOfSession = DateTime.Now;
            });
            UMI3DCollaborationEnvironmentLoader.Instance.OnUpdateUserList += UpdateParticipantsCount;
        }

        /// <summary>
        /// Update the participants count.
        /// </summary>
        private void UpdateParticipantsCount()
        {
            int usersCount = UMI3DCollaborationEnvironmentLoader.Instance.UserList.Count;

            m_participantCount.text = usersCount < 2 ? $"{usersCount}\nparticipant" : $"{usersCount}\nparticipants";
        }

        /// <summary>
        /// Bind the UI settings (mic, sound, avatar).
        /// </summary>
        private void BindSettings()
        {
            m_micButton.OnClicked.AddListener(() =>
            {
                EnvironmentSettings.Instance.micSetting.Toggle();
            });
            EnvironmentSettings.Instance.micSetting.OnValueChanged.AddListener(v =>
            {
                m_micButton.Toggle(v);
            });

            m_soundButton.OnClicked.AddListener(() =>
            {
                EnvironmentSettings.Instance.audioSetting.Toggle();
            });
            EnvironmentSettings.Instance.audioSetting.OnValueChanged.AddListener(v =>
            {
                m_soundButton.Toggle(v);
            });

            m_avatarButton.OnClicked.AddListener(() =>
            {
                EnvironmentSettings.Instance.avatarSetting.Toggle(); 
            });
            EnvironmentSettings.Instance.avatarSetting.OnValueChanged.AddListener(v =>
            {
                m_avatarButton.Toggle(v);
            });
        }

        /// <summary>
        /// Bind the UI for controllers bindings buttons.
        /// </summary>
        private void BindControllerButtons()
        {
            m_ctrlLeftBindings.onClick.AddListener(() =>
            {
                PlayerMenuManager.Instance.DisplayActionBinding(ControllerType.LeftHandController);
            });

            m_ctrlRightBindings.onClick.AddListener(() =>
            {
                PlayerMenuManager.Instance.DisplayActionBinding(ControllerType.RightHandController);
            });

            m_ctrlLeftBindings.gameObject.SetActive(false);
            m_ctrlRightBindings.gameObject.SetActive(false);

            m_closeButton.onClick.AddListener(() => PlayerMenuManager.Instance.Close(true));
        }

        private void Update()
        {
            if (!UMI3DEnvironmentLoader.Instance.isEnvironmentLoaded)
                return;
            var time = DateTime.Now - m_startOfSession;
            m_sessionTime.text = time.ToString("hh") + ":" + time.ToString("mm") + ":" + time.ToString("ss");
        }

        /// <summary>
        /// Update the UI with the name of the environment. If [media] is null then displays [Home].
        /// </summary>
        /// <param name="media"></param>
        public void SetEnvironmentName(MediaDto media)
        {
            m_sesssionName.text = (media  != null) ? $"Session name: {media.name}" : "Home";
        }

        /// <summary>
        /// Leaves the application or the environement.
        /// </summary>
        public void Leave()
        {
            System.Action<bool> leaveCallback = (b) =>
            {
                if (b)
                {
                    if (EnvironmentSettings.Instance.IsEnvironmentLoaded)
                        Connecting.Instance.Leave();
                    else
                        Application.Quit();
                }
            };

            string title = EnvironmentSettings.Instance.IsEnvironmentLoaded ? "Go back to main menu" : "Close application";
            DialogBox.Instance.Display(title, "Are you sure you want to leave ?", "Yes", leaveCallback);
        }

        /// <summary>
        /// Active the [controller] binding button.
        /// </summary>
        /// <param name="display"></param>
        /// <param name="controller"></param>
        /// <param name="toolName"></param>
        public void DisplayControllerButton(bool display, ControllerType controller, string toolName)
        {
            switch (controller)
            {
                case ControllerType.LeftHandController:
                    m_ctrlLeftBindings.gameObject.SetActive(display);
                    m_ctrlLeftBindingsLabel.text = toolName;
                    break;
                case ControllerType.RightHandController:
                    m_ctrlRightBindings.gameObject.SetActive(display);
                    m_ctrlRightBindingsLabel.text = toolName;
                    break;
                default:
                    break;
            }
        }
    }
}