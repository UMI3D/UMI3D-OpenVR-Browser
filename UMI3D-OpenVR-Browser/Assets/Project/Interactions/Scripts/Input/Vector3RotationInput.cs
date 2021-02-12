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
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;

[System.Serializable]
public class Vector3RotationInput : AbstractCursorBasedManipulationInput
{
    public override bool IsCompatibleWith(DofGroupEnum dofs)
    {
        return dofs == DofGroupEnum.RX_RY_RZ;
    }

    protected override ManipulationRequestDto ComputeManipulationArgument(DofGroupEnum dofs)
    {
        Quaternion rotationInWorld = cursor.transform.rotation * Quaternion.Inverse(cursorRotationOnActivation);
        Vector3 rotationInWorldRemapped = new Vector3(
            (rotationInWorld.x > 180) ? rotationInWorld.x - 360 : rotationInWorld.x,
            (rotationInWorld.y > 180) ? rotationInWorld.y - 360 : rotationInWorld.y,
            (rotationInWorld.z > 180) ? rotationInWorld.z - 360 : rotationInWorld.z);
        Quaternion rotationInFrame = Quaternion.Euler(frameOfReference.InverseTransformDirection(rotationInWorldRemapped) * strength / networkFrameRate);

        return new ManipulationRequestDto()
        {
            rotation = rotationInFrame
        };
    }
}