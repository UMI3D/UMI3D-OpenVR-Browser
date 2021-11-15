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
using System.Linq;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.cdk.interaction;
using umi3d.cdk.menu;
using umi3d.cdk.menu.interaction;
using umi3d.common;
using umi3d.common.interaction;
using umi3d.common.userCapture;
using UnityEngine;

[System.Serializable]
public class MenuInput : AbstractUMI3DInput
{
    #region Fields

    /// <summary>
    /// Oculus input observer binded to this input.
    /// </summary>
    public AbstractController oculusInput;

    /// <summary>
    /// Associtated interaction (if any).
    /// </summary>
    public AbstractInteractionDto associatedInteraction { get; protected set; }
    /// <summary>
    /// Avatar bone linked to this input.
    /// </summary>
    public uint bone = BoneType.RightHand;

    ulong toolId;
    ulong hoveredObjectId;

    protected BoneDto boneDto;
    bool risingEdgeEventSent;

    /// <summary>
    /// Menu item displayed in the controller menu.
    /// </summary>
    public HoldableButtonMenuItem menuItem;

    #endregion

    #region Methods

    public override void Associate(AbstractInteractionDto interaction, ulong toolId, ulong hoveredObjectId)
    {
        if (associatedInteraction != null)
        {
            throw new System.Exception("This input is already binded to a interaction ! (" + associatedInteraction + ")");
        }

        if (IsCompatibleWith(interaction) && interaction is EventDto ev)
        {
            this.hoveredObjectId = hoveredObjectId;
            this.toolId = toolId;
            associatedInteraction = interaction;
            menuItem = new HoldableButtonMenuItem
            {
                Name = associatedInteraction.name,
                Holdable = ev.hold,
                associatedInteractionDto = interaction,
                toolId = toolId,
                hoveredObjectId = hoveredObjectId,
                associatedInput = this
            };
            //menuItem.Subscribe(Pressed);

            PlayerMenuManager player = PlayerMenuManager.FindInstanceAssociatedToController(oculusInput);
            player.AddMenuItemToParamatersMenu(menuItem);
        }
        else
        {
            throw new System.Exception("Trying to associate an uncompatible interaction !");
        }
    }

    public override void Associate(ManipulationDto manipulation, DofGroupEnum dofs, ulong toolId, ulong hoveredObjectId)
    {
        if (associatedInteraction != null)
        {
            throw new System.Exception("This input is already binded to a interaction ! (" + associatedInteraction + ")");
        }

        if (IsCompatibleWith(manipulation))
        {

            this.hoveredObjectId = hoveredObjectId;
            this.toolId = toolId;
            associatedInteraction = manipulation;
            menuItem = new HoldableButtonMenuItem
            {
                Name = dofs.ToString(),
                Holdable = true,
                associatedInteractionDto = associatedInteraction,
                toolId = toolId,
                hoveredObjectId = hoveredObjectId,
                associatedInput = this,
                dofs = dofs
            };

            PlayerMenuManager player = PlayerMenuManager.FindInstanceAssociatedToController(oculusInput);
            player.AddMenuItemToParamatersMenu(menuItem);
        }
        else
        {

        }
    }

    public override AbstractInteractionDto CurrentInteraction()
    {
        return associatedInteraction;
    }

    public override void Dissociate()
    {
        associatedInteraction = null;
        PlayerMenuManager player = PlayerMenuManager.FindInstanceAssociatedToController(oculusInput);
        player.RemoveItemFromParametersMenu(menuItem);
        menuItem.UnSubscribe(Pressed);
        menuItem = null;
    }

    public override bool IsAvailable()
    {
        return associatedInteraction == null;
    }

    public override bool IsCompatibleWith(AbstractInteractionDto interaction)
    {
        return interaction is EventDto || interaction is ManipulationDto;
    }

    void Pressed(bool down)
    {
        /*if (boneDto == null)
            boneDto = new BoneDto() { boneType = BoneType.RightHand };

        if (down)
        {
            onInputDown.Invoke();
            if ((associatedInteraction).hold)
            {
                var eventdto = new EventStateChangedDto
                {
                    active = true,
                    boneType = boneDto.boneType,
                    id = associatedInteraction.id,
                    toolId = this.toolId,
                    hoveredObjectId = hoveredObjectId
                };
                UMI3DClientServer.SendData(eventdto, true);
                risingEdgeEventSent = true;
            }
            else
            {
                var eventdto = new EventTriggeredDto
                {
                    boneType = boneDto.boneType,
                    id = associatedInteraction.id,
                    toolId = this.toolId,
                    hoveredObjectId = hoveredObjectId
                };
                UMI3DClientServer.SendData(eventdto, true);
            }
        }
        else
        {
            onInputUp.Invoke();
            if ((associatedInteraction).hold)
            {
                if (risingEdgeEventSent)
                {
                    var eventdto = new EventStateChangedDto
                    {
                        active = false,
                        boneType = boneDto.boneType,
                        id = associatedInteraction.id,
                        toolId = this.toolId,
                        hoveredObjectId = hoveredObjectId
                    };
                    UMI3DClientServer.SendData(eventdto, true);
                    risingEdgeEventSent = false;
                }
            }
        }*/
    }

    public override void UpdateHoveredObjectId(ulong hoveredObjectId)
    {
        this.hoveredObjectId = hoveredObjectId;
    }

    #endregion
}
