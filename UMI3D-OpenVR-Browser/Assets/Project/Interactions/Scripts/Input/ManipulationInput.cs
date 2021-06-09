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
using umi3d.common.interaction;
using UnityEngine;

public class ManipulationInput : AbstractCursorBasedManipulationInput
{
    [SerializeField]
    List<DofGroupEnum> implementedDofs = new List<DofGroupEnum>();

    public override bool IsCompatibleWith(DofGroupEnum dofs)
    {
        return implementedDofs.Contains(dofs);
    }

    protected override ManipulationRequestDto ComputeManipulationArgument(DofGroupEnum dofs)
    {
        ManipulationRequestDto res = new ManipulationRequestDto();

        switch (dofs)
        {
            case DofGroupEnum.ALL:
                res.translation = GetTranslation3Axis();
                res.rotation = GetRotation3Axis();
                break;
            case DofGroupEnum.X:
            case DofGroupEnum.Y:
            case DofGroupEnum.Z:
                res.translation = GetTranslation1Axis(dofs);
                break;
            case DofGroupEnum.XY:
            case DofGroupEnum.XZ:
            case DofGroupEnum.YZ:
                res.translation = GetTranslation2Axis(dofs);
                break;
            case DofGroupEnum.XYZ:
                res.translation = GetTranslation3Axis();
                break;
            case DofGroupEnum.RX:
            case DofGroupEnum.RY:
            case DofGroupEnum.RZ:
                res.rotation = GetRotation1Axis(dofs);
                break;
            case DofGroupEnum.RX_RY:
            case DofGroupEnum.RX_RZ:
            case DofGroupEnum.RY_RZ:
                res.rotation = GetRotation2Axis(dofs);
                break;
            case DofGroupEnum.RX_RY_RZ:
                res.rotation = GetRotation3Axis();
                break;
            case DofGroupEnum.X_RX:
                res.translation = GetTranslation1Axis(DofGroupEnum.X);
                res.rotation = GetRotation1Axis(DofGroupEnum.RX);
                break;
            case DofGroupEnum.Y_RY:
                res.translation = GetTranslation1Axis(DofGroupEnum.Y);
                res.rotation = GetRotation1Axis(DofGroupEnum.RY);
                break;
            case DofGroupEnum.Z_RZ:
                res.translation = GetTranslation1Axis(DofGroupEnum.Z);
                res.rotation = GetRotation1Axis(DofGroupEnum.RZ);
                break;
            default:
                break;
        }

        return res;
    }

    private Vector3 GetTranslation1Axis(DofGroupEnum dofs)
    {
        Vector3 axis = (dofs == DofGroupEnum.Z) ? new Vector3(0, 0, 1) : ((dofs == DofGroupEnum.X) ? new Vector3(1, 0, 0) : new Vector3(0, 1, 0));
        Vector3 distanceInWorld = cursor.position - cursorPositionOnActivation;
        Vector3 distanceInFrame = frameOfReference.InverseTransformDirection(Vector3.Project(distanceInWorld, frameOfReference.TransformDirection(axis)));

        return distanceInFrame * translationStrenght;
    }

    private Vector3 GetTranslation2Axis(DofGroupEnum dofs)
    {
        Vector3 normal = (dofs == DofGroupEnum.XY) ? new Vector3(0, 0, 1) : ((dofs == DofGroupEnum.YZ) ? new Vector3(1, 0, 0) : new Vector3(0, 1, 0));
        Vector3 distanceInWorld = cursor.position - cursorPositionOnActivation;
        Vector3 distanceInFrame = frameOfReference.InverseTransformDirection(Vector3.ProjectOnPlane(distanceInWorld, frameOfReference.TransformDirection(normal)));

        return distanceInFrame * translationStrenght;
    }

    private Vector3 GetTranslation3Axis()
    {
        Vector3 distanceInWorld = cursor.position - cursorPositionOnActivation;
        Vector3 distanceInFrame = frameOfReference.InverseTransformDirection(distanceInWorld);

        return distanceInFrame * translationStrenght;
    }

    private Quaternion GetRotation1Axis(DofGroupEnum dofs)
    {
        Quaternion rotationInWorld = cursor.transform.rotation * Quaternion.Inverse(cursorRotationOnActivation);
        Vector3 rotationInWorldRemapped = new Vector3(
            (rotationInWorld.x > 180) ? rotationInWorld.x - 360 : rotationInWorld.x,
            (rotationInWorld.y > 180) ? rotationInWorld.y - 360 : rotationInWorld.y,
            (rotationInWorld.z > 180) ? rotationInWorld.z - 360 : rotationInWorld.z);
        Quaternion rotationInFrame = Quaternion.Euler(
            Vector3.Scale(frameOfReference.InverseTransformDirection(rotationInWorldRemapped),
                (dofs == DofGroupEnum.RX) ? new Vector3(1, 0, 0) :
                (dofs == DofGroupEnum.RY) ? new Vector3(0, 1, 0) :
                new Vector3(0, 0, 1))
            * rotationStrenght);

        return rotationInFrame;
    }

    private Quaternion GetRotation2Axis(DofGroupEnum dofs)
    {
        Quaternion rotationInWorld = cursor.transform.rotation * Quaternion.Inverse(cursorRotationOnActivation);
        Vector3 rotationInWorldRemapped = new Vector3(
            (rotationInWorld.x > 180) ? rotationInWorld.x - 360 : rotationInWorld.x,
            (rotationInWorld.y > 180) ? rotationInWorld.y - 360 : rotationInWorld.y,
            (rotationInWorld.z > 180) ? rotationInWorld.z - 360 : rotationInWorld.z);
        Quaternion rotationInFrame = Quaternion.Euler(
            Vector3.Scale(frameOfReference.InverseTransformDirection(rotationInWorldRemapped),
                (dofs == DofGroupEnum.RX_RY) ? new Vector3(1, 1, 0) :
                (dofs == DofGroupEnum.RX_RZ) ? new Vector3(1, 0, 1) :
                new Vector3(0, 1, 1))
            * rotationStrenght);

        return rotationInFrame;
    }

    private Quaternion GetRotation3Axis()
    {
        Quaternion rotationInWorld = cursor.transform.rotation * Quaternion.Inverse(cursorRotationOnActivation);
        Vector3 rotationInWorldRemapped = new Vector3(
            (rotationInWorld.x > 180) ? rotationInWorld.x - 360 : rotationInWorld.x,
            (rotationInWorld.y > 180) ? rotationInWorld.y - 360 : rotationInWorld.y,
            (rotationInWorld.z > 180) ? rotationInWorld.z - 360 : rotationInWorld.z);
        Quaternion rotationInFrame = Quaternion.Euler(frameOfReference.InverseTransformDirection(rotationInWorldRemapped) * rotationStrenght);

        return rotationInFrame;
    }
}
