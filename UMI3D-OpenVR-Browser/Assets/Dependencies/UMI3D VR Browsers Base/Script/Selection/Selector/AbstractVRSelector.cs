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

using System.Collections.Generic;
using umi3d.cdk.interaction;
using umi3d.cdk.interaction.selection.feedback;
using umi3d.cdk.interaction.selection.projector;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.interactions.selection
{
    /// <summary>
    /// Ba template for object selector
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractVRSelector<T> : AbstractSelector, ISelector where T : MonoBehaviour
    {
        /// <summary>
        /// Controller the selector belongs to
        /// </summary>
        protected VRController controller;

        protected SelectableObjectType supportedObjectType; 

        public class SelectionEvent : UnityEvent<SelectionData<T>>
        { };

        /// <summary>
        /// Event triggered when a selection occurs
        /// </summary>
        public SelectionEvent selectionEvent = new SelectionEvent();

        /// <summary>
        /// Event triggered when a deselection occurs
        /// </summary>
        public SelectionEvent deselectionEvent = new SelectionEvent();

        /// <summary>
        /// Manage feedback when an object is selected
        /// </summary>
        public AbstractSelectionFeedbackHandler<T> selectionFeedbackHandler;

        protected bool isSelecting = false;

        public bool IsSelecting => isSelecting;

        public SelectionData<T> LastSelected;

        /// <inheritdoc/>
        protected override void ActivateInternal()
        {
            base.ActivateInternal();
            selectionEvent.AddListener(OnSelection);
            deselectionEvent.AddListener(OnDeselection);
        }

        /// <inheritdoc/>
        protected override void DeactivateInternal()
        {
            base.DeactivateInternal();
            selectionEvent.RemoveAllListeners();
            deselectionEvent.RemoveAllListeners();
        }

        /// <summary>
        /// Cached information of the previous selection. FILO queue with a max lenght.
        /// </summary>
        /// <typeparam name="SD"></typeparam>
        public class Cache
        {
            /// <summary>
            /// Cached objects info
            /// </summary>
            public LinkedList<SelectionData> Objects { get; } = new LinkedList<SelectionData>();

            /// <summary>
            /// Max size of the cache
            /// </summary>
            protected uint maxSize = 5;

            /// <summary>
            /// Add an object to the cache
            /// </summary>
            /// <param name="data"></param>
            public virtual void Add(SelectionData data)
            {
                if (Objects.Count == 0)
                {
                    Objects.AddLast(data);
                    return;
                }

                bool lastObjectIsNull = Objects.Last.Value == null;

                if ((data == null && !lastObjectIsNull)
                    || (data != null && !data.Equals(Objects.Last.Value)))
                {
                    Objects.AddLast(data);
                    if (Objects.Count > maxSize)
                        Objects.RemoveFirst();
                }
            }

            /// <summary>
            /// Remove all objects from the cache
            /// </summary>
            public void Clear()
            {
                Objects.Clear();
            }
        }

        /// <inheritdoc/>
        public class Cache<SD> : Cache
        {
            public void Add(SelectionData<SD> data)
            {
                if (Objects.Count == 0)
                {
                    Objects.AddLast(data);
                    return;
                }

                bool lastObjectIsNull = Objects.Last.Value == null;

                if ((data == null && !lastObjectIsNull)
                    || (data != null && !data.Equals(Objects.Last.Value)))
                {
                    Objects.AddLast(data);
                    if (Objects.Count > maxSize)
                        Objects.RemoveFirst();
                }
            }
        }

        /// <summary>
        /// Previously proposed objects
        /// </summary>
        [HideInInspector]
        public Cache<SelectionData<T>> propositionSelectionCache = new Cache<SelectionData<T>>();

        // <summary>
        /// Manages projection on the controller
        /// </summary>
        [HideInInspector]
        public IProjector<T> projector;

        protected override void Awake()
        {
            controller = GetComponentInParent<VRSelectionManager>().controller;
            base.Awake();
        }

        /// <summary>
        /// Triggered when an Element is selected
        /// </summary>
        /// <param name="deselectionData"></param>
        protected virtual void OnSelection(SelectionData<T> selectionData)
        {
            selectionFeedbackHandler?.StartFeedback(selectionData);
        }

        /// <summary>
        /// Triggered when an Element is deselected
        /// </summary>
        /// <param name="deselectionData"></param>
        protected virtual void OnDeselection(SelectionData<T> deselectionData)
        {
            selectionFeedbackHandler?.EndFeedback(deselectionData);
        }

        /// <summary>
        /// Checks if the element :
        ///     - exists
        ///     - has at least one associated interaction
        ///     - has not been seen its tool projected yet
        /// </summary>
        /// <param name="icToSelect"></param>
        /// <returns></returns>
        protected virtual bool CanSelect(T icToSelect)
        {
            return icToSelect != null;
        }

        /// <summary>
        /// Retrieve the best proposition for each detector of a selector
        /// </summary>
        /// <returns></returns>
        public abstract List<SelectionData> Detect();

        /// <summary>
        /// Select the last proposed <see cref="T"/> and provides its infos. <br/>
        /// You probably want to use <see cref="Select(SelectionData)"/> instead.
        /// </summary>
        public override void Select()
        {
            Select(propositionSelectionCache.Objects.Last?.Value);
        }

        /// <summary>
        /// Select a <see cref="T"/> and provides its infos
        /// </summary>
        public void Select(SelectionData data)
        {
            Select(data as SelectionData<T>);
        }

        /// <summary>
        /// Select a <see cref="T"/> and provides its infos
        /// </summary>
        /// <param name="icToSelect"></param>
        /// <param name="selectionInfo"></param>
        protected virtual void Select(SelectionData<T> selectionInfo)
        {
            if (LastSelected?.selectedObject == selectionInfo.selectedObject && isSelecting)
                return;
            if (LastSelected != null && isSelecting)
            { 
                Deselect(LastSelected);
            }
            projector.Project(selectionInfo.selectedObject, controller);
            selectionInfo.hasBeenSelected = true;
            LastSelected = selectionInfo;
            isSelecting = true;
            selectionEvent.Invoke(selectionInfo);
        }

        /// <summary>
        /// Select a <see cref="T"/> and provides its infos
        /// </summary>
        public void Deselect(SelectionData data)
        {
            Deselect(data as SelectionData<T>);
        }

        /// <summary>
        /// Deselect object and remove feedback
        /// </summary>
        /// <param name="interactableToDeselectInfo"></param>
        protected virtual void Deselect(SelectionData<T> interactableToDeselectInfo)
        {
            if (LastSelected != null && isSelecting)
            {
                projector.Release(interactableToDeselectInfo.selectedObject, controller);
                isSelecting = false;
                deselectionEvent.Invoke(interactableToDeselectInfo);
            }
        }
    }
}