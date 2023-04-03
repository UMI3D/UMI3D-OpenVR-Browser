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

using System.Collections.Generic;
using umi3d.cdk.interaction;
using umi3d.cdk.menu;
using umi3d.cdk.menu.interaction;
using umi3d.cdk.userCapture;
using umi3d.common.interaction;
using umi3dVRBrowsersBase.interactions.input;
using umi3dVRBrowsersBase.ui.playerMenu;
using UnityEngine;

namespace umi3dVRBrowsersBase.interactions
{
    public partial class VRController : AbstractController
    {
        #region Fields

        [HideInInspector]
        public MenuAsset ObjectMenu;

        /// <summary>
        /// Type of this controller
        /// </summary>
        public ControllerType type;

        /// <summary>
        /// Asoociated bone.
        /// </summary>
        public UMI3DClientUserTrackingBone bone;

        #region Inputs Fields

        /// <summary>
        /// Id of the current hovered UMI3DNode.
        /// </summary>
        public ulong hoveredObjectId;

        public bool IsInputPressed = false;

        #endregion Inputs Fields

        private float timeSinceLastInput = 0;

        /// <summary>
        /// Time to wait after last input before considering it as unused.
        /// </summary>
        private float inputUsageTimeout = 10;

        #endregion Fields

        #region Methods

        #region Monobehaviour Life Cycle

        protected virtual void Awake()
        {
            ObjectMenu = Resources.Load<MenuAsset>("ParametersMenu");

            Physics.queriesHitBackfaces = true;

            foreach (AbstractUMI3DInput input in manipulationInputs)
                input.Init(this);
            foreach (AbstractUMI3DInput input in booleanInputs)
                input.Init(this);
        }

        protected virtual void Update()
        {
            if (timeSinceLastInput <= inputUsageTimeout)
                timeSinceLastInput += Time.deltaTime;
        }

        #endregion

        #region Tool : projection and release

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="releasable"></param>
        /// <param name="reason"></param>
        /// <param name="hoveredObjectId"></param>
        public override void Project(AbstractTool tool, bool releasable, InteractionMappingReason reason, ulong hoveredObjectId)
        {
            base.Project(tool, releasable, reason, hoveredObjectId);

            if (currentTool == tool) // It means projection succedded
            {
                PlayerMenuManager.Instance.MenuHeader.DisplayControllerButton(true, type, tool.name);
            }
        }

        /// <summary>
        /// Check if a tool can be projected on this controller.
        /// </summary>
        /// <param name="tool"> The tool to be projected.</param>
        /// <returns></returns>
        public override bool IsCompatibleWith(AbstractTool tool)
        {
            return tool.interactionsLoaded.TrueForAll(inter =>
                (inter is ManipulationDto ) ?
                (inter as ManipulationDto).dofSeparationOptions.Exists(
                    group => !group.separations.Exists(
                        dof => (dof.dofs == DofGroupEnum.X_RX) || (dof.dofs == DofGroupEnum.Y_RY) || (dof.dofs == DofGroupEnum.Z_RZ)))
                : true);
        }

        /// <summary>
        /// Check if a tool requires the generation of a menu to be projected.
        /// </summary>
        /// <param name="tool"> The tool to be projected.</param>
        /// <returns></returns>
        public override bool RequiresMenu(AbstractTool tool)
        {
            return false;

            List<AbstractInteractionDto> manips = tool.interactionsLoaded.FindAll(x => x is ManipulationDto);
            List<AbstractInteractionDto> events = tool.interactionsLoaded.FindAll(x => x is EventDto);
            List<AbstractInteractionDto> param = tool.interactionsLoaded.FindAll(x => x is AbstractParameterDto);
            return ((manips.Count > 1) || (events.Count > 3) || (param.Count > 0));
        }

        /// <summary>
        /// If current tool is not null, releases it.
        /// </summary>
        public void ReleaseCurrentTool()
        {
            if (currentTool != null)
            {
                InteractionMapper.Instance.ReleaseTool(currentTool.id);
            }
        }

