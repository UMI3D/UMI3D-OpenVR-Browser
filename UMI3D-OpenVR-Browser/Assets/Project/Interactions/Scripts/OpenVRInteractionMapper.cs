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
using UnityEngine;
using umi3d.cdk.interaction;
using umi3d.cdk.menu;

namespace QuestBrowser.Interactions
{
    /// <summary>
    /// Custom InteractionMapper for the Oculus Quest.
    /// </summary>
    public class OpenVRInteractionMapper : umi3d.cdk.interaction.InteractionMapper
    {
        #region Fields

        /// <summary>
        /// If an interactable is loaded before its gameobject, the interaction mapper will wait <see cref="interactableLoadTimeout"/> seconds before destroying it.
        /// </summary>
        public float interactableLoadTimeout = 60;

        #region Data

        /// <summary>
        /// Get the tool associated to an interaction.
        /// </summary>
        private Dictionary<string, Tool> interactionsIdToTool = new Dictionary<string, Tool>();

        public AbstractController lastControllerUsedInMenu;

        /// <summary>
        /// Associate a tool id and if it is releasbale or not.
        /// </summary>
        
        private Dictionary<string, bool> releasableTools = new Dictionary<string, bool>();

        #endregion

        #endregion

        #region Methods

        void Start()
        {
            toolboxMenu = new Menu { Name = "Toolbox" };
        }

        public override void ResetModule()
        {
            base.ResetModule();

            releasableTools.Clear();
        }

        /// <inheritdoc/>
        public override bool SelectTool(string toolId, bool releasable, string hoveredObjectId, InteractionMappingReason reason = null)
        {
            AbstractTool tool = GetTool(toolId);
            if (tool == null)
                throw new Exception("tool does not exist");

            if (toolIdToController.ContainsKey(tool.id))
            {
                throw new Exception("Tool already projected");
            }
            
            AbstractController controller = GetController(tool, reason);
            if (controller != null)
            {
                if (!controller.IsAvailableFor(tool))
                {
                    if (ShouldForceProjection(controller, tool, reason) || ShouldForceProjection(controller, reason))
                    {
                        ReleaseTool(controller.tool.id);
                    }
                    else
                    {
                        return false;
                    }
                }

                if (releasableTools.ContainsKey(tool.id))
                    releasableTools[tool.id] = releasable;
                else
                    releasableTools.Add(tool.id, releasable);

                bool res = SelectTool(tool.id, releasable, controller, hoveredObjectId, reason);
                if (res)
                    lastReason = reason;
                return res;
            }
            else
            {
                throw new Exception("No controller is compatible with this tool");
            }
        }

        public override void ReleaseTool(string toolId, InteractionMappingReason reason = null)
        {
            base.ReleaseTool(toolId, reason);
            lastReason = null;
        }

        InteractionMappingReason lastReason = null;

        /// <summary>
        /// To remove in the future, made because InteractionMapper.ShouldForceProjection(AbstractController, AbstractTool, Reason) can be overriden
        /// </summary>
        private bool ShouldForceProjection(AbstractController controller, InteractionMappingReason reason)
        {
            bool res = false;

            var oculusController = controller as OpenVRController;
            if(oculusController != null)
            {
                if (oculusController.controllersMenu.WasHiddenLastFrame && reason is AutoProjectOnHover)
                    res = true;
                else if (lastReason is AutoProjectOnHover && reason is AutoProjectOnHover)
                    res = true;

            } else
            {
                Debug.LogError("controller must be an instance of OculusCOntroller");
            }

            return res;
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
                //Make sure to project on the controller which was used to select the tool via menu
                if (lastControllerUsedInMenu == null)
                {
                    Debug.LogError("GetController requested from menu but lastControllerUsedInMenu is null");
                    return null;
                }
                return lastControllerUsedInMenu;
            }
            else if (reason is RequestedUsingSelector requestedUsingSelectionReason)
            {
                return requestedUsingSelectionReason.controller;
            }
            else
            {
                return GetController(tool);
            }
        }

        public Tool GetToolFromInteraction(string interactionId)
        {
            return interactionsIdToTool[interactionId];
        }

        /// <summary>
        /// Returns all tools which are not in a toolbox.
        /// </summary>
        /// <returns></returns>
        public List<Tool> GetToolsWithoutToolbox()
        {
            Dictionary<string, Toolbox> toolIdToToolbox = new Dictionary<string, Toolbox>();

            foreach (var toolbox in GetToolboxes())
            {
                foreach (Tool tool in toolbox.tools)
                {
                    toolIdToToolbox.Add(tool.id, toolbox);
                }
            }

            return new List<AbstractTool>(GetTools()).FindAll(t => !toolIdToToolbox.ContainsKey(t.id)).FindAll(t => t is Tool).ConvertAll(t => t as Tool);
        }

        /// <summary>
        /// Returns true if users can release the tool associated to toolId
        /// </summary>
        public bool IsToolReleasable(string toolId)
        {
            bool res = true;

            if (releasableTools.ContainsKey(toolId))
                res = releasableTools[toolId];

            return res;
        }

        /// <inheritdoc/>
        public override bool SwitchTools(string select, string release, bool releasable, string hoveredObjectId, InteractionMappingReason reason = null)
        {
            if (toolIdToController.ContainsKey(release))
            {
                AbstractController controller = toolIdToController[release];
                ReleaseTool(release);
                if (!SelectTool(select, releasable, controller, hoveredObjectId, reason))
                {
                    if (SelectTool(release, releasable, controller, hoveredObjectId))
                    {
                        lastReason = reason;
                        return false;
                    }
                    else
                        throw new Exception("Internal error");
                }
                else
                {
                    lastReason = reason;
                }
            }
            else
            {
                foreach (var c in Controllers)
                {
                    var menu = MenuOpenner.FindInstanceAssociatedToController(c);
                    var tool = menu?.playerMenuManager.currentToolMenu?.tool;
                    if (tool != null && tool.id == release)
                    {
                        if (SelectTool(select, releasable, hoveredObjectId, new RequestedUsingSelector { controller = c }))
                        {
                            menu.Close();
                            lastReason = new RequestedFromMenu();
                            return true;
                        }
                    }
                }

                if (!SelectTool(select, releasable, hoveredObjectId, reason))
                {
                    throw new Exception("Internal error");
                }
            }
            return true;
        }

        #region CRUD
        public override void CreateTool(Tool tool)
        {
            foreach (var interaction in tool.dto.interactions)
            {
                interactionsIdToTool[interaction.id] = tool;
            }
            base.CreateTool(tool);
        }

        #endregion

        #endregion

    }
}
