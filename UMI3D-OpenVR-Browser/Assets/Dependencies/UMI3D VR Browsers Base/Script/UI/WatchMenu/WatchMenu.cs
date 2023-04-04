﻿/*
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

using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.menu;
using umi3dVRBrowsersBase.connection;
using umi3dVRBrowsersBase.interactions;
using umi3dVRBrowsersBase.settings;
using umi3dVRBrowsersBase.ui.playerMenu;
using UnityEngine;

namespace umi3dVRBrowsersBase.ui.watchMenu
{
    /// <summary>
    /// This class manages the display and items of all UMI3DWatches.
    /// </summary>
    public class WatchMenu : AbstractMenuManager
    {
        #region Static Fields and Methods

        /// <summary>
        /// Stores all instances.
        /// </summary>
        public static HashSet<WatchMenu> instances = new HashSet<WatchMenu>();

        /// <summary>
        /// Pins a <see cref="Menu"/> to all watch instances.
        /// </summary>
        /// <param name="menu"></param>
        public static void PinMenu(Menu menu)
        {
            foreach (WatchMenu watch in instances)
                watch.Pin(menu);
        }

        /// <summary>
        /// Unpins <see cref="Menu"/> from all watch instances.
        /// </summary>
        /// <param name="menu"></param>
        public static void UnPinMenu(Menu menu)
        {
            foreach (WatchMenu watch in instances)
                watch.UnPin(menu);
        }

        public static void UnPinAllMenus()
        {
            foreach (WatchMenu watch in instances)
                watch.UnPinAll();
        }

        /// <summary>
        /// If exists returns the first <see cref="WatchMenu"/> associated to <paramref name="controller"/>.
        /// Warning : this method convert an Hashset to an array so do not used it each frame.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static WatchMenu FindInstanceAssociatedToController(ControllerType controller)
        {
            return Array.Find(instances.ToArray(), w => w.associatedController == controller);
        }

        #endregion

        #region Fields

        [SerializeField]
        [Tooltip("Controller associated to this menu")]
        private ControllerType associatedController;

        [Header("Wristband buttons")]

        [SerializeField]
        [Tooltip("Button to display all menus pinned by users")]
        private OnOffButton pinMenuButton;

        [SerializeField]
        [Tooltip("Button to display the player menu")]
        private OnOffButton playerMenuButton;

        [SerializeField]
        [Tooltip("Root of the settings menu")]
        private GameObject settingsMenuRoot;

        [Header("Settings menu")]

        [SerializeField]
        [Tooltip("Button to enable/disable microphone")]
        private OnOffButton setMicButton;

        [SerializeField]
        [Tooltip("Button to enable/disable sound")]
        private OnOffButton setSoundButton;

        [SerializeField]
        [Tooltip("Button to send/stop user's avatar movments")]
        private OnOffButton setAvatarButton;

        [Header("Notification")]
        [Tooltip("Where all local notifications will be instanciated")]
        public Transform notificationContainer;

        /// <summary>
        /// Transform of the player camera.
        /// </summary>
        private Transform playerCamera;

        /// <summary>
        /// Angle to considerer this wath in the field of view of the player
        /// </summary>
        private float detectionConeAngle = 20f;

        /// <summary>
        /// Height of the detection cone to consider this watch inside the player's field of view.
        /// </summary>
        private float detectionConeDistance = 1f;

        #endregion

        #region Methods

        #region Monobehavior's callback

        private void Awake()
        {
            instances.Add(this);
        }

        private void Start()
        {
            settingsMenuRoot.SetActive(false);

            var rootContainer = new Menu();

            menuDisplayManager.menuAsset.menu = rootContainer;

            BindSettingButtons();

            //PlayerMenuManager.Instance.onMenuOpen.AddListener(() => playerMenuButton.ForceSelectionHighlight());
            //PlayerMenuManager.Instance.onMenuClose.AddListener(() => playerMenuButton.ForceDeselectionHighlight());

            playerCamera = PlayerMenuManager.Instance.PlayerCameraTransform;
        }

        private void OnDestroy()
        {
            instances.Remove(this);
        }


        #endregion

        /// <summary>
        /// Binds all setting buttons to their actions.
        /// </summary>
        private void BindSettingButtons()
        {
            EnvironmentSettings.Instance.micSetting.OnValueChanged.AddListener(setMicButton.Toggle);
            EnvironmentSettings.Instance.audioSetting.OnValueChanged.AddListener(setSoundButton.Toggle);
            EnvironmentSettings.Instance.avatarSetting.OnValueChanged.AddListener(setAvatarButton.Toggle);
            PlayerMenuManager.Instance.onMenuClose.AddListener(() => { if (playerMenuButton.IsOn) playerMenuButton.Toggle(false);  });
            PlayerMenuManager.Instance.onMenuOpen.AddListener(() => { if (!playerMenuButton.IsOn) playerMenuButton.Toggle(true); });

            EmoteMenu.EmoteButtonStatusChanged += value =>
            {
                pinMenuButton.Toggle(value);
                IsOpen = value;
            };

            SetMicStatus(false);
        }

        #region Abstract Menu Manager

        /// <summary>
        /// Opens the menu with all pinned items.
        /// </summary>
        [ContextMenu("Open watch menu")]
        public override void Open()
        {
            base.Open();
            //menuDisplayManager.Display(true);
            EmoteMenu.Instance.Display();
            UnityEngine.Debug.Log("<color=green>TODO: </color>" + $"open");
        }

        /// <summary>
        /// Closes the menu with all pinned items.
        /// </summary>
        public override void Close()
        {
            base.Close();
            //menuDisplayManager.Hide();
            EmoteMenu.Instance.Hide();
            UnityEngine.Debug.Log("<color=green>TODO: </color>" + $"close");
        }

        /// <summary>
        /// Toggles the display of the menu with all pinned items.
        /// </summary>
        public void ToggleDisplayPinMenu()
        {
            if (IsOpen)
                Close();
            else
                Open();
        }

        /// <summary>
        /// Toggles the display of <see cref="umi3dVRBrowsersBase.ui.playerMenu.PlayerMenuManager"/>
        /// </summary>
        public void ToggleDisplayPlayerMenu()
        {
            if (PlayerMenuManager.Instance.IsMenuOpen)
            {
                PlayerMenuManager.Instance.Close();
            }
            else
            {
                PlayerMenuManager.Instance.Open();
            }
        }

        #endregion

        #region Setting Menu

        private bool isSettingsMenuOpened = false;

        /// <summary>
        /// Opens the menu to change environment settings.
        /// </summary>
        public void OpenSettingsMenu()
        {
            if (!isSettingsMenuOpened)
            {
                isSettingsMenuOpened = true;
                settingsMenuRoot.SetActive(true);
            }
        }

        /// <summary>
        /// Closes the menu to change environment settings.
        /// </summary>
        public void CloseSettingsMenu()
        {
            if (isSettingsMenuOpened)
            {
                isSettingsMenuOpened = false;
                settingsMenuRoot.SetActive(false);
            }
        }

        /// <summary>
        /// Toggles the display of the menu to change environment settings.
        /// </summary>
        public void ToggleSettingsMenuDisplay()
        {
            if (isSettingsMenuOpened)
                CloseSettingsMenu();
            else
                OpenSettingsMenu();
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
        /// Asks to change the microphone status.
        /// </summary>
        /// <param name="val"></param>
        public void SetMicStatus(bool val)
        {
            EnvironmentSettings.Instance.micSetting.SetValue(val);
        }

        /// <summary>
        /// Asks to change the send user's avatar movments status.
        /// </summary>
        /// <param name="val"></param>
        public void SetAvatarStatus(bool val)
        {
            EnvironmentSettings.Instance.avatarSetting.SetValue(val);
        }

        /// <summary>
        /// Asks the sound activation/deactivation.
        /// </summary>
        /// <param name="val"></param>
        public void SetSoundStatus(bool val)
        {
            EnvironmentSettings.Instance.audioSetting.SetValue(val);
        }

        #endregion


        /// <summary>
        /// Pins a <see cref="Menu"/> to the watch menu.
        /// </summary>
        /// <param name="menu"></param>
        public void Pin(Menu menu)
        {
            menu.OnDestroy.RemoveAllListeners();
            menuDisplayManager.menu.Add(menu);

            if (menuDisplayManager.isDisplaying)
                menuDisplayManager.Display(true);
        }

        /// <summary>
        /// Unpins a <see cref="Menu"/> to the watch menu.
        /// </summary>
        /// <param name="menu"></param>
        public void UnPin(Menu menu)
        {
            menuDisplayManager.menu.Remove(menu);
        }

        // <summary>
        /// Unpins all menus.
        /// </summary>
        /// <param name="menu"></param>
        public void UnPinAll()
        {
            menuDisplayManager.menu.RemoveAll();

            if (menuDisplayManager.isDisplaying)
                menuDisplayManager.Display(true);
        }

        /// <summary>
        /// Is this object considered in the player's field of view ? Based on a detection cone defined by <see cref="detectionConeAngle"/> and <see cref="detectionConeDistance"/>.
        /// </summary>
        public bool IsObjectInPlayerFieldOfView()
        {
            Vector3 coneDirection = playerCamera.forward;
            Vector3 cameraToWatch = this.transform.position - playerCamera.position;

            if (cameraToWatch.magnitude > detectionConeDistance)
                return false;

            if (Vector3.Angle(coneDirection, cameraToWatch) < detectionConeAngle)
                return true;
            else
                return false;
        }

        #endregion
    }
}
