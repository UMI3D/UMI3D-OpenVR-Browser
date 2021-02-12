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

public class FillMenuWithRandom : MonoBehaviour
{
    public MenuDisplayManager menuDisplayer;
    public MenuAsset menu;
    public int childrenPerNode = 10;
    public int leafPerNode = 5;
    public int depth = 3;

    void Start()
    {
        FillWithRandom();
        menuDisplayer.Display(true);
    }

    [ContextMenu("Fill")]
    void FillWithRandom()
    {
        Menu newMenu = new Menu()
        {
            Name = "RandomMenu",
            navigable = true
        };

        List<Menu> currentLine = new List<Menu>();
        currentLine.Add(newMenu);
        for (int d = 0; d<depth - 1; d++)
        {
            List<Menu> nextLine = new List<Menu>();
            foreach (Menu m in currentLine)
            {
                for (int c = 0; c < childrenPerNode; c++)
                {
                    Menu child = new Menu()
                    {
                        Name = Random.Range(0, 1000).ToString(),
                        navigable = true                        
                    };
                    m.Add(child);
                    nextLine.Add(child);
                }
                for (int l = 0; l < leafPerNode; l++)
                {
                    MenuItem leaf = new MenuItem()
                    {
                        Name = "leaf " + Random.Range(0, 1000).ToString()
                    };
                    m.Add(leaf);
                }
            }
            currentLine = nextLine;
        }

        foreach (Menu m in currentLine)
        {
            for (int c = 0; c < childrenPerNode; c++)
            {
                MenuItem child = new MenuItem()
                {
                    Name = Random.Range(0, 1000).ToString()
                };
                m.Add(child);
            }
        }

        menu.menu = newMenu;
    }

    [ContextMenu("Insert One")]
    public void InsertOneAtRoot()
    {
        menu.menu.Add(new Menu()
        {
            Name = "new random"
        });
        menuDisplayer.Display(true);
    }


    [ContextMenu("UpdateDisplay")]
    public void UpdateDisplay()
    {
        menuDisplayer.Display(true);
    }
}
