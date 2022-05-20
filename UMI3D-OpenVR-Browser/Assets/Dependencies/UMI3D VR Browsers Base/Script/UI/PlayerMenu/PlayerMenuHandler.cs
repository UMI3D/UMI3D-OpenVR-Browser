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

using System;
using umi3dVRBrowsersBase.selection;
using UnityEngine;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    /// <summary>
    /// Handler to enable users to move <see cref="PlayerMenuManager"/>.
    /// </summary>
    public class PlayerMenuHandler : MonoBehaviour, IDraggableElement
    {
        #region Methods

        [SerializeField]
        [Tooltip("Root of the player menu")]
        Transform playerMenu;

        /// <summary>
        /// Is drag and drop set up ?
        /// </summary>
        bool isDragAndDropSet;

        /// <summary>
        /// Offset between selector and draggable object at the begining.
        /// </summary>
        float startOffset;

        /// <summary>
        /// Player camera's transform;
        /// </summary>
        Transform playerCamera;

        /// <summary>
        /// Scale of <see cref="playerMenu"/> when it is displayed.
        /// </summary>
        Vector3 defaultScale;

        /// <summary>
        /// Scale of <see cref="playerMenu"/> when it is dragged by a user.
        /// </summary>
        [Tooltip("Size of the player menu when it is dragged by a user")]
        [SerializeField]
        Vector3 reducedScale = new Vector3(0.0006f, 0.0006f, 0.0006f);

        [Tooltip("Distance bewteen user and player menu when it is dragged")]
        [SerializeField]
        float distanceFromUserWhenDragged = .4f;

        #endregion

        #region Fields

        void Start()
        {
            playerCamera = PlayerMenuManager.Instance.PlayerCameraTransform;

            defaultScale = playerMenu.localScale;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNormal()
        {
            return playerMenu.forward;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPosition()
        {
            return transform.position;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public bool IsDraggingAllowed()
        {
            return true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="position"></param>
        public void OnDrag(Vector3 position, Transform selector)
        {
            if (!isDragAndDropSet)
            {
                isDragAndDropSet = true;
                startOffset = distanceFromUserWhenDragged;
            }
            else
            {
                playerMenu.position = selector.position + selector.forward * startOffset + (playerMenu.position - transform.position);

                playerMenu.LookAt(new Vector3(playerCamera.position.x, playerMenu.transform.position.y, playerCamera.transform.position.z));
                playerMenu.Rotate(0, 180, 0);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnDragStart()
        {
            playerMenu.localScale = reducedScale;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnDragStop()
        {
            playerMenu.localScale = defaultScale;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnDropFailCallback()
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="callback"></param>
        public void SetDestroyCallback(Action callback)
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public DragAndDropType GetDragType()
        {
            return DragAndDropType.Spatial;
        }

        #endregion
    }
}