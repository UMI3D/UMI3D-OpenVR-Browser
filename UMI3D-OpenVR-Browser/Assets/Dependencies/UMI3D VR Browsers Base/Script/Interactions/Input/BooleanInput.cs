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

using System.Collections;
using umi3d.cdk;
using umi3d.common.interaction;
using umi3dVRBrowsersBase.selection;
using umi3dVRBrowsersBase.ui;
using umi3dVRBrowsersBase.ui.playerMenu;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.interactions.input
{
    /// <summary>
    /// Input for UMI3D Event.
    /// </summary>
    [System.Serializable]
    public class BooleanInput : AbstractVRInput
    {
        #region Fields

        /// <summary>
        /// Oculus input observer binded to this input.
        /// </summary>
        public VRInputObserver vrInput;

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
        /// True if the rising edge event has been sent through network (to avoid sending falling edge only).
        /// </summary>
        private bool risingEdgeEventSent = false;

        /// <summary>
        /// Is input button down ?
        /// </summary>
        private bool isDown = false;

        public class VRInteractionEvent : UnityEvent<uint> { };

        [HideInInspector]
        public static VRInteractionEvent BooleanEvent = new VRInteractionEvent();

        #endregion

        #region Methods

        /// <summary>
        /// Callback called on oculus input up.
        /// </summary>
        /// <param name="fromAction"></param>
        /// <param name="fromSource"></param>
        /// <see cref="Associate(AbstractInteractionDto)"/>
        private void VRInput_onStateUp()
        {
            if (ParameterGear.Instance.IsHovered || PlayerMenuManager.Instance.IsHovered || VRClickableElementSelector.IsElementHovered())
                return;

            onActionUp.Invoke();
        }

        /// <summary>
        /// Callback called on oculus input down.
        /// </summary>
        /// <param name="fromAction"></param>
        /// <param name="fromSource"></param>
        /// <see cref="Associate(AbstractInteractionDto)"/>
        private void VRInput_onStateDown()
        {
            if (ParameterGear.Instance.IsHovered || PlayerMenuManager.Instance.IsHovered || VRClickableElementSelector.IsElementHovered())
                return;

            onActionDown.Invoke();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="toolId"></param>
        /// <param name="hoveredObjectId"></param>
        public override void Associate(AbstractInteractionDto interaction, ulong toolId, ulong hoveredObjectId)
        {
            if (associatedInteraction != null)
            {
                throw new System.Exception("This input is already binded to a interaction ! (" + associatedInteraction + ")");
            }

            if (IsCompatibleWith(interaction))
            {
                vrInput.AddOnStateUpListener(VRInput_onStateUp);
                vrInput.AddOnStateDownListener(VRInput_onStateDown);

                UnityAction<bool> action = (bool pressDown) =>
                {
                    if (pressDown)
                    {
                        if ((interaction as EventDto).hold)
                        {
                            UMI3DClientServer.SendData(new EventStateChangedDto()
                            {
                                active = true,
                                boneType = boneType,
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
                                boneType = boneType,
                                toolId = toolId,
                                id = interaction.id,
                                hoveredObjectId = hoveredObjectId
                            }, true);
                        }
                        (controller as VRController).IsInputPressed = true;
                        isDown = true;

                        BooleanEvent.Invoke(boneType);

                        if ((interaction as EventDto).TriggerAnimationId != 0)
                        {
                            var anim = UMI3DNodeAnimation.Get((interaction as EventDto).TriggerAnimationId);
                            if (anim != null)
                                anim.Start();
                        }

                        onInputDown.Invoke();
                    }
                    else
                    {
                        if ((interaction as EventDto).hold)
                        {
                            if (risingEdgeEventSent)
                            {
                                UMI3DClientServer.SendData(new EventStateChangedDto()
                                {
                                    active = false,
                                    boneType = boneType,
                                    id = interaction.id,
                                    toolId = toolId
                                }, true);
                                risingEdgeEventSent = false;
                            }
                        }
                        (controller as VRController).IsInputPressed = false;
                        isDown = false;
                        onInputUp.Invoke();

                        BooleanEvent.Invoke(boneType);

                        if ((interaction as EventDto).ReleaseAnimationId != 0)
                        {
                            var anim = UMI3DNodeAnimation.Get((interaction as EventDto).ReleaseAnimationId);
                            if (anim != null)
                                anim.Start();
                        }
                    }
                };

                onActionDown.AddListener(() => { action.Invoke(true); });
                onActionUp.AddListener(() => { action.Invoke(false); });

                base.Associate(interaction, toolId, hoveredObjectId);
            }
            else
            {
                throw new System.Exception("Trying to associate an uncompatible interaction !");
            }
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
            throw new System.Exception("Boolean input is not compatible with manipulation");
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Dissociate()
        {
            if (associatedInteraction == null)
                return;


            if ((associatedInteraction as EventDto).hold && risingEdgeEventSent)
            {
                onActionUp.AddListener(() => StartCoroutine(WaitAndDissociate()));
            }
            else
            {
                DissociateInternal();
            }
        }

        /// <summary>
        /// Dissociates after the end of the current frame.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitAndDissociate()
        {
            yield return new WaitForEndOfFrame();
            DissociateInternal();
        }

        /// <summary>
        /// Performs dissociation.
        /// </summary>
        private void DissociateInternal()
        {
            base.Dissociate();

            onActionUp.RemoveAllListeners();
            onActionDown.RemoveAllListeners();

            vrInput.RemoveOnStateUpListener(VRInput_onStateUp);
            vrInput.RemoveOnStateDownListener(VRInput_onStateDown);

            if (isDown)
            {
                (controller as VRController).IsInputPressed = false;
                isDown = false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public override bool IsCompatibleWith(AbstractInteractionDto interaction)
        {
            return (interaction is EventDto);
        }

        #endregion
    }
}