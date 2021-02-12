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
using UnityEngine;
using umi3d.cdk.menu;

public class BindingMenuItemCircularDisplayer : HoldableMenuItemCircularDisplayer
{
    public Material baseMat;
    
    Material associatedMaterial;


    public override void Display(bool forceUpdate = false)
    {
        Material mat = new Material(baseMat);
        mat.SetColor("_ButtonColor", associatedMaterial.GetColor("_IconColor"));
        foreach (var btn in quarters)
        {

            btn.GetComponent<MeshRenderer>().sharedMaterials = new Material[] { mat };
        }

        base.Display(forceUpdate);
    }

    public override void SetMenuItem(AbstractMenuItem menu)
    {
        base.SetMenuItem(menu);

        if (menu is BindingMenuItem item)
        {
            associatedMaterial = item.associatedMaterial;
        }
    }

    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return (menu is BindingMenuItem) ? 8 : 0;
    }
}
