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

using umi3d.cdk;
using umi3d.cdk.menu;
using umi3d.common.interaction;
using umi3d.common.userCapture;
using umi3dVRBrowsersBase.ui.playerMenu;

namespace umi3dVRBrowsersBase.interactions.input
{
    /// <summary>
    /// Base class for range parameter inoput.
    /// </summary>
    /// <typeparam name="InputMenuItem"></typeparam>
    /// <typeparam name="ParameterType"></typeparam>
    /// <typeparam name="ValueType"></typeparam>
    public abstract class AbstractRangeParameterInput<InputMenuItem, ParameterType, ValueType> : AbstractParameterInput<InputMenuItem, ParameterType, ValueType>
    where ValueType : System.IComparable
    where InputMenuItem : AbstractRangeInputMenuItem<ValueType>, new()
    where ParameterType : AbstractRangeParameterDto<ValueType>, new()
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="toolId"></param>
        /// <param name="hoveredObjectId"></param>
        public override void Associate(AbstractInteractionDto interaction, ulong toolId, ulong hoveredObjectId)
        {
            if (currentInteraction != null)
            {
                throw new System.Exception("This input is already associated to another interaction (" + currentInteraction + ")");
            }

            if (interaction is ParameterType parameterType)
            {
                var param = interaction as ParameterType;
                menuItem = new InputMenuItem()
                {
                    dto = interaction as ParameterType,
                    min = param.min,
                    max = param.max,
                    Name = param.name,
                    increment = param.increment
                };

                callback = x =>
                {
                    if ((x.CompareTo(param.min) >= 0) && (x.CompareTo(param.max) <= 0))
                    {
                        parameterType.value = x;

                        UMI3DClientServer.SendData(new ParameterSettingRequestDto()
                        {
                            id = param.id,
                            boneType = bone.boneType,
                            bonePosition = bone.transform.position,
                            boneRotation = bone.transform.rotation,
                            parameter = new ParameterType()
                            {
                                description = param.description,
                                id = param.id,
                                icon2D = param.icon2D,
                                icon3D = param.icon3D,
                                increment = param.increment,
                                max = param.max,
                                min = param.min,
                                name = param.name,
                                value = x
                            },
                            toolId = toolId
                        }, true);
                    }
                };

                menuItem.NotifyValueChange(param.value);

                menuItem.Subscribe(callback);

                PlayerMenuManager.Instance.CtrlToolMenu.AddParameter((controller as VRController).type, menuItem, DesynchronizeMenuItem);

                currentInteraction = interaction;
            }
            else
            {
                throw new System.Exception("Incompatible interaction");
            }
        }
    }
}