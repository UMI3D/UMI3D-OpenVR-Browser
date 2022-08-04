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

using System.Collections;
using umi3d.cdk.interaction;
using umi3d.cdk.interaction.selection;
using umi3d.cdk.interaction.selection.cursor;
using umi3d.cdk.interaction.selection.zoneselection;
using umi3dVRBrowsersBase.interactions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace umi3dbrowser.openvr.interaction.selection.cursor
{
    /// <summary>
    /// Display a laser using a cylinder (has to be configured directly in the scene).
    /// </summary>
    public class RayCursor : AbstractPointingCursor
    {
        [Header("Laser")]
        /// <summary>
        /// Ray cast from the controller
        /// </summary>
        public GameObject laserObject;

        private Renderer laserObjectRenderer;

        [Header("ImpactPoint")]
        /// <summary>
        /// Object displayed at the impact point
        /// </summary>
        public GameObject impactPoint;

        private Renderer impactPointRenderer;

        private VRController controller;

        /// <summary>
        /// True if the laser is currently displayed
        /// </summary>
        public bool IsDisplayed { get => laserObjectRenderer.enabled; }

        [Header("Materials")]
        /// <summary>
        /// Color when no intended object are detected
        /// </summary>
        [SerializeField]
        private Material defaultMaterial;

        /// <summary>
        /// Color when an object is selected
        /// </summary>
        [SerializeField]
        private Material selectionMaterial;

        /// <summary>
        /// True is the laser is altered
        /// </summary>
        private bool isChanged = false;

        /// <summary>
        /// True is the laser is infinite
        /// </summary>
        private bool isInfinite = true;

        /// <summary>
        /// Max length of the laser
        /// </summary>
        private float maxLength = 500;

        RaySelectionZone<Selectable> raycastHelperSelectable;
        RaySelectionZone<InteractableContainer> raycastHelperInteractable;

        /// <summary>
        /// Info of the interactable currently pointed at and associated raycast
        /// </summary>
        public PointingInfo trackingInfo = new PointingInfo();
        /// <summary>
        /// Info of the last interactable pointed at and associated raycast
        /// </summary>
        public PointingInfo lastTrackingInfo = new PointingInfo();

        protected virtual void Awake()
        {
            controller = GetComponentInParent<VRController>();
            laserObjectRenderer = laserObject.GetComponent<Renderer>();
            impactPointRenderer = impactPoint.GetComponent<Renderer>();

            if (impactPointRenderer != null)
                impactPointRenderer.material = defaultMaterial;

            if (laserObjectRenderer != null)
                laserObjectRenderer.material = defaultMaterial;

            SetInfinitePoint();
        }

        public void Update()
        {
            raycastHelperSelectable = new RaySelectionZone<Selectable>(transform.position, transform.up);
            raycastHelperInteractable = new RaySelectionZone<InteractableContainer>(transform.position, transform.up);

            //Update impact point position
            var closestInteractable = raycastHelperInteractable.GetClosestAndRaycastHit();
            var closestSelectable = raycastHelperSelectable.GetClosestAndRaycastHit();

            if (closestInteractable.obj != null && closestInteractable.raycastHit.distance < maxLength
                && closestSelectable.obj != null && closestInteractable.raycastHit.distance < maxLength)
            {
                if (closestInteractable.raycastHit.distance < closestSelectable.raycastHit.distance)
                    SetImpactPoint(closestInteractable.raycastHit.point);
                else
                    SetImpactPoint(closestSelectable.raycastHit.point);
            }
            else if (closestInteractable.obj != null && (closestInteractable.raycastHit.distance < maxLength))
                SetImpactPoint(closestInteractable.raycastHit.point);
            else if (closestSelectable.obj != null && closestSelectable.raycastHit.distance < maxLength)
                SetImpactPoint(closestSelectable.raycastHit.point);
            else if (!isInfinite)
                SetInfinitePoint();

            //cache cursor tracking info
            lastTrackingInfo = trackingInfo;
            var isHitting = closestInteractable.obj != null;
            if (isHitting)
            {
                trackingInfo = new PointingInfo()
                {
                    isHitting = true,
                    controller = controller,
                    target = closestInteractable.obj.Interactable,
                    targetContainer = closestInteractable.obj,
                    raycastHit = closestInteractable.raycastHit,
                    directionWorld = raycastHelperInteractable.direction,
                    direction = closestInteractable.obj.transform.InverseTransformDirection(raycastHelperInteractable.direction)
                };
            }
            else
            {
                trackingInfo = new PointingInfo()
                {
                    isHitting = false,
                    controller = controller,
                    target = default,
                    targetContainer = default,
                    directionWorld = raycastHelperInteractable.direction
                };
            }

            //trigger events relatively to position
            if (closestInteractable.obj != null && closestInteractable.obj != lastTrackingInfo.targetContainer) //new object
            {
                if (lastTrackingInfo.isHitting) //from one object to another
                    OnCursorExit.Invoke(lastTrackingInfo);
                OnCursorEnter.Invoke(trackingInfo);
            }
            else if (closestInteractable.obj != null && closestInteractable.obj == lastTrackingInfo.targetContainer) //same object
                OnCursorStay.Invoke(trackingInfo);
            else if (lastTrackingInfo.isHitting) //from one object to no object
                    OnCursorExit.Invoke(lastTrackingInfo);
        }


        /// <inheritdoc/>
        public override void ChangeAccordingToSelection(AbstractSelectionData selectedObject)
        {
            if (IsDisplayed)
            {
                if (isChanged && selectedObject == null)
                {
                    laserObjectRenderer.material = defaultMaterial;
                    impactPointRenderer.material = defaultMaterial;
                    isChanged = false;
                }
                else if (!isChanged && selectedObject != null)
                {
                    laserObjectRenderer.material = selectionMaterial;
                    impactPointRenderer.material = selectionMaterial;
                    isChanged = true;
                }
            }
        }

        /// <summary>
        /// Places the impact point at the end of the ray
        /// </summary>
        /// <param name="point"></param>
        /// <param name="displayImpact"></param>
        public void SetImpactPoint(Vector3 point, bool displayImpact = true)
        {
            impactPointRenderer.enabled = displayImpact;
            isInfinite = !displayImpact;
            impactPoint.transform.position = point;
            transform.localScale = new Vector3(transform.localScale.x, Vector3.Distance(this.transform.position, point), transform.localScale.z);
        }

        /// <summary>
        /// Remove the point from the ray
        /// </summary>
        public void SetInfinitePoint()
        {
            SetImpactPoint(this.transform.position + this.transform.up * maxLength, false);
            impactPointRenderer.enabled = false;
        }

        /// <inheritdoc/>
        public override void Display()
        {
            laserObjectRenderer.enabled = true;
            impactPointRenderer.enabled = true;
        }

        /// <inheritdoc/>
        public override void Hide()
        {
            laserObjectRenderer.enabled = false;
            impactPointRenderer.enabled = false;
        }
    }
}