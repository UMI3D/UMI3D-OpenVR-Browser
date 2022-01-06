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
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// This class is responsible for the screen which allows users to set paramater values.
/// </summary>
public class ScreenInputDisplayer : MonoBehaviour
{
    #region Fields

    public GameObject screenContainer;
    public GameObject screenContent;
    public GameObject screenMesh;

    /// <summary>
    /// Content position when it is displayed on this screen.
    /// </summary>
    public Vector3 contentPosition = new Vector3(0, 0, 2.67f);

    /// <summary>
    /// Callback called when users whant to close this screen.
    /// </summary>
    Action<bool> validationCallback;

    /// <summary>
    /// Button to validate modifications and close the screen.
    /// </summary>
    public ConfirmationButtonJoystickManipulator validateButton;

    /// <summary>
    /// Button to cancel modifications and close the screen.
    /// </summary>
    public ConfirmationButtonJoystickManipulator cancelButton;

    /// <summary>
    /// Returns true if the screen is currently displayed.
    /// </summary>
    public bool IsDisplayed { get; private set; }

    /// <summary>
    /// JoystickSelector used to control the screen.
    /// </summary>
    public JoystickSelector joystick;

    /// <summary>
    /// Current manipulator selected.
    /// </summary>
    AbstractJoystickManipulator currentManipulatorSelected;

    /// <summary>
    /// Objets to hide/show when the screen is hidden or displayed.
    /// </summary>
    public GameObject[] objectsToHide;

    private bool canSelectButton = true;

    /// <summary>
    /// Not directly related to this screen, but all displayers have an access to this object.
    /// </summary>
    public PlayerMenuManager playerMenuManager;

    #endregion

    #region Methods

    void Awake()
    {
        screenContainer.SetActive(false);
    }

    /// <summary>
    /// Displays the screen and content.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="validationCallback">>Do not forget do delete the content in the callback</param>
    /// <param name="firstManipulatorToSelect"></param>
    public void DisplayContent(GameObject content, Action<bool> validationCallback, AbstractJoystickManipulator firstManipulatorToSelect = null, bool linkValidateAndCancelButton = true)
    {
        if (screenContent.transform.childCount > 0)
        {
            Debug.LogError("A Something is alreay displayed in the screen");
        }
        DisplayCancelValidateButton();

        this.validationCallback = validationCallback;

        IsDisplayed = true;

        screenContainer.SetActive(true);
        content.transform.SetParent(screenContent.transform);
        content.transform.localRotation = Quaternion.Euler(90, 0, 0);
        content.transform.localPosition = contentPosition;
        content.transform.localScale = Vector3.one;

        if (firstManipulatorToSelect != null)
        {
            firstManipulatorToSelect.Select();
            currentManipulatorSelected = firstManipulatorToSelect;

            if (linkValidateAndCancelButton)
            {
                currentManipulatorSelected.nextDownManipulator = validateButton;
                currentManipulatorSelected.nextUpManipulator = cancelButton;
            }
            
            validateButton.nextUpManipulator = currentManipulatorSelected;
            validateButton.nextDownManipulator = currentManipulatorSelected;
            cancelButton.nextUpManipulator = currentManipulatorSelected;
            cancelButton.nextDownManipulator = currentManipulatorSelected;   
        }

        validateButton.UnSelect();
        cancelButton.UnSelect();

        foreach (var obj in objectsToHide)
            obj.SetActive(false);

        joystick.Desactivate();

        canSelectButton = false;

        if (joystick.selectButton.GetState(joystick.controller))
        {
            StartCoroutine(InitCanSelect());
        }
        else
        {
            canSelectButton = true;
        }
    }

    IEnumerator InitCanSelect()
    {
        canSelectButton = false;
        if (joystick.selectButton.GetState(joystick.controller))
        {
            yield return null;
        }
        canSelectButton = true;
    }

