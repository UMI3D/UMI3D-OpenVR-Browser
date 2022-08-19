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

using umi3dVRBrowsersBase.selection;
using umi3dVRBrowsersBase.ui;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.interactions.input
{
    /// <summary>
    /// Turns a <see cref="Slider"/> draggable by a <see cref="VRDragAndDropSelector"/>.
    /// </summary>
    public class DraggableSliderElement : AbstractDraggableElement
    {
        #region Fields

        [SerializeField]
        [Tooltip("Slider controller by this handle")]
        private Slider slider;

        /// <summary>
        /// Where the drag and drop started
        /// </summary>
        private Vector3 lastPosition = Vector3.zero;

        /// <summary>
        /// Is <see cref="lastPosition"/> sets ?
        /// </summary>
        private bool isSetUp = false;

        #endregion Fields

        #region Methods

        protected override void Awake()
        {
            base.Awake();
            slider = GetComponentInChildren<Slider>();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="position"></param>
        public override void DragMove(Vector3 position, Transform selector)
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

        /// <inheritdoc/>
        public override bool IsDraggingAllowed() => true;

        /// <inheritdoc/>
        public override DragAndDropType GetDragType() => DragAndDropType.Planar;

        #endregion Methods
    }
}