        /// <summary>
        /// Create a menu to access each interactions of a tool separately.
        /// </summary>
        /// <param name="interactions"></param>
        public override void CreateInteractionsMenuFor(AbstractTool tool)
        {
            List<AbstractInteractionDto> interactions = tool.interactionsLoaded;
            List<AbstractInteractionDto> manips = interactions.FindAll(inter => inter is ManipulationDto);
            foreach (AbstractInteractionDto manip in manips)
            {
                DofGroupOptionDto bestSeparationOption = FindBest((manip as ManipulationDto).dofSeparationOptions.ToArray());
                foreach (DofGroupDto sep in bestSeparationOption.separations)
                {
                    var manipSeparationMenu = new ManipulationMenuItem()
                    {
                        Name = manip.name + "-" + sep.name,
                        dof = sep,
                        interaction = manip as ManipulationDto
                    };

                    manipSeparationMenu.Subscribe(() =>
                    {
                        try
                        {
                            AbstractUMI3DInput newInput = projectionMemory.PartialProject(this, manip as ManipulationDto, sep, false, tool.id, hoveredObjectId);
                            if (newInput != null)
                            {
                                var toolInputs = new List<AbstractUMI3DInput>();
                                AbstractUMI3DInput[] buffer;
                                if (associatedInputs.TryGetValue(tool.id, out buffer))
                                {
                                    toolInputs = new List<AbstractUMI3DInput>(buffer);
                                    associatedInputs.Remove(tool.id);
                                }
                                toolInputs.Add(newInput);
                                associatedInputs.Add(tool.id, toolInputs.ToArray());
                            }
                            else
                                throw new System.Exception("Internal Error");
                        }
                        catch (ProjectionMemory.NoInputFoundException noInputException)
                        {
                            throw new System.Exception("Internal Error", noInputException);
                        }
                    });

                    Debug.Log("TODO : add this item to the player controllers menu");

                    if (FindInput(manip as ManipulationDto, sep, true) != null)
                    {
                        manipSeparationMenu.Select();
                    }
                }
            }

            List<AbstractInteractionDto> events = interactions.FindAll(inter => inter is EventDto);
            foreach (AbstractInteractionDto evt in events)
            {
                var eventMenu = new EventMenuItem()
                {
                    interaction = evt as EventDto,
                    Name = evt.name
                };

                eventMenu.Subscribe(() =>
                {
                    Debug.Log("Event menu item");
                    try
                    {
                        AbstractUMI3DInput newInput = projectionMemory.PartialProject(this, evt as EventDto, tool.id, hoveredObjectId, false);
                        if (newInput != null)
                        {
                            var toolInputs = new List<AbstractUMI3DInput>();
                            AbstractUMI3DInput[] buffer;
                            if (associatedInputs.TryGetValue(tool.id, out buffer))
                            {
                                toolInputs = new List<AbstractUMI3DInput>(buffer);
                                associatedInputs.Remove(tool.id);
                            }
                            toolInputs.Add(newInput);
                            associatedInputs.Add(tool.id, toolInputs.ToArray());
                        }
                        else
                            throw new System.Exception("Internal Error");
                    }
                    catch (ProjectionMemory.NoInputFoundException noInputException)
                    {
                        throw new System.Exception("Internal Error", noInputException);
                    }
                });

                Debug.Log("TODO : add this item to the player controllers menu");

                if (FindInput(evt as EventDto, true) != null)
                {
                    eventMenu.Select();
                }
            }

            ProjectParameters(tool, interactions, hoveredObjectId);
        }

        /// <summary>
        /// Projects all parameters on this tool.
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="interactions"></param>
        /// <param name="hoveredObjectId"></param>
        private void ProjectParameters(AbstractTool tool, List<AbstractInteractionDto> interactions, ulong hoveredObjectId)
        {
            AbstractUMI3DInput[] inputs = projectionMemory.Project(this, interactions.FindAll(inter => inter is AbstractParameterDto).ToArray(), tool.id, hoveredObjectId);
            var toolInputs = new List<AbstractUMI3DInput>();

            if (associatedInputs.TryGetValue(tool.id, out AbstractUMI3DInput[] buffer))
            {
                toolInputs = new List<AbstractUMI3DInput>(buffer);
                associatedInputs.Remove(tool.id);
            }
            toolInputs.AddRange(inputs);
            associatedInputs.Add(tool.id, toolInputs.ToArray());
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="reason"></param>
        public override void Release(AbstractTool tool, InteractionMappingReason reason)
        {
            base.Release(tool, reason);
            tool.onReleased(bone.boneType);

            PlayerMenuManager.Instance.CtrlToolMenu.ClearBindingList(type);
            PlayerMenuManager.Instance.MenuHeader.DisplayControllerButton(false, type, string.Empty);
        }

        #endregion

        #region Status

        /// <summary>
        /// Check if the user is currently using the selected Tool.
        /// </summary>
        /// <returns>returns true if the user is currently interacting with the tool.</returns>
        protected override bool isInteracting()
        {
            return (currentTool != null) && (timeSinceLastInput <= inputUsageTimeout);
        }

        /// <summary>
        /// Check if the user is currently manipulating the tools menu.
        /// </summary>
        /// <returns>returns true if the user is currently manipulating the tools menu.</returns>
        protected override bool isNavigating()
        {
            throw new System.NotImplementedException();
        }

        #endregion Status

        #region Change mapping

        [ContextMenu("SWIPE")]
        private void Swipe()
        {
            ChangeInputMapping(booleanInputs[0], booleanInputs[1]);
        }

        /// <summary>
        /// Swipes the inputs of two interactions.
        /// </summary>
        /// <param name="previousInput"></param>
        /// <param name="targetInput"></param>
        public void ChangeInputMapping(AbstractVRInput previousInput, AbstractVRInput targetInput)
        {
            AbstractInteractionDto inter = null;
            ulong toolId = 0, objectId = 0;

            if (!targetInput.IsAvailable())
            {
                inter = targetInput.CurrentInteraction();
                toolId = targetInput.GetToolId();
                objectId = targetInput.GetHoveredObjectId();

                targetInput.Dissociate();
            }

            targetInput.Associate(previousInput.CurrentInteraction(), previousInput.GetToolId(), previousInput.GetHoveredObjectId());
            previousInput.Dissociate();

            if (inter != null)
            {
                previousInput.Associate(inter, toolId, objectId);
            }
        }

        #endregion Change mapping

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        protected override ulong GetCurrentHoveredId()
        {
            if (tool == null)
                return 0;

            return tool.id;
        }

        #endregion Methods
    }
}