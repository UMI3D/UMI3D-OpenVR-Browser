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
        [SerializeField]
        protected Renderer cursorRenderer;

        [SerializeField]
        protected Renderer contactSphereRenderer;

        public bool IsDisplayed => cursorRenderer.enabled;

        private Collider selectedObjectCollider;

        private bool isTrackingSelectedObject;

        private MonoBehaviour trackedObject;

        public void Start()
        {
            cursorRenderer.enabled = false;
            contactSphereRenderer.enabled = false;
        }

        public void Update()
        {
            if (isTrackingSelectedObject)
            {
                SetContactSphere();
            }
        }

        public override void Display()
        {
            cursorRenderer.enabled = true;
        }

        public override void Hide()
        {
            cursorRenderer.enabled = false;
        }

        public override void ChangeAccordingToSelection(AbstractSelectionData selectedObjectData)
        {
            if (selectedObjectData != null)
            {
                if (!IsDisplayed)
                    Display();

                var objData = selectedObjectData as SelectionIntentData<MonoBehaviour>;
                if (objData.selectedObject != trackedObject) //new object case
                {
                    selectedObjectCollider = objData.selectedObject.GetComponent<Collider>();
                    SetContactSphere();
                    contactSphereRenderer.enabled = true;
                    isTrackingSelectedObject = true;
                    trackedObject = objData.selectedObject;
                }
            }
            else
            {
                if (IsDisplayed)
                    Hide();
                if (isTrackingSelectedObject)
                {
                    isTrackingSelectedObject = false;
                    contactSphereRenderer.enabled = false;
                    trackedObject = null;
                }
            }
        }

        public void SetContactSphere()
        {
            selectedObjectCollider.ClosestPoint(this.transform.position);
        }

    }
}