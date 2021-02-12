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
using umi3d.common.interaction;
using UnityEngine;
using umi3d.cdk.interaction;

[System.Serializable]
public class Vector3TranslationInput : AbstractCursorBasedManipulationInput
{
    public override bool IsCompatibleWith(DofGroupEnum dofs)
    {
        return dofs == DofGroupEnum.XYZ;
    }

    protected override ManipulationRequestDto ComputeManipulationArgument(DofGroupEnum dofs)
    {
        Vector3 distanceInWorld = cursor.position - cursorPositionOnActivation;
        Vector3 distanceInFrame = frameOfReference.InverseTransformDirection(distanceInWorld);

        return new ManipulationRequestDto()
        {
            translation = distanceInFrame * strength / networkFrameRate
        };
    }
}
