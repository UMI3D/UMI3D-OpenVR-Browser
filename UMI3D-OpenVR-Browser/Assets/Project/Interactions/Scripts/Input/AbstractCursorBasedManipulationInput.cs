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
using UnityEngine;

[System.Serializable]
public abstract class AbstractCursorBasedManipulationInput : AbstractManipulationInput
{
    /// <summary>
    /// Transform used for offset calculation.
    /// </summary>
    /// <see cref="cursorPositionOnActivation"/>
    public Transform cursor;

    /// <summary>
    /// World position of <see cref="cursor"/> on activation.
    /// </summary>
    protected Vector3 cursorPositionOnActivation;

    /// <summary>
    /// World rotation of <see cref="cursor"/> on activation.
    /// </summary>
    protected Quaternion cursorRotationOnActivation;

    protected override void ActivationButton_onStateDown()
    {
        cursorPositionOnActivation = cursor.position;
        cursorRotationOnActivation = cursor.rotation;
        base.ActivationButton_onStateDown();
    }

    public override void UpdateHoveredObjectId(ulong hoveredObjectId)
    {
        throw new System.NotImplementedException();
    }
}
