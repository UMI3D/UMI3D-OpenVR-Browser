﻿/*
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