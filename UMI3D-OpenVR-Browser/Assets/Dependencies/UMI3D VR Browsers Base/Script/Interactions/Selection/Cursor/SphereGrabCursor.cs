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

using umi3d.cdk.interaction;
using umi3dBrowsers.interaction.selection;
using umi3dBrowsers.interaction.selection.cursor;
using umi3dVRBrowsersBase.ui;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.interactions.selection.cursor
{
    /// <summary>
    /// Cursor for proximity selection <br/>
    /// To implement
    /// </summary>
    public class SphereGrabCursor : AbstractCursor
    {
        [SerializeField]
        protected GameObject cursorSphere;

        protected Renderer cursorRenderer;

        [SerializeField]
        protected GameObject contactSphere;

        protected Renderer contactSphereRenderer;

        public bool IsDisplayed => cursorRenderer.enabled;

        private Collider selectedObjectCollider;

        private bool isTrackingSelectedObject;

        private MonoBehaviour trackedObject;

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

                MonoBehaviour obj = null;
                obj = (selectedObjectData as SelectionIntentData<InteractableContainer>)?.selectedObject;
                //if (obj == null)
                //    obj = (selectedObjectData as SelectionIntentData<Selectable>)?.selectedObject;
                //if (obj == null)
                //    obj = (selectedObjectData as SelectionIntentData<AbstractClientInteractableElement>)?.selectedObject;
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
                    SetContactSphere();
                    contactSphereRenderer.enabled = true;
                }
            }
            else
            {
                if (IsDisplayed)
                    Hide();
                if (isTrackingSelectedObject)
                {
                    if (isConvexOverrided)
                        (selectedObjectCollider as MeshCollider).convex = false;
                    isTrackingSelectedObject = false;
                    contactSphereRenderer.enabled = false;
                    trackedObject = null;
                }
            }
        }

        public void SetContactSphere()
        {
            contactSphere.transform.position = selectedObjectCollider.ClosestPoint(this.transform.position);
        }

    }
}