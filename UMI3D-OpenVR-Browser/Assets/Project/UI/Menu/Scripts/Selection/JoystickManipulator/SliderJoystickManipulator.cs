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
using UnityEngine.UI;


/// <summary>
/// Manipualor to modify a slider with a joystick.
/// </summary>
public class SliderJoystickManipulator : AbstractJoystickManipulator
{
    public Slider slider;

    public Text displayValueLabel;

    public float joystickSensibility = .1f;

    public override void UpdateContent(JoystickSelector joystickSelector)
    {
        if (IsSelected)
        {
            float input = joystickSelector.GetJoystickValue().x;
            slider.value = Mathf.Clamp(slider.value + input * joystickSensibility, slider.minValue, slider.maxValue);
            if (displayValueLabel != null)
                displayValueLabel.text = System.Math.Round(slider.value, 1).ToString();
        }
    }
}
