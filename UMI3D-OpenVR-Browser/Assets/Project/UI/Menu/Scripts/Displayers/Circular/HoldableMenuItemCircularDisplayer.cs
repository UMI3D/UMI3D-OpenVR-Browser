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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using umi3d.cdk.menu.view;
using umi3d.cdk.menu;

public class HoldableMenuItemCircularDisplayer : AbstractDisplayer, ICircularDisplayer
{
    /// <summary>
    /// Label for the menu
    /// </summary>
    public Text circularMenuLabel;

    public Text informationText;

    /// <summary>
    /// Button menu item to display.
    /// </summary>
    protected HoldableButtonMenuItem menuItem;

    /// <summary>
    /// Canvas to modify the binding.
    /// </summary>
    public GameObject content;

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

    void Start()
    {
        content.SetActive(false);
    }

    /// <summary>
    /// Called when the element is not longer hovored in the circular menu.
    /// </summary>
    public void OnHoverExit()
    {
        transform.localScale = normalScale;
    }

    /// <summary>
    /// Called when the element is hovered in the circular menu.
    /// </summary>
    public void OnHoverEnter()
    {
        normalScale = transform.localScale;
        transform.localScale = hoveredScale;
    }

    /// <summary>
    /// Displays a canvas to enable users to modify the binfing of this element.
    /// </summary>
    public virtual void OnSelect()
    {
        IsContentDisplayed = true;
        content.SetActive(true);

        OpenVRController controler = screenDisplayer.playerMenuManager.controller;

        informationText.text = "Press any button top assign it this action";
        controler.StartListeningToChangeBinding(menuItem, informationText, menuItem.associatedInput);
    
        screenDisplayer.DisplayContent(content, (b) =>
        {
            if (b)
            {
                if (controler.WasInputSet)
                    controler.ChangeBinding((menuItem as HoldableButtonMenuItem)?.associatedInput);
                else
                    controler.ResetBindingModification();
            } else
            {
                controler.ResetBindingModification();
            }

            IsContentDisplayed = false;
            content.transform.SetParent(this.transform);
            content.SetActive(false);
        },
        null);

        screenDisplayer.HideCancelValidateButton();
    }

    public override void Display(bool forceUpdate = false)
    {
        circularMenuLabel.text = menu.ToString();
        this.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public override void SetMenuItem(AbstractMenuItem menu)
    {
        base.SetMenuItem(menu);
        name = menu.Name + " " + this.GetType();

        if (menu is HoldableButtonMenuItem item)
        {
            menuItem = item;
        }
    }

    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return (menu is HoldableButtonMenuItem) ? 6 : 0;
    }

    /*
    /// <summary>
    /// Notify that the button has been pressed.
    /// </summary>
    public void NotifyPressDown()
    {
        menuItem.NotifyValueChange(true);
    }
    public void NotifyPressUp()
    {
        menuItem.NotifyValueChange(false);
    }*/

}
