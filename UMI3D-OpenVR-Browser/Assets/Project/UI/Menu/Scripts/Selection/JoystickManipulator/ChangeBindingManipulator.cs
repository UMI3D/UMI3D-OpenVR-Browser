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

/*
public class ChangeBindingManipulator : AbstractJoystickManipulator
{
    [HideInInspector]
    public OculusController controller;

    [HideInInspector]
    public HoldableButtonMenuItem menuItem;

    public UnityEngine.UI.Text informationText;

    public override void Select()
    {
        if (!controller.WasInputSet)
        {
            informationText.text = "Press any button top assign it this action";
            controller.StartListeningToChangeBinding(menuItem, informationText, menuItem.associatedInput);
        }
            
        base.Select();
    }

    public override void UnSelect()
    {
        controller.StopListeningToChangeBinding();
        base.UnSelect();
    }
    public override void UpdateContent(JoystickSelector joystickSelector)
    {
        //Nothing to do.
    
}
}*/