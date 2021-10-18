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
using umi3d.cdk;
using umi3d.common;
using UnityEngine;
using umi3d.cdk.volumes;
using System.Linq;
using System.Collections.Generic;

namespace BrowserQuest.Navigation
{
    /// <summary>
    /// This class handles the generation of the navemesh.
    /// </summary>
    public class NavmeshManager : Singleton<NavmeshManager>
    {
        /// <summary>
        ///LayerMask name used by the navigation system.
        /// </summary>
       
        public string navmeshLayerName = "Navmesh";

        public float slopeAngleLimit = 50;

        public Material tpAreaHighlight;
        public Material tpAreaDefault;

        private LayerMask layer;

        // Start is called before the first frame update
        void Start()
        {
            layer = LayerMask.NameToLayer(navmeshLayerName);
            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(InitNavMesh);
        }


        /// <summary>
        /// Once the environment is loaded, generates the navmesh.
        /// </summary>
        private void InitNavMesh()
        {
            foreach (var entity in UMI3DEnvironmentLoader.Entities())
            {
                if (entity is UMI3DNodeInstance nodeInstance)
                {
                    UMI3DDto dto = (nodeInstance.dto as GlTFNodeDto)?.extensions.umi3d;

                    if (dto is UMI3DMeshNodeDto && !(dto is SubModelDto)) //subModels will be initialized with their associated UMI3DModel.
                        InitModel(nodeInstance);
                }
            }
            List<AbstractVolumeCell> cells = VolumePrimitiveManager.GetPrimitives().ToList<AbstractVolumeCell>();
            cells.AddRange(ExternalVolumeDataManager.GetCells());
            foreach (AbstractVolumeCell cell in cells)
            {
                if (cell.isTraversable)
                {
                    cell.GetBase(mesh =>
                    {
                        GameObject navmeshPart = new GameObject("TPArea for " + cell.ToString());
                        navmeshPart.AddComponent<MeshFilter>().mesh = mesh;
                        navmeshPart.AddComponent<MeshCollider>();
                        navmeshPart.AddComponent<TeleportArea>();
                        navmeshPart.AddComponent<MeshRenderer>().material = tpAreaDefault;
                        navmeshPart.layer = layer;
                    }, slopeAngleLimit); 
                }
                else
                {
                    GameObject obstacle = new GameObject("obstacle for " + cell.GetType());
                    obstacle.transform.position = Vector3.zero;
                    obstacle.transform.rotation = Quaternion.identity;
                    obstacle.transform.localScale = Vector3.one;
                    obstacle.AddComponent<MeshFilter>().mesh = cell.GetMesh();
                    obstacle.AddComponent<MeshCollider>();
                    obstacle.AddComponent<TeleportObstacle>();
                    obstacle.layer = layer;
                }
            }
        }

        

        /// <summary>
        /// Inits navmesh according to the data stored by nodeInstance and its children.
        /// </summary>
        /// <param name="nodeInstance"></param>
        void InitModel(UMI3DNodeInstance nodeInstance)
        {
            UMI3DMeshNodeDto meshNodeDto = (nodeInstance.dto as GlTFNodeDto)?.extensions.umi3d as UMI3DMeshNodeDto;

            if (meshNodeDto != null)
                SetUpGameObject(nodeInstance, meshNodeDto);
            else
            {
                SubModelDto subModelDto = (nodeInstance.dto as GlTFNodeDto)?.extensions.umi3d as SubModelDto;
                if (subModelDto != null)
                    SetUpGameObject(nodeInstance, subModelDto);
            }

            foreach (var child in nodeInstance.subNodeInstances)
            {
                InitModel(child);
            }
        }


        private void SetUpGameObject(UMI3DNodeInstance nodeInstance, UMI3DMeshNodeDto meshDto)
        {
            SetUpGameObject(nodeInstance, meshDto.isPartOfNavmesh, meshDto.isTraversable);
        }

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
                        } else
                        {
                            AddCollider(r.gameObject);
                        }
                    } else
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
    }
}