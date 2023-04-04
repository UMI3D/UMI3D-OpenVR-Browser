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

using umi3d.cdk.menu;
using umi3d.common;
using umi3dVRBrowsersBase.interactions;
using umi3dVRBrowsersBase.interactions.input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    /// <summary>
    /// This class is responsible for <see cref="ToolParametersMenu"/> and <see cref="ActionBindingMenu"/>.
    /// </summary>
    public class ControllerToolMenu : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        [Tooltip("Main gameobject of the panel")]
        private GameObject root;

        [Header("Header")]
        [SerializeField]
        [Tooltip("Button to switch to editBindingMenu")]
        private Button editBindingButton;

        [SerializeField]
        [Tooltip("Text associated to editBindingButton")]
        private Image editBindingButtonBck;

        [SerializeField]
        [Tooltip("Background for editBindingButton when it is not selected")]
        private Sprite editBindingButtonDefaultBck;

        [SerializeField]
        [Tooltip("Background for editBindingButton when it is selected")]
        private Sprite editBindingButtonActiveBck;

        [SerializeField]
        [Tooltip("Button to switch to edit parameter menu")]
        private Button editParametersButton;

        [SerializeField]
        [Tooltip("Image associated to editParametersButton")]
        private Image editParametersButtonBck;

        [SerializeField]
        [Tooltip("Background used for editParametersButton when it is not selected")]
        private Sprite editParametersButtonDefaultBck;

        [SerializeField]
        [Tooltip("Background used for editParametersButton when it is selected")]
        private Sprite editParametersButtonActiveBck;

        [Header("Menu")]

        [Tooltip("Class which handle parameter edition")]
        [SerializeField]
        public ToolParametersMenu toolParametersMenu;

        [Tooltip("Class which handle action bindings")]
        [SerializeField]
        private ActionBindingMenu actionBindingMenu;

        /// <summary>
        /// Is the menu currently displayed ?
        /// </summary>
        public bool IsOpen { get; private set; }

        public bool IsAsync => toolParametersMenu.IsAsync;

        /// <summary>
        /// Controller whose bindings or parameters are currently displayed.
        /// </summary>
        public ControllerType CurrentController { get; private set; }

        /// <summary>
        /// Event raised when a user click on the menu (not used).
        /// </summary>
        public UnityEvent OnClicked { get; private set; } = new UnityEvent();

        #endregion

        #region Methods

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            editBindingButton.onClick.AddListener(() =>
            {
                DisplayBindingMenu(CurrentController);
            });

            editParametersButton.onClick.AddListener(() =>
            {
                DisplayParameterMenu(CurrentController);
            });
        }

        /// <summary>
        /// Hides menu.
        /// </summary>
        [ContextMenu("Hide")]
        public void Hide()
        {
            IsOpen = false;
            root.SetActive(false);
        }

        public void Close()
        {
            toolParametersMenu.Close();
            Hide();
        }

        /// <summary>
        /// Sets the header label with current tool name.
        /// </summary>
        /// <param name="toolName"></param>
        public void SetToolName(string toolName)
        {
            toolParametersMenu.SetToolName(toolName);
        }

        /// <summary>
        /// Hides/shows the menu to refresh UI dislay to fix some Unity bugs.
        /// </summary>
        public async void RefreshBackground()
        {
            root.SetActive(!IsOpen);
            await UMI3DAsyncManager.Yield();
            root.SetActive(IsOpen);
        }

        #region Binding Menu

        /// <summary>
        /// Displays the binding menu with all events bound to <paramref name="controller"/>.
        /// </summary>
        /// <param name="controller"></param>
        public void DisplayBindingMenu(ControllerType controller)
        {
            IsOpen = true;
            root.SetActive(true);

            actionBindingMenu.gameObject.SetActive(true);
            editBindingButtonBck.sprite = editBindingButtonActiveBck;

            toolParametersMenu.gameObject.SetActive(false);
            editParametersButtonBck.sprite = editParametersButtonDefaultBck;

            CurrentController = controller;
            actionBindingMenu.DisplayCurrentBinding(controller);
        }


        /// <summary>
        /// Adds a new <see cref="ActionBindingEntry"/> to represent <paramref name="input"/>.
        /// </summary>
        /// <param name="input"></param>
        public void AddBinding(AbstractVRInput input)
        {
            actionBindingMenu.AddToList(input);
        }

        /// <summary>
        /// Clears all bindings added previously.
        /// </summary>
        /// <param name="type"></param>
        public void ClearBindingList(ControllerType type)
        {
            actionBindingMenu.ClearList(type);
        }

        #endregion

        #region Parameter Menu


        /// <summary>
        /// Adds a parameter menu item for a given <paramref name="controller"/>.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="item"></param>
        public void AddParameter(ControllerType controller, AbstractMenuItem item, System.Action callbackOnDesynchronize)
        {
            toolParametersMenu.AddParameter(controller, item, callbackOnDesynchronize);
        }

        /// <summary>
        /// Removes a parameter menu item for a given <paramref name="controller"/>.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="item"></param>
        public void RemoveParameter(ControllerType controller, AbstractMenuItem item)
        {
            toolParametersMenu.RemoveParameter(controller, item);
        }

        public void RememberParameters()
        {
            toolParametersMenu.Remember();
        }

        /// <summary>
        /// Displays all the parameters associated to <paramref name="controller"/>.
        /// </summary>
        /// <param name="controller"></param>
        public void DisplayParameterMenu(ControllerType controller, bool menuAsync = false)
        {
            IsOpen = true;

            root.SetActive(true);

            actionBindingMenu.gameObject.SetActive(false);
            editBindingButtonBck.sprite = editBindingButtonDefaultBck;

            toolParametersMenu.gameObject.SetActive(true);
            editParametersButtonBck.sprite = editParametersButtonActiveBck;

            CurrentController = controller;
            toolParametersMenu.Display(controller, isAsync: menuAsync);
        }

        #endregion

        #endregion
    }
}