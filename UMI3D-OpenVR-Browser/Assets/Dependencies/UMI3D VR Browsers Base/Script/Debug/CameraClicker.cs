/*
Copyright 2019 - 2022 Inetum

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

using umi3dVRBrowsersBase.ui;
using UnityEngine;

/// <summary>
/// Debug class to test <see cref="IClickableElement"/>.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraClicker : MonoBehaviour
{
    #region 

    private IClickableElement lastElement;
    private Camera cam;

    #endregion

    #region Methods

    private void Start()
    {
        cam = this.GetComponent<Camera>();
    }

    private void Update()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            IClickableElement clickable = hit.transform.GetComponent<IClickableElement>();

            if (clickable != null)
            {
                if (lastElement != clickable)
                {
                    lastElement?.HoverExit();
                    lastElement = clickable;
                    clickable.HoverEnter();
                }

                if (Input.GetMouseButtonDown(0))
                    clickable.Click(umi3dVRBrowsersBase.interactions.ControllerType.RightHandController);
            }
            else
            {
                if (lastElement != null)
                {
                    lastElement.HoverExit();
                    lastElement = null;
                }
            }
        }
        else
        {
            if (lastElement != null)
            {
                lastElement.HoverExit();
                lastElement = null;
            }
        }
    }

    #endregion
}
