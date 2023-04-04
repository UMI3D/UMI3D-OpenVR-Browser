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

using System;
using System.Collections;
using umi3dVRBrowsersBase.interactions;
using umi3dVRBrowsersBase.ui.displayers;
using umi3dVRBrowsersBase.ui.playerMenu;
using UnityEngine;
using UnityEngine.EventSystems;

namespace umi3dVRBrowsersBase.ui.keyboard
{
    /// <summary>
    /// Custom <see cref="UnityEngine.UI.InputField"/> which works with <see cref="Keyboard"/> (not displayed at the same time).
    /// If the keyboard and the inputfield must be visible at the same time <see cref="CustomInputWithKeyboard"/>.
    /// </summary>
    public class CustomInputWithKeyboardEnvironment : CustomInputSelectable
    {
        #region Fields

        /// <summary>
        /// Definies what to do once users finish to edit.
        /// </summary>
        private Action<string> editionCallback;

        /// <summary>
        /// Keyboard used to edit this input.
        /// </summary>
        protected Keyboard keyboard;

        /// <summary>
        /// Reference to the controller which has this parameter projected on it.
        /// </summary>
        protected ControllerType currentController;

        #endregion

        #region Methods

        protected override void Start()
        {
            keyboard = Keyboard.Instance;
            Debug.Assert(keyboard != null);
        }

        /// <summary>
        /// Setter for <see cref="editionCallback"/>.
        /// </summary>
        /// <param name="editionCallback"></param>
        public void SetEditionCallback(Action<string> editionCallback)
        {
            this.editionCallback = (res) => 
            { 
                editionCallback?.Invoke(res);
                PlayerMenuManager.Instance.gameObject.SetActive(true);
                StartCoroutine(UnSelectAllSelectable());
                PlayerMenuManager.Instance.RefreshBackground();
                if (PlayerMenuManager.Instance.CtrlToolMenu.toolParametersMenu.HasOnlyOneTextInput(out StringInputMenuItemDisplayerEnvironment input))
                {
                    PlayerMenuManager.Instance.CtrlToolMenu.toolParametersMenu.Close();
                    PlayerMenuManager.Instance.Close();
                }
            };
        }

        /// <summary>
        /// Unselects all <see cref="Selectable"/>.
        /// </summary>
        /// <returns></returns>
        IEnumerator UnSelectAllSelectable()
        {
            yield return null;
            EventSystem.current.SetSelectedGameObject(null, null);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            if (keyboard != null && !keyboard.WasClosedLastFrame)
            {
                Transform playerMenuTransform = PlayerMenuManager.Instance.MenuCanvasTransform;

                keyboard.OpenKeyboard(this.text, editionCallback, () => {
                    PlayerMenuManager.Instance.gameObject.SetActive(true);
                    StartCoroutine(UnSelectAllSelectable());
                    PlayerMenuManager.Instance.RefreshBackground();
                    if (PlayerMenuManager.Instance.CtrlToolMenu.toolParametersMenu.HasOnlyOneTextInput(out StringInputMenuItemDisplayerEnvironment input))
                    {
                        PlayerMenuManager.Instance.CtrlToolMenu.toolParametersMenu.Close();
                        PlayerMenuManager.Instance.Close();
                    }
                }, playerMenuTransform.position, playerMenuTransform.forward);

                currentController = PlayerMenuManager.Instance.CurrentController;
                PlayerMenuManager.Instance.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("Set keyboard of this input before enabling users to use it.");
            }
        }

        #endregion
    }
}