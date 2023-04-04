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
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk;
using umi3d.cdk.interaction;
using umi3d.common;
using umi3dBrowsers.interaction.selection;
using umi3dBrowsers.interaction.selection.cursor;
using umi3dBrowsers.interaction.selection.zoneselection;
using umi3dVRBrowsersBase.ui;
using UnityEngine;
using UnityEngine.Events;
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
        public bool IsDisplayed { get => _isDisplayed; }

        private bool _isDisplayed = false;

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

        #region fading

        /// <summary>
        /// Duration of the fadingg animation.
        /// </summary>
        protected readonly float fadeDuration = 0.25f;

        private float savedAlphaDefaultMaterial;
        private Coroutine fadeDefaultMaterialCoroutine;

        private float savedAlphaSelectedMaterial;
        private Coroutine fadeSelectedMaterialCoroutine;

        private bool fadeCoroutineRunning;

        #endregion fading

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

        public static UnityEvent<uint> changelastBone = new UnityEvent<uint>();

        protected virtual void Awake()
        {
            controller = GetComponentInParent<VRController>();
            laserObjectRenderer = laserObject.GetComponent<Renderer>();
            impactPointRenderer = impactPoint.GetComponent<Renderer>();

            if (impactPointRenderer != null)
                impactPointRenderer.material = defaultMaterial;

            if (laserObjectRenderer != null)
            {
                laserObjectRenderer.material = defaultMaterial;
                savedAlphaDefaultMaterial = defaultMaterial.color.a;
                savedAlphaSelectedMaterial = selectionMaterial.color.a;
            }

            SetInfinitePoint();

            OnCursorEnter.AddListener((PointingInfo trackingInfo) =>
            {
                if (controller != (trackingInfo.controller as VRController))
                    return;

                trackingInfo.targetContainer.Interactable.HoverEnter(
                    controller.bone.boneType,
                    controller.bone.transform.position,
                    new Vector4(controller.bone.transform.rotation.x, controller.bone.transform.rotation.y, controller.bone.transform.rotation.z, controller.bone.transform.rotation.w),
                    trackingInfo.targetContainer.Interactable.id,
                    trackingInfo.targetContainer.transform.InverseTransformPoint(trackingInfo.raycastHit.point),
                    trackingInfo.targetContainer.transform.InverseTransformDirection(trackingInfo.raycastHit.normal),
                    trackingInfo.directionWorld);

                if (trackingInfo.target.dto.HoverEnterAnimationId != 0)
                {
                    changelastBone.Invoke(controller.bone.boneType);
                    StartAnim(trackingInfo.target.dto.HoverEnterAnimationId);
                }
            });

            OnCursorExit.AddListener((PointingInfo trackingInfo) =>
            {
                if (controller != (trackingInfo.controller as VRController))
                    return;

                if (!IsNullOrDestroyed(trackingInfo.targetContainer))
                {
                    trackingInfo.targetContainer.Interactable.HoverExit(
                        controller.bone.boneType,
                        controller.bone.transform.position,
                        new Vector4(controller.bone.transform.rotation.x, controller.bone.transform.rotation.y, controller.bone.transform.rotation.z, controller.bone.transform.rotation.w),
                        trackingInfo.targetContainer.Interactable.id,
                        trackingInfo.targetContainer.transform.InverseTransformPoint(trackingInfo.raycastHit.point),
                        trackingInfo.targetContainer.transform.InverseTransformDirection(trackingInfo.raycastHit.normal),
                        trackingInfo.directionWorld);
                }
                else // It means the object hovered has been destroyed
                {
                    var hoverDto = new umi3d.common.interaction.HoverStateChangedDto()
                    {
                        toolId = trackingInfo.targetContainer.Interactable.id,
                        hoveredObjectId = trackingInfo.targetContainer.Interactable.id,
                        boneType = controller.bone.boneType,
                        state = false,
                        normal = Vector3.zero,
                        position = Vector3.zero,
                        direction = Vector3.zero
                    };
                    UMI3DClientServer.SendData(hoverDto, true);
                }

                if (trackingInfo.target.dto.HoverExitAnimationId != 0)
                {
                    changelastBone.Invoke(controller.bone.boneType);
                    StartAnim(trackingInfo.target.dto.HoverExitAnimationId);
                }
            });

            OnCursorStay.AddListener((PointingInfo trackingInfo) =>
            {
                if (controller != (trackingInfo.controller as VRController))
                    return;

                trackingInfo.targetContainer.Interactable.Hovered(
                    controller.bone.boneType,
                    controller.bone.transform.position,
                    new Vector4(controller.bone.transform.rotation.x, controller.bone.transform.rotation.y, controller.bone.transform.rotation.z, controller.bone.transform.rotation.w),
                    trackingInfo.targetContainer.Interactable.id,
                    trackingInfo.targetContainer.transform.InverseTransformPoint(trackingInfo.raycastHit.point),
                    trackingInfo.targetContainer.transform.InverseTransformDirection(trackingInfo.raycastHit.normal),
                    trackingInfo.directionWorld);
            });
        }

        protected async void StartAnim(ulong id)
        {
            var anim = UMI3DAbstractAnimation.Get(id);
            if (anim != null)
            {
                await anim.SetUMI3DProperty(
                    new SetUMI3DPropertyData(
                         new SetEntityPropertyDto()
                         {
                             entityId = id,
                             property = UMI3DPropertyKeys.AnimationPlaying,
                             value = true
                         },
                        UMI3DEnvironmentLoader.GetEntity(id))
                    );
                anim.Start();
            }
        }

        public static bool IsNullOrDestroyed(System.Object obj)
        {
            if (object.ReferenceEquals(obj, null)) return true;

            if (obj is UnityEngine.Object) return (obj as UnityEngine.Object) == null;

            return false;
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
            if (_isDisplayed)
                return;
            _isDisplayed = true;
            laserObjectRenderer.enabled = true;
            impactPointRenderer.enabled = true;
            FadeIn();
        }

        /// <inheritdoc/>
        public override void Hide()
        {
            if (!_isDisplayed)
                return;
            _isDisplayed = false;
            FadeOut();
        }

        private void FadeOut()
        {
            if (fadeCoroutineRunning)
            {
                StopCoroutine(fadeDefaultMaterialCoroutine);
                StopCoroutine(fadeSelectedMaterialCoroutine);
                fadeCoroutineRunning = false;
            }

            fadeDefaultMaterialCoroutine = StartCoroutine(FadingCoroutine(fadeDuration, defaultMaterial, targetAlpha: 0f));
            fadeSelectedMaterialCoroutine = StartCoroutine(FadingCoroutine(fadeDuration, selectionMaterial, targetAlpha: 0f));
        }

        private void FadeIn()
        {
            if (fadeCoroutineRunning)
            {
                StopCoroutine(fadeDefaultMaterialCoroutine);
                StopCoroutine(fadeSelectedMaterialCoroutine);
                fadeCoroutineRunning = false;
            }

            fadeDefaultMaterialCoroutine = StartCoroutine(FadingCoroutine(fadeDuration, defaultMaterial, targetAlpha: savedAlphaDefaultMaterial));
            fadeSelectedMaterialCoroutine = StartCoroutine(FadingCoroutine(fadeDuration, selectionMaterial, targetAlpha: savedAlphaSelectedMaterial));
        }

        private IEnumerator FadingCoroutine(float duration, Material material, float targetAlpha)
        {
            fadeCoroutineRunning = true;
            var timeStart = Time.time;
            var baseAlpha = material.color.a;

            while (Time.time < timeStart + duration && material.color.a != targetAlpha)
            {
                float value = baseAlpha + ((Time.time - timeStart) / duration) * (targetAlpha - baseAlpha);
                material.color = new Color(material.color.r,
                                            material.color.g,
                                            material.color.b,
                                            value);
                yield return new WaitForEndOfFrame();
            }
            material.color = new Color(material.color.r,
                                        material.color.g,
                                        material.color.b,
                                        targetAlpha);
            fadeCoroutineRunning = false;
        }

        private void OnDestroy()
        {
            // Put back original values in materials because they are serialized.
            selectionMaterial.color = new Color(selectionMaterial.color.r,
                                       selectionMaterial.color.g,
                                       selectionMaterial.color.b,
                                       savedAlphaSelectedMaterial);

            defaultMaterial.color = new Color(defaultMaterial.color.r,
                                       defaultMaterial.color.g,
                                       defaultMaterial.color.b,
                                       savedAlphaDefaultMaterial);
        }
    }
}