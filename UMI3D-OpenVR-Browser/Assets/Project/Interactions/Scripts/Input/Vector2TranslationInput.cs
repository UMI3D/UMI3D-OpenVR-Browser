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
public class Vector2TranslationInput : AbstractCursorBasedManipulationInput
{
    protected override ManipulationRequestDto ComputeManipulationArgument(DofGroupEnum dofs)
    {
        Vector3 normal = (dofs == DofGroupEnum.XY) ? new Vector3(0, 0, 1) : ((dofs == DofGroupEnum.YZ) ? new Vector3(1, 0, 0) : new Vector3(0, 1, 0));
        Vector3 distanceInWorld = cursor.position - cursorPositionOnActivation;
        Vector3 distanceInFrame = frameOfReference.InverseTransformDirection(Vector3.ProjectOnPlane(distanceInWorld, frameOfReference.TransformDirection(normal)));

        return new ManipulationRequestDto()
        {
            translation = distanceInFrame * strength / networkFrameRate
        };
    }

    public override bool IsCompatibleWith(DofGroupEnum dofs)
    {
        return (dofs == DofGroupEnum.XY) || (dofs == DofGroupEnum.XZ) || (dofs == DofGroupEnum.YZ);
    }

}
