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
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk.menu;
using umi3d.cdk.menu.interaction;
using umi3d.cdk.menu.view;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Display a form for the circular menu.
/// </summary>
public class FormCircularDisplayer : AbstractDisplayer, ICircularDisplayer
{
    #region Fields

    /// <summary>
    /// Label for the menu
    /// </summary>
    public Text circularMenuLabel;

    /// <summary>
    /// Label for the edition screen.
    /// </summary>
    public Text screenLabel;

    public GameObject screenBorder;

    /// <summary>
    /// Canvas to modify the value.
    /// </summary>
    public GameObject content;

    /// <summary>
    /// Viewport where input are displayed.
    /// </summary>
    public GameObject viewport;

    /// <summary>
    /// ScrollView where input are displayed.
    /// </summary>
    public RectTransform scrollView;

    /// <summary>
    /// All inputs currently displayed.
    /// </summary>
    private List<AbstractJoystickManipulator> inputs = new List<AbstractJoystickManipulator>();

    /// <summary>
    /// MenuItem associated.
    /// </summary>
    private FormMenuItem menuItem;

    [Header("Form input prefabs")]
    [Tooltip("Prefabs must be ordered like this : ToggleJoystickManipulator, SliderJoystickManipulator, DropdownJoystickManipulator and TextFieldJoystickManipulator.")]
    public AbstractJoystickManipulator[] inputManipulators;

    /// <summary>
    /// Keyboard used to edit textField.
    /// </summary>
    public GameObject keyboard;

    [Header("Circular Button display")]

    public PlayerMenuManager.MaterialGroup materialGroup;

    public List<GameObject> quarters;
    private int _currentQuarter = 0;
    public int CurrentQuarter
    {
        get
        {
            return _currentQuarter;
        }
        set
        {
            _currentQuarter = Mathf.Clamp(value, 0, quarters.Count);
            for (int i = 0; i < quarters.Count; i++)
            {
                quarters[i].SetActive(_currentQuarter == i);
            }
        }
    }

    public bool IsContentDisplayed { get; set; } = false;

    public ScreenInputDisplayer screenDisplayer { get; set; }

    public Vector3 hoveredScale = new Vector3(1.22f, 1, 1.22f);
    Vector3 normalScale;

    #endregion

    #region Methods

    void Start()
    {
        content.SetActive(false);
    }

    /// <summary>
    /// Displays the element in the circularMenu (not the toggle directly).
    /// </summary>
    /// <see cref="OnSelect"/>
    /// <param name="forceUpdate"></param>
    public override void Display(bool forceUpdate = false)
    {
        this.gameObject.SetActive(true);

        DisplayForm();

        circularMenuLabel.text = menuItem.ToString();
        screenLabel.text = circularMenuLabel.text;
    }

