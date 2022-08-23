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
using System.Linq;
using umi3dVRBrowsersBase.interactions;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.ui
{
    public abstract class AbstractDraggableElement : AbstractClientInteractableElement, IDraggableElement, IPressableElement
    {
        #region Fields

        private UnityEvent onPressedDown = new UnityEvent();
        private UnityEvent onPressedUp = new UnityEvent();

        /// <summary>
        /// True if the object is in the pressed state.
        /// </summary>
        private bool isPressed;

        /// <inheritdoc/>
        public UnityEvent OnPressedDown => onPressedDown;
        /// <inheritdoc/>
        public UnityEvent OnPressedUp => onPressedUp;

        /// <summary>
        /// Collider of the object. Only used for raycast check.
        /// </summary>
        protected Collider colliderRaycast;

        /// <summary>
        /// Contains the handle, normally a child component.
        /// </summary>
        [SerializeField]
        [Tooltip("Contains the handle, normally a child component.")]
        protected RectTransform referenceRect;

        /// <summary>
        /// World position of <see cref="scrollbarTransform"/> 's top left corner.
        /// </summary>
        protected Vector3 topLeftCorner;

        /// <summary>
        /// World position of <see cref="handleContainer"/> 's top right corner.
        /// </summary>
        protected Vector3 topRightCorner;
        /// <summary>
        /// Plane where the current dragding occurs.
        /// </summary>
        protected Plane currentDragPlane;
        /// <summary>
        /// Distance with the object at the start of the dragging.
        /// </summary>
        protected float startDistanceFromObject;
        /// <summary>
        /// True is the object is currently being dragged.
        /// </summary>
        protected bool isBeingDragged;
        /// <summary>
        /// Controller used for dragging if any.
        /// </summary>
        protected VRController usedController;

        /// <summary>
        /// World position of <see cref="scrollbarTransform"/> 's bottom left corner.
        /// </summary>
        protected Vector3 bottomLeftCorner;

        #endregion

        protected virtual void Awake()
        {
            colliderRaycast = GetComponentInChildren<Collider>();

            referenceRect = GetComponent<RectTransform>();
        }

        /// <inheritdoc/>
        public abstract DragAndDropType GetDragType();

        /// <inheritdoc/>
        public Vector3 GetNormal() => transform.forward;

        /// <inheritdoc/>
        public Vector3 GetPosition() => transform.position;


        /// <inheritdoc/>
        public abstract bool IsDraggingAllowed();

        /// <inheritdoc/>
        public abstract void DragMove(Vector3 position, Transform selector);

        /// <inheritdoc/>
        public virtual void OnDragStart()
        {
            Vector3[] corners = new Vector3[4];
            referenceRect.GetWorldCorners(corners);

            bottomLeftCorner = corners[0];
            topLeftCorner = corners[1];
            topRightCorner = corners[2];

            currentDragPlane = new Plane(GetNormal(), GetPosition());
            var ray = new Ray
            {
                direction = usedController.transform.forward,
                origin = usedController.transform.position
            };

            var closestHit = Physics.RaycastAll(ray)
                        .Where(hit => hit.collider == this.colliderRaycast)
                        .OrderBy(hit => hit.distance)
                        .FirstOrDefault();
            startDistanceFromObject = closestHit.distance;

            isBeingDragged = true;
        }

        /// <inheritdoc/>
        public virtual void OnDrag()
        {
            switch (GetDragType())
            {
                case DragAndDropType.Planar:
                    Ray ray = new Ray()
                    {
                        direction = usedController.transform.forward,
                        origin = usedController.transform.position
                    };
                    float enter;
                    if (currentDragPlane.Raycast(ray, out enter))
                    {
                        DragMove(ray.GetPoint(enter), usedController.transform);
                    }
                    break;
                case DragAndDropType.Spatial:
                    Vector3 point = usedController.transform.position + usedController.transform.forward * startDistanceFromObject;
                    DragMove(point, usedController.transform);
                    break;
                default:
                    break;
            }
        }
        /// <inheritdoc/>
        public virtual void OnDragStop()
        {
            isBeingDragged = false;
        }

        /// <inheritdoc/>
        public virtual void OnDropFailCallback()
        {
        }

        /// <inheritdoc/>
        public virtual void SetDestroyCallback(Action callback)
        {
            
        }

        /// <inheritdoc/>
        public override void Select(VRController controller)
        {
            usedController = controller;
        }

        /// <inheritdoc/>
        public override void Deselect(VRController controller)
        {
            usedController = null;
        }

        /// <inheritdoc/>
        public void PressDown(ControllerType controller)
        {
            OnDragStart();
            isPressed = true;
        }

        /// <inheritdoc/>
        public void PressUp(ControllerType controller)
        {
            OnDragStop();
            isPressed = false;
        }

        /// <inheritdoc/>
        public bool IsPressed(ControllerType controller)
        {
            return isPressed;
        }

        /// <inheritdoc/>
        public void PressStay(ControllerType type)
        {
            OnDrag();
        }
    }
}