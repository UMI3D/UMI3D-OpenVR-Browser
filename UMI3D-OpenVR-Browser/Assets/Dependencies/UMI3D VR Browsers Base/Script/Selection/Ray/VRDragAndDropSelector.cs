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
using System.Linq;
using umi3dVRBrowsersBase.interactions;
using UnityEngine;

namespace umi3dVRBrowsersBase.selection
{
    /// <summary>
    /// Selector for <see cref="IDraggableElement"/> and <see cref="IDropElementHandler"/>.
    /// </summary>
    public class VRDragAndDropSelector : MonoBehaviour
    {
        #region Static 

        /// <summary>
        /// List of element which can receive a drag and drop.
        /// </summary>
        private static HashSet<IDropElementHandler> droppableHandlers = new HashSet<IDropElementHandler>();

        /// <summary>
        /// Adds <paramref name="droppableHandler"/> to the list of current available droppableHandler
        /// </summary>
        /// <param name="droppableHandler"></param>
        public static void RegisterElement(IDropElementHandler droppableHandler)
        {
            droppableHandlers.Add(droppableHandler);
        }

        /// <summary>
        /// Reverts <see cref="RegisterElement(IDropElementHandler)"/>.
        /// </summary>
        /// <param name="droppableHandler"></param>
        public static void UnRegisterElement(IDropElementHandler droppableHandler)
        {
            droppableHandlers.Remove(droppableHandler);
        }

        #endregion

        #region Fields

        [SerializeField]
        [Tooltip("Associated laser")]
        private Laser laser;

        [SerializeField]
        [Tooltip("For better performance, limit the number of hover update per frame")]
        private int hoverFPS = 60;

        /// <summary>
        /// Stores last time an update was performed.
        /// </summary>
        private float lastUpdateTime;

        /// <summary>
        /// Controller used to handle this selector.
        /// </summary>
        public ControllerType controller;

        /// <summary>
        /// Inputs possible to drag and drop.
        /// </summary>
        public List<ActionType> actions = new List<ActionType>();

        public bool IsActivated { get; set; } = true;

        /// <summary>
        /// Reference to the input manager.
        /// </summary>
        private AbstractControllerInputManager inputManager;

        /// <summary>
        /// Element which is currently dragged by a user.
        /// </summary>
        private IDraggableElement currentElementDragged = null;

        /// <summary>
        /// Element which is currently dragged by a user.
        /// </summary>
        private IDraggableElement currentElementHovered = null;

        /// <summary>
        /// Plane where <see cref="currentElementDragged"/> is currently dragged.
        /// </summary>
        private Plane currentDragPlane;

        /// <summary>
        /// Ray used to raycast.
        /// </summary>
        private Ray ray;

        /// <summary>
        /// Element hit by <see cref="ray"/>.
        /// </summary>
        private RaycastHit hit;

        /// <summary>
        /// Distance at the beginning from selector to object dragged.
        /// </summary>
        private float startDistanceFromObject;

        #endregion

        #region Methods

        private void Start()
        {
            inputManager = AbstractControllerInputManager.Instance;
            Debug.Assert(inputManager != null);
        }

