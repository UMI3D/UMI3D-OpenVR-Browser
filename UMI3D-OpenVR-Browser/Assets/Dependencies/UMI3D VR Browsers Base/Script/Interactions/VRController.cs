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
using umi3d.cdk.menu.interaction;
using umi3d.cdk.userCapture;
using umi3d.common.interaction;
using umi3dBrowsers.interaction.selection.cursor;
using umi3dVRBrowsersBase.interactions.input;
using umi3dVRBrowsersBase.ui.playerMenu;
using UnityEngine;

namespace umi3dVRBrowsersBase.interactions
{
    public class VRController : AbstractController
    {
        #region Fields

        /// <summary>
        /// Type of this controller
        /// </summary>
        public ControllerType type;

        /// <summary>
        /// Asoociated bone.
        /// </summary>
        public UMI3DClientUserTrackingBone bone;

        #region Inputs Fields

        [Header("Manipulations")]

        public List<ManipulationInput> manipulationInputs = new List<ManipulationInput>();

        [Header("Other")]
        public List<BooleanInput> booleanInputs = new List<BooleanInput>();

        [Tooltip("Input used by default for an hold event")]
        public BooleanInput HoldInput;

        /// <summary>
        /// Id of the current hovered UMI3DNode.
        /// </summary>
        public ulong hoveredObjectId;

        private int inputhash = 0;

        private List<AbstractUMI3DInput> lastComputedInputs = null;
        private List<MenuInput> menuInputs = new List<MenuInput>();
        private List<FormMenuInput> formInputs = new List<FormMenuInput>();

        /// <summary>
        /// Concatenation of all <see cref="AbstractUMI3DInput"/> stored by this controller.
        /// </summary>
        public override List<AbstractUMI3DInput> inputs
        {
            get
            {
                int newHash = manipulationInputs.GetHashCode() +
                                booleanInputs.GetHashCode();
                if ((lastComputedInputs != null) && (newHash == inputhash))
                {
                    return lastComputedInputs;
                }
                else
                {
                    var buffer = new List<AbstractUMI3DInput>();
                    buffer.AddRange(manipulationInputs);
                    buffer.AddRange(booleanInputs);
                    buffer.AddRange(menuInputs);
                    buffer.AddRange(formInputs);
                    lastComputedInputs = buffer;
                    inputhash = newHash;
                    return buffer;
                }
            }
        }


        /// <summary>
        /// Instantiated float parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<FloatParameterInput> floatParameterInputs = new List<FloatParameterInput>();

        /// <summary>
        /// Instantiated float range parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<FloatRangeParameterInput> floatRangeParameterInputs = new List<FloatRangeParameterInput>();

        /// <summary>
        /// Instantiated int parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<IntParameterInput> intParameterInputs = new List<IntParameterInput>();

        /// <summary>
        /// Instantiated bool parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<BooleanParameterInput> boolParameterInputs = new List<BooleanParameterInput>();

        /// <summary>
        /// Instantiated string parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<StringParameterInput> stringParameterInputs = new List<StringParameterInput>();

        /// <summary>
        /// Instantiated string enum parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<StringEnumParameterInput> stringEnumParameterInputs = new List<StringEnumParameterInput>();

        public bool IsInputPressed = false;

        #endregion

        private float timeSinceLastInput = 0;

        /// <summary>
        /// Time to wait after last input before considering it as unused.
        /// </summary>
        private float inputUsageTimeout = 10;

        #endregion

        #region Methods 

