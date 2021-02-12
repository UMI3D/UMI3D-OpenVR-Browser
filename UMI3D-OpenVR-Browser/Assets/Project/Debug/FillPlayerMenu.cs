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
using UnityEngine;

/// <summary>
/// Debug class
/// </summary>
public class FillPlayerMenu : MonoBehaviour
{
    public MenuAsset menuAsset;

    void Start()
    {
        //FillMenu();
        /*PlayerMenuManager.instances.ForEach(i =>
        {
            i.ClearParameterMenu();
            for (int j = 0; j < 6; j++)
            {
                i.AddMenuItemToParamatersMenu(new EventMenuItem { Name = "Test " + j });
            }
        });*/
    }

    public void Debug()
    {
        UnityEngine.Debug.Log("Pomme");
    }
    void FillMenu()
    {
        ToolboxSubMenu toolboxSubMenu = new ToolboxSubMenu()
        {
            Name = "Toolbox with sub",
            toolbox = null,
            navigable = true

        };
        ToolboxSubMenu toolboxMenuItems = new ToolboxSubMenu()
        {
            Name = "ToolboxItems",
            toolbox = null,
            navigable = true
        };

        List<MenuItem> interactions = new List<MenuItem>();
        interactions.Add(new BooleanInputMenuItem() { Name = "Toggle" });
        interactions.Add(new TextInputMenuItem() { Name = "TextField" });
        interactions.Add(new FloatRangeInputMenuItem { Name = "Float" });

        for (int i = 0; i < 2; i++)
        {
            interactions.Add(new MenuItem() { Name = "Test" + i });
        }

        ToolMenuItem toolMenuItem = new ToolMenuItem()
        {
            Name = "Dummy Tool Menu",
            tool = null,
            MenuItems = interactions,
            navigable = true
        };


        menuAsset.menu.Add(toolboxSubMenu);
        menuAsset.menu.Add(toolboxMenuItems);
        menuAsset.menu.Add(toolMenuItem);
        /*menuAsset.menu.Add(new ToolMenuItem { Name = "Fraise"});
        menuAsset.menu.Add(new ToolMenuItem { Name = "Poire"});
        menuAsset.menu.Add(new ToolMenuItem { Name = "Pomme"});
        menuAsset.menu.Add(new ToolMenuItem { Name = "Peche"});
        menuAsset.menu.Add(new ToolMenuItem { Name = "Ananas"});
        menuAsset.menu.Add(new ToolMenuItem { Name = "Figue"});*/
    }
}
