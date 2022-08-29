﻿/*
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

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    /// <summary>
    /// Handler to enable users to move <see cref="PlayerMenuManager"/>.
    /// </summary>
    public class PlayerMenuHandler : AbstractDraggableElement
    {
        #region Methods

        [SerializeField]
        [Tooltip("Root of the player menu")]
        private Transform playerMenu;

        /// <summary>
        /// Is drag and drop set up ?
        /// </summary>
        private bool isDragAndDropSet;

        /// <summary>
        /// Offset between selector and draggable object at the begining.
        /// </summary>
        private float startOffset;

        /// <summary>
        /// Player camera's transform;
        /// </summary>
        private Transform playerCamera;

        /// <summary>
        /// Scale of <see cref="playerMenu"/> when it is displayed.
        /// </summary>
        private Vector3 defaultScale;

        /// <summary>
        /// Scale of <see cref="playerMenu"/> when it is dragged by a user.
        /// </summary>
        [Tooltip("Size of the player menu when it is dragged by a user")]
        [SerializeField]
        private Vector3 reducedScale = new Vector3(0.0006f, 0.0006f, 0.0006f);

        [Tooltip("Distance bewteen user and player menu when it is dragged")]
        [SerializeField]
        private float distanceFromUserWhenDragged = .4f;

        #endregion Methods

        #region Fields

        private void Start()
        {
            playerCamera = PlayerMenuManager.Instance.PlayerCameraTransform;

            defaultScale = playerMenu.localScale;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override bool IsDraggingAllowed() => true;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="position"></param>
        public override void DragMove(Vector3 position, Transform selector)
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
        public override void OnDragStart()
        {
            base.OnDragStart();
            playerMenu.localScale = reducedScale;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void OnDragStop()
        {
            base.OnDragStop();
            playerMenu.localScale = defaultScale;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override DragAndDropType GetDragType() => DragAndDropType.Spatial;

        #endregion Fields
    }
}