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
using umi3d.cdk.interaction;
using umi3d.cdk.menu;
using umi3d.cdk.userCapture;
using umi3d.common.interaction;
using umi3d.common.userCapture;
using umi3dVRBrowsersBase.ui.playerMenu;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.interactions.input
{
    /// <summary>
    /// Base class for parameter input.
    /// </summary>
    /// <typeparam name="InputMenuItem"></typeparam>
    /// <typeparam name="ParameterType"></typeparam>
    /// <typeparam name="ValueType"></typeparam>
    public abstract class AbstractParameterInput<InputMenuItem, ParameterType, ValueType> : AbstractUMI3DInput
        where InputMenuItem : AbstractInputMenuItem<ValueType>, new()
        where ParameterType : AbstractParameterDto<ValueType>, new()

    {
        #region Fields

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

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="controller"></param>
        public override void Init(AbstractController controller)
        {
            base.Init(controller);
            bone = (controller as VRController).bone;
            UnityEngine.Debug.Assert(bone != null, "Bone of " + controller.name + " not set.");
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="hoveredObjectId"></param>
        public override void UpdateHoveredObjectId(ulong hoveredObjectId)
        {
            throw new System.NotImplementedException();
        }

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

            if (interaction is ParameterType paramType)
            {
                menuItem = new InputMenuItem()
                {
                    dto = interaction as ParameterType,
                    Name = interaction.name
                };

                menuItem.NotifyValueChange((interaction as ParameterType).value);
                PlayerMenuManager.Instance.CtrlToolMenu.AddParameter((controller as VRController).type, menuItem, DesynchronizeMenuItem);

                var param = interaction as ParameterType;

                UnityEngine.Debug.Log(interaction.GetType());

                callback = x =>
                {
                    paramType.value = x;

                    UMI3DClientServer.SendData(new ParameterSettingRequestDto()
                    {
                        boneType = bone.boneType,
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
                };
                menuItem.Subscribe(callback);

                currentInteraction = interaction;
            }
            else
            {
                throw new System.Exception("Incompatible interaction");
            }
        }

        public void DesynchronizeMenuItem()
        {
            menuItem.UnSubscribe(callback);
            PlayerMenuManager.Instance.CtrlToolMenu.RemoveParameter((controller as VRController).type, menuItem);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="manipulation"></param>
        /// <param name="dofs"></param>
        /// <param name="toolId"></param>
        /// <param name="hoveredObjectId"></param>
        public override void Associate(ManipulationDto manipulation, DofGroupEnum dofs, ulong toolId, ulong hoveredObjectId)
        {
            throw new System.Exception("Incompatible interaction");
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override AbstractInteractionDto CurrentInteraction()
        {
            return currentInteraction;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Dissociate()
        {
            currentInteraction = null;
            PlayerMenuManager.Instance.CtrlToolMenu.RemoveParameter((controller as VRController).type, menuItem);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public override bool IsCompatibleWith(AbstractInteractionDto interaction)
        {
            return interaction is ParameterType;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override bool IsAvailable()
        {
            return currentInteraction == null;
        }

        #endregion
    }
}