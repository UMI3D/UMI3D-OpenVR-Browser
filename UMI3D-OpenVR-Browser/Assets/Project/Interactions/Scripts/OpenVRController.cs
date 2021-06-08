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
using UnityEngine;
using umi3d.cdk.interaction;
using umi3d.common.interaction;

using System.Linq;
using umi3d.cdk.menu.interaction;
using System.Collections;
using umi3d.cdk.menu;
using umi3d.cdk.userCapture;

public class OpenVRController : AbstractController
{
    #region Fields

    public PlayerMenuManager controllersMenu;
    public string hoveredObjectId;
    public InteractableRaySelector interactableRaySelector;

    public Valve.VR.SteamVR_Input_Sources controllerType;


    [SerializeField]
    protected OpenVRInputObserver releaseCurrentToolButton;

    [SerializeField]
    UMI3DClientUserTrackingBone bone;

    #region Inputs Fields

    [Header("Manipulations")]

    public List<ManipulationInput> manipulationInputs = new List<ManipulationInput>();

    [Header("Other")]
    public List<BooleanInput> booleanInputs = new List<BooleanInput>();

    private int inputhash = 0;

    private List<AbstractUMI3DInput> lastComputedInputs = null;
    List<MenuInput> menuInputs = new List<MenuInput>();
    List<FormMenuInput> formInputs = new List<FormMenuInput>();

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
                List<AbstractUMI3DInput> buffer = new List<AbstractUMI3DInput>();
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

