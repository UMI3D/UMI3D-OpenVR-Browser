using UnityEngine;
using umi3d.common;
using umi3d.cdk.menu;
using umi3d.cdk.menu.view;
using umi3d.common.interaction;
using UnityEngine.Events;
using umi3d.cdk.collaboration;
using System.Linq;
using System.Collections.Generic;

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
    static (MenuItem, ParameterSettingRequestDto) GetInteractionItem(AbstractInteractionDto dto)
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
                LocalInfoRequestInputMenuItem localReq = new LocalInfoRequestInputMenuItem() { dto = localInfoRequestParameterDto };
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
        //icon;
        return (result, requestDto);
    }

    /// <summary>
    /// Generates the MenuItems according to the parameters contained by form.
    /// </summary>
    /// <param name="form"></param>
    /// <param name="callback"></param>
    public void Display(FormDto form, System.Action<FormAnswerDto> callback)
    {
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
            FormAnswerDto answer = new FormAnswerDto()
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

            foreach (var param in form.fields)
            {
                var c = GetInteractionItem(param);
                menu.menu.Add(c.Item1);
                answer.answers.Add(c.Item2);
            }

            ButtonMenuItem send = new ButtonMenuItem() { Name = "Join", toggle = false };
            UnityAction<bool> action = (bool b) =>
            {
                menuDisplayer.Hide(true);
                menu.menu.RemoveAll();

                //DebugForm(form);

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
