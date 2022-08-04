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

namespace umi3d.cdk.interaction.selection
{
    /// <summary>
    /// Information of selection tagging interface
    /// </summary>
    public abstract class AbstractSelectionData
    {
        /// <summary>
        /// Controller source of detection
        /// </summary>
        public AbstractController controller;

        /// <summary>
        /// Set to true when the object is the one chosen to be projected on the controller
        /// </summary>
        public bool hasBeenSelected;
    }
}

namespace umi3dVRBrowsersBase.interactions.selection
{
    using umi3d.cdk.interaction;
    using umi3d.cdk.interaction.selection;
    using UnityEngine;

    /// <summary>
    /// Selection info on an object
    /// </summary>
    /// <typeparam name="T">InteractableContainer or Selectable</typeparam>
    public class SelectionData : AbstractSelectionData
    {
        /// <summary>
        /// Selection Intent Detection paradigm used for detection
        /// </summary>
        public DetectionOrigin detectionOrigin;

        /// <summary>
        /// Selection Intent Detection paradigm used for detection
        /// </summary>
        public SelectableObjectType objectType;
    }

    /// <summary>
    /// Selection info on an object
    /// </summary>
    /// <typeparam name="T">InteractableContainer or Selectable</typeparam>
    public class SelectionData<T> : SelectionData
    {
        /// <summary>
        /// Selected object
        /// </summary>
        public T selectedObject;

        public SelectionData(SelectableObjectType objType)
        {
            objectType = objType;
        }
    }

    public enum SelectableObjectType
    {
        /// <summary>
        /// Corresponds to <see cref="InteractableContainer"/>
        /// </summary>
        INTERACTABLE,

        /// <summary>
        /// Corresponds to <see cref="UnityEngine.UI.Selectable"/>
        /// </summary>
        SELECTABLE,

        /// <summary>
        /// Corresponds to <see cref="ui.AbstractClientInteractableElement"/>
        /// </summary>
        CLIENT_ELEMENT
    }

    /// <summary>
    /// Selection Intent Detection paradigm used for detection
    /// </summary>
    public enum DetectionOrigin
    {
        /// <summary>
        /// Object is detected through pointing by a controller
        /// </summary>
        POINTING,

        /// <summary>
        /// Object is detected through proximity with a controller
        /// </summary>
        PROXIMITY
    }
}