        protected virtual void Awake()
        {
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

        [ContextMenu("SWIPE")]
        private void Swipe()
        {
            ChangeInputMapping(booleanInputs[0], booleanInputs[1]);
        }

        /// <summary>
        /// Clear all menus and the projected tools.
        /// </summary>
        public override void Clear()
        {
            Debug.LogError("TODO : Clear menu");

            ReleaseCurrentTool();

            foreach (BooleanInput input in booleanInputs)
            {
                if (!input.IsAvailable())
                    input.Dissociate();
            }
            foreach (ManipulationInput input in manipulationInputs)
            {
                if (!input.IsAvailable())
                    input.Dissociate();
            }
            foreach (MenuInput input in menuInputs)
            {
                if (!input.IsAvailable())
                    input.Dissociate();
            }
            foreach (FormMenuInput input in formInputs)
            {
                if (!input.IsAvailable())
                    input.Dissociate();
            }

            menuInputs.ForEach((a) => { Destroy(a); });
            menuInputs = new List<MenuInput>();

            formInputs.ForEach((i) => Destroy(i));
            formInputs = new List<FormMenuInput>();
        }

        #region Find Input

        /// <summary>
        /// Find the best dof separation for this controller.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public override DofGroupOptionDto FindBest(DofGroupOptionDto[] options)
        {
            return options[0];
        }

        /// <summary>
        /// Find the best free input for a given manipulation dof.
        /// </summary>
        /// <param name="manip">Manipulation to associate input to</param>
        /// <param name="dof">Degree of freedom to associate input to</param>
        /// <returns></returns>
        public override AbstractUMI3DInput FindInput(ManipulationDto manip, DofGroupDto dof, bool unused = true)
        {
            return FindInput(manip, dof.dofs, unused);
        }

        /// <summary>
        /// Find the best free input for a given <see cref="ManipulationDto"/>.
        /// </summary>
        /// <param name="manip"></param>
        /// <param name="dofs"></param>
        /// <param name="unused"></param>
        /// <returns></returns>
        public AbstractUMI3DInput FindInput(ManipulationDto manip, DofGroupEnum dofs, bool unused = true)
        {
            AbstractVRInput result = null;

            foreach (ManipulationInput input in manipulationInputs)
            {
                if (input.IsCompatibleWith(manip))
                {
                    if (input.IsAvailable() || !unused)
                    {
                        result = input;
                        break;
                    }
                }
            }

            if (result == null)
            {
                //if no input was found
                result = this.gameObject.AddComponent<MenuInput>();
                result.Init(this);
                menuInputs.Add(result as MenuInput);

            }

            PlayerMenuManager.Instance.CtrlToolMenu.AddBinding(result);

            return result;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="unused"></param>
        /// <param name="tryToFindInputForHoldableEvent"></param>
        /// <returns></returns>
        public override AbstractUMI3DInput FindInput(EventDto evt, bool unused = true, bool tryToFindInputForHoldableEvent = false)
        {
            AbstractVRInput res = null;

            if (HoldInput != null && tryToFindInputForHoldableEvent && HoldInput.IsAvailable())
                res = HoldInput;

            if (res == null)
            {
                foreach (BooleanInput input in booleanInputs)
                {
                    if (input.IsAvailable() || !unused)
                    {
                        res = input;
                        break;
                    }
                }
            }

            if (res == null)
            {
                //if no boolean input was found
                foreach (MenuInput menu in menuInputs)
                {
                    if (menu.IsAvailable())
                    {
                        res = menu;
                        break;
                    }
                }
            }

            if (res == null)
            {
                MenuInput menuInput = this.gameObject.AddComponent<MenuInput>();
                menuInput.Init(this);
                menuInputs.Add(menuInput);
                res = menuInput;
            }

            PlayerMenuManager.Instance.CtrlToolMenu.AddBinding(res);

            return res;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="param"></param>
        /// <param name="unused"></param>
        /// <returns></returns>
        public override AbstractUMI3DInput FindInput(AbstractParameterDto param, bool unused = true)
        {
            if (param is FloatRangeParameterDto)
            {
                FloatRangeParameterInput floatRangeInput = floatRangeParameterInputs.Find(i => i.IsAvailable());
                if (floatRangeInput == null)
                {
                    floatRangeInput = this.gameObject.AddComponent<FloatRangeParameterInput>();
                    floatRangeInput.Init(this);
                    floatRangeParameterInputs.Add(floatRangeInput);
                }
                return floatRangeInput;
            }
            else if (param is FloatParameterDto)
            {
                FloatParameterInput floatInput = floatParameterInputs.Find(i => i.IsAvailable());
                if (floatInput == null)
                {
                    floatInput = this.gameObject.AddComponent<FloatParameterInput>();
                    floatInput.Init(this);
                    floatParameterInputs.Add(floatInput);
                }
                return floatInput;
            }
            else if (param is IntegerParameterDto)
            {
                IntParameterInput intInput = intParameterInputs.Find(i => i.IsAvailable());
                if (intInput == null)
                {
                    intInput = new IntParameterInput();
                    intParameterInputs.Add(intInput);
                }
                return intInput;

            }
            else if (param is IntegerRangeParameterDto)
            {
                throw new System.NotImplementedException();
            }
            else if (param is BooleanParameterDto)
            {
                BooleanParameterInput boolInput = boolParameterInputs.Find(i => i.IsAvailable());
                if (boolInput == null)
                {
                    boolInput = this.gameObject.AddComponent<BooleanParameterInput>();
                    boolInput.Init(this);
                    boolParameterInputs.Add(boolInput);
                }
                return boolInput;
            }
            else if (param is StringParameterDto)
            {
                StringParameterInput stringInput = stringParameterInputs.Find(i => i.IsAvailable());
                if (stringInput == null)
                {
                    stringInput = this.gameObject.AddComponent<StringParameterInput>();
                    stringInput.Init(this);
                    stringParameterInputs.Add(stringInput);
                }
                return stringInput;
            }
            else if (param is EnumParameterDto<string>)
            {
                StringEnumParameterInput stringEnumInput = stringEnumParameterInputs.Find(i => i.IsAvailable());
                if (stringEnumInput == null)
                {
                    stringEnumInput = this.gameObject.AddComponent<StringEnumParameterInput>();
                    stringEnumInput.Init(this);
                    stringEnumParameterInputs.Add(stringEnumInput);
                }
                return stringEnumInput;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="form"></param>
        /// <param name="unused"></param>
        /// <returns></returns>
        public override AbstractUMI3DInput FindInput(FormDto form, bool unused = true)
        {
            foreach (FormMenuInput input in formInputs)
            {
                if (input.IsAvailable())
                    return input;
            }

            FormMenuInput menuInput = this.gameObject.AddComponent<FormMenuInput>();
            menuInput.Init(this);
            formInputs.Add(menuInput);
            return menuInput;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="link"></param>
        /// <param name="unused"></param>
        /// <returns></returns>
        public override AbstractUMI3DInput FindInput(LinkDto link, bool unused = true)
        {
            throw new System.NotImplementedException();
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
            return tool.interactions.TrueForAll(inter =>
                (inter is ManipulationDto) ?
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

            List<AbstractInteractionDto> manips = tool.interactions.FindAll(x => x is ManipulationDto);
            List<AbstractInteractionDto> events = tool.interactions.FindAll(x => x is EventDto);
            List<AbstractInteractionDto> param = tool.interactions.FindAll(x => x is AbstractParameterDto);
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
            List<AbstractInteractionDto> interactions = tool.interactions;
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

        #endregion

        #region Change mapping

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

        #endregion

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

        #endregion
    }

}