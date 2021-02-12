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
using UnityEngine;
using umi3d.common;
using umi3d.cdk.menu;
using umi3d.cdk.menu.view;
using umi3d.common.interaction;
using UnityEngine.Events;

/// <summary>
/// This class manages the display of a FormDto.
/// </summary>
public class FormAsker : Singleton<FormAsker>
{
    [SerializeField] private MenuAsset menu;
    [SerializeField] private MenuDisplayManager menuDisplayer;

    /// <summary>
    /// Array of objects to hide when the environment is loading.
    /// </summary>
    public GameObject[] objectToHideWhenLoading;


    /// <summary>
    /// Creates the right MenuItem according to the dto type.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    static MenuItem GetInteractionItem(AbstractInteractionDto dto)
    {
        MenuItem result = null;
        switch (dto)
        {
            case BooleanParameterDto booleanParameterDto:
                var b = new BooleanInputMenuItem() { dto = booleanParameterDto };
                b.NotifyValueChange(booleanParameterDto.value);
                b.Subscribe((x) =>
                {
                    booleanParameterDto.value = x;
                });
                result = b;
                break;
            case FloatRangeParameterDto floatRangeParameterDto:
                var f = new FloatRangeInputMenuItem() { dto = floatRangeParameterDto, max = floatRangeParameterDto.max, min = floatRangeParameterDto.min, value = floatRangeParameterDto.value, increment = floatRangeParameterDto.increment };
                f.Subscribe((x) =>
                {
                    floatRangeParameterDto.value = x;
                });
                result = f;
                break;
            case EnumParameterDto<string> enumParameterDto:
                var en = new DropDownInputMenuItem() { dto = enumParameterDto, options = enumParameterDto.possibleValues };
                en.NotifyValueChange(enumParameterDto.value);
                en.Subscribe((x) =>
                {
                    enumParameterDto.value = x;
                });
                result = en;
                break;
            case StringParameterDto stringParameterDto:
                var s = new TextInputMenuItem() { dto = stringParameterDto };
                s.NotifyValueChange(stringParameterDto.value);
                s.Subscribe((x) =>
                {
                    stringParameterDto.value = x;
                });
                result = s;
                break;
            default:
                result = new MenuItem();
                result.Subscribe(() => Debug.Log("hellooo 2"));
                break;
        }
        result.Name = dto.name;
        //icon;
        return result;
    }

    /// <summary>
    /// Generates the MenuItems according to the parameters contained by form.
    /// </summary>
    /// <param name="form"></param>
    /// <param name="callback"></param>
    public void Display(FormDto form, System.Action<FormDto> callback)
    {
        if (form == null)
        {
            callback.Invoke(form);
            LoadingScreen.Instance.SetLoadingScreen();
            LoadingScreen.Instance.Display("Loading environment ...");
            HideObjects();
        }
        else
        {
            LoadingScreen.Instance.Hide();
            this.gameObject.SetActive(true);
            menu.menu.RemoveAll();
            foreach (var param in form.fields)
            {
                menu.menu.Add(GetInteractionItem(param));
            }
            ButtonMenuItem send = new ButtonMenuItem() { Name = "Join", toggle = false };
            UnityAction<bool> action = (bool b) =>
            {
                menuDisplayer.Hide(true);
                menu.menu.RemoveAll();

                //DebugForm(form);

                callback.Invoke(form);
                LoadingScreen.Instance.SetLoadingScreen();
                LoadingScreen.Instance.Display("Loading environment ...");
                HideObjects();
            };
            send.Subscribe(action);
            menu.menu.Add(send);
            menuDisplayer.Display(true);
        }
    }

    private void HideObjects()
    {
        foreach (var obj in objectToHideWhenLoading)
            obj.SetActive(false);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    protected override void Awake()
    {
        base.Awake();
        Hide();
    }


    void DebugForm(FormDto form)
    {
        if (form != null && form.fields != null)
            foreach (var dto in form.fields)
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

}
