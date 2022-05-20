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
using System.Collections;
using System.Collections.Generic;
using umi3dVRBrowsersBase.interactions;
using UnityEngine;
using Valve.VR;

public class OpenVRInputManager : AbstractControllerInputManager
{
    public string GrabAction => "/actions/UMI3D/in/Grab";
    public string JoystickAxisAction => "/actions/UMI3D/in/JoystickAxis";
    public string JoystickButtonAction => "/actions/UMI3D/in/JoystickButton";
    public string PrimaryButtonAction => "/actions/UMI3D/in/PrimaryButton";
    public string SecondaryButtonAction => "/actions/UMI3D/in/SecondaryButton";
    public string TriggerAction => "/actions/UMI3D/in/Trigger";
    public string HapticAction => "/actions/UMI3D/out/Haptic";

    #region Grab

    public override bool GetGrab(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SteamVR_Input.GetState(GrabAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetState(GrabAction, SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetGrabDown(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SteamVR_Input.GetStateDown(GrabAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetStateDown(GrabAction, SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetGrabUp(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SteamVR_Input.GetStateUp(GrabAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetStateUp(GrabAction, SteamVR_Input_Sources.RightHand);
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
                return SteamVR_Input.GetVector2(JoystickAxisAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetVector2(JoystickAxisAction, SteamVR_Input_Sources.RightHand);
            default:
                return Vector2.zero;
        }
    }

    public override bool GetJoystickButton(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SteamVR_Input.GetState(JoystickButtonAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetState(JoystickButtonAction, SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetJoystickButtonDown(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SteamVR_Input.GetStateDown(JoystickButtonAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetStateDown(JoystickButtonAction, SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetJoystickButtonUp(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SteamVR_Input.GetStateUp(JoystickButtonAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetStateUp(JoystickButtonAction, SteamVR_Input_Sources.RightHand);
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
                return SteamVR_Input.GetState(PrimaryButtonAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetState(PrimaryButtonAction, SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetPrimaryButtonDown(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SteamVR_Input.GetStateDown(PrimaryButtonAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetStateDown(PrimaryButtonAction, SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetPrimaryButtonUp(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SteamVR_Input.GetStateUp(PrimaryButtonAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetStateUp(PrimaryButtonAction, SteamVR_Input_Sources.RightHand);
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
                return SteamVR_Input.GetState(SecondaryButtonAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetState(SecondaryButtonAction, SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetSecondaryButtonDown(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SteamVR_Input.GetStateDown(SecondaryButtonAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetStateDown(SecondaryButtonAction, SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetSecondaryButtonUp(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SteamVR_Input.GetStateUp(SecondaryButtonAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetStateUp(SecondaryButtonAction, SteamVR_Input_Sources.RightHand);
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
                return SteamVR_Input.GetState(TriggerAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetState(TriggerAction, SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetTriggerDown(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SteamVR_Input.GetStateDown(TriggerAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetStateDown(TriggerAction, SteamVR_Input_Sources.RightHand);
            default:
                return false;
        }
    }

    public override bool GetTriggerUp(ControllerType controller)
    {
        switch (controller)
        {
            case ControllerType.LeftHandController:
                return SteamVR_Input.GetStateUp(TriggerAction, SteamVR_Input_Sources.LeftHand);
            case ControllerType.RightHandController:
                return SteamVR_Input.GetStateUp(TriggerAction, SteamVR_Input_Sources.RightHand);
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
                SteamVR_Input.GetVibrationAction(HapticAction)
                    .Execute(0f, vibrationDuration, vibrationFrequency, vibrationAmplitude, SteamVR_Input_Sources.LeftHand);
                break;
            case ControllerType.RightHandController:
                SteamVR_Input.GetVibrationAction(HapticAction)
                    .Execute(0f, vibrationDuration, vibrationFrequency, vibrationAmplitude, SteamVR_Input_Sources.RightHand);
                break;
        }
    }
}
