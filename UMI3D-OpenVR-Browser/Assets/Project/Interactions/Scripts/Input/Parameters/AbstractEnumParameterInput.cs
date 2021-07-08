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
using umi3d.cdk;
using umi3d.cdk.menu;
using umi3d.common;
using umi3d.common.interaction;
using umi3d.common.userCapture;
using UnityEngine.Events;

public abstract class AbstractEnumParameterInput<InputMenuItem, ValueType> : AbstractParameterInput<InputMenuItem, EnumParameterDto<ValueType>, ValueType>
    where InputMenuItem : AbstractEnumInputMenuItem<ValueType>, new()
{
    public override void Associate(AbstractInteractionDto interaction, ulong toolId, ulong hoveredObjectId)
    {
        if (currentInteraction != null)
        {
            throw new System.Exception("This input is already associated to another interaction (" + currentInteraction + ")");
        }

        if (interaction is EnumParameterDto<ValueType>)
        {
            EnumParameterDto<ValueType> stringEnum = interaction as EnumParameterDto<ValueType>;
            UnityEngine.Debug.Log("<color=orange>TODO : Currently bone == null. </color>");
            callback = newValue =>
            {
                UMI3DClientServer.SendData(new ParameterSettingRequestDto()
                {
                    toolId = toolId,
                    id = interaction.id,
                    parameter = new EnumParameterDto<ValueType>()
                    {
                        value = newValue,
                        id = stringEnum.id,
                        icon3D = stringEnum.icon3D,
                        icon2D = stringEnum.icon2D,
                        description = stringEnum.description,
                        name = stringEnum.name,
                        possibleValues = stringEnum.possibleValues
                    },
                    boneType = bone == null ? BoneType.None : bone.boneType,
                }, true);
            };

            menuItem = new InputMenuItem()
            {
                dto = stringEnum,
                Name = interaction.name,
                options = stringEnum.possibleValues
            };

            menuItem.NotifyValueChange(stringEnum.value);

            menuItem.Subscribe(callback);

            rootMenu.Add(menuItem);

            currentInteraction = interaction;
        }
        else
        {
            throw new System.Exception("Incompatible interaction");
        }
    }

}
