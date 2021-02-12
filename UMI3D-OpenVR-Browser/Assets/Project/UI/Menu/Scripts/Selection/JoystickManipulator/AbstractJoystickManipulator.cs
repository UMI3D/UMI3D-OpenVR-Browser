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

/// <summary>
/// This class is used to make a gameobject selectable and usable by a joystick.
/// This class is different from JoytickSelectorTarget.
/// </summary>
public abstract class AbstractJoystickManipulator : MonoBehaviour
{
    /// <summary>
    /// If this users move the joystick right, they will select this manipulator.
    /// </summary>
    public AbstractJoystickManipulator nextRightManipulator;

    /// <summary>
    /// If users move the joystick left, they will select this manipulator.
    /// </summary>
    public AbstractJoystickManipulator nextLeftManipulator;

    /// <summary>
    /// If users move the joystick upo, they will select this manipulator.
    /// </summary>
    public AbstractJoystickManipulator nextUpManipulator;

    /// <summary>
    /// If users move the joystick down, they will select this manipulator.
    /// </summary>
    public AbstractJoystickManipulator nextDownManipulator;

    /// <summary>
    /// Returns true if this AbstractJoystickManipulator is selected.
    /// </summary>
    public bool IsSelected { get; private set; } = false;

    [Tooltip("This object will be show or hide to indicate to users if this manipulator is selected or not")]
    public GameObject statusIndicator;

    /// <summary>
    /// If true, other systems should ignore the input from the joystick 
    /// </summary>
    public bool wantToCaptureInput = false;

    /// <summary>
    /// Changes the status of the manipulator (its value for example) depending on the joytick inputs.
    /// </summary>
    /// <param name="joystickSelector"></param>
    public abstract void UpdateContent(JoystickSelector joystickSelector);

    /// <summary>
    /// Selects this AbstractJoystickManipulator;
    /// </summary>
    public virtual void Select()
    {
        IsSelected = true;
        if(statusIndicator != null)
            statusIndicator.SetActive(true);
    }

    /// <summary>
    /// Unselects this AbstractJoystickManipulator;
    /// </summary>
    public virtual void UnSelect()
    {
        IsSelected = false;
        if (statusIndicator != null)
            statusIndicator.SetActive(false);
    }
}
