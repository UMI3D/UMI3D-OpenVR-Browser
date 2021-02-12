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
using UnityEngine.UI;
using umi3d.cdk.menu;
using umi3d.cdk.menu.view;
using UnityEngine;

public class DummyMenuDisplayer : AbstractDisplayer
{
    public Text label;

    public override void Clear()
    {
        Destroy(this.gameObject);
    }

    public override void Display(bool forceUpdate = false)
    {
        this.gameObject.SetActive(true);
        label.text = menu.Name;
    }

    public override void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return 1;
    }

    public override void Select()
    {
        base.Select();
    }
}
