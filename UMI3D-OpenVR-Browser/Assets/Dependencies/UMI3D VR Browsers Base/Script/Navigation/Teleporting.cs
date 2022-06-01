/*
Copyright 2019 - 2022 Inetum

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


namespace umi3dVRBrowsersBase.navigation
{
    /// <summary>
    /// Teleports users where they want. 
    /// </summary>
    public class Teleporting : MonoBehaviour
    {
        /// <summary>
        /// Player object.
        /// </summary>
        public GameObject teleportingObject;

        /// <summary>
        /// Player camera object.
        /// </summary>
        public GameObject centerEyeAnchor;

        /// <summary>
        /// Teleportation preview.
        /// </summary>
        public TeleportArc arc;

        /// <summary>
        /// Teleports player.
        /// </summary>
        [ContextMenu("Teleport")]
        public void Teleport()
        {
            Vector3? position = arc.GetPointedPoint();

            if (position.HasValue)
            {
                Vector3 offset = teleportingObject.transform.rotation * centerEyeAnchor.transform.localPosition;
                teleportingObject.transform.position = new Vector3(position.Value.x - offset.x,
                                                                   position.Value.y,
                                                                   position.Value.z - offset.z);
            }
        }

    }
}
