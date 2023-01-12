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
using System;
using System.Collections;
using System.Collections.Generic;
using umi3dVRBrowsersBase.interactions;
using UnityEngine;
using Valve.VR;

public class OpenVRInputManager : AbstractControllerInputManager
{
    [Header("SteamVR Actions")]
    public SteamVR_Action_Boolean GrabAction;
    public SteamVR_Action_Vector2 JoystickAxisAction;
    public SteamVR_Action_Boolean JoystickButtonAction;
    public SteamVR_Action_Boolean PrimaryButtonAction;
    public SteamVR_Action_Boolean SecondaryButtonAction;
    public SteamVR_Action_Boolean TriggerAction;
    public SteamVR_Action_Vibration HapticAction;

    public Dictionary<ControllerType, bool> isTeleportDown = new Dictionary<ControllerType, bool>();

    protected override void Awake()
    {
        base.Awake();

        foreach (ControllerType ctrl in Enum.GetValues(typeof(ControllerType)))
        {
            isTeleportDown.Add(ctrl, false);
        }
    }

    #region Grab

    public override bool GetGrab(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return GrabAction.GetState(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return GrabAction.GetState(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetGrabDown(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return GrabAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return GrabAction.GetStateDown(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetGrabUp(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return GrabAction.GetStateUp(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return GrabAction.GetStateUp(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    #endregion

    #region Joystick

    public override Vector2 GetJoystickAxis(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return JoystickAxisAction.GetAxis(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return JoystickAxisAction.GetAxis(SteamVR_Input_Sources.RightHand);
            default:
                return Vector2.zero;
        }
    }

    public override bool GetJoystickButton(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return JoystickButtonAction.GetState(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return JoystickButtonAction.GetState(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetJoystickButtonDown(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return JoystickButtonAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return JoystickButtonAction.GetStateDown(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetJoystickButtonUp(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return JoystickButtonAction.GetStateUp(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return JoystickButtonAction.GetStateUp(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }


    public override bool GetRightSnapTurn(ControllerType controller)
    {
        var res = GetJoystick(controller);

        if (res)
        {
            (float pole, float magnitude) = GetJoystickPoleAndMagnitude(controller);

            if ((pole >= 0 && pole < 20) || (pole > 340 && pole <= 360))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return res;
    }

    public override bool GetLeftSnapTurn(ControllerType controller)
    {
        var res = GetJoystick(controller);

        if (res)
        {
            (float pole, float magnitude) = GetJoystickPoleAndMagnitude(controller);

            if (pole > 160 && pole <= 200)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return res;
    }

    private (float, float) GetJoystickPoleAndMagnitude(ControllerType controller)
    {
        var getAxis = GetJoystickAxis(controller);

        Vector2 axis = getAxis.normalized;
        float pole = 0.0f;

        if (axis.x != 0)
            pole = Mathf.Atan(axis.y / axis.x);
        else
            if (axis.y == 0)
            pole = 0;
        else if (axis.y > 0)
            pole = Mathf.PI / 2;
        else
            pole = -Mathf.PI / 2;

        pole *= Mathf.Rad2Deg;

        if (axis.x < 0)
            if (axis.y >= 0)
                pole = 180 - Mathf.Abs(pole);
            else
                pole = 180 + Mathf.Abs(pole);
        else if (axis.y < 0)
            pole = 360 + pole;

        return (pole, getAxis.magnitude);
    }

    #endregion

    #region Primary Button

    public override bool GetPrimaryButton(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return PrimaryButtonAction.GetState(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return PrimaryButtonAction.GetState(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetPrimaryButtonDown(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return PrimaryButtonAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return PrimaryButtonAction.GetStateDown(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetPrimaryButtonUp(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return PrimaryButtonAction.GetStateUp(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return PrimaryButtonAction.GetStateUp(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    #endregion

    #region Secondary Button

    public override bool GetSecondaryButton(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SecondaryButtonAction.GetState(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SecondaryButtonAction.GetState(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetSecondaryButtonDown(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SecondaryButtonAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SecondaryButtonAction.GetStateDown(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetSecondaryButtonUp(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SecondaryButtonAction.GetStateUp(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SecondaryButtonAction.GetStateUp(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    #endregion

    #region Trigger

    public override bool GetTrigger(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return TriggerAction.GetState(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return TriggerAction.GetState(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetTriggerDown(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return TriggerAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return TriggerAction.GetStateDown(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetTriggerUp(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return TriggerAction.GetStateUp(SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return TriggerAction.GetStateUp(SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    #endregion

    public override bool GetTeleportDown(ControllerType controller)
    {
        var res = GetJoystickDown(controller);

        if (res)
        {

            (float pole, float magnitude) = GetJoystickPoleAndMagnitude(controller);

            if ((pole > 20 && pole < 160))
            {
                isTeleportDown[controller] = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        return res;
    }

    public override bool GetTeleportUp(ControllerType controller)
    {
        var res = GetJoystickUp(controller) && isTeleportDown[controller];

        if (res)
        {
            isTeleportDown[controller] = false;
        }

        return res;
    }

    public override void VibrateController(ControllerType controller, float vibrationDuration, float vibrationFrequency, float vibrationAmplitude)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                HapticAction.Execute(0f, vibrationDuration, vibrationFrequency, vibrationAmplitude, SteamVR_Input_Sources.LeftHand);
                break;
            case ControllerType.RightHandController:
                HapticAction.Execute(0f, vibrationDuration, vibrationFrequency, vibrationAmplitude, SteamVR_Input_Sources.RightHand);
                break;
        }
    }
}
