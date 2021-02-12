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
using Valve.VR;

/// <summary>
/// Select <see cref="JoystickSelectorTarget"/> instances of a given <see cref="JoystickSelectorTargetGroup"/> by using raycast.
/// </summary>
public class JoystickSelector : MonoBehaviour
{
    /// <summary>
    /// Used for debuging without building on hardware.
    /// </summary>
    [Header("Debug---------")]
    public Color joystickDebug;
    public bool selectButtonDebug;
    [Header("--------------")]

    /// <summary>
    /// Joystick to use for selection.
    /// </summary>
    public SteamVR_Action_Vector2 joystick;

    /// <summary>
    /// Controller to use.
    /// </summary>
    public SteamVR_Input_Sources controller;

    /// <summary>
    /// Button to use for selection.
    /// </summary>
    public SteamVR_Action_Boolean selectButton;

    /// <summary>
    /// Goup to select targets from.
    /// </summary>
    public JoystickSelectorTargetGroup groupToSelectTargetsFrom;

    /// <summary>
    /// Raycast origin.
    /// </summary>
    public Transform raycastOrigin;

    /// <summary>
    /// Axis orthogonal to the selection plane in local space.
    /// </summary>
    public Vector3 localSelectionPlaneNormalAxis = Vector3.up;

    /// <summary>
    /// Angular offset to add along the <see cref="localSelectionPlaneNormalAxis"/> to the <see cref="GetJoystickValue"/> value.
    /// </summary>
    public float angularOffset = 0;

    /// <summary>
    /// Time spent between every selection update (low values can lead to performance issues).
    /// </summary>
    [Range(0f, 1f)]
    public float updatePeriod = 0.2f;

    public float deadzone = 0.1f;

    /// <summary>
    /// Is the selector currently enabled ?
    /// </summary>
    public bool isActivated { get; protected set; } = false;


    /// <summary>
    /// Internal update routine running when the selector is activated
    /// </summary>
    /// <see cref="Activate"/>
    /// <see cref="SelectionUpdate"/>
    protected Coroutine selectionUpdate = null;



    /// <summary>
    /// Return the joystick's value using <see cref="joystickDebug"/> in editor mode, and the real hardware value otherwise.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetJoystickValue()
    {
        return joystick.GetAxis(controller);

#if UNITY_EDITOR
        Color.RGBToHSV(joystickDebug, out float H, out float S, out float V);
        return new Vector2(Mathf.Cos(H * 360 * Mathf.Deg2Rad), Mathf.Sin(H * 360 * Mathf.Deg2Rad));
#else
        Debug.Log(OVRInput.Get(joystick, controller));
        return OVRInput.Get(joystick, controller);
#endif
    }


    [ContextMenu("Activate")]
    public void Activate()
    {
        if (!isActivated)
        {
            if (selectionUpdate != null)
                throw new System.Exception("Internal error !");
            
            selectionUpdate = StartCoroutine(SelectionUpdate());
            isActivated = true;
        }
    }

    [ContextMenu("Desactivate")]
    public void Desactivate()
    {
        if (isActivated)
        {
            if (selectionUpdate == null)
                throw new System.Exception("Internal error !");

            StopCoroutine(selectionUpdate);
            selectionUpdate = null;
            isActivated = false;
            lastHoveredTarget?.NotifyHoverExit();
            currentSelectedTarget?.NotifyHoverExit();
        }
    }

    void OnEnable()
    {
        Activate();
    }

    void OnDisable()
    {
        Desactivate();
    }

    JoystickSelectorTarget lastHoveredTarget = null;
    JoystickSelectorTarget currentSelectedTarget = null;
    bool canSelect = true;

    protected IEnumerator SelectionUpdate()
    {
        while (true)
        {
            if (canSelect)
            {
                HoverAndSelect();
            }
            yield return new WaitForSeconds(updatePeriod);
        }
    }

    private void HoverAndSelect()
    {
        //Joystick Y axis remapped and in global frame.
        Vector3 remapedYAxis = this.transform.TransformDirection(
            Quaternion.Euler(angularOffset * localSelectionPlaneNormalAxis)
            * Vector3.ProjectOnPlane(Vector3.forward, localSelectionPlaneNormalAxis)
            ).normalized;

        //Joystick X axis remapped and in global frame.
        Vector3 remapedXAxis = Vector3.Cross(this.transform.TransformDirection(localSelectionPlaneNormalAxis), remapedYAxis).normalized;

        Vector2 joystickValue = GetJoystickValue();

        if (joystickValue.magnitude > deadzone)
        {
            Vector3 rayDirection = joystickValue.x * remapedXAxis + joystickValue.y * remapedYAxis;

            bool foundSomething = false;
            RaycastHit[] hits = Physics.RaycastAll(raycastOrigin.position, rayDirection);
            Debug.DrawRay(raycastOrigin.position, rayDirection, Color.red);
            foreach (RaycastHit hit in hits)
            {
                JoystickSelectorTarget target = hit.transform.GetComponent<JoystickSelectorTarget>();
                if (target == null)
                    target = hit.transform.GetComponentInParent<JoystickSelectorTarget>();

                if (target != null) //a target is pointed at
                {
                    if (lastHoveredTarget == null) //new target, no previous one
                    {
                        target.NotifyHoverEnter();
                    }
                    else
                    {
                        if (target.GetInstanceID() == lastHoveredTarget.GetInstanceID()) //stayed on the same target
                        {
                            //nothing to do.
                        }
                        else //switched from one target to another
                        {
                            lastHoveredTarget.NotifyHoverExit();
                            target.NotifyHoverEnter();
                        }
                    }
                    lastHoveredTarget = target;
                    foundSomething = true;
                    break;
                }
            }
            if (!foundSomething) //nothing is pointed at
            {
                if (lastHoveredTarget != null) //first frame after leaving the previous target
                {
                    lastHoveredTarget.NotifyHoverExit();
                    lastHoveredTarget = null;
                }
            }

        }
        else if (lastHoveredTarget != null)
        {
            lastHoveredTarget.NotifyHoverExit();
            lastHoveredTarget = null;
        }

        if (selectButton.GetState(controller) || selectButtonDebug)
        {
            if (lastHoveredTarget != null && currentSelectedTarget == null)
            {
                lastHoveredTarget.Select();
                currentSelectedTarget = lastHoveredTarget;
                StartCoroutine(ResetCanSelect());
            }
            selectButtonDebug = false;
        }
        else
        {
            if (currentSelectedTarget != null)
                currentSelectedTarget.OnDeselect();
            currentSelectedTarget = null;
        }
    }

    /// <summary>
    /// When a selection is performed, disable a new selection or a new hover while users do not realease selection Button or during a minimum waiting time.
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetCanSelect()
    {
        canSelect = false;
        float waitingTime = 0.0f;
        while (selectButton.GetState(controller) && waitingTime < .8f)
        {
            waitingTime += Time.deltaTime;
            yield return null;
        }
        canSelect = true;
    }
}
