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

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace umi3dBrowsers.interaction.selection.zoneselection
{
    /// <summary>
    /// Zone selection based on a collider. Requires an handler to manager the collider gameobject.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ColliderSelectionZone<T> : AbstractSelectionZone<T> where T : Component
    {
        /// <summary>
        /// Handles the collider component in the scene
        /// </summary>
        private ColliderZoneSelectionHandler<T> handler;

        /// <summary>
        /// Returns a list of objects presents inside the collider and the associated info
        /// </summary>
        private List<ObjectInsideCollider<T>> ObjectsInsideCollider { get => handler.ObjectsInCollider; }

        public ColliderSelectionZone(ColliderZoneSelectionHandler<T> zoneColliderHandler) : base()
        {
            this.handler = zoneColliderHandler;
        }

        /// <inheritdoc/>
        public override List<T> GetObjectsInZone()
        {
            return ObjectsInsideCollider.Select(o => o.obj).ToList();
        }

        /// <inheritdoc/>
        public override bool IsObjectInZone(T obj)
        {
            return ObjectsInsideCollider.Select(o => o.obj).Contains(obj);
        }

        /// <inheritdoc/>
        public override T GetClosestInZone()
        {
            var objectsInZone = ObjectsInsideCollider;
            if (objectsInZone.Count == 0) //Zone is empty
                return null;

            ObjectInsideCollider<T> closestObj = objectsInZone[0];
            float minDist = float.MaxValue;

            foreach (ObjectInsideCollider<T> obj in objectsInZone) //just looking for the closest one
            {
                float distance = Vector3.Distance(handler.transform.position, obj.collider.ClosestPoint(handler.transform.position));
                if (distance < minDist)
                {
                    minDist = distance;
                    closestObj = obj;
                }
            }

            return closestObj.obj;
        }
    }

    /// <summary>
    /// Wrapper for objects an their detected collider
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ObjectInsideCollider<T> where T : Component
    {
        public T obj;
        public Collider collider;
    }
}