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
using umi3d.cdk.menu.view;
using umi3d.cdk.menu;
using umi3d.cdk;
using umi3d.common;

public class EnvironmentDisplayer : AbstractDisplayer
{

    GameObject icon;

    public override void Clear()
    {
        Destroy(this.gameObject);
    }

    public override void Display(bool forceUpdate = false)
    {
        if (icon != null)
        {
            if (forceUpdate)
            {
                Destroy(icon);
            }
            else
            {
                icon.SetActive(true);
                return;
            }
        }

        icon = new GameObject("pivot");
        icon.transform.SetParent(this.transform);
        icon.transform.localPosition = Vector3.zero;
        icon.transform.localRotation = Quaternion.identity;
        icon.transform.localScale = Vector3.one;

        //MediaDto media = (menu as MediaMenuItem).media;
        //UMI3DResourcesManager.DownloadObject(media.Icon3D, icon.transform, mesh =>
        //{
        //    mesh.transform.localPosition = Vector3.zero;
        //    mesh.transform.localRotation = Quaternion.Euler(90, 0, 0);
        //    mesh.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //}, 
        //s => { throw new System.Exception("Failed to load icon for " + media.Name); });


    }

    public override void Hide()
    {
        icon.SetActive(false);
    }

    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return (menu is MediaMenuItem) ? 1 : 0;
    }

    [ContextMenu("Select")]
    public override void Select()
    {
        base.Select();
    }
}
