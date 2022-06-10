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
using inetum.unityUtils;
using umi3d.cdk.menu;
using umi3dVRBrowsersBase.interactions;
using umi3dVRBrowsersBase.interactions.input;
using umi3dVRBrowsersBase.selection;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    [RequireComponent(typeof(BoxCollider))]
    public partial class PlayerMenuManager
    {
        /// <summary>
        /// Is the player Menu open.
        /// </summary>
        public bool IsOpen { get; private set; } = false;

        public bool IsHovered { get; private set; } = false;

        public ControllerType CurrentController => m_controllerToolMenu.CurrentController;

        /// <summary>
        /// Header of the player menu.
        /// </summary>
        public PlayerMenuHeader MenuHeader => m_playerMenuHeader;

        /// <summary>
        /// Menu for toolboxes.
        /// </summary>
        public PlayerToolboxMenu ToolboxesMenu => m_playerToolboxMenu;

        /// <summary>
        /// Menu for controller tool.
        /// </summary>
        public ControllerToolMenu CtrlToolMenu => m_controllerToolMenu;

        /// <summary>
        /// Transform of the player camera.
        /// </summary>
        public Transform PlayerCameraTransform => m_playerCamera.transform;

        /// <summary>
        /// Transform of the Player Menu Canvas.
        /// </summary>
        public Transform MenuCanvasTransform => m_playerMenuCanvas.transform;

        [SerializeField]
        [Tooltip("Main player VR camera.")]
        private Camera m_playerCamera = null;
        [SerializeField]
        [Tooltip("How far the menu will be from the player camera.")]
        private float m_distanceFromCamera = 0f;
        [SerializeField]
        [Tooltip("Canvas of the player menu")]
        private GameObject m_playerMenuCanvas = null;

        [Header("Header And Sub-Menus.")]
        [SerializeField]
        [Tooltip("")]
        private PlayerMenuHeader m_playerMenuHeader;
        [SerializeField]
        [Tooltip("")]
        private PlayerToolboxMenu m_playerToolboxMenu;
        [SerializeField]
        [Tooltip("")]
        private ControllerToolMenu m_controllerToolMenu;


        private BoxCollider m_menuCollider;

        public UnityEvent onMenuOpen = new UnityEvent();
        public UnityEvent onMenuClose = new UnityEvent();
    }

    public partial class PlayerMenuManager : IClickableElement
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public UnityEvent OnClicked { get; private set; } = new UnityEvent();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="controller"></param>
        public void Click(ControllerType controller)
        { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void HoverEnter()
            => IsHovered = true;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void HoverExit()
            => IsHovered = false;
    }

    public partial class PlayerMenuManager
    {
        #region Player Menu

        /// <summary>
        /// Open the Player Menu and position it in front of the player.
        /// </summary>
        /// <param name="forceOpen"></param>
        public void Open(bool forceOpen = false)
        {
            if (IsOpen && !forceOpen) return;

            var playerCameraPosition = m_playerCamera.transform.position;
            var playerCameraRotation = m_playerCamera.transform.rotation;
            transform.rotation = Quaternion.identity;
            transform.position = new Vector3(playerCameraPosition.x, playerCameraPosition.y, playerCameraPosition.z + m_distanceFromCamera);
            transform.RotateAround(playerCameraPosition, Vector3.up, playerCameraRotation.eulerAngles.y);
            m_playerMenuCanvas.SetActive(true);
            m_menuCollider.enabled = true;
            ParameterGear.Instance.Hide();

            ToolboxesMenu.Open();
            CloseSubWindow();

            onMenuOpen?.Invoke();
            IsOpen = true;
        }

        /// <summary>
        /// Close the player menu.
        /// </summary>
        /// <param name="forceClose"></param>
        public void Close(bool forceClose = false)
        {
            if (!IsOpen && !forceClose) return;
            CloseSubWindow();
            m_playerMenuCanvas.SetActive(false);
            m_menuCollider.enabled = false;

            onMenuClose?.Invoke();
            IsOpen = false;
        }

        #endregion

        public void CloseSubWindow()
        {
            ToolboxesMenu.Close();
            CtrlToolMenu.Hide();
        }

        public void OpenParameterMenu(ControllerType controller)
        {
            Open(true);
            CtrlToolMenu.DisplayParameterMenu(controller);
        }

        public void DisplayActionBinding(ControllerType controller)
        {
            Open(true);
            CtrlToolMenu.DisplayBindingMenu(controller);
        }

        public void DisplayParameterInToolbox(AbstractMenu menu)
        {
            Open(true);
            ToolboxesMenu.Open();
            ToolboxesMenu.NavigateTo(menu);
        }
    }

    public partial class PlayerMenuManager : SingleBehaviour<PlayerMenuManager>
    {
        protected override void Awake()
        {
            base.Awake();
            Debug.Assert(m_playerCamera != null, "Player Camera is null in Player Menu Manager");
            m_menuCollider = GetComponent<BoxCollider>();
            Close(true);
        }

        private void Start()
        {
            MenuHeader.Initialize();
        }
    }
}

