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

using System;
using umi3dVRBrowsersBase.ui;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.interactions.input
{
    /// <summary>
    /// Turns a <see cref="Scrollbar"/> draggable by a <see cref="VRDragAndDropSelector"/>.
    /// </summary>
    [RequireComponent(typeof(Scrollbar))]
    public class DraggableScrollbarElement : AbstractDraggableElement, IPressableElement
    {
        /// <summary>
        /// Scrollbar controller by this handle.
        /// </summary>
        private Scrollbar scrollbar;

        #region Methods

        protected override void Awake()
        {
            base.Awake();
            scrollbar = GetComponentInChildren<Scrollbar>();
        }

        /// <inheritdoc/>
        public override bool IsDraggingAllowed() => true;

        /// <inheritdoc/>
        public override void DragMove(Vector3 position, Transform selector)
        {
            try
            {
                scrollbar.value = Mathf.Clamp(Mathf.Sign(Vector3.Dot(position - bottomLeftCorner, topLeftCorner - bottomLeftCorner)) * (position - bottomLeftCorner).magnitude / (topLeftCorner - bottomLeftCorner).magnitude, 0, 1);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Impossible to set scrollbar value " + e.Message);
            }
        }

        /// <inheritdoc/>
        public override DragAndDropType GetDragType()
            => DragAndDropType.Planar;

        #endregion Methods
    }
}