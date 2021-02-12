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
using umi3d.cdk;
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using umi3d.common.interaction;
using umi3d.cdk.interaction;
using umi3d.cdk.userCapture;

[System.Serializable]
public class BooleanInput : AbstractUMI3DInput
{
    /// <summary>
    /// Oculus input observer binded to this input.
    /// </summary>
    public OpenVRInputObserver inputObserver;


    /// <summary>
    /// Avatar bone linked to this input.
    /// </summary>
    public UMI3DClientUserTrackingBone bone;


    /// <summary>
    /// Associtated interaction (if any).
    /// </summary>
    protected AbstractInteractionDto associatedInteraction;

    /// <summary>
    /// Event raised on user input release (first frame).
    /// </summary>
    [SerializeField]
    protected UnityEvent onActionUp = new UnityEvent();

    /// <summary>
    /// Event raised on user input (first frame only).
    /// </summary>
    [SerializeField]
    protected UnityEvent onActionDown = new UnityEvent();

    /// <summary>
    /// Menu item displayed in the controller menu.
    /// </summary>
    public BindingMenuItem menuItem;


    /// <summary>
    /// True if the rising edge event has been sent through network (to avoid sending falling edge only).
    /// </summary>
    private bool risingEdgeEventSent = false;

    [Tooltip("This material which will be used on the controller and in the menu to show that this input is associated to an interaction")]
    public Material highlightMat;

    /// <summary>
    /// If true, users are modifying input bindings so this input should not trigger anything.
    /// </summary>
    public bool isInputBeeingModified = false;

    /// <summary>
    /// Callback called on oculus input up.
    /// </summary>
    /// <param name="fromAction"></param>
    /// <param name="fromSource"></param>
    /// <see cref="Associate(AbstractInteractionDto)"/>
    private void OculusInput_onStateUp()
    {
        if (isInputBeeingModified)
            return;

        onActionUp.Invoke();
    }

    /// <summary>
    /// Callback called on oculus input down.
    /// </summary>
    /// <param name="fromAction"></param>
    /// <param name="fromSource"></param>
    /// <see cref="Associate(AbstractInteractionDto)"/>
    private void OculusInput_onStateDown()
    {
        if (isInputBeeingModified)
            return;

        onActionDown.Invoke();
    }


    public override void Associate(AbstractInteractionDto interaction, string toolId, string hoveredObjectId)
    {
        if (associatedInteraction != null)
        {
            throw new System.Exception("This input is already binded to a interaction ! (" + associatedInteraction + ")");
        }

        if (IsCompatibleWith(interaction))
        {
            inputObserver.AddOnStateUpListener(OculusInput_onStateUp);
            inputObserver.AddOnStateDownListener(OculusInput_onStateDown);

            PlayerMenuManager player = PlayerMenuManager.FindInstanceAssociatedToController(inputObserver.controller);

            if (player == null)
                Debug.LogError("Player menu manager should no be null");

            UnityAction<bool> action = (bool pressDown) =>
            {
                if (pressDown)
                {
                    if ((interaction as EventDto).hold)
                    {
                        UMI3DClientServer.SendData(new EventStateChangedDto()
                        {
                            active = true,
                            boneType = bone.boneType,
                            id = interaction.id,
                            toolId = toolId,
                            hoveredObjectId = hoveredObjectId
                        }, true);
                        risingEdgeEventSent = true;
                    }
                    else
                    {
                        UMI3DClientServer.SendData(new EventTriggeredDto()
                        {
                            boneType = bone.boneType,
                            toolId = toolId,
                            id = interaction.id,
                            hoveredObjectId = hoveredObjectId
                        }, true);
                    }

                    onInputDown.Invoke();
                } else
                {
                    if ((interaction as EventDto).hold)
                    {
                        if (risingEdgeEventSent)
                        {
                            //UMI3DHttpClient.Interact(interaction.id, new object[2] { false, bone.id });
                            UMI3DClientServer.SendData(new EventStateChangedDto()
                            {
                                active = false,
                                boneType = bone.boneType,
                                id = interaction.id,
                                toolId = toolId
                            }, true);
                            risingEdgeEventSent = false;
                        }
                    }

                    onInputUp.Invoke();
                }
            };

            onActionDown.AddListener(() => { if (!player.IsDisplaying) action.Invoke(true); });
            onActionUp.AddListener(() => { if (!player.IsDisplaying) { action.Invoke(false);}});

            ControllerHintDisplayer.DisplayHint(inputObserver.button, inputObserver.controller, highlightMat, interaction.name);
            DisplayBindingInMenu(interaction, toolId, hoveredObjectId, player, action);
            associatedInteraction = interaction;
        }
        else
        {
            throw new System.Exception("Trying to associate an uncompatible interaction !");
        }
    }

    /// <summary>
    /// Displays the current binding in the menu of the associated controller.
    /// </summary>
    /// <param name="interaction"></param>
    /// <param name="associatedColor"></param>
    private void DisplayBindingInMenu(AbstractInteractionDto interaction, string toolId, string hoveredObjectId, PlayerMenuManager player, UnityAction<bool> action)
    {
        Debug.Log("<color=orange>Work on icon</color>");
        menuItem = new BindingMenuItem {
            Name = interaction.name,
            associatedMaterial = highlightMat,
            Holdable = (interaction as EventDto).hold,
            associatedInteractionDto = interaction as EventDto,
            toolId = toolId,
            hoveredObjectId = hoveredObjectId,
            associatedInput = this
        };
        //menuItem.Subscribe(action);
        player.AddMenuItemToParamatersMenu(menuItem);
    }

    /// <summary>
    /// Hides the current binding in the menu of the associated controller.
    /// </summary>
    private void HideBindingInMenu()
    {
       if(menuItem != null)
       {
            PlayerMenuManager player = PlayerMenuManager.FindInstanceAssociatedToController(inputObserver.controller);

            if (player == null)
                return;
            player.RemoveItemFromParametersMenu(menuItem);
       }
    }

    public override void Associate(ManipulationDto manipulation, DofGroupEnum dofs, string toolId, string hoveredObjectId)
    {
        throw new System.Exception("Boolean input is not compatible with manipulation");
    }

    public override AbstractInteractionDto CurrentInteraction()
    {
        return associatedInteraction;
    }

    public override void Dissociate()
    {
        if (associatedInteraction == null)
            return;

        if ((associatedInteraction as EventDto).hold && risingEdgeEventSent) 
        {
            onActionUp.AddListener(() => StartCoroutine(waitAndDissociate()));
        }
        else
        {
            DissociateInternal();
        }
    }

    private IEnumerator waitAndDissociate()
    {
        yield return new WaitForEndOfFrame();
        DissociateInternal();
    }

    private void DissociateInternal()
    {
        onActionUp.RemoveAllListeners();
        onActionDown.RemoveAllListeners();

        inputObserver.RemoveOnStateUpListener(OculusInput_onStateUp);
        inputObserver.RemoveOnStateDownListener(OculusInput_onStateDown);

        ControllerHintDisplayer.HideHint(inputObserver);
        HideBindingInMenu();
        associatedInteraction = null;
    }

    public override bool IsCompatibleWith(AbstractInteractionDto interaction)
    {
        return (interaction is EventDto);
    }

    public override bool IsAvailable()
    {
        return associatedInteraction == null;
    }
    

    public override void UpdateHoveredObjectId(string hoveredObjectId)
    {
        throw new System.NotImplementedException();
    }
}
