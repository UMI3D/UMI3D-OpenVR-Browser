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
using System.Linq;
using umi3d.cdk.interaction;
using umi3dBrowsers.interaction.selection;
using umi3dBrowsers.interaction.selection.cursor;
using umi3dBrowsers.interaction.selection.zoneselection;
using umi3dVRBrowsersBase.ui;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.interactions.selection.cursor
{
    /// <summary>
    /// Display a laser using a cylinder (has to be configured directly in the scene).
    /// </summary>
    public class RayCursor : AbstractPointingCursor
    {
        [Header("Laser"), SerializeField, Tooltip("Laser's cylindric part.")]
        public GameObject laserObject;

        private Renderer laserObjectRenderer;

        [Header("ImpactPoint"), SerializeField, Tooltip("Laser's impact sphere.")]
        private GameObject impactPoint;

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
        [SerializeField, Tooltip("Color when no intended object are detected.")]
        private Material defaultMaterial;

        /// <summary>
        /// Color when an object is selected
        /// </summary>
        [SerializeField, Tooltip("Color when an object is selected.")]
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

        private RaySelectionZone<Selectable> raycastHelperSelectable;
        private RaySelectionZone<InteractableContainer> raycastHelperInteractable;
        private RaySelectionZone<AbstractClientInteractableElement> raycastHelperElement;

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

            OnCursorEnter.AddListener((PointingInfo trackingInfo) =>
            {
                if (controller != (trackingInfo.controller as VRController))
                    return;

                trackingInfo.targetContainer.Interactable.HoverEnter(
                    controller.bone.boneType,
                    trackingInfo.targetContainer.Interactable.id,
                    trackingInfo.targetContainer.transform.InverseTransformPoint(trackingInfo.raycastHit.point),
                    trackingInfo.targetContainer.transform.InverseTransformDirection(trackingInfo.raycastHit.normal),
                    trackingInfo.directionWorld);
            });

            OnCursorExit.AddListener((PointingInfo trackingInfo) =>
            {
                if (controller != (trackingInfo.controller as VRController))
                    return;


                trackingInfo.targetContainer.Interactable.HoverExit(
                    controller.bone.boneType,
                    trackingInfo.targetContainer.Interactable.id,
                    trackingInfo.targetContainer.transform.InverseTransformPoint(trackingInfo.raycastHit.point),
                    trackingInfo.targetContainer.transform.InverseTransformDirection(trackingInfo.raycastHit.normal),
                    trackingInfo.directionWorld);
            });

            OnCursorStay.AddListener((PointingInfo trackingInfo) =>
            {
                if (controller != (trackingInfo.controller as VRController))
                    return;

                trackingInfo.targetContainer.Interactable.Hovered(
                    controller.bone.boneType,
                    trackingInfo.targetContainer.Interactable.id,
                    trackingInfo.targetContainer.transform.InverseTransformPoint(trackingInfo.raycastHit.point),
                    trackingInfo.targetContainer.transform.InverseTransformDirection(trackingInfo.raycastHit.normal),
                    trackingInfo.directionWorld);
            });
        }

        /// <summary>
        /// True if the object is below the max length of the ray
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private bool IsAtReach(object obj, float distance)
        {
            return obj != null && distance < maxLength;
        }

        public void Update()
        {
            raycastHelperSelectable = new RaySelectionZone<Selectable>(transform.position, transform.up);
            raycastHelperInteractable = new RaySelectionZone<InteractableContainer>(transform.position, transform.up);
            raycastHelperElement = new RaySelectionZone<AbstractClientInteractableElement>(transform.position, transform.up);

            //Update impact point position
            var closestInteractable = raycastHelperInteractable.GetClosestAndRaycastHit();
            var closestSelectable = raycastHelperSelectable.GetClosestAndRaycastHit();
            var closestElement = raycastHelperElement.GetClosestAndRaycastHit();

            var listObjects = new List<KeyValuePair<object, RaycastHit>>()
            {
                new KeyValuePair<object, RaycastHit>(closestInteractable.obj, closestInteractable.raycastHit),
                new KeyValuePair<object, RaycastHit>(closestSelectable.obj, closestSelectable.raycastHit),
                new KeyValuePair<object, RaycastHit>(closestElement.obj, closestElement.raycastHit),
            };

            var listReachableObjects = listObjects.Where(x => IsAtReach(x.Key, x.Value.distance));

            if (listReachableObjects.Any())
                SetImpactOnClosest(listReachableObjects.Select(x => x.Value));
            else
                SetInfinitePoint();

            //cache cursor tracking info
            lastTrackingInfo = trackingInfo;

            var isHitting = closestInteractable.obj != null
                && ((closestInteractable.obj.Interactable.InteractionDistance < 0)
                        || closestInteractable.obj.Interactable.InteractionDistance >= (closestInteractable.obj.transform.position - controller.transform.position).magnitude);

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
                    direction = transform.InverseTransformDirection(raycastHelperInteractable.direction)
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
                    directionWorld = raycastHelperInteractable.direction,
                    direction = transform.InverseTransformDirection(raycastHelperInteractable.direction)
                };
            }

            //trigger events relatively to position
            if (closestInteractable.obj != null && closestInteractable.obj != lastTrackingInfo.targetContainer) //new object
            {
                if (lastTrackingInfo.isHitting) //from one object to another
                    OnCursorExit.Invoke(lastTrackingInfo);

                if (IsCloseEnough(closestInteractable.obj))
                    OnCursorEnter.Invoke(trackingInfo);
            }
            else if (closestInteractable.obj != null && closestInteractable.obj == lastTrackingInfo.targetContainer) //same object
            {
                if (IsCloseEnough(closestInteractable.obj))
                    OnCursorStay.Invoke(trackingInfo);
                else if (lastTrackingInfo.isHitting)
                    OnCursorExit.Invoke(lastTrackingInfo);
            }
            else if (lastTrackingInfo.isHitting) //from one object to no object
            {
                OnCursorExit.Invoke(lastTrackingInfo);
            }
        }

        /// <summary>
        /// Returns true if <paramref name="i"/> is close enough to controller to be triggered.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private bool IsCloseEnough(InteractableContainer container)
        {
            if (container == null || container.Interactable == null)
                return false;
            return (container.Interactable.InteractionDistance < 0) ||
                (container.Interactable.InteractionDistance >= (container.transform.position - controller.transform.position).magnitude);
        }

        /// <inheritdoc/>
        public override void ChangeAccordingToSelection(AbstractSelectionData selectedObject)
        {
            var selectedInfo = selectedObject as SelectionIntentData;
            if (IsDisplayed)
            {
                if (isChanged && selectedInfo == null)
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
        /// Look for the best raycasthit and set the impact point
        /// </summary>
        /// <param name="raycastHits"></param>
        private void SetImpactOnClosest(IEnumerable<RaycastHit> raycastHits)
        {
            var minDist = raycastHits.Min(x => x.distance);
            foreach (RaycastHit rh in raycastHits)
            {
                if (rh.distance == minDist)
                {
                    SetImpactPoint(rh.point);
                    break;
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