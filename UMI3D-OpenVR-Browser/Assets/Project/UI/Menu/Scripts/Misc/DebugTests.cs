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
using umi3d.cdk.menu.view;
using umi3d.cdk.menu;
using UnityEngine;
using umi3d.cdk.menu.interaction;
using umi3d.cdk.interaction;
using umi3d.common.interaction;

public class DebugTests : MonoBehaviour
{
    public MenuDisplayManager menuDisplayManager;

    [ContextMenu("Test")]
    void Test()
    {
        ToolboxSubMenu toolboxSubMenu = new ToolboxSubMenu()
        {
            Name = "Dummy toolbox submenu",
            toolbox = null,
            navigable = true

        };
        ToolboxSubMenu toolboxSubMenu2 = new ToolboxSubMenu()
        {
            Name = "Dummy toolbox submenu",
            toolbox = null,
            navigable = true

        };

        List<Menu> interactions = new List<Menu>();
        interactions.Add(new Menu() { Name = "inter1" });
        interactions.Add(new Menu() { Name = "inter2" });
        interactions.Add(new Menu() { Name = "inter3" });
        interactions.Add(new Menu() { Name = "inter4" });

        ToolMenuItem toolMenuItem = new ToolMenuItem()
        {
            Name = "Dummy Tool Menu",
            tool = null,
            SubMenu = interactions,
            navigable = true
        };


        menuDisplayManager.menuAsset.menu.Add(toolboxSubMenu);
        menuDisplayManager.menuAsset.menu.Add(toolboxSubMenu2);
        menuDisplayManager.menuAsset.menu.Add(toolMenuItem);
        menuDisplayManager.menuAsset.menu.Add(new ToolMenuItem());
        menuDisplayManager.menuAsset.menu.Add(new ToolMenuItem());
        menuDisplayManager.menuAsset.menu.Add(new ToolMenuItem());
        menuDisplayManager.menuAsset.menu.Add(new ToolMenuItem());
        menuDisplayManager.menuAsset.menu.Add(new ToolMenuItem());
        menuDisplayManager.menuAsset.menu.Add(new ToolMenuItem());

        menuDisplayManager.Display(true);
    }

}
