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
/// Display a hint on a controller to show users they can use certain input to interact with the environment.
/// </summary>
public class ControllerHintDisplayer : Singleton<ControllerHintDisplayer>
{
    public Material baseGhostColorMaterial;

    [Space]

    public List<InputHintParameters> rightInputParameters;
    public List<InputHintParameters> leftInputParameters;


    void Start()
    {
        foreach (var inputParam in rightInputParameters)
            inputParam.ghostRenderer.gameObject.SetActive(false);

        foreach (var inputParam in leftInputParameters)
            inputParam.ghostRenderer.gameObject.SetActive(false);
    }

    /// <summary>
    /// Displays a ghost on top of the joystick button to show users they can use it.
    /// </summary>
    /// <param name="button"></param>
    /// <param name="controller"></param>
    /// <param name="associatedColor"></param>
    /// <param name="hint"></param>
    public static void DisplayHint(SteamVR_Action button, SteamVR_Input_Sources controller, Material highlightMat, string hint = "")
    {
        if (Exists)
        {
            InputHintParameters inputParam = Instance.FindAssociatedInputHintParamaters(button, controller);

            if (inputParam != null)
            {
                inputParam.ghostRenderer.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("<color=orange>No data found for button : " +  button + "</color> ");
            }
        }
    }

    public static void DisplayHint(OpenVRInputObserver observer)
    {
        Debug.Log("<color=orange>TODO</color>");
        DisplayHint(observer.button, observer.controller, new Material(Shader.Find("Standard")));
    }

    /// <summary>
    /// Hides the ghost displayed before to show users the button is no longer usable.
    /// </summary>
    /// <param name="button"></param>
    /// <param name="controller"></param>
    public static void HideHint(SteamVR_Action button, SteamVR_Input_Sources controller)
    {
        if (Exists)
        {
            InputHintParameters inputParam = Instance.FindAssociatedInputHintParamaters(button, controller);

            if (inputParam != null)
            {
                inputParam.ghostRenderer.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("No button found.");
            }
        }
    }

    public static void HideHint(OpenVRInputObserver observer)
    {
        HideHint(observer.button, observer.controller);
    }

    private InputHintParameters FindAssociatedInputHintParamaters(SteamVR_Action button, SteamVR_Input_Sources controller)
    {
        InputHintParameters inputParam = null;
        switch (controller)
        {

            case SteamVR_Input_Sources.LeftHand:
                inputParam = Instance.leftInputParameters.Find(i => i.button == button);
                break;
            case SteamVR_Input_Sources.RightHand:
                inputParam = Instance.rightInputParameters.Find(i => i.button == button);
                break;
            default:
                throw new System.ArgumentException("This controller " + controller + " is not supported by the controller hint displayer");
        }
        return inputParam;
    }


    [System.Serializable]
    public class InputHintParameters{

        public SteamVR_Action_Boolean button;

        [Tooltip("Renderer which represents the input")]
        public MeshRenderer ghostRenderer;

        [HideInInspector]
        public Material[] defaultMaterials;
    }
}
