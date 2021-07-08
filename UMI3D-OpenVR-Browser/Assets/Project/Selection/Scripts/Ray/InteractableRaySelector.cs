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
using System.Collections;
using UnityEngine;
using umi3d.cdk;
using umi3d.common;
using UnityEngine.Events;
using umi3d.cdk.interaction;
using umi3d.common.interaction;
using umi3d.cdk.userCapture;
using Valve.VR;

/// <summary>
/// Ray controller for PickListener and HoverListener
/// </summary>
public class InteractableRaySelector : RaySelector<InteractableContainer>
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
    private uint boneId;

    private Coroutine hoverCoroutine;
    private Vector3 lastHoveredPos;
    private Vector3 lastHoveredNormal;
    private Vector3 lastHoveredDirection;
    private InteractableContainer lastActiveHoveredInteractable = null;
    private ulong lastActiveHoveredInteractableId;
    private bool shouldHover = false;

    private Interactable.Event onHoverExit = new Interactable.Event();

    [Header("Vibration")]
    public SteamVR_Action_Vibration hapticAction;
    public HapticActionSettings hapticSettings;

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

                if(interactable != null)
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

    private void Pulse()
    {
        hapticAction.Execute(0, hapticSettings.duration, hapticSettings.frequency, hapticSettings.amplitude, (controller as OpenVRController).controllerType);
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
            ulong currentHoveredId;

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
                    Pulse();
                    if (lastActiveHoveredInteractable != null)
                    {
                        lastActiveHoveredInteractable.Interactable.HoverExit(boneId, lastActiveHoveredInteractableId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
                        onHoverExit.Invoke(lastActiveHoveredInteractable.Interactable);
                        SelectionHighlight.Instance.DisableHoverHighlight(lastActiveHoveredInteractable.gameObject);
                    }
                }

                lastHoveredPos = interactableContainer.transform.InverseTransformPoint(hit.Value.point);
                lastHoveredNormal = interactableContainer.transform.InverseTransformDirection(hit.Value.normal).normalized;
                lastHoveredDirection = interactableContainer.transform.InverseTransformDirection(hit.Value.point - this.transform.position).normalized;
                shouldHover = true;
                if (!laser.hovering)
                    laser.OnHoverEnter(this.gameObject.GetInstanceID());


                lastActiveHoveredInteractable = interactableContainer;
                lastActiveHoveredInteractableId = currentHoveredId;
            }
            else if (lastActiveHoveredInteractable != null)
            {
                lastActiveHoveredInteractable.Interactable.HoverExit(boneId, lastActiveHoveredInteractableId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
                onHoverExit.Invoke(lastActiveHoveredInteractable.Interactable);
                SelectionHighlight.Instance.DisableHoverHighlight(lastActiveHoveredInteractable.gameObject);
                laser.OnHoverExit(this.gameObject.GetInstanceID());
            }
            else if (laser.hovering)
                laser.OnHoverExit(this.gameObject.GetInstanceID());

            (controller as OpenVRController).hoveredObjectId = lastActiveHoveredInteractableId;
        }
        else if (lastActiveHoveredInteractable != null)
        {
            lastActiveHoveredInteractable.Interactable.HoverExit(boneId, lastActiveHoveredInteractableId, lastHoveredPos, lastHoveredNormal, lastHoveredDirection);
            onHoverExit.Invoke(lastActiveHoveredInteractable.Interactable);
            SelectionHighlight.Instance.DisableHoverHighlight(lastActiveHoveredInteractable.gameObject);
            laser.OnHoverExit(this.gameObject.GetInstanceID());
            lastActiveHoveredInteractable = null;
            lastActiveHoveredInteractableId = 0;
            (controller as OpenVRController).hoveredObjectId = 0;
        }
        else if (laser.hovering)
            laser.OnHoverExit(this.gameObject.GetInstanceID());
    }

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

    public ulong GetLastHoveredInteractableId()
    {
        return lastActiveHoveredInteractableId;
    }
    
}
