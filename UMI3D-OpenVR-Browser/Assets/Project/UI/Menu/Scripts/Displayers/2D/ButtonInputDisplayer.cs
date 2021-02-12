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
using UnityEngine;
using UnityEngine.UI;
using umi3d.cdk.menu.view;
using umi3d.cdk.menu;

public class ButtonInputDisplayer : AbstractInputMenuItemDisplayer<bool>
{
    private ButtonMenuItem menuItem;

    public Button button;
    public Text label;


    public void NotifyPress()
    {
        menuItem.NotifyValueChange(true);
    }

    public override void SetMenuItem(AbstractMenuItem menu)
    {
        base.SetMenuItem(menu);
        menuItem = menu as ButtonMenuItem;
    }

    public override void Display(bool forceUpdate = false)
    {
        this.gameObject.SetActive(true);
        label.text = menuItem.Name;
        button.onClick.AddListener(this.NotifyPress);
    }

    public override void Hide()
    {
        this.gameObject.SetActive(false);
        button.onClick.RemoveListener(this.NotifyPress);
    }

    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return (menu is ButtonMenuItem) ? 2 : 0;
    }
}
