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
using Valve.VR;

public class OpenVRInteractableProximitySelector : InteractableProximitySelector
{
    public SteamVR_Input_Sources vrController;
    public SteamVR_Action_Boolean button;
    public PlayerMenuManager playerMenu;

    
    protected override void Update()
    {
        if (playerMenu.IsDisplayingToolParameters || (controller as OpenVRController).IsInputPressed)
            return;

        base.Update();
        if (button.GetState(vrController))
        {
            if (activated)
                Select();
        }
    }
}
