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
/// Displays a toggle for a circular menu.
/// </summary>
public class BooleanCircularDisplayer : AbstractBooleanInputDisplayer, ICircularDisplayer
{
    #region Fields

    /// <summary>
    /// Label for the menu
    /// </summary>
    public Text circularMenuLabel;

    /// <summary>
    /// Canvas to modify the value.
    /// </summary>
    public GameObject content;

    /// <summary>
    /// Toggle
    /// </summary>
    public Toggle toggle;

    public Text toggleLabel;

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

    /// <summary>
    /// Manipulator to control the toggle with a joystick.
    /// </summary>
    public ToggleJoytickManipulator toggleManipulator;

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
        toggle.isOn = GetValue();
        toggleLabel.text = menuItem.ToString();
        toggle.onValueChanged.AddListener(NotifyValueChange);

        circularMenuLabel.text = menuItem.ToString();
    }

    public override void Hide()
    {
        toggle.onValueChanged.RemoveListener(NotifyValueChange);
        this.gameObject.SetActive(false);
    }

    public override void Clear()
    {
        base.Clear();
        toggle.onValueChanged.RemoveListener(NotifyValueChange);
    }

    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return (menu is BooleanInputMenuItem) ? 2 : 0;
    }

    public override void SetMenuItem(AbstractMenuItem menu)
    {
        base.SetMenuItem(menu);
        name = menu.Name + "(" + GetType() + ")";
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

        bool previousValue = GetValue();

        screenDisplayer.DisplayContent(content, (b) =>
        {
             if (!b)
                 toggle.isOn = previousValue;

            IsContentDisplayed = false;
            content.transform.SetParent(this.transform);
            content.SetActive(false);
        },
        toggleManipulator);
    }

    #endregion
}