    /// <summary>
    /// Validates or cancels (depending on param val) modifications and hides this screen.
    /// </summary>
    /// <param name="val"></param>
    public void HideScreen(bool val)
    {
        validationCallback.Invoke(val);
        screenContainer.SetActive(false);
        IsDisplayed = false;
        foreach (var obj in objectsToHide)
            obj.SetActive(true);

        joystick.Activate();
        SetScreenSize(1);
    }

    public void HideCancelValidateButton()
    {
        cancelButton.gameObject.SetActive(false);
        validateButton.gameObject.SetActive(false);

        cancelButton.UnSelect();
        validateButton.UnSelect();
        currentManipulatorSelected = null;
        canSelectButton = false;
    }

    public void DisplayCancelValidateButton()
    {
        cancelButton.gameObject.SetActive(true);
        validateButton.gameObject.SetActive(true);
        validateButton.Select();
        currentManipulatorSelected = validateButton;
        canSelectButton = true;
    }

    void LateUpdate()
    {
        if(currentManipulatorSelected != null && canSelectButton)
        {
            if (!currentManipulatorSelected.wantToCaptureInput)
                UpdateSelection();

            currentManipulatorSelected.UpdateContent(joystick);
        }
    }

    bool wasJoystickReleased = true;
    Vector2 previousSelectedAxe = Vector2.zero;

    /// <summary>
    /// Updates the currentManipulatorSelected according to the joystick inputs.
    /// </summary>
    void UpdateSelection()
    {
        Vector2 joystickValue = joystick.GetJoystickValue();

        if (joystickValue.magnitude > joystick.deadzone)
        {
            Vector2 selectedAxe;
            if (Mathf.Abs(joystickValue.x) > Mathf.Abs(joystickValue.y))
            {
                if (joystickValue.x > 0)
                    selectedAxe = Vector2.right;
                else
                    selectedAxe = Vector2.left;
            } else
            {
                if (joystickValue.y > 0)
                    selectedAxe = Vector2.up;
                else
                    selectedAxe = Vector2.down;
            }

            if (previousSelectedAxe != selectedAxe)
            {
                wasJoystickReleased = true;
            }

            previousSelectedAxe = selectedAxe;

            if (wasJoystickReleased)
            {
                SelectNextButton(selectedAxe);
                wasJoystickReleased = false;
            }
        }else
        {
            wasJoystickReleased = true;
        }
    }

    /// <summary>
    /// Tries to change the currentManipulatorSelected;
    /// </summary>
    /// <param name="selectedAxe"></param>
    void SelectNextButton(Vector2 selectedAxe)
    {
        if (selectedAxe == Vector2.right && currentManipulatorSelected.nextRightManipulator != null)
        {
            currentManipulatorSelected.UnSelect();
            currentManipulatorSelected = currentManipulatorSelected.nextRightManipulator;
            currentManipulatorSelected.Select();
        }
        else if (selectedAxe == Vector2.left && currentManipulatorSelected.nextLeftManipulator != null)
        {
            currentManipulatorSelected.UnSelect();
            currentManipulatorSelected = currentManipulatorSelected.nextLeftManipulator;
            currentManipulatorSelected.Select();
        }
        else if (selectedAxe == Vector2.up && currentManipulatorSelected.nextUpManipulator != null)
        {
            Debug.Log($"current manipulator = [{currentManipulatorSelected.name}]; nextup manipulator = [{currentManipulatorSelected.nextUpManipulator}]");
            currentManipulatorSelected.UnSelect();
            currentManipulatorSelected = currentManipulatorSelected.nextUpManipulator;
            currentManipulatorSelected.Select();
        }
        else if (selectedAxe == Vector2.down && currentManipulatorSelected.nextDownManipulator != null)
        {
            currentManipulatorSelected.UnSelect();
            currentManipulatorSelected = currentManipulatorSelected.nextDownManipulator;
            currentManipulatorSelected.Select();
        }
        Debug.Log($"current manipulator = [{currentManipulatorSelected}]");
    }

    public void SetScreenSize(float height)
    {
        var newScale = screenMesh.transform.localScale;
        newScale.z = height;
        screenMesh.transform.localScale = newScale; 
    }

    #endregion
}
