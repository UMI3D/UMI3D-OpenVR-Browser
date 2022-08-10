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
using umi3d.cdk.collaboration;
using umi3d.cdk.interaction;
using umi3dBrowsers.interaction.selection;
using umi3dBrowsers.interaction.selection.feedback;
using umi3dBrowsers.interaction.selection.projector;
using umi3dBrowsers.interaction.selection.selector;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.interactions.selection.selector
{
    /// <summary>
    /// Ba template for object selector
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractVRSelector<T> : AbstractSelector, IIntentSelector where T : MonoBehaviour
    {
        #region fields

        /// <summary>
        /// Controller the selector belongs to
        /// </summary>
        protected VRController controller;

        public class SelectionEvent : UnityEvent<SelectionIntentData<T>>
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

        /// <summary>
        /// True when an object is currently selected by the selector
        /// </summary>
        protected bool isSelecting = false;

        /// <summary>
        /// Info concerning the last selected object
        /// </summary>
        public SelectionIntentData<T> LastSelected;

        /// <summary>
        /// Previously proposed objects
        /// </summary>
        [HideInInspector]
        public Cache<SelectionIntentData<T>> propositionSelectionCache = new Cache<SelectionIntentData<T>>();

        // <summary>
        /// Manages projection on the controller
        /// </summary>
        [HideInInspector]
        public IProjector<T> projector;

        #endregion fields

        #region lifecycle

        protected override void Awake()
        {
            controller = GetComponentInParent<VRSelectionManager>().controller; //controller is required before awake
            base.Awake();
            UMI3DCollaborationClientServer.Instance.OnRedirection.AddListener(OnEnvironmentLeave);
        }

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
        /// Executed when the environment is left
        /// </summary>
        protected void OnEnvironmentLeave()
        {
            if (isSelecting && LastSelected?.selectedObject != null)
                Deselect(LastSelected);
        }

        #endregion lifecycle

        #region caching

        /// <summary>
        /// Cached information of the previous selection. FILO queue with a max lenght.
        /// </summary>
        /// <typeparam name="SD"></typeparam>
        public class Cache
        {
            /// <summary>
            /// Cached objects info
            /// </summary>
            public LinkedList<SelectionIntentData> Objects { get; } = new LinkedList<SelectionIntentData>();

            /// <summary>
            /// Max size of the cache
            /// </summary>
            protected uint maxSize = 5;

            /// <summary>
            /// Add an object to the cache
            /// </summary>
            /// <param name="data"></param>
            public virtual void Add(SelectionIntentData data)
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
            public void Add(SelectionIntentData<SD> data)
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

        #endregion caching

        #region selection

        /// <summary>
        /// Executed when an Element is selected
        /// </summary>
        /// <param name="deselectionData"></param>
        protected virtual void OnSelection(SelectionIntentData<T> selectionData)
        {
            selectionFeedbackHandler?.StartFeedback(selectionData);
        }

        /// <summary>
        /// Executed when an Element is deselected
        /// </summary>
        /// <param name="deselectionData"></param>
        protected virtual void OnDeselection(SelectionIntentData<T> deselectionData)
        {
            selectionFeedbackHandler?.EndFeedback(deselectionData);
        }

        /// <summary>
        /// Checks if the element could be selected
        /// </summary>
        /// <param name="icToSelect"></param>
        /// <returns>True if the object could be selected by the selector</returns>
        protected virtual bool CanSelect(T icToSelect)
        {
            return icToSelect != null;
        }

        /// <summary>
        /// Retrieve the best proposition for each detector of a selector
        /// </summary>
        /// <returns></returns>
        public abstract List<SelectionIntentData> GetIntentDetections();

        /// <summary>
        /// Select the last proposed <see cref="T"/> and provides its infos. <br/>
        /// You probably want to use <see cref="Select(SelectionIntentData)"/> instead.
        /// </summary>
        public override void Select()
        {
            Select(propositionSelectionCache.Objects.Last?.Value);
        }

        /// <summary>
        /// Select a <see cref="T"/> and provides its infos
        /// </summary>
        public void Select(SelectionIntentData data)
        {
            Select(data as SelectionIntentData<T>);
        }

        /// <summary>
        /// Select a <see cref="T"/> and provides its infos
        /// </summary>
        /// <param name="icToSelect"></param>
        /// <param name="selectionInfo"></param>
        protected virtual void Select(SelectionIntentData<T> selectionInfo)
        {
            if (isSelecting)
            {
                if (selectionInfo == null && LastSelected != null) //the selector was selecting something before and should remember it choose to select nothing this time
                {
                    Deselect(LastSelected);
                    LastSelected = null;
                    return;
                }
                else if (LastSelected?.selectedObject == selectionInfo.selectedObject) //  the selector was selecting the same target before
                    return;
                else if (LastSelected != null) // the selector was selecting something else before
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
        public void Deselect(SelectionIntentData data)
        {
            Deselect(data as SelectionIntentData<T>);
        }

        /// <summary>
        /// Deselect object and remove feedback
        /// </summary>
        /// <param name="interactableToDeselectInfo"></param>
        protected virtual void Deselect(SelectionIntentData<T> interactableToDeselectInfo)
        {
            projector.Release(interactableToDeselectInfo.selectedObject, controller);
            isSelecting = false;
            deselectionEvent.Invoke(interactableToDeselectInfo);
        }

        /// <inheritdoc/>
        public bool IsSelecting() => isSelecting;

        #endregion selection
    }
}