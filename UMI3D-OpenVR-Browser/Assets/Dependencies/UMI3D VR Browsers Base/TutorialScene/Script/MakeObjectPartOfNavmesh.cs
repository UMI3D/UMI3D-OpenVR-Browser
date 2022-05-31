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


using umi3dVRBrowsersBase.navigation;
using UnityEngine;

namespace umi3dVRBrowsersBase.tutorial
{
    /// <summary>
    /// Sets an objet walkable or as an obstacle for a user.
    /// </summary>
    public class MakeObjectPartOfNavmesh : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Walkable or obstacle ?")]
        private NavMeshType type;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("Navmesh");

            if (type == NavMeshType.Walkable)
                gameObject.AddComponent<TeleportArea>();
        }
    }

    public enum NavMeshType { Walkable, Obstacle };
}