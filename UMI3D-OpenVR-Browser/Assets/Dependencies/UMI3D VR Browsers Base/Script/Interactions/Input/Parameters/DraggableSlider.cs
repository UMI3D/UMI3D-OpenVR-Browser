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
using umi3dVRBrowsersBase.selection;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.interactions.input
{
    /// <summary>
    /// Turns a <see cref="Slider"/> draggable by a <see cref="VRDragAndDropSelector"/>.
    /// </summary>
    public class DraggableSlider : MonoBehaviour, IDraggableElement
    {
        #region Fields

        [SerializeField]
        [Tooltip("Slider controller by this handle")]
        Slider slider;

        [SerializeField]
        [Tooltip("Element which contains the handle, normally direct parent")]
        RectTransform handleContainer;

        /// <summary>
        /// World position of <see cref="handleContainer"/> 's top left corner.
        /// </summary>
        private Vector3 topLeftCorner;

        /// <summary>
        /// World position of <see cref="handleContainer"/> 's top right corner.
        /// </summary>
        private Vector3 topRightCorner;

        /// <summary>
        /// Where the drag and drop started
        /// </summary>
        private Vector3 lastPosition = Vector3.zero;

        /// <summary>
        /// Is <see cref="lastPosition"/> sets ?
        /// </summary>
        private bool isSetUp = false;

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNormal()
        {
            return transform.forward;
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
            if (!isSetUp)
            {
                lastPosition = position;
                isSetUp = true;
            }
            else
            {
                Vector3 sliderDirection = (topRightCorner - topLeftCorner);
                Vector3 offset = (position - lastPosition) / sliderDirection.magnitude;

                float delta = offset.magnitude * (Vector3.Dot(offset, sliderDirection) >= 0 ? 1 : -1);

                lastPosition = position;

                slider.value = Mathf.Clamp(slider.value + (slider.maxValue - slider.minValue) * delta, slider.minValue, slider.maxValue);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnDragStart()
        {
            Vector3[] corners = new Vector3[4];
            handleContainer.GetWorldCorners(corners);

            topLeftCorner = corners[1];
            topRightCorner = corners[2];

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnDragStop()
        {
            isSetUp = false;
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
            return DragAndDropType.Planar;
        }

        #endregion
    }
}