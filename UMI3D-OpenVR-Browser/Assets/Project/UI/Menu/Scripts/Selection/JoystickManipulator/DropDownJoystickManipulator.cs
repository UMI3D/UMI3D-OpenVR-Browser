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
using MainThreadDispatcher;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manipualor to modify an horizontal dropdown with a joystick.
/// </summary>
public class DropDownJoystickManipulator : AbstractJoystickManipulator
{
    public HorizontalDropdown dropdown;

    [Tooltip("When the joystick is pressed, the time before the next/previous choice is selected")]
    public float intervalBetweenSelectionTime = 1;

    Coroutine selectionCoroutine;

    public override void UpdateContent(JoystickSelector joystickSelector)
    {
        if (!IsSelected)
            return;
        if (selectionCoroutine == null)
            selectionCoroutine = UnityMainThreadDispatcher.Instance().StartCoroutine(Select(joystickSelector));

        /*if (OVRInput.Get(joystickSelector.selectButton, joystickSelector.controller))
        {
            if (selectionCoroutine == null)
                selectionCoroutine = UnityMainThreadDispatcher.Instance().StartCoroutine(Select(joystickSelector));
            wantToCaptureInput = true;
        } else
        {
            if (selectionCoroutine != null)
            {
                UnityMainThreadDispatcher.Instance().StopCoroutine(selectionCoroutine);
                selectionCoroutine = null;
            }
            wantToCaptureInput = false;
        }*/
    }

    IEnumerator Select(JoystickSelector joystickSelector)
    {
        float time = 0;
        bool joystickWasReleased = true;

        while (true)
        {
            Vector2 joystickInput = joystickSelector.GetJoystickValue();
            
            if (time > intervalBetweenSelectionTime || joystickWasReleased)
            {
                float xInput = joystickInput.x;
                if ((xInput > 0) && (xInput > joystickSelector.deadzone))
                {
                    dropdown.SelectNextItem();
                    time = 0;
                    joystickWasReleased = false;
                }
                else if ((xInput < 0) && (xInput < -joystickSelector.deadzone))
                {
                    dropdown.SelectPreviousItem();
                    time = 0;
                    joystickWasReleased = false;
                }
            }
            if (joystickInput.magnitude < joystickSelector.deadzone && !joystickWasReleased)
                joystickWasReleased = true;

            time += Time.deltaTime;

            yield return 0;
        }
    }

    public override void UnSelect()
    {
        base.UnSelect();
        if (selectionCoroutine != null)
        {
            UnityMainThreadDispatcher.Instance().StopCoroutine(selectionCoroutine);
            selectionCoroutine = null;
        }
            
    }
}
