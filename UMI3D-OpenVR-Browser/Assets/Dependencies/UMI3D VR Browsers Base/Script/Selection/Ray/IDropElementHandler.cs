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
    /// Makes an object able to receive <see cref="IDraggableElement"/>.
    /// </summary>
    public interface IDropElementHandler
    {
        /// <summary>
        /// Overriding required to call <see cref="VRDragAndDropSelector.RegisterElement(IDropElementHandler)"/>. 
        /// </summary>
        void OnEnable();

        /// <summary>
        /// Overriding required to call <see cref="VRDragAndDropSelector.UnRegisterElement(IDropElementHandler)(IDropElementHandler)"/>. 
        /// </summary>
        void OnDisable();

        /// <summary>
        /// Defines what to do when this element receive a <see cref="IDraggableElement"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        bool OnElementDropped(IDraggableElement element);

        /// <summary>
        /// Returns world position of the element.
        /// </summary>
        /// <returns></returns>
        Vector3 GetPosition();

        /// <summary>
        /// Returns the distance tolerance to considerer an <see cref="IDraggableElement"/> was released on top of this element.
        /// </summary>
        /// <returns></returns>
        float GetDropTolerance();
    }
}