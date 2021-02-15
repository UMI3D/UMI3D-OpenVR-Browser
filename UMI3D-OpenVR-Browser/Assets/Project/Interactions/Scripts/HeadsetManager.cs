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

using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using Valve.VR;

/// <summary>
/// This class manages controllers displays and interactions depending on the HMD used (Rift, Rift S, HTC Vive, etc).
/// </summary>
public class HeadsetManager : Singleton<HeadsetManager>
{
    public enum HeadsetType
    {
        OculusRift,
        OculusRiftS,
        HtcVive,
        Default
    }

    [System.Serializable]
    public class HeadsetControllerParameter
    {
        public HeadsetType type;
        public GameObject[] controllerModels = new GameObject[2];

        /// <summary>
        /// (local value).
        /// </summary>
        public Vector3 leftHandMenuOffset;

        /// <summary>
        /// (local value).
        /// </summary>
        public Vector3 rightHandMenuOffset;
    }

    #region Fields

    /// <summary>
    /// Transform which contains the left menu.
    /// </summary>
    public Transform leftHandAnchor;

    /// <summary>
    /// Transform which contains the right menu.
    /// </summary>
    public Transform rightHandAnchor;

    /// <summary>
    /// List of parameters for each headset supported.
    /// </summary>
    public List<HeadsetControllerParameter> headsetParameters;

    /// <summary>
    /// Returns the type of headset used to run this application.
    /// </summary>
    public HeadsetType CurrentHeadSetType
    {
        get;
        private set;
    } = HeadsetType.Default;

    /// <summary>
    /// Two controllers.
    /// </summary>
    public List<OpenVRController> controllers;

    #endregion

    #region Methods

    void Start()
    {
        SetUpController();
    }

    /// <summary>
    /// Checks which headset is used to display the right controllers.
    /// </summary>
    void SetUpController()
    {
        SteamVR manager = SteamVR.instance;

        string deviceName = (SteamVR.instance.hmd_TrackingSystemName + " " + SteamVR.instance.hmd_ModelNumber).ToLower();

        Debug.Log("<color=cyan>This app is running on " + deviceName + "</color>");

        switch (deviceName)
        {
            case "oculus quest":
                CurrentHeadSetType = HeadsetType.Default;
                break;
            default:
                CurrentHeadSetType = HeadsetType.Default;
                break;
        }
        CurrentHeadSetType = HeadsetType.HtcVive;
        DisplayControllers();
    }

    /// <summary>
    /// Once the CurrentHeadseType is st, display the controller models according to its value.
    /// </summary>
    void DisplayControllers()
    {
        foreach(var param in headsetParameters)
        {
            if (param.type == CurrentHeadSetType)
            {
                leftHandAnchor.localPosition = param.leftHandMenuOffset;
                rightHandAnchor.localPosition = param.rightHandMenuOffset;
                EnableGameObjects(true, param.controllerModels);
            } else
            {
                EnableGameObjects(false, param.controllerModels);
            }
        }

        //HTC Vive has only 2 action buttons.
        if (CurrentHeadSetType == HeadsetType.HtcVive)
        {
            foreach (var controller in controllers)
                controller.booleanInputs = controller.booleanInputs.GetRange(0, 2);
        }
    }

    /// <summary>
    /// Displays or hides a list of gameobjects.
    /// </summary>
    /// <param name="enable"></param>
    /// <param name="objs"></param>
    void EnableGameObjects(bool enable, params GameObject[] objs)
    {
        foreach (var o in objs)
            o.SetActive(enable);
    }

    #endregion
}

