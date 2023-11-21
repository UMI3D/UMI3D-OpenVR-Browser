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
using umi3dBrowsers.interaction.selection.intentdetector;
using umi3dBrowsers.interaction.selection.projector;
using umi3dBrowsers.interaction.selection.selector;
using umi3dVRBrowsersBase.ui.playerMenu;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.interactions.selection.selector
{
    /// <summary>
    /// Base template for object selector
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
        /// Event triggered each frame when a selection is available
        /// </summary>
        public SelectionEvent selectionStayEvent = new SelectionEvent();

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
        /// True when a selection cannot occur since an object is manipulated
        /// </summary>
        public bool LockedSelector { get; protected set; } = false;

        /// <summary>
        /// Info concerning the last selected object
        /// </summary>
        public SelectionIntentData<T> LastSelected;

        /// <summary>
        /// Previously proposed objects
        /// </summary>
        [HideInInspector]
        public LinkedList<SelectionIntentData> selectionPropositions = new LinkedList<SelectionIntentData>();

        // <summary>
        /// Manages projection on the controller
        /// </summary>
        [HideInInspector]
        public IProjector<T> projector;

        /// <summary>
        /// Previously detected objects for virtual hand
        /// </summary>
        [HideInInspector]
        public Cache<T> detectionCacheProximity = new Cache<T>();

        /// <summary>
        /// Previously detected objects from virtual pointing
        /// </summary>
        [HideInInspector]
        public Cache<T> detectionCachePointing = new Cache<T>();

        /// <summary>
        /// Selection Intent Detectors (virtual pointing). In order of decreasing priority.
        /// </summary>
        [HideInInspector]
        public List<AbstractDetector<T>> PointingDetectors
        {
            get
            {
                if (_pointingDetectors == null)
                {
                    _pointingDetectors = GetPointingDetectors();
                }
                return _pointingDetectors;
            }
        }

        private List<AbstractDetector<T>> _pointingDetectors;

        /// <summary>
        /// Selection Intent Detector (virtual hand). In order of decreasing priority.
        /// </summary>
        [HideInInspector]
        public List<AbstractDetector<T>> ProximityDetectors
        {
            get
            {
                if (_proximityDetectors == null)
                {
                    _proximityDetectors = GetProximityDetectors();
                }
                return _proximityDetectors;
            }
        }

        private List<AbstractDetector<T>> _proximityDetectors;

        #endregion fields

        #region lifecycle

        protected override void Awake()
        {
            controller = GetComponentInParent<VRSelectionManager>().controller; //controller is required before awake
            base.Awake();
        }

        private void OnEnable()
        {
            if (UMI3DCollaborationClientServer.Exists)
                UMI3DCollaborationClientServer.Instance.OnRedirection.AddListener(OnEnvironmentLeave);
        }

        private void OnDisable()
        {
            if (UMI3DCollaborationClientServer.Exists)
                UMI3DCollaborationClientServer.Instance.OnRedirection.RemoveListener(OnEnvironmentLeave);
        }

        protected virtual void Update()
        {
            if (IsSelecting())
                selectionStayEvent.Invoke(LastSelected);
        }

        /// <inheritdoc/>
        protected override void ActivateInternal()
        {
            base.ActivateInternal();
            selectionEvent.AddListener(OnSelection);
            selectionStayEvent.AddListener(OnSelectionStay);
            deselectionEvent.AddListener(OnDeselection);

            foreach (var detector in ProximityDetectors)
                detector.Init(controller);
            foreach (var detector in PointingDetectors)
                detector.Init(controller);
        }

        /// <inheritdoc/>
        protected override void DeactivateInternal()
        {
            base.DeactivateInternal();
            selectionEvent.RemoveAllListeners();
            selectionStayEvent.RemoveAllListeners();
            deselectionEvent.RemoveAllListeners();

            foreach (var detector in ProximityDetectors)
                detector.Reinit();
            foreach (var detector in PointingDetectors)
                detector.Reinit();
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

                if ((data == null && !lastObjectIsNull) // leaving an object
                    || (data != null && !data.Equals(Objects.Last.Value))) // moving to another object
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
        /// Executed when an Element is selected
        /// </summary>
        /// <param name="deselectionData"></param>
        protected virtual void OnSelectionStay(SelectionIntentData<T> selectionData)
        {
            selectionFeedbackHandler?.UpdateFeedback(selectionData);
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
        /// Checks if the element could be selected.
        /// </summary>
        /// Defaut implementation just checks non-nullity.
        /// <param name="objToSelect">Object to select</param>
        /// <returns>True if the object could be selected by the selector.</returns>
        protected virtual bool CanSelect(T objToSelect)
        {
            return objToSelect != null;
        }

        /// <summary>
        /// Create an appropriate selection intent data.
        /// </summary>
        /// <param name="obj">Object that is selection intent target.</param>
        /// <param name="origin">Paradigm of selection used.</param>
        /// <returns></returns>
        public abstract SelectionIntentData CreateSelectionIntentData(T obj, DetectionOrigin origin);

        /// <summary>
        /// Retrieve the best proposition for each detector of a selector
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<SelectionIntentData> GetIntentDetections()
        {
            foreach (var detector in ProximityDetectors) // priority for proximity
            {
                if (!detector.isRunning)
                    continue;

                var objToSelectProximity = detector.PredictTarget();
                var detectionInfo = CreateSelectionIntentData(objToSelectProximity, DetectionOrigin.PROXIMITY);
                detectionCacheProximity.Add(detectionInfo);
                if (CanSelect(objToSelectProximity))
                    selectionPropositions.AddLast(detectionInfo);
            }

            foreach (var detector in PointingDetectors)
            {
                if (!detector.isRunning)
                    continue;

                var objToSelectPointed = detector.PredictTarget();
                var detectionInfo = CreateSelectionIntentData(objToSelectPointed, DetectionOrigin.POINTING);
                detectionCachePointing.Add(detectionInfo);
                if (CanSelect(objToSelectPointed))
                    selectionPropositions.AddLast(detectionInfo);
            }

            return selectionPropositions;
        }

        /// <summary>
        /// Select the last proposed <see cref="T"/> and provides its infos. <br/>
        /// You probably want to use <see cref="Select(SelectionIntentData)"/> instead.
        /// </summary>
        public override void Select()
        {
            Select(selectionPropositions.First?.Value);
        }

        /// <summary>
        /// Select a <see cref="T"/> and provides its infos
        /// </summary>
        public void Select(SelectionIntentData data)
        {
            Select(data as SelectionIntentData<T>);
            selectionPropositions.Clear();
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
                {
                    Deselect(LastSelected);
                    if (PlayerMenuManager.Instance.parameterGear.IsDisplayed)
                        PlayerMenuManager.Instance.parameterGear.Hide();
                }

            }

            projector.Project(selectionInfo.selectedObject, controller);
            selectionInfo.hasBeenSelected = true;
            LastSelected = selectionInfo;
            isSelecting = true;
            selectionEvent?.Invoke(selectionInfo);
            foreach (var detector in PointingDetectors)
                detector.Reinit();
            foreach (var detector in ProximityDetectors)
                detector.Reinit();
        }

        /// <inheritdoc/>
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

            foreach (var detector in PointingDetectors)
                detector.Reinit();
            foreach (var detector in ProximityDetectors)
                detector.Reinit();
        }

        /// <inheritdoc/>
        public bool IsSelecting() => isSelecting;

        /// <summary>
        /// Retrieves detectors associated with virtual hand selection detection.
        /// </summary>
        /// <returns></returns>
        public abstract List<AbstractDetector<T>> GetProximityDetectors();

        /// <summary>
        /// Retrieves  detectors associated with virtual pointing selection detection.
        /// </summary>
        /// <returns></returns>
        public abstract List<AbstractDetector<T>> GetPointingDetectors();

        #endregion selection
    }
}