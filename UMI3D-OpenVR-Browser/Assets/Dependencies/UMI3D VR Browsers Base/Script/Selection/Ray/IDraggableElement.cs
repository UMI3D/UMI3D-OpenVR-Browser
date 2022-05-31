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

using UnityEngine;

namespace umi3dVRBrowsersBase.selection
{
    /// <summary>
    /// Interface to make an object draggable by a VR controller.
    /// </summary>
    public interface IDraggableElement
    {
        /// <summary>
        /// Called when this element is started being dragged.
        /// </summary>
        void OnDragStart();

        /// <summary>
        /// Called each update when this element is dragged.
        /// </summary>
        /// <param name="position">World space</param>
        void OnDrag(Vector3 position, Transform selector);

        /// <summary>
        /// Called when this element stopped being dragged.
        /// </summary>
        void OnDragStop();

        /// <summary>
        /// Returns world position of the element.
        /// </summary>
        /// <returns></returns>
        Vector3 GetPosition();

        /// <summary>
        /// Defines what to do when this element is not released on top of a <see cref="IDropElementHandler"/>.
        /// </summary>
        void OnDropFailCallback();

        /// <summary>
        /// Is the element currently draggable ?
        /// </summary>
        /// <returns></returns>
        bool IsDraggingAllowed();

        /// <summary>
        /// Sets a action which must be called when this element is destroyed
        /// </summary>
        void SetDestroyCallback(System.Action callback);

        /// <summary>
        /// Normal of the plane of the drag and drop. Only relevant if <see cref="GetDragType"/> returns planar mode.
        /// </summary>
        /// <returns></returns>Z
        Vector3 GetNormal();

        /// <summary>
        /// Defines if the element if dragged in a plane defined by <see cref="GetNormal"/> or in 3D.
        /// </summary>
        /// <returns></returns>
        DragAndDropType GetDragType();
    }
}