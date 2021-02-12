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
using umi3d.cdk.interaction;
using umi3d.cdk.menu;
using umi3d.cdk.userCapture;
using umi3d.common.interaction;
using UnityEngine.Events;

public abstract class AbstractParameterInput<InputMenuItem, ParameterType, ValueType> : AbstractUMI3DInput 
    where InputMenuItem : AbstractInputMenuItem<ValueType>, new()
    where ParameterType : AbstractParameterDto<ValueType>, new()

{
    /// <summary>
    /// Menu to insert <see cref="menuItem"/> in.
    /// </summary>
    public Menu rootMenu;

    /// <summary>
    /// Associated menu item.
    /// </summary>
    protected InputMenuItem menuItem;



    /// <summary>
    /// Avatar bone linked to this input.
    /// </summary>
    public UMI3DClientUserTrackingBone bone;

    /// <summary>
    /// Interaction currently associated to this input.
    /// </summary>
    protected AbstractInteractionDto currentInteraction;

    /// <summary>
    /// Associated callback
    /// </summary>
    /// <see cref="Associate(AbstractInteractionDto)"/>
    protected UnityAction<ValueType> callback;


    public override void UpdateHoveredObjectId(string hoveredObjectId)
    {
        throw new System.NotImplementedException();
    }

    public override void Associate(AbstractInteractionDto interaction, string toolId, string hoveredObjectId)
    {
        if (currentInteraction != null)
        {
            throw new System.Exception("This input is already associated to another interaction (" + currentInteraction + ")");
        }
        
        if (interaction is ParameterType)
        {
            menuItem = new InputMenuItem()
            {
                dto = interaction as ParameterType,
                Name = interaction.name
            };

            UnityEngine.Debug.Log("<color=orange>TODO : Currently bone == null. </color>");
            rootMenu.Add(menuItem);

            menuItem.NotifyValueChange((interaction as ParameterType).value);

            ParameterType param = interaction as ParameterType;

            callback = x => UMI3DClientServer.SendData(new ParameterSettingRequestDto()
            {
                boneType = bone?.boneType,
                toolId = toolId,
                parameter = new ParameterType()
                {
                    id = param.id,
                    description = param.description,
                    name = param.name,
                    icon2D = param.icon2D,
                    icon3D = param.icon3D,
                    value = x
                },
                id = interaction.id
            }, true);
            menuItem.Subscribe(callback);

            currentInteraction = interaction;
        }
        else
        {
            throw new System.Exception("Incompatible interaction");
        }
    }

    public override void Associate(ManipulationDto manipulation, DofGroupEnum dofs, string toolId, string hoveredObjectId)
    {
        throw new System.Exception("Incompatible interaction");
    }

    public override AbstractInteractionDto CurrentInteraction()
    {
        return currentInteraction;
    }

    public override void Dissociate()
    {
        currentInteraction = null;
        menuItem.UnSubscribe(callback);
        rootMenu.Remove(menuItem);
    }

    public override bool IsCompatibleWith(AbstractInteractionDto interaction)
    {
        return interaction is ParameterType;
    }

    public override bool IsAvailable()
    {
        return currentInteraction == null;
    }
}
