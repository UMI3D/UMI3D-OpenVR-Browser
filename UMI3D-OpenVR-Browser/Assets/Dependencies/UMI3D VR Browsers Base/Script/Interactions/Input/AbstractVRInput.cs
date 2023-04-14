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

using umi3d.cdk.interaction;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3dVRBrowsersBase.interactions.input
{
    /// <summary>
    /// Base <see cref="AbstractUMI3DInput"/> specific for VR browsers.
    /// </summary>
    public abstract class AbstractVRInput : AbstractUMI3DInput
    {
        #region Fields

        /// <summary>
        /// Associtated interaction (if any).
        /// </summary>
        protected AbstractInteractionDto associatedInteraction;

        [HideInInspector]
        /// <summary>
        /// Avatar bone linked to this input.
        /// </summary>
        public uint boneType;

        [HideInInspector]
        /// <summary>
        /// Avatar bone transform linked to this input.
        /// </summary>
        public Transform boneTransform;

        /// <summary>
        /// If associated to a tool, its id.
        /// </summary>
        protected ulong toolId;

        /// <summary>
        /// Id of the object which had <see cref="associatedInteraction"/>.
        /// </summary>
        protected ulong hoveredObjectId;

        #endregion

        #region Methods 

        public override void Init(AbstractController controller)
        {
            base.Init(controller);

            if (controller is VRController vrController)
            {
                boneType = vrController.bone.boneType;
                boneTransform = vrController.bone.transform;
            } 
            else
            {
                Debug.LogError("Internal error, controllers must be VRController");
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
            if (IsCompatibleWith(interaction) && IsAvailable())
            {
                SetFields(interaction, toolId, hoveredObjectId);
            }
            else
            {
                Debug.Log("INTERNAL ERROR");
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
            if (IsCompatibleWith(manipulation) && IsAvailable())
            {
                SetFields(manipulation, toolId, hoveredObjectId);
            }
            else
            {
                Debug.Log("INTERNAL ERROR");
            }
        }

        /// <summary>
        /// Sets usefull data.
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="toolId"></param>
        /// <param name="hoveredObjectId"></param>
        private void SetFields(AbstractInteractionDto interaction, ulong toolId, ulong hoveredObjectId)
        {
            this.associatedInteraction = interaction;
            this.toolId = toolId;
            this.hoveredObjectId = hoveredObjectId;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override AbstractInteractionDto CurrentInteraction()
        {
            return associatedInteraction;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Dissociate()
        {
            this.associatedInteraction = null;
            this.toolId = 0;
            this.hoveredObjectId = 0;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override bool IsAvailable()
        {
            return associatedInteraction == null;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="hoveredObjectId"></param>
        public override void UpdateHoveredObjectId(ulong hoveredObjectId)
        {
            this.hoveredObjectId = hoveredObjectId;
        }

        /// <summary>
        /// Return id of current associated tool.
        /// </summary>
        /// <returns></returns>
        public ulong GetToolId()
        {
            return toolId;
        }

        /// <summary>
        /// Returns id of the objet which had current associated interaction.
        /// </summary>
        /// <returns></returns>
        public ulong GetHoveredObjectId()
        {
            return hoveredObjectId;
        }

        #endregion
    }
}