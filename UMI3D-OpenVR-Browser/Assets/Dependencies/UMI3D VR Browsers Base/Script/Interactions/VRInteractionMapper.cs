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

using System;
using System.Collections.Generic;
using umi3d.cdk.interaction;
using umi3d.cdk.menu;
using umi3dVRBrowsersBase.ui.playerMenu;
using UnityEngine;

namespace umi3dVRBrowsersBase.interactions
{
    /// <summary>
    /// Custom InteractionMapper for the VR UMI3D Browsers.
    /// </summary>
    public class VRInteractionMapper : InteractionMapper
    {
        #region Fields

        /// <summary>
        /// If an interactable is loaded before its gameobject, the interaction mapper will wait <see cref="interactableLoadTimeout"/> seconds before destroying it.
        /// </summary>
        public float interactableLoadTimeout = 60;

        /// <summary>
        /// Last controller used by users in a menu to trigger a UI.
        /// </summary>
        public static ControllerType lastControllerUsedToClick = ControllerType.RightHandController;

        #region Data

        /// <summary>
        /// Get the tool associated to an interaction.
        /// </summary>
        private Dictionary<ulong, GlobalTool> interactionsIdToTool = new Dictionary<ulong, GlobalTool>();

        public AbstractController lastControllerUsedInMenu;

        /// <summary>
        /// Associate a tool id and if it is releasable or not.
        /// </summary>

        private Dictionary<ulong, bool> releasableTools = new Dictionary<ulong, bool>();

        #endregion

        #endregion

        #region Methods

        private void Start()
        {
            toolboxMenu = new Menu { Name = "Toolbox" };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void ResetModule()
        {
            base.ResetModule();

            releasableTools.Clear();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="toolId"></param>
        /// <param name="releasable"></param>
        /// <param name="hoveredObjectId"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public override bool SelectTool(ulong toolId, bool releasable, ulong hoveredObjectId, InteractionMappingReason reason = null)
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
                {
                    lastReason = reason;
                }
                return res;
            }
            else
            {
                throw new Exception("No controller is compatible with this tool");
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="toolId"></param>
        /// <param name="reason"></param>
        public override void ReleaseTool(ulong toolId, InteractionMappingReason reason = null)
        {
            base.ReleaseTool(toolId, reason);
            lastReason = null;
        }

        private InteractionMappingReason lastReason = null;

        /// <summary>
        /// To remove in the future, made because InteractionMapper.ShouldForceProjection(AbstractController, AbstractTool, Reason) can be overriden
        /// </summary>
        private bool ShouldForceProjection(AbstractController controller, InteractionMappingReason reason)
        {
            bool res = false;

            var vrController = controller as VRController;
            if (vrController != null)
            {
                if (lastReason is AutoProjectOnHover && reason is AutoProjectOnHover)
                    res = true;
            }
            else
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

        /// <summary>
        /// Returns true if users can release the tool associated to toolId
        /// </summary>
        public bool IsToolReleasable(ulong toolId)
        {
            bool res = true;

            if (releasableTools.ContainsKey(toolId))
                res = releasableTools[toolId];

            return res;
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="select"></param>
        /// <param name="release"></param>
        /// <param name="releasable"></param>
        /// <param name="hoveredObjectId"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public override bool SwitchTools(ulong select, ulong release, bool releasable, ulong hoveredObjectId, InteractionMappingReason reason = null)
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
                foreach (AbstractController c in Controllers)
                {
                    if ((c as VRController).type == lastControllerUsedToClick)
                    {
                        if (SelectTool(select, releasable, hoveredObjectId, new RequestedUsingSelector { controller = c }))
                        {
                            PlayerMenuManager.Instance.Close();
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

        /// <summary>
        /// Gets <see cref="Transform"/> of <paramref name="controller"/>.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public Transform GetControllerTransform(ControllerType controller)
        {
            foreach (AbstractController c in Controllers)
            {
                if ((c as VRController)?.type == controller)
                    return c.transform;
            }
            return null;
        }

        /// <summary> 
        /// Returns <see cref="VRController"/> associated to <paramref name="controller"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public VRController GetController(ControllerType controller)
        {
            foreach (AbstractController c in Controllers)
            {
                if ((c as VRController)?.type == controller)
                    return (c as VRController);
            }
            return null;
        }

        #endregion
    }
}
