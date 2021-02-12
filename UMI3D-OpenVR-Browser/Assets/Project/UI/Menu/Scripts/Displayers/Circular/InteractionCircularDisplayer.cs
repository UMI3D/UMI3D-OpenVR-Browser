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
using UnityEngine;
using UnityEngine.UI;

public class InteractionCircularDisplayer : CircularItemDisplayer
{
    public List<GameObject> quarters;
    public PlayerMenuManager.MaterialGroup materialGroup;

    private int currentQarter_ = 0;
    public int currentQarter
    {
        get
        {
            return currentQarter_;
        }
        set
        {
            currentQarter_ = Mathf.Clamp(value, 0, quarters.Count);
            for (int i = 0; i < quarters.Count; i++)
            {
                quarters[i].SetActive(currentQarter_ == i);
            }
        }
    }

    public Text text;


    public override void Display(bool forceUpdate = false)
    {
        base.Display(forceUpdate);
    }


    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return menu is InteractionMenuItem ? base.IsSuitableFor(menu) + 1 : 0;
    }

    public void HoverEnter()
    {
        foreach (GameObject q in quarters)
        {
            MeshRenderer rnd = q.GetComponent<MeshRenderer>();
            Material[] m = rnd.materials;
            m[materialGroup.materialIndex] = materialGroup.hoverred;
            rnd.materials = m;
        }
    }

    public void HoverExit()
    {
        foreach (GameObject q in quarters)
        {
            MeshRenderer rnd = q.GetComponent<MeshRenderer>();
            Material[] m = rnd.materials;
            m[materialGroup.materialIndex] = materialGroup.enabled;
            rnd.materials = m;
        }
    }
}