    public override void Hide()
    {
        HideForm();

        this.gameObject.SetActive(false);
    }

    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return (menu is FormMenuItem) ? 9 : 0;
    }

    public override void SetMenuItem(AbstractMenuItem menu)
    {
        base.SetMenuItem(menu);
        name = menu.Name + "(" + GetType() + ")";

        if (menu is FormMenuItem form)
            menuItem = form;
        else
            Debug.LogError("This displayer is only made for FormMenuItem.");

    }

    /// <summary>
    /// Called when the element is not longer hovored in the circular menu.
    /// </summary>
    public void OnHoverExit()
    {
        foreach (GameObject q in quarters)
        {
            MeshRenderer rnd = q.GetComponent<MeshRenderer>();
            Material[] m = rnd.materials;
            m[materialGroup.materialIndex] = materialGroup.enabled;
            rnd.materials = m;
        }
        transform.localScale = normalScale;
    }

    /// <summary>
    /// Called when the element is hovered in the circular menu.
    /// </summary>
    public void OnHoverEnter()
    {
        foreach (GameObject q in quarters)
        {
            MeshRenderer rnd = q.GetComponent<MeshRenderer>();
            Material[] m = rnd.materials;
            m[materialGroup.materialIndex] = materialGroup.hoverred;
            rnd.materials = m;
        }
        normalScale = transform.localScale;
        transform.localScale = hoveredScale;
    }

    /// <summary>
    /// Displays a canvas to enable users to modify the value of this element.
    /// </summary>
    public void OnSelect()
    {
        IsContentDisplayed = true;
        content.SetActive(true);

        inputs[0].nextUpManipulator = screenDisplayer.cancelButton;

        inputs[inputs.Count - 1].nextDownManipulator = screenDisplayer.validateButton;

        float screenHeight = 1 + 0.5f * (inputs.Count - 1);
        if (inputs.Count > 1)
            screenHeight += .3f;
        screenDisplayer.SetScreenSize(screenHeight);

        var pos = screenLabel.transform.localPosition;
        pos.y = 1.7f + 3.5f * (inputs.Count - 1);
        screenLabel.transform.localPosition = pos;
        //pos.y = -40;
        screenBorder.transform.localPosition = pos;

        scrollView.sizeDelta = new Vector2(scrollView.sizeDelta.x, 60 + 30 * (inputs.Count - 1));


        //Set Keyboard position;
        Vector3[] corners = new Vector3[4];
        screenBorder.gameObject.GetComponent<RectTransform>().GetWorldCorners(corners);
        keyboard.transform.position = ((corners[1] + corners[2]) / 2);

        screenDisplayer.DisplayContent(content, (b) =>
        {
            if (b)
            {
                DebugForm(menuItem.dto);
                menuItem.Select();
            }

            IsContentDisplayed = false;
            content.transform.SetParent(this.transform);
            content.SetActive(false);
        },
        inputs[0],
        false);

        screenDisplayer.validateButton.nextUpManipulator = inputs[inputs.Count - 1];
        screenDisplayer.cancelButton.nextUpManipulator = inputs[inputs.Count - 1];
        screenDisplayer.validateButton.nextDownManipulator = inputs[0];
        screenDisplayer.cancelButton.nextDownManipulator = inputs[0];
    }

    void DebugForm(FormDto form)
    {
        foreach(var dto in form.fields)
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

    void HideForm()
    {
        foreach (var i in inputs)
            Destroy(i.gameObject);
    }

    /// <summary>
    /// Iterates through FormDto.fields to display the form.
    /// </summary>
    void DisplayForm()
    {
        for (int i = 0; i < menuItem.dto.fields.Count; i++)
        {
            var input = DisplayInput(menuItem.dto.fields[i]);
            input.gameObject.SetActive(true);
            if (i != 0)
            {
                input.nextUpManipulator = inputs[i - 1];
                input.UnSelect();
            }
        }

        for (int i = 0; i < inputs.Count; i++)
        {
            if (i + 1 < inputs.Count)
            {
                inputs[i].nextDownManipulator = inputs[i + 1];
            }
        }
    }

    /// <summary>
    /// Creates the right input depending on the dto type.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    AbstractJoystickManipulator DisplayInput(AbstractInteractionDto dto)
    {
        switch (dto)
        {
            case BooleanParameterDto booleanParameterDto:
                ToggleJoytickManipulator toggleManipulator = Instantiate(inputManipulators[0], viewport.transform) as ToggleJoytickManipulator;
                toggleManipulator.toggle.isOn = booleanParameterDto.value;
                toggleManipulator.toggle.onValueChanged.AddListener(b =>
                {
                    booleanParameterDto.value = b;
                });
                inputs.Add(toggleManipulator);
                return toggleManipulator;
            case FloatRangeParameterDto floatRangeParameterDto:
                SliderJoystickManipulator sliderManipulator = Instantiate(inputManipulators[1], viewport.transform) as SliderJoystickManipulator;
                Slider slider = sliderManipulator.slider;
                slider.value = floatRangeParameterDto.value;
                sliderManipulator.displayValueLabel.text = floatRangeParameterDto.value.ToString();
                sliderManipulator.slider.onValueChanged.AddListener(f =>
                {
                    floatRangeParameterDto.value = f;
                });

                slider.minValue = floatRangeParameterDto.min;
                slider.maxValue = floatRangeParameterDto.max;

                inputs.Add(sliderManipulator);
                return sliderManipulator;
            case EnumParameterDto<string> enumParameterDto:
                DropDownJoystickManipulator dropDownJoystickManipulator = Instantiate(inputManipulators[2], viewport.transform) as DropDownJoystickManipulator;
                dropDownJoystickManipulator.dropdown.SetChoices(enumParameterDto.possibleValues);
                dropDownJoystickManipulator.dropdown.value = enumParameterDto.value;
                dropDownJoystickManipulator.dropdown.onValueChanged.AddListener(v =>
                {
                    enumParameterDto.value = v;
                });
                inputs.Add(dropDownJoystickManipulator);
                return dropDownJoystickManipulator;
            case StringParameterDto stringParameterDto:
                TextFieldJoystickManipulator textFieldJoystickManipulator = Instantiate(inputManipulators[3], viewport.transform) as TextFieldJoystickManipulator;
                textFieldJoystickManipulator.keyboard = keyboard;
                textFieldJoystickManipulator.inputField.text = stringParameterDto.value;
                textFieldJoystickManipulator.inputField.onValueChanged.AddListener(v =>
                {
                    stringParameterDto.value = v;
                });
                inputs.Add(textFieldJoystickManipulator);
                return textFieldJoystickManipulator;
            default:
                throw new System.ArgumentException(dto.GetType() + " is not supported by this form displayer");
        }
    }

    #endregion
}
