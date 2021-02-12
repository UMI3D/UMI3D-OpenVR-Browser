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
using QuestBrowser.Interactions;
using umi3d.cdk.interaction;
using umi3d.cdk.menu;
using umi3d.cdk.menu.interaction;

public class ToolCircularItemContainer : CircularItemContainer
{
    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return base.IsSuitableFor(menu) + ((menu is ToolMenuItem) ? 1 : 0);
    }

    public override void SetMenuItem(AbstractMenuItem menu)
    {
        base.SetMenuItem(menu);
        name = menu.Name + " " + menu.GetType();
    }

    public override void Select()
    {
        (InteractionMapper.Instance as OpenVRInteractionMapper).lastControllerUsedInMenu = screenDisplayer.playerMenuManager.controller;
        if (menu is ToolMenuItem toolMenu)
        {
            screenDisplayer.playerMenuManager.currentToolMenu = toolMenu;
            if (toolMenu.SubMenu.Count > 0)
            {
                PlayerMenuManager manager = screenDisplayer.playerMenuManager;
                manager.toolAndToolBoxDisplayDepth--;
                manager.DisplayParametersPanel();
            }
        }
        
        base.Select();
    }
}
