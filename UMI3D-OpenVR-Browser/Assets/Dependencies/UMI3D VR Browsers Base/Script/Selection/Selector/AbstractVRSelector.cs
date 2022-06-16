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
using umi3dVRBrowsersBase.interactions;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dbrowser.openvr.interaction.selection
{
    /// <summary>
    /// Ba template for object selector
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractVRSelector<T> : AbstractSelector where T : MonoBehaviour
    {
        /// <summary>
        /// Controller the selector belongs to
        /// </summary>
        protected VRController controller;

        public class SelectionEvent : UnityEvent<SelectionData<T>>
        { };

        public SelectionEvent selectionEvent = new SelectionEvent();
        public SelectionEvent deselectionEvent = new SelectionEvent();

        /// <summary>
        /// Previously detected objects for virtual hand
        /// </summary>
        [HideInInspector]
        public SelectionCache<T> detectionCacheProximity = new SelectionCache<T>();

        /// <summary>
        /// Previously detected objects from virtual pointing
        /// </summary>
        [HideInInspector]
        public SelectionCache<T> detectionCachePointing = new SelectionCache<T>();

        /// <summary>
        /// Cached information of the previous selection
        /// </summary>
        /// <typeparam name="SD"></typeparam>
        public class SelectionCache<SD>
        {
            public LinkedList<SD> Objects { get; } = new LinkedList<SD>();

            protected uint maxSize = 5;

            public virtual void Add(SD data)
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

            public void Clear()
            {
                Objects.Clear();
            }
        }

        protected override void Awake()
        {
            controller = GetComponentInParent<VRSelectionManager>().controller;
            base.Awake();
        }

        protected virtual void Update()
        {
            if (activated)
                Select();
        }

        /// <summary>
        /// Triggered when an object is selected
        /// </summary>
        /// <param name="deselectionData"></param>
        protected abstract void OnSelection(SelectionData<T> selectionData);

        /// <summary>
        /// Triggered when an object is deselected
        /// </summary>
        /// <param name="deselectionData"></param>
        protected abstract void OnDeselection(SelectionData<T> deselectionData);
    }
}