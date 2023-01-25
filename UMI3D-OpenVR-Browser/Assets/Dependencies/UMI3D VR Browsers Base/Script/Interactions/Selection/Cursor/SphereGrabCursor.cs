/*
Copyright 2019 - 2023 Inetum
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

using System.Collections;
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
        public bool IsZoneHintDisplayed { get => _isDisplayed; }

        private bool _isDisplayed = false;

        #region fading

        /// <summary>
        /// Duration of the fading growth animation.
        /// </summary>
        protected float fadeDuration = 0.1f;

        private Vector3 defaultSphereScale;

        private bool growCoroutineRunning;

        private Coroutine growCoroutine;

        #endregion fading

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
            defaultSphereScale = cursorSphere.transform.localScale;
            cursorCollider = cursorSphere.GetComponentInChildren<SphereCollider>();
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
            if (_isDisplayed)
                return;
            DisplayZoneHint();
            DisplayContactSphereHint();
        }

        public void DisplayZoneHint()
        {
            cursorRenderer.enabled = true;
            _isDisplayed = true;

            if (growCoroutineRunning)
                StopCoroutine(growCoroutine);
            growCoroutine = StartCoroutine(Grow(defaultSphereScale, fadeDuration));
        }

        public IEnumerator Grow(Vector3 targetScale, float duration)
        {
            growCoroutineRunning = true;
            float startTime = Time.time;
            Vector3 baseScale = cursorSphere.transform.localScale;
            while (Time.time < startTime + duration && cursorSphere.transform.localScale != targetScale)
            {
                cursorSphere.transform.localScale = Vector3.Slerp(baseScale, targetScale, (Time.time - startTime) / fadeDuration);
                yield return new WaitForEndOfFrame();
            }
            cursorSphere.transform.localScale = targetScale;
            growCoroutineRunning = false;
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
            _isDisplayed = false;

            if (growCoroutineRunning)
                StopCoroutine(growCoroutine);
            growCoroutine = StartCoroutine(Grow(Vector3.zero, fadeDuration));
        }

        public void HideContactSphereHint()
        {
            contactSphereRenderer.enabled = false;
            contactSphere.transform.position = this.transform.position;
        }

        #endregion Display

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
                    isConvexOverrided = false;

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
                    if (isConvexOverrided && selectedObjectCollider is MeshCollider meshCollider)
                        meshCollider.convex = false;
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