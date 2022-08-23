﻿/*
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

using umi3d.cdk.interaction;
using umi3dBrowsers.interaction.selection.intentdetector;
using umi3dBrowsers.interaction.selection.intentdetector.method;
using umi3dBrowsers.interaction.selection.zoneselection;
using UnityEngine;

namespace umi3dVRBrowsersBase.interactions.selection.intentdetector
{
    /// <summary>
    /// Detector for proximity selection with a collider
    /// </summary>
    public class ColliderProximityInteractableDetector : AbstractGrabInteractableDetector
    {
        [SerializeField, Tooltip("Handler for proximity selection.")]
        protected InteractableColliderZoneSelectionHandler proximityColliderHandler;

        /// <inheritdoc/>
        protected override void SetDetectionMethod()
        {
            detectionMethod = new ColliderProximityDetectionMethod<InteractableContainer>(proximityColliderHandler);
        }
    }
}