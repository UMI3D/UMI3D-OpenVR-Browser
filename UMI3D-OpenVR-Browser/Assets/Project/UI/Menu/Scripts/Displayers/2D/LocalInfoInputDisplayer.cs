﻿/*
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
using umi3d.cdk.menu;
using umi3d.cdk.menu.view;
using System;
using umi3d.common.interaction;

public class LocalInfoInputDisplayer : AbstractLocalInfoRequestInputDisplayer 
{
    public Toggle toggleRead;
    public Toggle toggleWrite;
    public Text label;

    public override void Display(bool forceUpdate = false)
    {
        this.gameObject.SetActive(true);
        (toggleRead.isOn, toggleWrite.isOn) = GetValue();
        toggleRead.onValueChanged.AddListener(ReadValueChange);
        toggleWrite.onValueChanged.AddListener(WriteValueChange);
        label.text = "Server " + ((LocalInfoRequestParameterDto)menuItem.dto).serverName + " requests acces to local data : " + ((LocalInfoRequestParameterDto)menuItem.dto).key;
    }

    void ReadValueChange(bool read)
    {
        NotifyValueChange((read, toggleWrite.isOn));
    }
    void WriteValueChange(bool write)
    {
        NotifyValueChange((toggleRead.isOn, write));
    }

    public override void Hide()
    {
        toggleRead.onValueChanged.RemoveListener(ReadValueChange);
        toggleWrite.onValueChanged.RemoveListener(WriteValueChange);
        this.gameObject.SetActive(false);
    }

    public override int IsSuitableFor(AbstractMenuItem menu) => (menu is LocalInfoRequestInputMenuItem) ? 2 : 0;
}