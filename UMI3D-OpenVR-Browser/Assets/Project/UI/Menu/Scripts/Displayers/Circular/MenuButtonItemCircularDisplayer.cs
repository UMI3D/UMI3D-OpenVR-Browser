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

public class MenuButtonItemCircularDisplayer : AbstractDisplayer, ICircularDisplayer
{
    /// <summary>
    /// Label for the menu
    /// </summary>
    public Text circularMenuLabel;

    /// <summary>
    /// Button menu to display
    /// </summary>
    protected ButtonMenuItem menuItem;

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

    public PlayerMenuManager.MaterialGroup materialGroup;

    public bool IsContentDisplayed { get; set; } = false;

    public ScreenInputDisplayer screenDisplayer { get; set; }

    /// <summary>
    /// Scale when the object is hovered
    /// </summary>
    public Vector3 hoveredScale = new Vector3(1.22f, 1, 1.22f);
    Vector3 normalScale;

    bool isHovered = false;

    /// <summary>
    /// Called when the element is not longer hovored in the circular menu.
    /// </summary>
    public void OnHoverExit()
    {
        isHovered = true;
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
        isHovered = false;
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

    public void OnSelect()
    {
        // Nothing to do
    }

    public override void Display(bool forceUpdate = false)
    {
        this.gameObject.SetActive(true);

    }

    public override void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void NotifyPress()
    {
        menuItem?.NotifyValueChange(!menuItem.GetValue());
    }

    public override void SetMenuItem(AbstractMenuItem menu)
    {
        base.SetMenuItem(menu);
        name = menu.Name + " " + this.GetType();

        if (menuItem is ButtonMenuItem item)
            menuItem = item;
    }

    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return (menu is ButtonMenuItem) ? 5 : 0;
    }

    bool wasPressed = false;

    void Update()
    {
        if (isHovered && screenDisplayer != null)
        {
            JoystickSelector joystickSelector = screenDisplayer.joystick;
            if (joystickSelector.selectButton.GetState(joystickSelector.controller))
            {
                if (!wasPressed) {
                    NotifyPress();
                    wasPressed = true;
                }
            }
            else
            {
                wasPressed = false;
            }
        }
    }
}
