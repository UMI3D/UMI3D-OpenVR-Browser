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
using umi3d.cdk;
using umi3d.cdk.interaction;
using umi3d.cdk.userCapture;
using umi3d.common.interaction;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public abstract class AbstractManipulationInput : AbstractUMI3DInput, IModifiableBindingInput
{
    /// <summary>
    /// Boolean input to trigger to activate this input.
    /// </summary>
    [SerializeField]
    public OpenVRInputObserver activationButton;

    /// <summary>
    /// Avatar bone linked to this input.
    /// </summary>
    public UMI3DClientUserTrackingBone bone;

    /// <summary>
    /// Frame rate applied to message emission through network (high values can cause network flood).
    /// </summary>
    public float networkFrameRate = 1;

    /// <summary>
    /// Input multiplicative strength.
    /// </summary>
    public float translationStrenght = 1;

    /// <summary>
    /// Input multiplicative strength.
    /// </summary>
    public float rotationStrenght = 200;

    /// <summary>
    /// First argument is the frame of reference, the second is the dof.
    /// </summary>
    [System.Serializable]
    public class ManipulationInfo : UnityEvent<Transform, DofGroupEnum> { }
    public ManipulationInfo onActivation = new ManipulationInfo();
    public UnityEvent onDesactivation = new UnityEvent();



    /// <summary>
    /// Frame of reference of the <see cref="associatedManipulation"/> (if any).
    /// </summary>
    protected Transform frameOfReference;

    /// <summary>
    /// Launched coroutine for network message sending (if any).
    /// </summary>
    /// <see cref="NetworkMessageSender"/>
    protected Coroutine messageSenderCoroutine;

    /// <summary>
    /// Associtated interaction (if any).
    /// </summary>
    protected ManipulationDto associatedManipulation = null;

    /// <summary>
    /// Associtated interaction's choosen dof (if any).
    /// </summary>
    protected DofGroupEnum associatedManipulationDof;

    [Tooltip("This material which will be used on the controller and in the menu to show that this input is associated to an interaction")]
    public Material highlightMat;

    /// <summary>
    /// Menu item displayed in the controller menu.
    /// </summary>
    public BindingMenuItem menuItem;

    private OpenVRController openVRController;

    bool isDown = false;

    string associatedToolId;

    string currentHoveredObjectId;

    public bool IsInputBeeingModified { get; set; }


    public abstract bool IsCompatibleWith(DofGroupEnum dofs);

    public override bool IsCompatibleWith(AbstractInteractionDto interaction)
    {
        return (interaction is ManipulationDto) &&
            (interaction as ManipulationDto).dofSeparationOptions.Exists(sep => sep.separations.Exists(dof => IsCompatibleWith(dof.dofs)));
    }

    public override bool IsAvailable()
    {
        return (associatedManipulation == null) && !isDown;
    }

    public override AbstractInteractionDto CurrentInteraction()
    {
        return associatedManipulation;
    }


    public override void Init(AbstractController controller)
    {
        base.Init(controller);

        if (messageSenderCoroutine != null)
        {
            StopCoroutine(messageSenderCoroutine);
            onInputUp.Invoke();
            messageSenderCoroutine = null;
        }
    }

    public override void Associate(ManipulationDto manipulation, DofGroupEnum dofs, string toolId, string hoveredObjectId)
    {
        if (associatedManipulation != null)
        {
            throw new System.Exception("This input is already binded to a interaction ! (" + associatedManipulation + ")");
        }

        if (IsCompatibleWith(dofs))
        {
            associatedManipulation = manipulation;
            associatedManipulationDof = dofs;
            associatedToolId = toolId;
            currentHoveredObjectId = hoveredObjectId;

            GameObject frame = UMI3DEnvironmentLoader.GetNode(manipulation.frameOfReference).gameObject;
            if (frame == null)
                throw new System.Exception("No frame of reference found for this manipulation");
            else
                frameOfReference = frame.transform;

            activationButton.AddOnStateDownListener(ActivationButton_onStateDown);
            activationButton.AddOnStateUpListener(ActivationButton_onStateUp);
            onActivation.Invoke(frameOfReference, dofs);

            PlayerMenuManager player = PlayerMenuManager.FindInstanceAssociatedToController(activationButton.controller);

            if (player == null)
                Debug.LogError("Player menu manager should no be null");

            ControllerHintDisplayer.DisplayHint(activationButton.button, activationButton.controller, highlightMat, manipulation.name);
            DisplayBindingInMenu(manipulation, toolId, hoveredObjectId, player);
            openVRController = player.controller;
        }
        else
        {
            throw new System.Exception("Trying to associate an uncompatible interaction !" + dofs);
        }
    }

    public override void Associate(AbstractInteractionDto interaction, string toolId, string hoveredObjectId)
    {
        if (associatedManipulation != null)
        {
            throw new System.Exception("This input is already binded to a interaction ! (" + associatedManipulation + ")");
        }

        if (IsCompatibleWith(interaction))
        {
            foreach (DofGroupOptionDto group in (interaction as ManipulationDto).dofSeparationOptions)
            {
                foreach (DofGroupDto sep in group.separations)
                {
                    if (IsCompatibleWith(sep.dofs))
                    {
                        Associate(interaction as ManipulationDto, sep.dofs, toolId, hoveredObjectId);
                        return;
                    }
                }
            }
        }
        else
        {
            throw new System.Exception("Trying to associate an uncompatible interaction !");
        }
    }

    public override void Dissociate()
    {
        if (messageSenderCoroutine != null)
        {
            StopCoroutine(messageSenderCoroutine);
            onInputUp.Invoke();
            messageSenderCoroutine = null;
        }

        associatedManipulation = null;
        activationButton.RemoveOnStateDownListener(ActivationButton_onStateDown);
        activationButton.RemoveOnStateUpListener(ActivationButton_onStateUp);
        ControllerHintDisplayer.HideHint(activationButton.button, activationButton.controller);
        HideBindingInMenu();
        onDesactivation.Invoke();

        if (isDown)
        {
            openVRController.IsInputPressed = false;
            isDown = false;
        }
    }

    protected virtual void ActivationButton_onStateUp()
    {
        if (messageSenderCoroutine != null)
        {
            StopCoroutine(messageSenderCoroutine);
            onInputUp.Invoke();
            messageSenderCoroutine = null;
        }
        openVRController.IsInputPressed = false;
        isDown = false;
    }

    protected virtual void ActivationButton_onStateDown()
    {
        if (messageSenderCoroutine != null)
            StopCoroutine(messageSenderCoroutine);

        messageSenderCoroutine = StartCoroutine(NetworkMessageSender());
        onInputDown.Invoke();
        openVRController.IsInputPressed = true;
        isDown = true;
    }



    protected abstract ManipulationRequestDto ComputeManipulationArgument(DofGroupEnum dofs);

    protected IEnumerator NetworkMessageSender()
    {
        while (associatedManipulation != null)
        {
            if (true)//!PlayerMenuManager.IsDisplaying())
            {
                ManipulationRequestDto arg = ComputeManipulationArgument(associatedManipulationDof);

                arg.boneType = bone.boneType;
                arg.id = associatedManipulation.id;
                arg.toolId = associatedToolId;
                arg.hoveredObjectId = currentHoveredObjectId;
                UMI3DClientServer.SendData(arg, true);
            }
            yield return new WaitForSeconds(1f / networkFrameRate);
        }
    }

    /// <summary>
    /// Displays the current binding in the menu of the associated controller.
    /// </summary>
    /// <param name="associatedColor"></param>
    private void DisplayBindingInMenu(ManipulationDto manipulation, string toolId, string hoveredObjectId, PlayerMenuManager player)
    {
        menuItem = new BindingMenuItem
        {
            Name = associatedManipulationDof.ToString(),
            associatedMaterial = highlightMat,
            Holdable = true,
            toolId = toolId,
            hoveredObjectId = hoveredObjectId,
            associatedInteractionDto = manipulation,
            associatedInput = this,
            dofs = associatedManipulationDof
        };
        player.AddMenuItemToParamatersMenu(menuItem);
    }

    /// <summary>
    /// Hides the current binding in the menu of the associated controller.
    /// </summary>
    private void HideBindingInMenu()
    {
        if (menuItem != null)
        {
            PlayerMenuManager player = PlayerMenuManager.FindInstanceAssociatedToController(activationButton.controller);

            if (player == null)
                return;
            player.RemoveItemFromParametersMenu(menuItem);
        }
    }

    public string GetCurrentButtonName()
    {
        if (activationButton != null)
            return activationButton.button.ToString();
        else
            return string.Empty;
    }

    public OpenVRInputObserver GetOpenVRObserverObersver()
    {
        return activationButton;
    }

    public BindingMenuItem GetBindingMenuItem()
    {
        return menuItem;
    }
}