        /// <summary>
        /// Handles the inputs to perform the drag and drop.
        /// </summary>
        private void Update()
        {
            if (!IsActivated)
                return;

            if (Time.time < lastUpdateTime + (1f / hoverFPS))
            {
                return;
            }

            lastUpdateTime = Time.time;

            ray = new Ray(transform.position, transform.forward);

            if (currentElementDragged != null)
            {
                HandleDragAndDrop();
            }
            else
            {
                if (Physics.Raycast(ray, out hit, 100))
                {
                    IDraggableElement draggable = hit.transform.GetComponents<IDraggableElement>().Where(c => (c is MonoBehaviour mono && mono.enabled)).FirstOrDefault();

                    if (draggable != null)
                    {
                        if (currentElementHovered != null && currentElementHovered != draggable)
                        {
                            laser.OnHoverExit(currentElementHovered.GetHashCode());
                            currentElementHovered = draggable;
                            laser.OnHoverEnter(currentElementHovered.GetHashCode());
                        }
                        else if (currentElementHovered != draggable)
                        {
                            currentElementHovered = draggable;
                            laser.OnHoverEnter(currentElementHovered.GetHashCode());
                        }

                        CheckIfDragAndDropStart(draggable);
                    }
                    else if (currentElementHovered != null)
                    {
                        laser.OnHoverExit(currentElementHovered.GetHashCode());
                        currentElementHovered = null;
                    }
                }
                else
                {
                    if (currentElementHovered != null)
                    {
                        laser.OnHoverExit(currentElementHovered.GetHashCode());
                        currentElementHovered = null;
                    }
                }
            }
        }

        /// <summary>
        /// If an element is currently drag and drop, performs the manipualtion.
        /// </summary>
        private void HandleDragAndDrop()
        {
            bool isPressed = false;
            foreach (ActionType action in actions)
            {
                if (inputManager.GetButton(controller, action))
                {
                    isPressed = true;
                    break;
                }
            }

            if (isPressed)
            {
                switch (currentElementDragged.GetDragType())
                {
                    case DragAndDropType.Planar:
                        float enter;
                        if (currentDragPlane.Raycast(ray, out enter))
                        {
                            currentElementDragged.OnDrag(ray.GetPoint(enter), this.transform);
                            laser.SetImpactPoint(hit.point);
                        }
                        break;
                    case DragAndDropType.Spatial:
                        Vector3 point = transform.position + transform.forward * startDistanceFromObject;
                        laser.SetImpactPoint(hit.point);
                        currentElementDragged.OnDrag(point, this.transform);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                currentElementDragged.OnDragStop();
                CheckDrop();
                currentElementDragged = null;
            }
        }

        /// <summary>
        /// Checks if a drag and drop can start when a user clicks.
        /// </summary>
        /// <param name="draggable"></param>
        private void CheckIfDragAndDropStart(IDraggableElement draggable)
        {
            if (draggable == null || draggable == default)
                return;

            bool isPressed = false;
            foreach (ActionType action in actions)
            {
                if (inputManager.GetButton(controller, action))
                {
                    isPressed = true;
                    break;
                }
            }

            if (isPressed)
            {
                if (draggable.IsDraggingAllowed())
                {
                    currentElementDragged = draggable;
                    draggable.SetDestroyCallback(() => currentElementDragged = null);
                    draggable.OnDragStart();
                    currentDragPlane = new Plane(draggable.GetNormal(), draggable.GetPosition());

                    startDistanceFromObject = hit.distance;
                    laser.OnHoverEnter(currentElementDragged.GetHashCode());
                }
            }
        }

        /// <summary>
        /// Checks if <see cref="currentElementDragged"/> is dropped on one of the <see cref="droppableHandlers"/>.
        /// </summary>
        private void CheckDrop()
        {
            (IDropElementHandler, float) dropData = (null, Mathf.Infinity);

            float tmpDistance;

            foreach (IDropElementHandler dropHandler in droppableHandlers)
            {
                tmpDistance = Vector3.Distance(dropHandler.GetPosition(), currentElementDragged.GetPosition());

                if (tmpDistance < dropHandler.GetDropTolerance() && currentElementDragged != dropHandler)
                {
                    if ((dropData.Item1 == null) || (dropData.Item2 > tmpDistance))
                        dropData = (dropHandler, tmpDistance);
                }
            }

            if (dropData.Item1 == null || !dropData.Item1.OnElementDropped(currentElementDragged))
            {
                currentElementDragged.OnDropFailCallback();
            }
        }

        #endregion
    }

    /// <summary>
    /// Defines if its is a 2D or a 3D drag and drop.
    /// </summary>
    public enum DragAndDropType { Planar, Spatial }
}