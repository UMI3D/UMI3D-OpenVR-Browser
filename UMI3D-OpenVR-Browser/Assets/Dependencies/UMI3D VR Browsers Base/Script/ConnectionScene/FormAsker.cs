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
        /// Creates the right MenuItem according to the dto type.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private static (MenuItem, ParameterSettingRequestDto) GetInteractionItem(AbstractInteractionDto dto)
        {
            MenuItem result = null;
            ParameterSettingRequestDto requestDto = null;
            switch (dto)
            {
                case BooleanParameterDto booleanParameterDto:
                    var b = new BooleanInputMenuItem() { dto = booleanParameterDto };
                    b.NotifyValueChange(booleanParameterDto.value);
                    requestDto = new ParameterSettingRequestDto()
                    {
                        toolId = dto.id,
                        id = booleanParameterDto.id,
                        parameter = booleanParameterDto.value,
                        hoveredObjectId = 0
                    };
                    b.Subscribe((x) =>
                    {
                        booleanParameterDto.value = x;
                        requestDto.parameter = x;
                    });
                    result = b;
                    break;
                case FloatRangeParameterDto floatRangeParameterDto:
                    var f = new FloatRangeInputMenuItem() { dto = floatRangeParameterDto, max = floatRangeParameterDto.max, min = floatRangeParameterDto.min, value = floatRangeParameterDto.value, increment = floatRangeParameterDto.increment };
                    requestDto = new ParameterSettingRequestDto()
                    {
                        toolId = dto.id,
                        id = floatRangeParameterDto.id,
                        parameter = floatRangeParameterDto.value,
                        hoveredObjectId = 0
                    };
                    f.Subscribe((x) =>
                    {
                        floatRangeParameterDto.value = x;
                        requestDto.parameter = x;
                    });
                    result = f;
                    break;
                case EnumParameterDto<string> enumParameterDto:
                    var en = new DropDownInputMenuItem() { dto = enumParameterDto, options = enumParameterDto.possibleValues };
                    en.NotifyValueChange(enumParameterDto.value);
                    requestDto = new ParameterSettingRequestDto()
                    {
                        toolId = dto.id,
                        id = enumParameterDto.id,
                        parameter = enumParameterDto.value,
                        hoveredObjectId = 0
                    };
                    en.Subscribe((x) =>
                    {
                        enumParameterDto.value = x;
                        requestDto.parameter = x;
                    });
                    result = en;
                    break;
                case StringParameterDto stringParameterDto:
                    var s = new TextInputMenuItem() { dto = stringParameterDto };
                    s.NotifyValueChange(stringParameterDto.value);
                    requestDto = new ParameterSettingRequestDto()
                    {
                        toolId = dto.id,
                        id = stringParameterDto.id,
                        parameter = stringParameterDto.value,
                        hoveredObjectId = 0
                    };
                    s.Subscribe((x) =>
                    {
                        stringParameterDto.value = x;
                        requestDto.parameter = x;
                    });
                    result = s;
                    break;
                case LocalInfoRequestParameterDto localInfoRequestParameterDto:
                    var localReq = new LocalInfoRequestInputMenuItem() { dto = localInfoRequestParameterDto };
                    localReq.NotifyValueChange(localInfoRequestParameterDto.value);
                    requestDto = new ParameterSettingRequestDto()
                    {
                        toolId = dto.id,
                        id = localInfoRequestParameterDto.id,
                        parameter = localInfoRequestParameterDto.value,
                        hoveredObjectId = 0
                    };
                    localReq.Subscribe((x) =>
                    {
                        localInfoRequestParameterDto.value = x;
                        requestDto.parameter = x;
                    }
                    );
                    result = localReq;
                    break;
                default:
                    result = new MenuItem();
                    result.Subscribe(() => Debug.Log($"Missing case for {dto?.GetType()}"));
                    break;
            }
            result.Name = dto.name;

            return (result, requestDto);
        }

        /// <summary>
        /// Generates the MenuItems according to the parameters contained by form.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="callback"></param>
        public void Display(FormDto form, System.Action<FormAnswerDto> callback)
        {
            ConnectionMenuManager.instance.keyboard.Hide();

            if (form == null)
            {
                callback.Invoke(null);
                LoadingScreen.Instance.SetLoadingScreen();
                LoadingScreen.Instance.Display("Loading environment ...");
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

                LoadingScreen.Instance.Hide();
                this.gameObject.SetActive(true);
                menu.menu.RemoveAll();

                foreach (AbstractParameterDto param in form.fields)
                {
                    (MenuItem, ParameterSettingRequestDto) c = GetInteractionItem(param);
                    menu.menu.Add(c.Item1);
                    answer.answers.Add(c.Item2);
                }

                var send = new EventMenuItem() { Name = "Join", toggle = false };
                UnityAction<bool> action = (bool b) =>
                {
                    menuDisplayer.Hide(true);
                    menu.menu.RemoveAll();
                    ConnectionMenuManager.instance.keyboard.Hide();

                    callback.Invoke(answer);
                    LoadingScreen.Instance.SetLoadingScreen();
                    LoadingScreen.Instance.Display("Loading environment ...");
                    HideObjects();
                    Hide();
                    LocalInfoSender.CheckFormToUpdateAuthorizations(form);
                };
                send.Subscribe(action);
                menu.menu.Add(send);
                menuDisplayer.Display(true);

                ConnectionMenuManager.instance.ShowPreviousNavigationButton(() =>
                {
                    Hide();
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
        private void DebugForm(FormDto form)
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