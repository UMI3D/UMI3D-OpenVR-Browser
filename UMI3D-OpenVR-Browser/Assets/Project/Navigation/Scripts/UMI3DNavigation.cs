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
using umi3d.cdk;
using umi3d.common;

public class UMI3DNavigation : AbstractNavigation
{

    public override void Activate() { }

    public override void Disable() { }


    public override void Teleport(TeleportDto data)
    {
        /*Teleporting tp = rig.transform.root.GetComponentInChildren<Teleporting>();
        if (tp != null)
        {
            tp.transform.position = data.position;
            tp.transform.rotation = data.rotation;
        }*/
        this.transform.position = data.position;
        this.transform.rotation = data.rotation;
    }


    public override void Navigate(NavigateDto data)
    {
        Teleport(new TeleportDto() { position = data.position, rotation = this.transform.rotation });
    }
}
