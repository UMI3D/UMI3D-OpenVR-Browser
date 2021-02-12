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
using umi3d.cdk.menu.view;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a dropdown for a circular menu.
/// </summary>
public class DropdownCircularInputDisplayer : AbstractDropDownInputDisplayer, ICircularDisplayer
{
    #region Fields

    /// <summary>
    /// Label for the menu
    /// </summary>
    public Text circularMenuLabel;

    /// <summary>
    /// Dropdown.
    /// </summary>
    public HorizontalDropdown dropdown;

    /// <summary>
    /// Dropdown label
    /// </summary>
    public Text dropdonwLabel;

    /// <summary>
    /// Canvas to modify the value.
    /// </summary>
    public GameObject content;

    /// <summary>
    /// Different positions for the element in the circular menu.
    /// </summary>
    public List<GameObject> quarters;
    private int _currentQuarter = 0;

    /// <summary>
    /// Current position
    /// </summary>
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

    public PlayerMenuManager.MaterialGroup materialGroup;

    /// <summary>
    /// Manipulator to control the dropdown with a joystick.
    /// </summary>
    public DropDownJoystickManipulator dropdownManipulator;

    #endregion

    #region Methods

    void Start()
    {
        content.SetActive(false);
    }

    public override void Clear()
    {
        base.Clear();
        dropdown.onValueChanged.RemoveAllListeners();
    }

    public override void Display(bool forceUpdate = false)
    {
        this.gameObject.SetActive(true);
        dropdown.SetChoices(menuItem.options);
        dropdown.value = GetValue();
        dropdown.onValueChanged.AddListener((str) => NotifyValueChange(str));

        circularMenuLabel.text = menuItem.ToString();
        dropdonwLabel.text = circularMenuLabel.text;
    }

    public override void Hide()
    {
        dropdown.onValueChanged.RemoveAllListeners();
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Displays a canvas to enable users to modify the value of this element.
    /// </summary>
    public void OnSelect()
    {
        IsContentDisplayed = true;
        content.SetActive(true);

        string previousValue = dropdown.value;

        screenDisplayer.DisplayContent(content, (b) =>
        {
            if (!b)
                dropdown.value = previousValue;

            IsContentDisplayed = false;
            content.transform.SetParent(this.transform);
            content.SetActive(false);
        },
        dropdownManipulator);
    }

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

    public override void SetMenuItem(AbstractMenuItem menu)
    {
        base.SetMenuItem(menu);
        name = menu.Name + "(" + GetType() + ")";
    }

    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return (menu is DropDownInputMenuItem) ? 5 : 0;
    }

    #endregion
}
