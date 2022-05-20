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
using umi3dVRBrowsersBase.interactions.input;
using umi3dVRBrowsersBase.selection;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    /// <summary>
    /// Element drag and droppable, associated to a <see cref="ActionBindingEntry"/>, to enable users to change action bindings.
    /// </summary>
    public class ActionBindingEntryInput : MonoBehaviour, IDraggableElement, IDropElementHandler
    {
        #region

        [SerializeField]
        [Tooltip("Distance to consider an element is dropped on top of this element")]
        private float dropTolerance = .5f;

        /// <summary>
        /// World position of the element before it started being dragged.
        /// </summary>
        private Vector3 positionBeforeDragAndDrop;

        /// <summary>
        /// Associated <see cref="ActionBindingEntry"/>
        /// </summary>
        [SerializeField]
        private ActionBindingEntry actionBinding;

        /// <summary>
        /// Callback called when this element is destroyed.
        /// </summary>
        private Action destroyCallback;

        [SerializeField]
        [Tooltip("Background of the entry")]
        private Image background;

        [SerializeField]
        [Tooltip("Button drag and droppable to change action binding")]
        private Button button;

        [SerializeField]
        [Tooltip("Background used when associated action is bound to a controller input")]
        private Sprite backgroundForBinding;

        [SerializeField]
        [Tooltip("Background used when associated action is not bound to a controller input")]
        private Sprite backgroundWithoutBinding;

        #endregion

        #region DragAnDrop

        public void OnEnable()
        {
            VRDragAndDropSelector.RegisterElement(this);
        }

        public void OnDisable()
        {
            VRDragAndDropSelector.UnRegisterElement(this);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public float GetDropTolerance()
        {
            return dropTolerance;
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
        /// <param name="position"></param>
        public void OnDrag(Vector3 position, Transform selector)
        {
            transform.position = position;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnDragStart()
        {
            positionBeforeDragAndDrop = transform.position;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnDragStop()
        {

        }

        /// <summary>
        /// Sets the backgound according to binding type.
        /// </summary>
        internal void SetBackground(ActionBindingType type)
        {
            switch (type)
            {
                case ActionBindingType.Button:
                    background.sprite = backgroundForBinding;
                    button.enabled = true;
                    break;
                case ActionBindingType.NotBound:
                    background.sprite = backgroundWithoutBinding;
                    button.enabled = false;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool OnElementDropped(IDraggableElement element)
        {
            if (element != this && element is ActionBindingEntryInput entryInput)
            {
                entryInput.OnDropFailCallback();

                actionBinding.SwitchInputTo(entryInput.actionBinding);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnDropFailCallback()
        {
            transform.position = positionBeforeDragAndDrop;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public bool IsDraggingAllowed()
        {
            return actionBinding.input is BooleanInput;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="callback"></param>
        public void SetDestroyCallback(Action callback)
        {
            destroyCallback = callback;
        }

        private void OnDestroy()
        {
            destroyCallback?.Invoke();
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

    public enum ActionBindingType
    {
        Button,
        NotBound
    }
}