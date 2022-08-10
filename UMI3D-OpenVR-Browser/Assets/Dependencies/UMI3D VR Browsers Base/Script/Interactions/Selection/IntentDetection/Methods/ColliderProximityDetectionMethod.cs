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
        protected ColliderSelectionZone<T> zoneSelection;

        protected ColliderZoneSelectionHandler<T> proximityColliderHandler;

        protected bool hasHandlerLoaded = false;

        public ColliderProximityDetectionMethod(ColliderZoneSelectionHandler<T> proximityColliderHandler) : base()
        {
            this.proximityColliderHandler = proximityColliderHandler;
        }


        public override T PredictTarget()
        {
            return zoneSelection.GetClosestInZone();
        }

        public override void Init(AbstractController controller)
        {
            base.Init(controller);
            zoneSelection = new ColliderSelectionZone<T>(proximityColliderHandler);
        }

        public override void Reset()
        {
            base.Reset();
            zoneSelection = new ColliderSelectionZone<T>(proximityColliderHandler);
        }
    }
}