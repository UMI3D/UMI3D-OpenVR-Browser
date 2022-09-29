﻿/*
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

using umi3d.cdk.interaction;
using umi3dBrowsers.interaction.selection;
using umi3dBrowsers.interaction.selection.cursor;
using UnityEngine;

namespace umi3dVRBrowsersBase.interactions.selection.cursor
{
    /// <summary>
    /// Cursor for proximity selection <br/>
    /// To implement
    /// </summary>
    public class SphereGrabCursor : AbstractCursor
    {
        /// <summary>
        /// Sphere around the hand while selecting.
        /// </summary>
        [SerializeField, Tooltip("Sphere around the hand while selecting.")]
        protected GameObject cursorSphere;

        protected Renderer cursorRenderer;

        private SphereCollider cursorCollider;

        /// <summary>
        /// Sphere on the object while selecting.
        /// </summary>
        [SerializeField, Tooltip("Sphere on the object while selecting.")]
        protected GameObject contactSphere;

        protected Renderer contactSphereRenderer;

        /// <summary>
        /// Is the cursor displayed?
        /// </summary>
        public bool IsZoneHintDisplayed => cursorRenderer.enabled;

        private Collider selectedObjectCollider;

        private bool isTrackingSelectedObject;

        /// <summary>
        /// Contact tracked object that is close to the hand
        /// </summary>
        private MonoBehaviour trackedObject;

        /// <summary>
        /// Is the convex property of the contact tracked object overrided ?
        /// </summary>
        private bool isConvexOverrided = false;

        public void Awake()
        {
            cursorRenderer = cursorSphere.GetComponent<Renderer>();
            contactSphereRenderer = contactSphere.GetComponent<Renderer>();
        }

        public void Start()
        {
            cursorRenderer.enabled = false;
            contactSphereRenderer.enabled = false;
            cursorCollider = GetComponentInChildren<SphereCollider>();
        }

        public void Update()
        {
            if (isTrackingSelectedObject)
                SetContactSphere();
        }

        #region Display

        /// <inheritdoc/>
        public override void Display()
        {
            DisplayZoneHint();
            DisplayContactSphereHint();
        }

        public void DisplayZoneHint()
        {
            cursorRenderer.enabled = true;
        }

        public void DisplayContactSphereHint()
        {
            contactSphereRenderer.enabled = true;
        }

        /// <inheritdoc/>
        public override void Hide()
        {
            HideZoneHint();
            HideContactSphereHint();
        }

        public void HideZoneHint()
        {
            cursorRenderer.enabled = false;
        }

        public void HideContactSphereHint()
        {
            contactSphereRenderer.enabled = false;
            contactSphere.transform.position = this.transform.position;
        }

        #endregion

        /// <inheritdoc/>
        public override void ChangeAccordingToSelection(AbstractSelectionData selectedObjectData)
        {
            var obj = (selectedObjectData as SelectionIntentData<InteractableContainer>)?.selectedObject;

            if (selectedObjectData != null)
            {
                if (!IsZoneHintDisplayed)
                    DisplayZoneHint();

                if (obj == null)
                    return;

                if (obj != trackedObject) //new object case
                {
                    selectedObjectCollider = obj.GetComponentInChildren<Collider>();
                    if (selectedObjectCollider is MeshCollider)
                    {
                        if (!(selectedObjectCollider as MeshCollider).convex)
                        {
                            (selectedObjectCollider as MeshCollider).convex = true; //? Unity Physics.ClosestPoint works only on convex meshes
                            isConvexOverrided = true;
                        }
                    }

                    trackedObject = obj;
                    isTrackingSelectedObject = true;
                    DisplayContactSphereHint();
                }
            }
            else
            {
                if (IsZoneHintDisplayed)
                    Hide();
                if (isTrackingSelectedObject)
                {
                    if (isConvexOverrided)
                        (selectedObjectCollider as MeshCollider).convex = false;
                    isTrackingSelectedObject = false;
                    HideContactSphereHint();

                    trackedObject = null;
                }
            }
        }


        /// <summary>
        /// Set the contact sphere on the closent point available on the contact collider
        /// </summary>
        public void SetContactSphere()
        {
            contactSphere.transform.position = selectedObjectCollider.ClosestPoint(this.transform.position);
        }
    }
}