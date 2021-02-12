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

/// <summary>
/// Manipualor to modify a text input with a joystick.
/// </summary>
public class TextFieldJoystickManipulator : AbstractJoystickManipulator
{
    public UnityEngine.UI.InputField inputField;

    public GameObject keyboard;

    bool wasTriggerReleased = true;

    public bool modifiyKeybordPosition = false;

    public override void UpdateContent(JoystickSelector joystickSelector)
    {
        if (joystickSelector.selectButton.GetState(joystickSelector.controller))
        {
            if (wasTriggerReleased)
            {
                if (!keyboard.activeInHierarchy)
                {
                    inputField.Select();
                    keyboard.SetActive(true);
                } else
                {
                    keyboard.SetActive(false);
                }
                wasTriggerReleased = false;
            }

        } else
        {
            wasTriggerReleased = true;
        }
    }

    public override void UnSelect()
    {
        keyboard.SetActive(false);
        base.UnSelect();
    }
}
