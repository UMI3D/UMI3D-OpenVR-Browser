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
using umi3dVRBrowsersBase.ui;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.interactions.input
{
    /// <summary>
    /// Turns a <see cref="Scrollbar"/> draggable by a <see cref="VRDragAndDropSelector"/>.
    /// </summary>
    [RequireComponent(typeof(Scrollbar))]
    public class DraggableScrollbarElement : AbstractClientInteractableElement, IDraggableElement, IPressableElement
    {
        #region Fields

        /// <summary>
        /// Scrollbar controller by this handle.
        /// </summary>
        Scrollbar scrollbar;

        /// <summary>
        /// <see cref="RectTransform"/> associated to <see cref="scrollbar"/>.
        /// </summary>
        RectTransform scrollbarTransform;

        /// <summary>
        /// World position of <see cref="scrollbarTransform"/> 's top left corner.
        /// </summary>
        private Vector3 topLeftCorner;

        /// <summary>
        /// World position of <see cref="scrollbarTransform"/> 's bottom left corner.
        /// </summary>
        private Vector3 bottomLeftCorner;

        private UnityEvent onPressedDown = new UnityEvent();
        private UnityEvent onPressedUp = new UnityEvent();
        public UnityEvent OnPressedDown => onPressedDown;

        public UnityEvent OnPressedUp => onPressedUp;

        #endregion

        #region Methods

        private void Awake()
        {
            scrollbar = GetComponent<Scrollbar>();
            Debug.Assert(scrollbar != null);

            scrollbarTransform = GetComponent<RectTransform>();
            Debug.Assert(scrollbarTransform != null);
        }

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
            try
            {
                scrollbar.value = Mathf.Clamp(Mathf.Sign(Vector3.Dot(position - bottomLeftCorner, topLeftCorner - bottomLeftCorner)) * (position - bottomLeftCorner).magnitude / (topLeftCorner - bottomLeftCorner).magnitude, 0, 1);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Impossible to set scrollbar value " + e.Message);
            }
        }

        public override void Interact(VRController controller)
        {
            OnDragStart();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnDragStart()
        {
            Vector3[] corners = new Vector3[4];
            scrollbarTransform.GetWorldCorners(corners);

            bottomLeftCorner = corners[0];
            topLeftCorner = corners[1];
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnDragStop()
        {
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

        public void PressDown(ControllerType controller)
        {
            OnDragStart();
        }

        public void PressUp(ControllerType controller)
        {
            OnDragStop();
        }

        public bool IsPressed(ControllerType controller)
        {
            throw new NotImplementedException();
        }

        public override void Select(VRController controller)
        {
            
        }

        public override void Deselect(VRController controller)
        {
            
        }

        #endregion
    }
}