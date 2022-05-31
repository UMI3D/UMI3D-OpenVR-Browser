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

using umi3d.cdk;
using umi3d.common;
using UnityEngine;

namespace umi3dVRBrowsersBase.navigation
{
    /// <summary>
    /// This class handles the generation of the navemesh.
    /// </summary>
    public class NavmeshManager : MonoBehaviour
    {
        #region Fields

        /// <summary>
        ///LayerMask name used by the navigation system.
        /// </summary>

        public string navmeshLayerName = "Navmesh";

        private LayerMask layer;

        #endregion

        #region Methods

        private void Start()
        {
            layer = LayerMask.NameToLayer(navmeshLayerName);
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(InitNavMesh);
        }

        /// <summary>
        /// Once the environment is loaded, generates the navmesh.
        /// </summary>
        private void InitNavMesh()
        {
            foreach (UMI3DEntityInstance entity in UMI3DEnvironmentLoader.Entities())
            {
                if (entity is UMI3DNodeInstance nodeInstance)
                {
                    UMI3DDto dto = (nodeInstance.dto as GlTFNodeDto)?.extensions.umi3d;

                    if (dto is UMI3DMeshNodeDto && !(dto is SubModelDto)) //subModels will be initialized with their associated UMI3DModel.
                        InitModel(nodeInstance);
                }
            }
        }

        /// <summary>
        /// Inits navmesh according to the data stored by nodeInstance and its children.
        /// </summary>
        /// <param name="nodeInstance"></param>
        private void InitModel(UMI3DNodeInstance nodeInstance)
        {
            var meshNodeDto = (nodeInstance.dto as GlTFNodeDto)?.extensions.umi3d as UMI3DMeshNodeDto;

            if (meshNodeDto != null)
                SetUpGameObject(nodeInstance, meshNodeDto);
            else
            {
                var subModelDto = (nodeInstance.dto as GlTFNodeDto)?.extensions.umi3d as SubModelDto;
                if (subModelDto != null)
                    SetUpGameObject(nodeInstance, subModelDto);
            }

            foreach (UMI3DNodeInstance child in nodeInstance.subNodeInstances)
            {
                InitModel(child);
            }
        }

        /// <summary>
        /// Sets up associated <see cref="GameObject"/> of <paramref name="nodeInstance"/> according to its navmesh properties.
        /// </summary>
        /// <param name="nodeInstance"></param>
        /// <param name="meshDto"></param>
        private void SetUpGameObject(UMI3DNodeInstance nodeInstance, UMI3DMeshNodeDto meshDto)
        {
            SetUpGameObject(nodeInstance, meshDto.isPartOfNavmesh, meshDto.isTraversable);
        }

        /// <summary>
        /// Sets up associated <see cref="GameObject"/> of <paramref name="nodeInstance"/> according to its navmesh properties.
        /// </summary>
        /// <param name="nodeInstance"></param>
        /// <param name="subModelDto"></param>
        private void SetUpGameObject(UMI3DNodeInstance nodeInstance, SubModelDto subModelDto)
        {
            SetUpGameObject(nodeInstance, subModelDto.isPartOfNavmesh, subModelDto.isTraversable);
        }

        /// <summary>
        /// If a gameobject is part of the navmesh or not traversable, sets it up.
        /// </summary>
        /// <param name="nodeInstance"></param>
        /// <param name="isPartOfNavMesh"></param>
        /// <param name="isTraversable"></param>
        private void SetUpGameObject(UMI3DNodeInstance nodeInstance, bool isPartOfNavmesh, bool isTraversable)
        {
            if (isPartOfNavmesh || !isTraversable)
            {
                Collider collider;
                foreach (Renderer r in nodeInstance.renderers)
                {
                    if (!r.TryGetComponent(out collider))
                    {
                        MeshFilter filter;
                        if (r.TryGetComponent<MeshFilter>(out filter))
                        {
                            if (filter.sharedMesh != null && filter.sharedMesh.isReadable)
                                AddCollider(r.gameObject);
                            else
                                Debug.LogWarning(nodeInstance.gameObject.name + " can't be used for the navemesh or to limit it because its mesh is not readable.");
                        }
                        else
                        {
                            AddCollider(r.gameObject);
                        }
                    }
                    else
                    {
                        r.gameObject.layer = layer;
                    }
                    if (isPartOfNavmesh)
                    {
                        r.gameObject.AddComponent<TeleportArea>();
                    }

                }
            }
        }

        /// <summary>
        /// Adds a collider to a gameobject and changes its layer.
        /// </summary>
        /// <param name="obj"></param>
        private void AddCollider(GameObject obj)
        {
            obj.AddComponent<MeshCollider>();
            obj.layer = layer;
        }

        #endregion
    }
}