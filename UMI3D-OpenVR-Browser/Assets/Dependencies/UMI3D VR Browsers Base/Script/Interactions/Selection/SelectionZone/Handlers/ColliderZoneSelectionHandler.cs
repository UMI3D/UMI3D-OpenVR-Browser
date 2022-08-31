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
using UnityEngine;

namespace umi3dBrowsers.interaction.selection.zoneselection
{
    /// <summary>
    /// Handler for collider detection. Manages triggers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ColliderZoneSelectionHandler<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// Associated collider defining the selection zone
        /// </summary>
        [Tooltip("Collider defining the selection zone")]
        public Collider zoneCollider;

        /// <summary>
        /// Objects that are currently in the collider.
        /// </summary>
        public List<ObjectInsideCollider<T>> ObjectsInCollider { get; } = new List<ObjectInsideCollider<T>>();

        protected void Awake()
        {
            zoneCollider = GetComponentInChildren<Collider>();
        }

        protected void FixedUpdate()
        {
            foreach (ObjectInsideCollider<T> obj in ObjectsInCollider)
            {
                if ((obj.Equals(null)) || (obj.collider == null) || (obj.obj == null))
                    ObjectsInCollider.Remove(obj);
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            T neighbour = other.GetComponent<T>();
            if (neighbour == null)
                neighbour = other.GetComponentInParent<T>();

            if (neighbour != null && neighbour.isActiveAndEnabled)
            {
                ObjectsInCollider.Add(new ObjectInsideCollider<T>()
                {
                    obj = neighbour,
                    collider = other
                });
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            T neighbour = other.GetComponent<T>();
            if (neighbour == null)
                neighbour = other.GetComponentInParent<T>();

            if (neighbour != null)
                ObjectsInCollider.RemoveAll(n => n.collider.Equals(other));
        }
    }
}