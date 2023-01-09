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

using System.Collections.Generic;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.cdk.volumes;
using umi3d.common;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.navigation
{
    /// <summary>
    /// This class handles the generation of the navemesh.
    /// </summary>
    public class NavmeshManager : MonoBehaviour
    {
        #region Fields

        [Tooltip("Default layer for traversable objets")]
        public LayerMask defaultLayer;

        [Tooltip("Layer for objects part of the navmesh")]
        public LayerMask navmeshLayer;

        /// <summary>
        /// Association between volume id and gameobject.
        /// </summary>
        private Dictionary<ulong, GameObject> cellIdToGameobjects = new Dictionary<ulong, GameObject>();

        #endregion

        #region Methods

        private void Start()
        {
            UMI3DEnvironmentLoader.Instance.onNodePartOfNavmeshSet += SetPartOfNavmesh;
            UMI3DEnvironmentLoader.Instance.onNodeTraversableSet += SetTraversable;

            UMI3DCollaborationClientServer.Instance.OnLeaving.AddListener(Reset);
            VolumePrimitiveManager.SubscribeToPrimitiveCreation(OnPrimitiveCreated, false);
            VolumePrimitiveManager.SubscribeToPrimitiveDelete(OnPrimitiveDeleted);
        }

        /// <summary>
        /// Sets <paramref name="node"/> as part of the navmesh.
        /// </summary>
        /// <param name="node"></param>
        private void SetPartOfNavmesh(UMI3DNodeInstance node)
        {
            if (node.IsPartOfNavmesh)
            {
                if (node.gameObject.GetComponent<Collider>() != null)
                {
                    AddTeleportArea(node.gameObject);
                }

                foreach (var renderer in node.renderers)
                {
                    if (renderer.TryGetComponent<Collider>(out Collider c))
                    {
                        AddTeleportArea(renderer.gameObject);
                    }
                    else
                    {
                        Debug.LogWarning(renderer.gameObject.name + " is set as part of the navmesh but it does not have colliders");
                    }
                }
            }
            else if (node.IsTraversable)
            {
                SetDefaultSettings(node);
            } else
            {
                DeleteTeleportArea(node);
            }
        }

        /// <summary>
        /// Sets colliders and layers according to <see cref="node.IsTraversable"/>
        /// </summary>
        /// <param name="node"></param>
        private void SetTraversable(UMI3DNodeInstance node)
        {
            if (node.IsPartOfNavmesh)
                return;
            if (node.IsTraversable)
            {
                SetDefaultSettings(node);
                return;
            } else
            {
                TeleportArea tpArea;

                if (node.gameObject.TryGetComponent<TeleportArea>(out tpArea))
                {
                    GameObject.Destroy(tpArea);
                }

                node.gameObject.layer = ToLayer(navmeshLayer);

                foreach (var renderer in node.renderers)
                {
                    if (renderer.gameObject.TryGetComponent<TeleportArea>(out tpArea))
                    {
                        GameObject.Destroy(tpArea);
                    }

                    renderer.gameObject.layer = ToLayer(navmeshLayer);
                }
            }

        }

        /// <summary>
        /// Adds a <see cref="TeleportArea"/> to <paramref name="go"/>.
        /// </summary>
        /// <param name="go"></param>
        private void AddTeleportArea(GameObject go)
        {
            if (go.GetComponent<TeleportArea>() == null)
            {
                go.AddComponent<TeleportArea>();
            }

            go.layer = ToLayer(navmeshLayer);
        }

        /// <summary>
        /// Sets <paramref name="node"/> to default layer and removes its <see cref="TeleportArea"/>.
        /// </summary>
        /// <param name="node"></param>
        private void SetDefaultSettings(UMI3DNodeInstance node)
        {
            SetLayer(node, defaultLayer);

            DeleteTeleportArea(node);
        }

        /// <summary>
        /// Sets a node and its renderers to layer <see cref="mask"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="mask"></param>
        private void SetLayer(UMI3DNodeInstance node, LayerMask mask)
        {
            if (node.gameObject.GetComponent<Collider>() != null)
            {
                node.gameObject.layer = ToLayer(navmeshLayer);
            }

            foreach (var renderer in node.renderers)
            {
                renderer.gameObject.layer = ToLayer(mask);
            }
        }

        /// <summary>
        /// Deletes all <see cref="TeleportArea"/> set to <paramref name="node"/> and its renderers.
        /// </summary>
        /// <param name="node"></param>
        private void DeleteTeleportArea(UMI3DNodeInstance node)
        {
            TeleportArea tpArea;
            if (node.gameObject.TryGetComponent<TeleportArea>(out tpArea))
            {
                GameObject.Destroy(tpArea);
            }

            foreach (var renderer in node.renderers)
            {
                if (renderer.gameObject.TryGetComponent<TeleportArea>(out tpArea))
                {
                    GameObject.Destroy(tpArea);
                }
            }
        }

        /// <summary> Converts given bitmask to layer number </summary>
        /// <returns> layer number </returns>
        public static int ToLayer(int bitmask)
        {
            int result = bitmask > 0 ? 0 : 31;
            while (bitmask > 1)
            {
                bitmask = bitmask >> 1;
                result++;
            }
            return result;
        }

        #region Volume

        /// <summary>
        /// Creates an obstacle when a new primitive non traversable is created.
        /// </summary>
        /// <param name="primitive"></param>
        private void OnPrimitiveCreated(AbstractVolumeCell primitive)
        {
            if (primitive.isTraversable)
                return;

            GameObject go = new GameObject("Obstacle-Volume-" + primitive.Id());
            go.transform.SetParent((primitive as AbstractPrimitive)?.rootNode?.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            switch (primitive)
            {
                case Box box:
                    BoxCollider boxCollider = go.AddComponent<BoxCollider>();
                    boxCollider.center = box.bounds.center;
                    boxCollider.size = box.bounds.size;
                    break;
                case Cylinder cylinder:
                    CapsuleCollider capsuleCollider = go.AddComponent<CapsuleCollider>();
                    capsuleCollider.height = cylinder.height;
                    capsuleCollider.radius = cylinder.radius;
                    break;
                default:
                    Destroy(go);
                    Debug.LogError("Primitive of type " + primitive?.GetType() + " not supported.");
                    break;
            }
        }

        /// <summary>
        /// If <paramref name="primitive"/> had a related obstacle, deletes it.
        /// </summary>
        /// <param name="primitive"></param>
        private void OnPrimitiveDeleted(AbstractVolumeCell primitive)
        {
            if (cellIdToGameobjects.ContainsKey(primitive.Id()))
            {
                GameObject go = cellIdToGameobjects[primitive.Id()];
                if (go != null)
                    Destroy(go);
                cellIdToGameobjects.Remove(primitive.Id());
            }
        }

        /// <summary>
        /// Resets navmesh data.
        /// </summary>
        private void Reset()
        {
            foreach (var primitive in cellIdToGameobjects)
            {
                if (primitive.Value != null)
                    Destroy(primitive.Value);
            }
            cellIdToGameobjects.Clear();
        }

        #endregion

        #endregion
    }
}