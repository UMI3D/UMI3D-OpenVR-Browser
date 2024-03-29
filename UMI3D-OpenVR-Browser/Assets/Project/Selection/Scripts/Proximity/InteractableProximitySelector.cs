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
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

using umi3d.cdk;
using umi3d.cdk.interaction;
using umi3d.cdk.userCapture;
using umi3d.common.interaction;

public class InteractableProximitySelector : ProximitySelector<InteractableContainer>
{
    public Transform viewport;
    public InteractableProjector interactableProjector;
    public AbstractController controller;

    /// <summary>
    /// How many times per second the hover event should be sent.
    /// </summary>
    [Tooltip("How many times per second the hover event should be sent.")]
    public float hoveredFPS = 30f;


    public UMI3DClientUserTrackingBone boneType;
    private uint boneId; //bonetype

    private Coroutine hoverCoroutine;
    private Vector3 lastHoveredPos;
    private Vector3 lastHoveredNormal;
    private Vector3 lastHoveredDirection;
    private Interactable lastHoveredInteractable = null;
    private ulong lastHoveredInteractableObjectId;
    private bool shouldHover = false;

    private Interactable.Event onHoverExit = new Interactable.Event();

    [Header("Vibration")]
    public SteamVR_Action_Vibration hapticAction;
    public HapticActionSettings hapticSettings;

    public override void Select()
    {
        InteractableContainer closest = GetClosest()?.component;
        
        if (closest != null)
        {
            if ((closest.Interactable.dto.interactions != null) && (closest.Interactable.dto.interactions.Count > 0))
            {
                if (interactableProjector.IsProjected(closest.Interactable))
                    return;

                List<AbstractInteractionDto> inters = closest.Interactable.dto.interactions;
                if ((inters.FindAll(x => x is ManipulationDto).Count == 0)
                    && (inters.FindAll(x => x is AbstractParameterDto).Count == 0)
                    && (inters.FindAll(x => x is EventDto).Count <= 2))
                    return;

                interactableProjector.Project(closest.Interactable, lastHoveredInteractableObjectId, new RequestedUsingSelector<InteractableProximitySelector>() { controller = this.controller });
            }
        }
    }


    protected override void ActivateInternal()
    {
        base.ActivateInternal();
        boneId = boneType.boneType;// ToDto().boneType;
        hoverCoroutine = StartCoroutine(UpdateHovered());
    }


    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        if (lastHoveredInteractable != null)
        {
            lastHoveredInteractable.HoverExit(boneId, lastHoveredInteractableObjectId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
            onHoverExit.Invoke(lastHoveredInteractable);
            SelectionHighlight.Instance.DisableHoverHighlight(UMI3DEnvironmentLoader.GetNode(lastHoveredInteractableObjectId).gameObject);
            lastHoveredInteractable = null;
            lastHoveredInteractableObjectId = 0;
        }

        StopCoroutine(hoverCoroutine);
        interactableProjector.UnProjectAll();
    }


    /// <summary>
    /// The coroutine of the UpdateHovered.
    /// </summary>
    IEnumerator UpdateHovered()
    {
        while (gameObject != null)
        {
            if (shouldHover)
            {
                shouldHover = false;
                if (lastHoveredInteractable != null)
                    lastHoveredInteractable.Hovered(boneId, lastHoveredInteractableObjectId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
            }
            if (hoveredFPS == 0f)
                yield return new WaitForSeconds(1f / 60f);
            else
                yield return new WaitForSeconds(1f / hoveredFPS);
        }
        yield return null;
    }

    private void Pulse()
    {
        hapticAction.Execute(0, hapticSettings.duration, hapticSettings.frequency, hapticSettings.amplitude, (controller as OpenVRController).controllerType);
    }


    /// <summary>
    /// Handle hover behavior.
    /// </summary>
    private void UpdateHover()
    {
        Closest closest = GetClosest();
        if (closest == null)
        {
            if (lastHoveredInteractable != null)
            {
                lastHoveredInteractable.HoverExit(boneId, lastHoveredInteractableObjectId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
                SelectionHighlight.Instance.DisableHoverHighlight(UMI3DEnvironmentLoader.GetNode(lastHoveredInteractableObjectId).gameObject);
                onHoverExit.Invoke(lastHoveredInteractable);
                lastHoveredInteractable = null;
                lastHoveredInteractableObjectId = 0;
            }
            return;
        }            

        Interactable interactable = closest.component.Interactable;
        if (interactable != null && interactable.Active)
        {
            if (interactable != lastHoveredInteractable)
            {
                if ((interactable.dto.interactions != null) && (interactable.dto.interactions.Count > 0))
                {
                    interactableProjector.OnHover(closest.component, lastHoveredInteractableObjectId, onHoverExit, controller);
                }
                interactable.HoverEnter(boneId, UMI3DEnvironmentLoader.GetNodeID(closest.collider), lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
                Pulse();
                if (lastHoveredInteractable != null)
                {
                    lastHoveredInteractable.HoverExit(boneId, lastHoveredInteractableObjectId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
                    SelectionHighlight.Instance.DisableHoverHighlight(UMI3DEnvironmentLoader.GetNode(lastHoveredInteractableObjectId).gameObject);
                    onHoverExit.Invoke(lastHoveredInteractable);
                }
            }

            lastHoveredPos = closest.localHoverPosition;
            lastHoveredNormal = closest.localHoverNormal;
            lastHoveredDirection = closest.collider.transform.InverseTransformDirection(closest.collider.transform.TransformPoint(closest.localHoverPosition) - this.transform.position).normalized;
            shouldHover = true;
        }
        else if (lastHoveredInteractable != null)
        {
            lastHoveredInteractable.HoverExit(boneId, lastHoveredInteractableObjectId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
            SelectionHighlight.Instance.DisableHoverHighlight(UMI3DEnvironmentLoader.GetNode(lastHoveredInteractableObjectId).gameObject);
            onHoverExit.Invoke(lastHoveredInteractable);
        }

        lastHoveredInteractable = interactable;
        lastHoveredInteractableObjectId = UMI3DEnvironmentLoader.GetNodeID(closest.collider); //check here
        (controller as OpenVRController).hoveredObjectId = lastHoveredInteractableObjectId;
    }

    protected virtual void Update()
    {
        if (activated)
        {
            UpdateHover();
        }
    }
}
