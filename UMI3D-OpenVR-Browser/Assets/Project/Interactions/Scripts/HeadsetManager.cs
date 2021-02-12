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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// This class manages controllers displays and interactions depending on the HMD used (Rift, Rift S, HTC Vive, etc).
/// </summary>
public class HeadsetManager : MonoBehaviour
{
    public enum HeadsetType
    {
        OculusQuest,
        OculusRift,
        OculusRiftS,
        HtcVive,
        Default
    }
    
    [System.Serializable]
    public class HeadsetControllerParameter {
        public HeadsetType type;
        public GameObject[] controllerModels = new GameObject[2];
    }

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

    void Start()
    {
        
        SetUpController();
    }

    void SetUpController()
    {
        SteamVR manager = SteamVR.instance;

        string deviceName = (SteamVR.instance.hmd_TrackingSystemName + " " + SteamVR.instance.hmd_ModelNumber).ToLower();

        Debug.Log("<color=cyan>This app is running on " + deviceName + "</color>");

        switch (deviceName)
        {
            case "oculus quest":
                CurrentHeadSetType = HeadsetType.OculusQuest;
                break;
            default:
                CurrentHeadSetType = HeadsetType.Default;
                break;
        }
    }
}


