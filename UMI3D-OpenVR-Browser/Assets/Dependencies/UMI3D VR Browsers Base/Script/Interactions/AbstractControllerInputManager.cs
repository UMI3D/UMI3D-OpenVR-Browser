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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace umi3dVRBrowsersBase.interactions
{
    /// <summary>
    /// This class enables scripts to get input from VR devices. This class must be implemenented on each browser depending on the device.
    /// </summary>
    public abstract class AbstractControllerInputManager : SingleBehaviour<AbstractControllerInputManager>
    {
        #region Fields

        /// <summary>
        /// Duration (in seconds) of a controller vibration.
        /// </summary>
        public float vibrationDuration;

        /// <summary>
        /// Frequency used for a controller vibration.
        /// </summary>
        public float vibrationFrequency;

        /// <summary>
        /// Amplitude used for a controller vibration.
        /// </summary>
        public float vibrationAmplitude;

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if the input associated to <paramref name="action"/> of <paramref name="controller"/> is triggered (according to <paramref name="eventType"/>).
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="eventType"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual bool GetButton(ControllerType controller, InputEventType eventType, ActionType action)
        {
            bool res = false;

            switch (eventType)
            {
                case InputEventType.Down:
                    res = GetButtonDown(controller, action);
                    break;
                case InputEventType.Up:
                    res = GetButtonUp(controller, action);
                    break;
                case InputEventType.Hold:
                    res = GetButton(controller, action);
                    break;
                default:
                    break;
            }

            return res;
        }

        /// <summary>
        /// Returns true if the input associated to <paramref name="action"/> of <paramref name="controller"/> is down.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual bool GetButtonDown(ControllerType controller, ActionType action)
        {
            switch (action)
            {
                case ActionType.Trigger:
                    return GetTriggerDown(controller);
                case ActionType.Grab:
                    return GetGrabDown(controller);
                case ActionType.PrimaryButton:
                    return GetPrimaryButtonDown(controller);
                case ActionType.SecondaryButton:
                    return GetSecondaryButtonDown(controller);
                case ActionType.JoystickButton:
                    return GetJoystickButtonDown(controller);
                case ActionType.Teleport:
                    return GetTeleportDown(controller);
                case ActionType.RightSnapTurn:
                    return GetRightSnapTurn(controller);
                case ActionType.LeftSnapTurn:
                    return GetLeftSnapTurn(controller);
                default:
                    break;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the input associated to <paramref name="action"/> of <paramref name="controller"/> was just released.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual bool GetButtonUp(ControllerType controller, ActionType action)
        {
            switch (action)
            {
                case ActionType.Trigger:
                    return GetTriggerUp(controller);
                case ActionType.Grab:
                    return GetGrabUp(controller);
                case ActionType.PrimaryButton:
                    return GetPrimaryButtonUp(controller);
                case ActionType.SecondaryButton:
                    return GetSecondaryButtonUp(controller);
                case ActionType.JoystickButton:
                    return GetJoystickButtonUp(controller);
                case ActionType.Teleport:
                    return GetTeleportUp(controller);
                default:
                    break;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the input associated to <paramref name="action"/> of <paramref name="controller"/> is currently pressed.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual bool GetButton(ControllerType controller, ActionType action)
        {
            switch (action)
            {
                case ActionType.Trigger:
                    return GetTrigger(controller);
                case ActionType.Grab:
                    return GetGrab(controller);
                case ActionType.PrimaryButton:
                    return GetPrimaryButton(controller);
                case ActionType.SecondaryButton:
                    return GetSecondaryButton(controller);
                case ActionType.JoystickButton:
                    return GetJoystickButton(controller);
                default:
                    break;
            }
            return false;
        }

        #region Trigger 

        public abstract bool GetTriggerDown(ControllerType controller);

        public abstract bool GetTrigger(ControllerType controller);

        public abstract bool GetTriggerUp(ControllerType controller);

        #endregion

        #region Grab

        public abstract bool GetGrabDown(ControllerType controller);

        public abstract bool GetGrab(ControllerType controller);

        public abstract bool GetGrabUp(ControllerType controller);

        #endregion

        #region Primary Button

        public abstract bool GetPrimaryButtonDown(ControllerType controller);

        public abstract bool GetPrimaryButton(ControllerType controller);

        public abstract bool GetPrimaryButtonUp(ControllerType controller);

        #endregion

        #region Secondary Button

        public abstract bool GetSecondaryButtonDown(ControllerType controller);

        public abstract bool GetSecondaryButton(ControllerType controller);

        public abstract bool GetSecondaryButtonUp(ControllerType controller);

        #endregion

        #region Joystick Button

        public abstract bool GetJoystickButtonDown(ControllerType controller);

        public abstract bool GetJoystickButton(ControllerType controller);

        public abstract bool GetJoystickButtonUp(ControllerType controller);

        #endregion

        #region Teleport Button

        public abstract bool GetTeleportDown(ControllerType controller);

        public abstract bool GetTeleportUp(ControllerType controller);

        public abstract bool GetRightSnapTurn(ControllerType controller);
        public abstract bool GetLeftSnapTurn(ControllerType controller);

        #endregion


        /// <summary>
        /// Make a controller vibrate.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="vibrationDuration">in seconds</param>
        /// <param name="vibrationFrequency"></param>
        /// <param name="vibrationAmplitude"></param>
        public abstract void VibrateController(ControllerType controller, float vibrationDuration, float vibrationFrequency, float vibrationAmplitude);


        #region Unity Callback

        protected override void Awake()
        {
            base.Awake();

            foreach (ControllerType ctrl in Enum.GetValues(typeof(ControllerType)))
            {
                isJoystick.Add(ctrl, false);
                isJoystickDown.Add(ctrl, false);
                isJoystickUp.Add(ctrl, false);
            }
        }

        private void Update()
        {
            foreach (ControllerType ctrl in Enum.GetValues(typeof(ControllerType)))
            {
                Vector2 axis = GetJoystickAxis(ctrl);

                if (axis.magnitude > .1f)
                {
                    if (!isJoystick[ctrl])
                    {
                        isJoystickDown[ctrl] = true;
                    }
                    else
                    {
                        isJoystickDown[ctrl] = false;
                    }

                    isJoystick[ctrl] = true;
                    isJoystickUp[ctrl] = false;
                }
                else
                {
                    if (isJoystick[ctrl])
                    {
                        isJoystick[ctrl] = false;
                        isJoystickUp[ctrl] = true;
                    }
                    else
                    {
                        isJoystickUp[ctrl] = false;
                    }

                    isJoystickDown[ctrl] = false;
                }
            }
        }

        #endregion

        #region Joystick

        protected Dictionary<ControllerType, bool> isJoystick = new Dictionary<ControllerType, bool>();

        protected Dictionary<ControllerType, bool> isJoystickDown = new Dictionary<ControllerType, bool>();

        protected Dictionary<ControllerType, bool> isJoystickUp = new Dictionary<ControllerType, bool>();

        /// <summary>
        /// Returns current axis of <paramref name="controller"/> joystick.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public abstract Vector2 GetJoystickAxis(ControllerType controller);

        /// <summary>
        /// Returns true if <paramref name="controller"/> joystick starts being used.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public bool GetJoystickDown(ControllerType controller)
        {
            if (isJoystickDown.ContainsKey(controller))
                return isJoystickDown[controller];

            Debug.LogError("Internal error, unkown controller " + controller);
            return false;
        }

        /// <summary>
        /// Returns true if <paramref name="controller"/> joystick is being used.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public bool GetJoystick(ControllerType controller)
        {
            if (isJoystick.ContainsKey(controller))
                return isJoystickDown[controller];

            Debug.LogError("Internal error, unkown controller " + controller);
            return false;
        }

        /// <summary>
        /// Returns true if <paramref name="controller"/> joystick is being used.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public bool GetJoystickUp(ControllerType controller)
        {
            if (isJoystickUp.ContainsKey(controller))
                return isJoystickUp[controller];

            Debug.LogError("Internal error, unkown controller " + controller);
            return false;
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Lists all type of controllers.
    /// </summary>
    public enum ControllerType
    {
        LeftHandController,
        RightHandController
    }

    /// <summary>
    /// Lists all types of action.
    /// </summary>
    public enum ActionType
    {
        Trigger,
        Grab,
        PrimaryButton,
        SecondaryButton,
        JoystickButton,
        Teleport,
        RightSnapTurn,
        LeftSnapTurn
    }

    /// <summary>
    /// Lists of input uses.
    /// </summary>
    public enum InputEventType
    {
        Down,
        Up,
        Hold
    }
}