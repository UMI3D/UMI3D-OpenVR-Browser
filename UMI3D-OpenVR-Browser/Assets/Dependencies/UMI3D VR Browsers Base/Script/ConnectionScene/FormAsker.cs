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
using System;
using System.Collections.Generic;
using umi3d.cdk.collaboration;
using umi3d.cdk.menu;
using umi3d.cdk.menu.interaction;
using umi3d.cdk.menu.view;
using umi3d.common.interaction;
using umi3dVRBrowsersBase.interactions.input;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// This class manages the display of a FormDto.
    /// </summary>
    public class FormAsker : SingleBehaviour<FormAsker>
    {
        #region Fields

        [SerializeField]
        [Tooltip("Menu used by menuDisplayer")]
        private MenuAsset menu;

        [SerializeField]
        [Tooltip("Menu displayers which handles the dispaly of the form")]
        private MenuDisplayManager menuDisplayer;

        /// <summary>
        /// Array of objects to hide when the environment is loading.
        /// </summary>
        public GameObject[] objectToHideWhenLoading;

        #endregion

        #region Methods

        protected override void Awake()
        {
            base.Awake();
            Hide();
        }

        /// <summary>
        /// Generates the MenuItems according to the parameters contained by form.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="callback"></param>
        public void Display(ConnectionFormDto form, System.Action<FormAnswerDto> callback)
        {
            ConnectionMenuManager.instance.keyboard.Hide();

            if (form == null)
            {
                callback.Invoke(null);
                LoadingPanel.Instance.SetLoadingScreen();
                LoadingPanel.Instance.Display("Loading environment ...");
                HideObjects();
                Hide();
            }
            else
            {
                var answer = new FormAnswerDto()
                {
                    boneType = 0,
                    hoveredObjectId = 0,
                    id = form.id,
                    toolId = 0,
                    answers = new List<ParameterSettingRequestDto>()
                };

                LoadingPanel.Instance.HideLoadingScreen();
                LoadingPanel.Instance.Hide();
                LoadingPanel.Instance.DisplayObjectHidden();
                this.gameObject.SetActive(true);
                menu.menu.RemoveAll();
                menu.menu.Name = form.name;

                foreach (AbstractParameterDto param in form.fields)
                {
                    (MenuItem, ParameterSettingRequestDto) c = umi3d.cdk.interaction.GlobalToolMenuManager.GetInteractionItem(param);
                    menu.menu.Add(c.Item1);
                    answer.answers.Add(c.Item2);
                }

                var send = new EventMenuItem() { Name = "Join", hold = false };
                Action<bool> action = (bool b) =>
                {
                    Hide();
                    ConnectionMenuManager.instance.keyboard.Hide();
                    menuDisplayer.Hide(true);
                    menu.menu.RemoveAll();

                    callback.Invoke(answer);
                    LoadingPanel.Instance.SetLoadingScreen();
                    LoadingPanel.Instance.Display("Loading environment ...");
                    HideObjects();

                    LocalInfoSender.CheckFormToUpdateAuthorizations(form);
                };
                send.Subscribe(action);
                menu.menu.Add(send);
                menuDisplayer.CreateMenuAndDisplay(true, false);

                ConnectionMenuManager.instance.ShowPreviousNavigationButton(() =>
                {
                    Hide();
                    ConnectionMenuManager.onlyOneConnection = false;
                    ConnectionMenuManager.instance.DisplayHome();
                });

                ConnectionMenuManager.instance.ShowNextNavigationButton(() =>
                {
                    action(true);
                });
            }
        }

        /// <summary>
        /// Hides all objects of <see cref="objectToHideWhenLoading"/>.
        /// </summary>
        private void HideObjects()
        {
            foreach (GameObject obj in objectToHideWhenLoading)
                obj.SetActive(false);
        }

        /// <summary>
        /// Hides  panel.
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// Displays the content of a <see cref="FormDto"/> within Unity console.
        /// </summary>
        /// <param name="form"></param>
        private void DebugForm(ConnectionFormDto form)
        {
            if (form != null && form.fields != null)
                foreach (AbstractParameterDto dto in form.fields)
                    switch (dto)
                    {
                        case BooleanParameterDto booleanParameterDto:
                            Debug.Log(booleanParameterDto.value);
                            break;
                        case FloatRangeParameterDto floatRangeParameterDto:
                            Debug.Log(floatRangeParameterDto.value);
                            break;
                        case EnumParameterDto<string> enumParameterDto:
                            Debug.Log(enumParameterDto.value);
                            break;
                        case StringParameterDto stringParameterDto:
                            Debug.Log(stringParameterDto.value);
                            break;
                        default:
                            Debug.Log(dto);
                            break;
                    }
        }

        #endregion
    }
}