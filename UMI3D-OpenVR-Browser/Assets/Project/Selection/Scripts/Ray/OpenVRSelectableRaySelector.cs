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

public class OpenVRSelectableRaySelector : SelectableRaySelector
{
    public SteamVR_Input_Sources vrController;
    public SteamVR_Action_Boolean button;

    protected override void ActivateInternal()
    {
        if (!activated)
        {
            base.ActivateInternal();
        }
    }

    protected override void DeactivateInternal()
    {
        if (activated)
        {
            base.DeactivateInternal();
        }
    }
   
    protected override void Update()
    {
        base.Update();
        if (button.GetStateDown(vrController))
        {
            if (activated)
                Select();
        }
    }
}
