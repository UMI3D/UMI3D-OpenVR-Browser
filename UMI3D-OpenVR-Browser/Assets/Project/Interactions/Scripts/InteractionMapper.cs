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
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk;
using umi3d.cdk.interaction;
using umi3d.cdk.menu;
using umi3d.cdk.menu.interaction;
using umi3d.common.interaction;
using UnityEngine;

/*
public class InteractionMapper : AbstractInteractionMapper
{
    public new static InteractionMapper Instance { get { return AbstractInteractionMapper.Instance as InteractionMapper; } }

    /// <summary>
    /// If an interactable is loaded before its gameobject, the interaction mapper will wait <see cref="interactableLoadTimeout"/> seconds before destroying it.
    /// </summary>
    public float interactableLoadTimeout = 60;

    /// <summary>
    /// Menu to store toolboxes into.
    /// </summary>
    public Menu toolboxMenu = new Menu() { Name = "Toolboxes" };


    #region Data

    /// <summary>
    /// Associate a toolid with the controller the tool is projected on.
    /// </summary>
    public Dictionary<string, AbstractController> toolIdToController { get; protected set; } = new Dictionary<string, AbstractController>();

    /// <summary>
    /// Id to Dto interactions map.
    /// </summary>
    public Dictionary<string, AbstractInteractionDto> interactionsIdToDto { get; protected set; } = new Dictionary<string, AbstractInteractionDto>();

    /// <summary>
    /// Currently projected tools.
    /// </summary>
    private Dictionary<string, InteractionMappingReason> projectedTools = new Dictionary<string, InteractionMappingReason>();

    /// <summary>
    /// Get the tool associated to an interaction.
    /// </summary>
    private Dictionary<string, Tool> interactionsIdToTool = new Dictionary<string, Tool>();

    
    public AbstractController lastControllerUsedInMenu;

    #endregion


    public Tool GetToolFromInteraction(string interactionId)
    {
        return interactionsIdToTool[interactionId];
    }

    public List<Tool> GetToolsWithoutToolbox()
    {
        Dictionary<string, Toolbox> toolIdToToolbox = new Dictionary<string, Toolbox>();

        foreach (var toolbox in GetToolboxes())
        {
            foreach(Tool tool in toolbox.tools)
            {
                toolIdToToolbox.Add(tool.id, toolbox);
            }
        }

        return new List<AbstractTool>(GetTools()).FindAll(t => !toolIdToToolbox.ContainsKey(t.id)).FindAll(t => t is Tool).ConvertAll(t => t as Tool);
    }


    /// <summary>
    /// Reset the InteractionMapper module
    /// </summary>
    public override void ResetModule()
    {
        foreach (AbstractController c in Controllers)
            c.Clear();

        if (toolboxMenu != null)
        {
            toolboxMenu.RemoveAllSubMenu();
            toolboxMenu.RemoveAllMenuItem();
        }

        toolIdToController = new Dictionary<string, AbstractController>();
    }




    /// <summary>
    /// Select the best compatible controller for a given tool (not necessarily available).
    /// </summary>
    /// <param name="tool"></param>
    /// <returns></returns>
    protected AbstractController GetController(AbstractTool tool, InteractionMappingReason reason)
    {
        if (reason is RequestedFromMenu)
        {
            //Make sure to projetc on the controller which was used to select teh tool via menu
            if (lastControllerUsedInMenu != null)
            {
                return lastControllerUsedInMenu;
            }
        } else if (reason is RequestedUsingSelector requestedUsingSelectionReason)
        {
            return requestedUsingSelectionReason.controller;
        }

        foreach (AbstractController controller in Controllers)
        {
            if (controller.IsCompatibleWith(tool) && controller.IsAvailableFor(tool))
            {
                return controller;
            }
        }

        foreach (AbstractController controller in Controllers)
        {
            if (controller.IsCompatibleWith(tool))
            {
                return controller;
            }
        }

        return null;
    }

    /// <summary>
    /// Request a Tool to be released.
    /// </summary>
    /// <param name="dto">The tool to be released</param>
    public override void ReleaseTool(string toolId, InteractionMappingReason reason = null)
    {
        AbstractTool tool = GetTool(toolId);

        if (toolIdToController.TryGetValue(tool.id, out AbstractController controller))
        {
            controller.Release(tool, reason);
            toolIdToController.Remove(tool.id);
            projectedTools.Remove(tool.id);
        }
        else
        {
            throw new Exception("Tool not selected");
        }
    }

    /// <summary>
    /// Request the selection of a Tool.
    /// Be careful, this method could be called before the tool is added for async loading reasons
    /// </summary>
    /// <param name="dto">The tool to select</param>
    public override bool SelectTool(string toolId, bool releasable, string hoveredObjectId, InteractionMappingReason reason = null)
    {
        AbstractTool tool = GetTool(toolId);
        if (tool == null)
            throw new Exception("tool does not exist");

        if (toolIdToController.ContainsKey(tool.id))
        {
            throw new System.Exception("Tool already projected");
        }
       
        AbstractController controller = GetController(tool, reason);
        if (controller != null)
        {
            if (!controller.IsAvailableFor(tool))
            {
                if (ShouldForceProjection(controller, tool, reason))
                {
                    ReleaseTool(controller.tool.id);
                }
                else
                {
                    return false;
                }
            }

            return SelectTool(tool.id, releasable, hoveredObjectId, controller, reason);
        }
        else
        {
            throw new System.Exception("No controller is compatible with this tool");
        }
    }

    /// <summary>
    /// Request the selection of a Tool for a given controller.
    /// Be careful, this method could be called before the tool is added for async loading reasons.
    /// </summary>
    /// <param name="tool">The tool to select</param>
    /// <param name="controller">Controller to project the tool on</param>
    public bool SelectTool(string toolId, bool releasable, string hoveredObjectId, AbstractController controller, InteractionMappingReason reason = null)
    {
        AbstractTool tool = GetTool(toolId);
        if (controller.IsCompatibleWith(tool))
        {
            if (toolIdToController.ContainsKey(tool.id))
            {
                ReleaseTool(tool.id, new SwitchController());
            }

            toolIdToController.Add(tool.id, controller);
            projectedTools.Add(tool.id, reason);

            controller.Project(tool, releasable, reason, hoveredObjectId);
            return true;
        }
        else
        {
            throw new System.Exception("This controller is not compatible with this tool");
        }
    }

    /// <summary>
    /// Request a Tool to be replaced by another one.
    /// </summary>
    /// <param name="selected">The tool to be selected</param>
    /// <param name="released">The tool to be released</param>
    public override bool SwitchTools(string selected, string released, bool releasable, string hoveredObjectId, InteractionMappingReason reason = null)
    {
        ReleaseTool(released);
        if (!SelectTool(selected, releasable, hoveredObjectId, reason))
        {
            if (SelectTool(released, releasable, hoveredObjectId))
                return false;
            else
                throw new Exception("Internal error");
        }
        return true;
    }

    //this function will change/move in the future.
    protected bool ShouldForceProjection(AbstractController controller, AbstractTool tool, InteractionMappingReason reason)
    {
        if (controller.IsAvailableFor(tool))
            return true;

        if (controller.tool == null)
            return true; //check here

        if (projectedTools.TryGetValue(controller.tool.id, out InteractionMappingReason lastProjectionReason))
        {
            //todo : add some intelligence here.
            return !(reason is AutoProjectOnHover);
        }
        else
        {
            throw new Exception("Internal error");
        }
    }

    public override bool IsToolSelected(string toolId)
    {
        return projectedTools.ContainsKey(toolId);
    }


    #region CRUD



    public override void CreateToolbox(Toolbox toolbox)
    {
        toolboxMenu.Add(toolbox.sub);
    }

    public override void CreateTool(Tool tool)
    {
        foreach (var interaction in tool.dto.interactions)
        {
            interactionsIdToDto[interaction.id] = interaction;
            interactionsIdToTool[interaction.id] = tool;
        }
        tool.Menu.Subscribe(() =>
        {
            if (tool.Menu.toolSelected)
            {
                ReleaseTool(tool.id, new RequestedFromMenu());
            }
            else
            {
                SelectTool(tool.id, true, null, new RequestedFromMenu());
            }
        });
    }


    public override Toolbox GetToolbox(string id)
    {
        if (!ToolboxExists(id))
            throw new KeyNotFoundException();
        return UMI3DEnvironmentLoader.GetEntity(id)?.Object as Toolbox;
    }

    public override IEnumerable<Toolbox> GetToolboxes(Predicate<Toolbox> condition)
    {
        return Toolbox.Toolboxes().FindAll(condition);
    }

    public override AbstractTool GetTool(string id)
    {
        if (!ToolExists(id))
            throw new KeyNotFoundException();
        return UMI3DEnvironmentLoader.GetEntity(id)?.Object as AbstractTool;
    }

    public override IEnumerable<AbstractTool> GetTools(Predicate<AbstractTool> condition)
    {
        return UMI3DEnvironmentLoader.Entities().Where(e => e?.Object is AbstractTool).Select(e => e?.Object as AbstractTool).ToList().FindAll(condition);
    }

    public override AbstractInteractionDto GetInteraction(string id)
    {
        if (!InteractionExists(id))
            throw new KeyNotFoundException();
        interactionsIdToDto.TryGetValue(id, out AbstractInteractionDto inter);
        return inter;
    }

    public override IEnumerable<AbstractInteractionDto> GetInteractions(Predicate<AbstractInteractionDto> condition)
    {
        return interactionsIdToDto.Values.ToList().FindAll(condition);
    }

    public override bool ToolboxExists(string id)
    {
        return (UMI3DEnvironmentLoader.GetEntity(id)?.Object as Toolbox) != null;
    }

    public override bool ToolExists(string id)
    {
        return (UMI3DEnvironmentLoader.GetEntity(id)?.Object as AbstractTool) != null;
    }

    public override bool InteractionExists(string id)
    {
        return interactionsIdToDto.ContainsKey(id);
    }

    public override AbstractController GetController(string projectedToolId)
    {
        if (!IsToolSelected(projectedToolId))
            return null;

        toolIdToController.TryGetValue(projectedToolId, out AbstractController controller);
        return controller;
    }

    public override bool UpdateTools(string toolId, bool releasable, InteractionMappingReason reason = null)
    {
        throw new NotImplementedException();
    }


    #endregion
}
*/