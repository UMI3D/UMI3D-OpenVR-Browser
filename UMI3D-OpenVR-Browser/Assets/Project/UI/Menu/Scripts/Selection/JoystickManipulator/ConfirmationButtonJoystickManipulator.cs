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
using UnityEngine.Events;
using System.Linq;

public class ConfirmationButtonJoystickManipulator : AbstractJoystickManipulator
{
    public UnityEvent OnClick = new UnityEvent();

    private bool wasPressed = false;

    public Material defaultMat;

    public Material selectedMat;

    public Material pressMat;

    public MeshRenderer button;

    public override void UpdateContent(JoystickSelector joystickSelector)
    {
        if (IsSelected)
        {
            if (joystickSelector.selectButton.GetState(joystickSelector.controller))
            {
                if (!wasPressed)
                {
                    OnClick.Invoke();
                    wasPressed = true;
                    ChangeMat(pressMat);
                }
            } else
            {
                wasPressed = false;
                if (button.materials.Last() != selectedMat)
                    ChangeMat(selectedMat);
            }
        }
    }

    public override void Select()
    {
        base.Select();
        ChangeMat(selectedMat);
    }

    public override void UnSelect()
    {
        base.UnSelect();
        ChangeMat(defaultMat);
    }


    void ChangeMat(Material mat)
    {
        var mats = button.materials;
        mats[mats.Length - 1] = mat;
        button.materials = mats;
    }
}