        Debug.Assert(interactableRaySelector != null);
    }

    protected virtual void Update()
    {
        if (timeSinceLastInput <= inputUsageTimeout)
            timeSinceLastInput += Time.deltaTime;
    }

    /// <summary>
    /// Clear all menus and the projected tools.
    /// </summary>
    public override void Clear()
    {
        //controllersMenu.currentToolMenu.RemoveAll();
        controllersMenu.ClearParameterMenu();

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
        foreach(FormMenuInput input in formInputs)
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

    public AbstractUMI3DInput FindInput(ManipulationDto manip, DofGroupEnum dofs, bool unused = true)
    {
        AbstractUMI3DInput result = null;

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

        if (result != null)
            return result;

        //if no input was found
        MenuInput menuInput = this.gameObject.AddComponent<MenuInput>();
        menuInput.oculusInput = this;
        menuInput.bone = bone.boneType;
        menuInputs.Add(menuInput);
        return menuInput;
    }

    public override AbstractUMI3DInput FindInput(EventDto evt, bool unused = true)
    {
        foreach (BooleanInput input in booleanInputs)
        {
            if (input.IsAvailable() || !unused)
            {
                return input;
            }
        }

        //if no boolean input was found
        MenuInput menuInput = this.gameObject.AddComponent<MenuInput>();
        menuInput.oculusInput = this;
        menuInput.bone = bone.boneType;
        menuInputs.Add(menuInput);
        return menuInput;
    }

    public override AbstractUMI3DInput FindInput(AbstractParameterDto param, bool unused = true)
    {
        var toolParametersMenu = controllersMenu.toolParametersMenuDisplay.menu as Menu;
        Debug.Assert(toolParametersMenu != null);

        if (param is FloatRangeParameterDto)
        {
            FloatRangeParameterInput floatRangeInput = floatRangeParameterInputs.Find(i => i.IsAvailable());
            if (floatRangeInput == null)
            {
                floatRangeInput = this.gameObject.AddComponent<FloatRangeParameterInput>();
                //Debug.Assert(controllersMenu.currentToolMenu != null);
                //floatRangeInput.rootMenu = controllersMenu.currentToolMenu;
                floatRangeInput.rootMenu = toolParametersMenu;
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
                //Debug.Assert(controllersMenu.currentToolMenu != null);
                //floatInput.rootMenu = controllersMenu.currentToolMenu;
                floatInput.rootMenu = toolParametersMenu;
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
                //Debug.Assert(controllersMenu.currentToolMenu != null);
                //intInput.rootMenu = controllersMenu.currentToolMenu;
                intInput.rootMenu = toolParametersMenu;
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
                //Debug.Assert(controllersMenu.currentToolMenu != null);
                //boolInput.rootMenu = controllersMenu.currentToolMenu;
                boolInput.rootMenu = toolParametersMenu;
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
                //Debug.Assert(controllersMenu.currentToolMenu != null);
                //stringInput.rootMenu = controllersMenu.currentToolMenu;
                stringInput.rootMenu = toolParametersMenu;
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
                //Debug.Assert(controllersMenu.currentToolMenu != null);
                //stringEnumInput.rootMenu = controllersMenu.currentToolMenu;
                stringEnumInput.rootMenu = toolParametersMenu;
                stringEnumParameterInputs.Add(stringEnumInput);
            }
            return stringEnumInput;
        }
        else
        {
            return null;
        }
    }

    public override AbstractUMI3DInput FindInput(FormDto form, bool unused = true)
    {
        FormMenuInput menuInput = this.gameObject.AddComponent<FormMenuInput>();
        menuInput.oculusInput = this;
        menuInput.bone = bone.boneType;
        formInputs.Add(menuInput);
        return menuInput;
    }

    public override AbstractUMI3DInput FindInput(LinkDto link, bool unused = true)
    {
        throw new System.NotImplementedException();
    }

    #endregion

    #region Change input binding

    AbstractUMI3DInput newInput = null;

    HoldableButtonMenuItem holdableButtonWhichWantsToBeBinded = null;

    List<System.Action> chooseNewInputCallbacks = new List<System.Action>();

    List<AbstractUMI3DInput> otherInputs;

    public bool isListeningToInput { get; private set; } = false;

    public bool WasInputSet { get => (newInput != null); }

    /// <summary>
    /// A user wants to change the binding for the interaction (eventDto) associated to menuItem, so the controller listens to the new input.
    /// </summary>
    /// <param name="menuItem"></param>
    public void StartListeningToChangeBinding(HoldableButtonMenuItem menuItem, UnityEngine.UI.Text informationText, AbstractUMI3DInput previousInput)
    {
        if (isListeningToInput)
        {
            Debug.LogError("This controller is already listening to a new action binding");
            return;
        }

        isListeningToInput = true;

        //1.Find the right alternative inputs for menuItem;
        otherInputs = null;

        if (menuItem.associatedInteractionDto is EventDto)
            otherInputs = booleanInputs.ConvertAll(b => b as AbstractUMI3DInput);
        else if (menuItem.associatedInteractionDto is ManipulationDto)
        {
            otherInputs = manipulationInputs.ConvertAll(m => m as AbstractUMI3DInput);
        }
        else
        {
            Debug.LogError("Impossible to change the binding of this interaction " + menuItem.associatedInteractionDto?.GetType());
        }

        //2.Make them listen to users' trigger
        foreach (var otherIput in otherInputs)
        {
            System.Action callBack = () => {

                if (newInput != null)
                    return;

                newInput = otherIput;
                if (newInput == previousInput)
                {
                    informationText.text = "This is the same binding, please choose another button or validate to cancel.";
                    newInput = null;
                }
                else if (!newInput.IsAvailable())
                {
                    informationText.text = (otherIput as IModifiableBindingInput).GetCurrentButtonName() + " is already used for " + newInput.CurrentInteraction().name + ". Are you sure ?";
                }
                else
                {
                    informationText.text = (otherIput as IModifiableBindingInput).GetCurrentButtonName() + ".Are you sure ?";
                }
            };
            chooseNewInputCallbacks.Add(callBack);

            (otherIput as IModifiableBindingInput).IsInputBeeingModified = true;
            (otherIput as IModifiableBindingInput).GetOpenVRObserverObersver().AddOnStateDownListener(callBack);
        }

        holdableButtonWhichWantsToBeBinded = menuItem;
    }

    /// <summary>
    /// After choosing (or not) a new binbding
    /// </summary>
    public void StopListeningToChangeBinding()
    {
        if (!isListeningToInput)
        {
            Debug.Log("This controller is not listening to a new action binding, so you can't stop it.");
            return;
        }
        isListeningToInput = false;

        if (otherInputs != null)
        {
            for (int i = 0; i < otherInputs.Count; i++)
            {
                (otherInputs[i] as IModifiableBindingInput).IsInputBeeingModified = false;
                (otherInputs[i] as IModifiableBindingInput).GetOpenVRObserverObersver().RemoveOnStateDownListener(chooseNewInputCallbacks[i]);
            }
        }

        chooseNewInputCallbacks.Clear();
    }

    public void ResetBindingModification()
    {
        holdableButtonWhichWantsToBeBinded = null;
        newInput = null;
        holdableButtonWhichWantsToBeBinded = null;
    }

    /// <summary>
    /// Binds the interaction associated to menuItem to newInput.
    /// 
    /// Precondition :
    ///     - new Input must have been set before (with StartListeningToChangeBinding()).
    ///     - holdableButtonWhichWantsToBeBinded must have been set before (with StartListeningToChangeBinding()).
    /// </summary>
    /// <param name="menuItem"></param>
    /// <param name="previousInput">If the inbteraction was already binded, the previous input associated.</param>
    public void ChangeBinding(AbstractUMI3DInput previousInput = null)
    {
        if (holdableButtonWhichWantsToBeBinded == null)
        {
            Debug.LogError("No interaction was to set so its impossible to change its binding.");
            return;
        }

        if (newInput == null)
        {
            Debug.LogError(holdableButtonWhichWantsToBeBinded.Name + " would like to be binded on a new input but no one was set.");
            return;
        }

        //1. If an interaction was already binded on newInput, unbind it.
        bool assignPreviousInput = false;
        HoldableButtonMenuItem itemToReAssign = null;
        if (!newInput.IsAvailable())
        {
            itemToReAssign = (newInput as IModifiableBindingInput).GetBindingMenuItem();
            RemoveFromAssociatedInputs(itemToReAssign.toolId, newInput);
            assignPreviousInput = true;
            newInput.Dissociate();
        }


        //2. If the interaction associated to menuItem already had an input, unbind it.
        if (previousInput != null)
        {
            string toolId = holdableButtonWhichWantsToBeBinded.toolId;
            RemoveFromAssociatedInputs(toolId, previousInput);
            previousInput.Dissociate();
        }


        //3.Associate the interaction to the newInput. 
        newInput.Associate(holdableButtonWhichWantsToBeBinded.associatedInteractionDto,
            holdableButtonWhichWantsToBeBinded.toolId, holdableButtonWhichWantsToBeBinded.hoveredObjectId);
        AddToAssociateInputs(holdableButtonWhichWantsToBeBinded.toolId, newInput);

        Debug.Log("<color=cyan>" + holdableButtonWhichWantsToBeBinded.associatedInteractionDto.name + " is now binded on " + (newInput as IModifiableBindingInput).GetCurrentButtonName() + " before " + (newInput as IModifiableBindingInput)?.GetCurrentButtonName() + "</color>");

        //4. If newInput had an interaction previously binded, we have to find it a new input.
        if (assignPreviousInput)
        {
            AbstractUMI3DInput input = null;
            if (itemToReAssign.associatedInteractionDto is EventDto evt)
            {
                input = FindInput(evt, true);
            }
            else if (itemToReAssign.associatedInteractionDto is ManipulationDto manip)
            {
                input = FindInput(manip, itemToReAssign.dofs);
            }

            if (input != null)
            {
                input.Associate(itemToReAssign.associatedInteractionDto, itemToReAssign.toolId, itemToReAssign.hoveredObjectId);
                AddToAssociateInputs(itemToReAssign.toolId, input);
            }
        }

        ResetBindingModification();

        controllersMenu.DisplayToolParamatersMenu(true);
    }

    void RemoveFromAssociatedInputs(string toolId, AbstractUMI3DInput input)
    {
        if (associatedInputs.ContainsKey(toolId))
        {
            var inputs = associatedInputs[toolId].ToList();
            inputs.Remove(input);
            associatedInputs[toolId] = inputs.ToArray();
        }
    }

    void AddToAssociateInputs(string toolId, AbstractUMI3DInput input)
    {
        if (associatedInputs.ContainsKey(toolId))
        {
            var inputs = associatedInputs[toolId].ToList();
            inputs.Add(input);
            associatedInputs[toolId] = inputs.ToArray();
        }
    }

    void LateUpdate()
    {
        if (isListeningToInput && WasInputSet)
        {
            StartCoroutine(Reset());
        }      
    }

    IEnumerator Reset()
    {
        while (controllersMenu.screenDisplayer.joystick.selectButton.GetState(controllersMenu.screenDisplayer.joystick.controller))
            yield return null;

        StopListeningToChangeBinding();
        controllersMenu.screenDisplayer.DisplayCancelValidateButton();
    }
    #endregion

    #region Tool : projection and release

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

    public override void Project(AbstractTool tool, bool releasable, InteractionMappingReason reason, string hoveredObjectId)
    {
        //controllersMenu.currentToolMenu.RemoveAll();
        base.Project(tool, releasable, reason, hoveredObjectId);
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
        /*while (controllersMenu.currentToolMenu == null)
            yield return new WaitForEndOfFrame();*/

        List<AbstractInteractionDto> interactions = tool.interactions;
        List<AbstractInteractionDto> manips = interactions.FindAll(inter => inter is ManipulationDto);
        foreach (AbstractInteractionDto manip in manips)
        {
            DofGroupOptionDto bestSeparationOption = FindBest((manip as ManipulationDto).dofSeparationOptions.ToArray());
            foreach (DofGroupDto sep in bestSeparationOption.separations)
            {
                ManipulationMenuItem manipSeparationMenu = new ManipulationMenuItem()
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
                            List<AbstractUMI3DInput> toolInputs = new List<AbstractUMI3DInput>();
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

                //controllersMenu.currentToolMenu.Add(manipSeparationMenu);
                controllersMenu.AddMenuItemToParamatersMenu(manipSeparationMenu);

                if (FindInput(manip as ManipulationDto, sep, true) != null)
                {
                    manipSeparationMenu.Select();
                }
            }
        }

        List<AbstractInteractionDto> events = interactions.FindAll(inter => inter is EventDto);
        foreach (AbstractInteractionDto evt in events)
        {
            EventMenuItem eventMenu = new EventMenuItem()
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
                        List<AbstractUMI3DInput> toolInputs = new List<AbstractUMI3DInput>();
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


            //controllersMenu.currentToolMenu.Add(eventMenu);
            controllersMenu.AddMenuItemToParamatersMenu(eventMenu);

            if (FindInput(evt as EventDto, true) != null)
            {
                eventMenu.Select();
            }
        }

        ProjectParameters(tool, interactions, hoveredObjectId);

        //HDResourceCache.Download(tool.icon2D, texture => controllersMenu.currentToolMenu.icon2D = texture);
    }
    private void ProjectParameters(AbstractTool tool, List<AbstractInteractionDto> interactions, string hoveredObjectId)
    {
        AbstractUMI3DInput[] inputs = projectionMemory.Project(this, interactions.FindAll(inter => inter is AbstractParameterDto).ToArray(), tool.id, hoveredObjectId);
        List<AbstractUMI3DInput> toolInputs = new List<AbstractUMI3DInput>();

        if (associatedInputs.TryGetValue(tool.id, out AbstractUMI3DInput[] buffer))
        {
            toolInputs = new List<AbstractUMI3DInput>(buffer);
            associatedInputs.Remove(tool.id);
        }
        toolInputs.AddRange(inputs);
        associatedInputs.Add(tool.id, toolInputs.ToArray());

    }


    public override void Release(AbstractTool tool, InteractionMappingReason reason)
    {
        base.Release(tool, reason);
        tool.onReleased(bone.boneType);
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

    protected override string GetCurrentHoveredId()
    {
        return interactableRaySelector.GetLastHoveredInteractableId();
    }

    #endregion
}


