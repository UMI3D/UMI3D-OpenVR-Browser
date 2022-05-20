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

using System.Collections;
using System.Linq;
using umi3d.cdk;
using umi3d.cdk.interaction;
using umi3d.cdk.userCapture;
using umi3d.common.interaction;
using umi3dVRBrowsersBase.interactions;
using umi3dVRBrowsersBase.ui;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.selection
{
    /// <summary>
    /// Ray controller for PickListener and HoverListener
    /// </summary>
    public class InteractableRaySelector : RaySelector<InteractableContainer>
    {
        #region Fields

        /// <summary>
        /// Handler to project found <see cref="Interactable"/>.
        /// </summary>
        public InteractableProjector interactableProjector;

        /// <summary>
        /// Associated controller.
        /// </summary>
        public AbstractController controller;

        /// <summary>
        /// How many times per second the hover event should be sent.
        /// </summary>
        [Tooltip("How many times per second the hover event should be sent.")]
        public float hoveredFPS = 30f;

        /// <summary>
        /// Associated boneType.
        /// </summary>
        public UMI3DClientUserTrackingBone boneType;

        /// <summary>
        ///  Associated boneType id.
        /// </summary>
        private uint boneId;

        /// <summary>
        /// Coroutine which handles the hover.
        /// </summary>
        private Coroutine hoverCoroutine;

        /// <summary>
        /// Previous position of last hovered element.
        /// </summary>
        private Vector3 lastHoveredPos;

        /// <summary>
        /// Previous normal of the last hovered element.
        /// </summary>
        private Vector3 lastHoveredNormal;


        /// <summary>
        /// Previous direction of the last hovered element.
        /// </summary>
        private Vector3 lastHoveredDirection;

        /// <summary>
        /// Previous <see cref="Interactable"/> hovered.
        /// </summary>
        private InteractableContainer lastActiveHoveredInteractable = null;


        /// <summary>
        /// Previous object id which had <see cref="lastActiveHoveredInteractable"/>.
        /// </summary>
        private ulong lastActiveHoveredInteractableId;


        /// <summary>
        /// Should hover ?
        /// </summary>
        private bool shouldHover = false;

        /// <summary>
        /// Event raised when a hover ends.
        /// </summary>
        private Interactable.Event onHoverExit = new Interactable.Event();

        public class HoverEvent : UnityEvent<ulong, uint> { };

        /// <summary>
        /// Event raised when an hover starts.
        /// </summary>
        [HideInInspector]
        public static HoverEvent HoverEnter = new HoverEvent();

        /// <summary>
        /// Event raised while an <see cref="Interactable"/> is hovered.
        /// </summary>
        [HideInInspector]
        public static HoverEvent HoverUpdate = new HoverEvent();

        /// <summary>
        /// Event raised when an  <see cref="Interactable"/> stopped being hovered.
        /// </summary>
        [HideInInspector]
        public static HoverEvent HoverExit = new HoverEvent();

        /// <summary>
        /// Transform of the player's camera.
        /// </summary>
        public Transform cameraTransform;

        /// <summary>
        /// Last frame position of this selector.
        /// </summary>
        public Vector3 lastPosition;

        #endregion

        #region Methods

        protected virtual void Start()
        {
            Debug.Assert(cameraTransform != null, "Set camera transform");
            Debug.Assert(controller != null, "Set controller");
            Debug.Assert(boneType != null, "Set bone");
            Debug.Assert(interactableProjector != null, "Set projector");
        }

        /// <summary>
        /// The coroutine of the UpdateHovered.
        /// </summary>
        private IEnumerator UpdateHovered()
        {
            while (gameObject != null)
            {
                if (shouldHover)
                {
                    shouldHover = false;
                    if (lastActiveHoveredInteractable != null)
                        lastActiveHoveredInteractable.Interactable.Hovered(boneId, lastActiveHoveredInteractableId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
                }
                if (hoveredFPS == 0f)
                    yield return new WaitForSeconds(1f / 60f);
                else
                    yield return new WaitForSeconds(1f / hoveredFPS);
            }
            yield return null;
        }


        /// <summary>
        /// Pick the currently pointed at pick listener (if any).
        /// </summary>
        [ContextMenu("Select")]
        public override void Select()
        {
            RaycastHit? rh = GetClosestOfAllPointedObject();
            if (rh.HasValue)
            {
                InteractableContainer interactableContainer = rh.Value.transform.GetComponent<InteractableContainer>();
                if (interactableContainer == null)
                {
                    interactableContainer = rh.Value.transform.GetComponentInParent<InteractableContainer>();
                }

                if (interactableContainer != null)
                {
                    Interactable interactable = interactableContainer.Interactable;

                    if (interactable != null)
                    {
                        if ((interactable.dto.interactions != null) && (interactable.dto.interactions.Count > 0))
                        {
                            if (interactableProjector.IsProjected(interactable))
                                return;

                            if (interactable.dto.interactions.TrueForAll(x => x is EventDto) && (interactable.dto.interactions.Count < 3))
                                return;

                            interactableProjector.Project(interactable, lastActiveHoveredInteractableId, new RequestedUsingSelector<InteractableRaySelector>() { controller = this.controller });
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Makes controller vibrate.
        /// </summary>
        private void Pulse()
        {
            AbstractControllerInputManager.Instance.VibrateController((controller as VRController).type, .5f, .2f, .2f);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void ActivateInternal()
        {
            base.ActivateInternal();
            boneId = boneType.ToDto().boneType;
            hoverCoroutine = StartCoroutine(UpdateHovered());
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void DeactivateInternal()
        {
            base.DeactivateInternal();

            if (lastActiveHoveredInteractable != null)
            {

                lastActiveHoveredInteractable.Interactable.HoverExit(boneId, lastActiveHoveredInteractableId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
                onHoverExit.Invoke(lastActiveHoveredInteractable.Interactable);

                SelectionHighlight.Instance.DisableHoverHighlight(lastActiveHoveredInteractable.gameObject);
                lastActiveHoveredInteractable = null;
                lastActiveHoveredInteractableId = 0;
                laser.OnHoverExit(this.gameObject.GetInstanceID());
            }

            StopCoroutine(hoverCoroutine);
            interactableProjector.UnProjectAll();
        }

        /// <summary>
        /// Handle hover behavior.
        /// </summary>
        private void UpdateHover()
        {
            RaycastHit? hit = GetClosestPointedObject(true);

            if (hit != null)
            {
                InteractableContainer interactableContainer = hit.Value.transform.GetComponent<InteractableContainer>();

                if (interactableContainer == null)
                {
                    interactableContainer = hit.Value.transform.GetComponentInParent<InteractableContainer>();
                }

                ulong currentHoveredId = 0;

                if (interactableContainer != null && interactableContainer.Interactable != null && interactableContainer.Interactable.Active)
                {
                    Interactable interactable = interactableContainer.Interactable;
                    currentHoveredId = UMI3DEnvironmentLoader.GetNodeID(hit.Value.collider);

                    if (interactableContainer != lastActiveHoveredInteractable)
                    {
                        if ((interactable.dto.interactions != null) && (interactable.dto.interactions.Count > 0))
                        {
                            interactableProjector.OnHover(interactableContainer, currentHoveredId, onHoverExit, controller);
                        }

                        interactable.HoverEnter(boneId, currentHoveredId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);

                        if (interactable.interactions.Count(i => i is AbstractParameterDto) > 0)
                            ParameterGear.Instance.Display(interactable, hit.Value.point, transform.position - hit.Value.point, Vector3.ProjectOnPlane(transform.position - lastPosition, cameraTransform.forward));
                        else
                            ParameterGear.Instance.Hide();

                        Pulse();

                        if (interactable.dto.HoverEnterAnimationId != 0)
                        {
                            var anim = UMI3DNodeAnimation.Get(interactable.dto.HoverEnterAnimationId);
                            HoverEnter.Invoke(currentHoveredId, boneId);
                            if (anim != null)
                                anim.Start();
                        }

                        if (lastActiveHoveredInteractable != null)
                        {
                            lastActiveHoveredInteractable.Interactable.HoverExit(boneId, lastActiveHoveredInteractableId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
                            onHoverExit.Invoke(lastActiveHoveredInteractable.Interactable);

                            if (lastActiveHoveredInteractable.Interactable.dto.HoverExitAnimationId != 0)
                            {
                                var anim = UMI3DNodeAnimation.Get(lastActiveHoveredInteractable.Interactable.dto.HoverExitAnimationId);
                                HoverExit.Invoke(lastActiveHoveredInteractableId, boneId);
                                if (anim != null)
                                    anim.Start();
                            }

                            SelectionHighlight.Instance.DisableHoverHighlight(lastActiveHoveredInteractable.gameObject);
                        }
                    }

                    lastHoveredPos = interactableContainer.transform.InverseTransformPoint(hit.Value.point);
                    lastHoveredNormal = interactableContainer.transform.InverseTransformDirection(hit.Value.normal).normalized;
                    lastHoveredDirection = interactableContainer.transform.InverseTransformDirection(hit.Value.point - this.transform.position).normalized;
                    shouldHover = true;
                    if (!laser.Hovering)
                        laser.OnHoverEnter(this.gameObject.GetInstanceID());

                    lastActiveHoveredInteractable = interactableContainer;
                    lastActiveHoveredInteractableId = currentHoveredId;

                }
                else if (lastActiveHoveredInteractable != null)
                {
                    lastActiveHoveredInteractable.Interactable.HoverExit(boneId, lastActiveHoveredInteractableId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
                    onHoverExit.Invoke(lastActiveHoveredInteractable.Interactable);
                    ParameterGear.Instance.Hide();

                    if (lastActiveHoveredInteractable.Interactable.dto.HoverExitAnimationId != 0)
                    {
                        var anim = UMI3DNodeAnimation.Get(lastActiveHoveredInteractable.Interactable.dto.HoverExitAnimationId);
                        HoverExit.Invoke(lastActiveHoveredInteractableId, boneId);
                        if (anim != null)
                            anim.Start();
                    }

                    SelectionHighlight.Instance.DisableHoverHighlight(lastActiveHoveredInteractable.gameObject);
                    laser.OnHoverExit(this.gameObject.GetInstanceID());
                }
                else if (laser.Hovering)
                    laser.OnHoverExit(this.gameObject.GetInstanceID());

                (controller as VRController).hoveredObjectId = lastActiveHoveredInteractableId;

            }
            else if (lastActiveHoveredInteractable != null)
            {
                lastActiveHoveredInteractable.Interactable.HoverExit(boneId, lastActiveHoveredInteractableId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
                onHoverExit.Invoke(lastActiveHoveredInteractable.Interactable);
                ParameterGear.Instance.Hide();

                if (lastActiveHoveredInteractable.Interactable.dto.HoverExitAnimationId != 0)
                {
                    var anim = UMI3DNodeAnimation.Get(lastActiveHoveredInteractable.Interactable.dto.HoverExitAnimationId);
                    HoverExit.Invoke(lastActiveHoveredInteractableId, boneId);
                    if (anim != null)
                        anim.Start();
                }

                SelectionHighlight.Instance.DisableHoverHighlight(lastActiveHoveredInteractable.gameObject);
                laser.OnHoverExit(this.gameObject.GetInstanceID());
                lastActiveHoveredInteractable = null;
                lastActiveHoveredInteractableId = 0;
                (controller as VRController).hoveredObjectId = 0;
            }
            else if (laser.Hovering)
                laser.OnHoverExit(this.gameObject.GetInstanceID());

            lastPosition = transform.position;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void UpdateRayDisplayer()
        {
            RaycastHit? hit = GetClosestPointedObject();
            if (hit != null)
            {
                laser.SetImpactPoint(hit.Value.point, false);
            }
            else
            {
                laser.SetInfinitePoint();
            }
        }

        protected override void Update()
        {
            base.Update();
            if (activated)
            {
                UpdateHover();
            }
        }

        /// <summary>
        /// Returns the id of the last <see cref="Interactable"/> hovered.
        /// </summary>
        /// <returns></returns>
        public ulong GetLastHoveredInteractableId()
        {
            return lastActiveHoveredInteractableId;
        }

        #endregion
    }
}