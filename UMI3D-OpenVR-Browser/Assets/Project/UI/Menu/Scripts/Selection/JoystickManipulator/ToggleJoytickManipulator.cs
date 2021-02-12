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
using UnityEngine.UI;


/// <summary>
/// Manipualor to modify a toggle with a joystick.
/// </summary>
public class ToggleJoytickManipulator : AbstractJoystickManipulator
{
    private bool wasPressed = false;

    public Toggle toggle;

    /// <summary>
    /// Updates the value of the toggle depending on joystickSelector input.
    /// </summary>
    /// <param name="joystickSelector"></param>
    public override void UpdateContent(JoystickSelector joystickSelector)
    {
        if (IsSelected)
        {
            if (joystickSelector.selectButton.GetState(joystickSelector.controller))
            {
                if (!wasPressed) {
                    toggle.isOn = !toggle.isOn;
                    wasPressed = true;
                }
            }
            else
            {
                wasPressed = false;
            }
        }
    }
}
