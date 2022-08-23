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

namespace umi3dBrowsers.interaction.selection.intentdetector.method
{
    using umi3d.cdk.interaction;
    using zoneselection;

    /// <summary>
    /// Implementation of a simple virtual hand method
    /// </summary>
    public class ColliderProximityDetectionMethod<T> : AbstractDetectionMethod<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Zone selection helper based on a collider used for the method.
        /// </summary>
        protected ColliderSelectionZone<T> zoneSelection;

        /// <summary>
        /// Collider handler used by the selection zone for the method.
        /// </summary>
        protected ColliderZoneSelectionHandler<T> proximityColliderHandler;

        public ColliderProximityDetectionMethod(ColliderZoneSelectionHandler<T> proximityColliderHandler) : base()
        {
            this.proximityColliderHandler = proximityColliderHandler;
        }

        /// <inheritdoc/>
        public override T PredictTarget()
        {
            return zoneSelection.GetClosestInZone();
        }

        /// <inheritdoc/>
        public override void Init(AbstractController controller)
        {
            base.Init(controller);
            zoneSelection = new ColliderSelectionZone<T>(proximityColliderHandler);
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();
            zoneSelection = new ColliderSelectionZone<T>(proximityColliderHandler);
        }
    }
}