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
using umi3d.cdk.interaction;
using umi3d.common.interaction;
using umi3dVRBrowsersBase.ui;
using umi3dVRBrowsersBase.ui.playerMenu;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.interactions.input
{
    /// <summary>
    /// Base input class for UMI3D Manipulation
    /// </summary>
    [System.Serializable]
    public abstract class AbstractManipulationInput : AbstractVRInput
    {
        #region Fields

        /// <summary>
        /// Boolean input to trigger to activate this input.
        /// </summary>
        [SerializeField]
        public VRInputObserver activationButton;

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
        /// Associtated interaction's choosen dof (if any).
        /// </summary>
        protected DofGroupEnum associatedManipulationDof;

        [Tooltip("This material which will be used on the controller and in the menu to show that this input is associated to an interaction")]
        public Material highlightMat;

        /// <summary>
        /// Is input button pressed ?
        /// </summary>
        private bool isDown = false;

        #endregion

        #region Methods

        /// <summary>
        /// Is the input compatible with degrees of freedom of <paramref name="dofs"/>.
        /// </summary>
        /// <param name="dofs"></param>
        /// <returns></returns>
        public abstract bool IsCompatibleWith(DofGroupEnum dofs);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public override bool IsCompatibleWith(AbstractInteractionDto interaction)
        {
            return (interaction is ManipulationDto) &&
                (interaction as ManipulationDto).dofSeparationOptions.Exists(sep => sep.separations.Exists(dof => IsCompatibleWith(dof.dofs)));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="controller"></param>
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="manipulation"></param>
        /// <param name="dofs"></param>
        /// <param name="toolId"></param>
        /// <param name="hoveredObjectId"></param>
        public override void Associate(ManipulationDto manipulation, DofGroupEnum dofs, ulong toolId, ulong hoveredObjectId)
        {
            if (associatedInteraction != null)
            {
                throw new System.Exception("This input is already binded to a interaction ! (" + associatedInteraction + ")");
            }

            if (IsCompatibleWith(dofs))
            {
                associatedManipulationDof = dofs;
                base.Associate(manipulation, dofs, toolId, hoveredObjectId);

                GameObject frame = UMI3DEnvironmentLoader.GetNode(manipulation.frameOfReference).gameObject;
                if (frame == null)
                    throw new System.Exception("No frame of reference found for this manipulation");
                else
                    frameOfReference = frame.transform;

                activationButton.AddOnStateDownListener(ActivationButton_onStateDown);
                activationButton.AddOnStateUpListener(ActivationButton_onStateUp);
                onActivation.Invoke(frameOfReference, dofs);
            }
            else
            {
                throw new System.Exception("Trying to associate an uncompatible interaction !" + dofs);
            }
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Dissociate()
        {
            if (messageSenderCoroutine != null)
            {
                StopCoroutine(messageSenderCoroutine);
                onInputUp.Invoke();
                messageSenderCoroutine = null;
            }

            base.Dissociate();

            activationButton.RemoveOnStateDownListener(ActivationButton_onStateDown);
            activationButton.RemoveOnStateUpListener(ActivationButton_onStateUp);
            onDesactivation.Invoke();

            if (isDown)
            {
                (controller as VRController).IsInputPressed = false;
                isDown = false;
            }
        }

        /// <summary>
        /// Called when input button is released.
        /// </summary>
        protected virtual void ActivationButton_onStateUp()
        {
            if (PlayerMenuManager.Instance.parameterGear.IsHovered 
                || PlayerMenuManager.Instance.IsMenuHovered)
                return;

            if (messageSenderCoroutine != null)
            {
                StopCoroutine(messageSenderCoroutine);
                onInputUp.Invoke();
                messageSenderCoroutine = null;
            }

            (controller as VRController).IsInputPressed = false;
            isDown = false;
        }

        /// <summary>
        /// Called when input button is pressed.
        /// </summary>
        protected virtual void ActivationButton_onStateDown()
        {
            if (PlayerMenuManager.Instance.parameterGear.IsHovered 
                || PlayerMenuManager.Instance.IsMenuHovered)
                return;

            if (messageSenderCoroutine != null)
                StopCoroutine(messageSenderCoroutine);

            messageSenderCoroutine = StartCoroutine(NetworkMessageSender());
            onInputDown.Invoke();
            (controller as VRController).IsInputPressed = true;
            isDown = true;
        }

        /// <summary>
        /// Projects current user's movments to <paramref name="dofs"/>.
        /// </summary>
        /// <param name="dofs"></param>
        /// <returns></returns>
        protected abstract ManipulationRequestDto ComputeManipulationArgument(DofGroupEnum dofs);

        /// <summary>
        /// Coroutine responsible for sending manipualtion data to UMI3D server.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator NetworkMessageSender()
        {
            var wait = new WaitForSeconds(1f / networkFrameRate);

            while (associatedInteraction != null)
            {
                if (true)
                {
                    ManipulationRequestDto arg = ComputeManipulationArgument(associatedManipulationDof);

                    arg.boneType = boneType;
                    arg.id = associatedInteraction.id;
                    arg.toolId = toolId;
                    arg.hoveredObjectId = hoveredObjectId;
                    UMI3DClientServer.SendData(arg, true);
                }
                yield return wait;
            }
        }

        #endregion
    }
}