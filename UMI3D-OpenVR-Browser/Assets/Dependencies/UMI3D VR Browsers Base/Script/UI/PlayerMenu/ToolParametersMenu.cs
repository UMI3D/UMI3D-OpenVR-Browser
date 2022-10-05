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
using umi3d.cdk.menu;
using umi3dVRBrowsersBase.interactions;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    /// <summary>
    /// This class handles the edition of all parameters projected on each controller.
    /// </summary>
    public class ToolParametersMenu : AbstractMenuManager
    {
        #region Fields

        /// <summary>
        /// List of all parameter menu items by controller.
        /// </summary>
        private Dictionary<ControllerType, List<AbstractMenuItem>> parameters = new Dictionary<ControllerType, List<AbstractMenuItem>>();
        private Dictionary<ControllerType, Dictionary<string, AbstractMenuItem>> cachedParameters = new Dictionary<ControllerType, Dictionary<string, AbstractMenuItem>>();
        private Dictionary<AbstractMenuItem, Action> cachedCallbacks = new Dictionary<AbstractMenuItem, Action>();
        /// <summary>
        /// The menu is async when opened through the parameter gear.
        /// </summary>
        private bool isAsync;
        public bool IsAsync => isAsync;

        #endregion

        #region Methods

        /// <summary>
        /// Inits different fields.
        /// </summary>
        private void InitFields()
        {
            if (parameters.Count == 0)
                foreach (ControllerType type in Enum.GetValues(typeof(ControllerType)))
                {
                    parameters.Add(type, new List<AbstractMenuItem>());
                }
        }

        /// <summary>
        /// Adds a parameter menu item for a given <paramref name="controller"/>.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="item"></param>
        public void AddParameter(ControllerType controller, AbstractMenuItem item, Action callbackOnDesynchronize)
        {
            if (isAsync)
                return;

            if (parameters.Count == 0) InitFields();

            parameters[controller].Add(item);

            if (!cachedCallbacks.ContainsKey(item))
                cachedCallbacks.Add(item, callbackOnDesynchronize);
        }

        /// <summary>
        /// Removes a parameter menu item for a given <paramref name="controller"/>.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="item"></param>
        public void RemoveParameter(ControllerType controller, AbstractMenuItem item)
        {
            if (isAsync)
                return;

            if (parameters.Count == 0)
            {
                InitFields();
            }

            parameters[controller].Remove(item);
        }

        /// <summary>
        /// Displays the menu (do not use <see cref="ToolParametersMenu.Open()"/>
        /// </summary>
        /// <param name="controller"></param>
        public void Display(ControllerType controller, bool isAsync = false)
        {
            Open();

            this.isAsync = isAsync;

            menuDisplayManager.menu.RemoveAll();

            if (isAsync)
            {
                foreach (var item in cachedParameters[controller].Values)
                {
                    menuDisplayManager.menu.Add(item);
                }
            }
            else
            {
                cachedParameters.Clear();
                cachedCallbacks.Clear();
                foreach (AbstractMenuItem item in parameters[controller])
                    menuDisplayManager.menu.Add(item);
            }

            menuDisplayManager.Display(true);
        }

        public void Remember()
        {
            cachedParameters.Clear();
            foreach (var controller in parameters.Keys)
            {
                int i = 0;
                cachedParameters.Add(controller, new Dictionary<string, AbstractMenuItem>());
                foreach (var item in parameters[controller])
                {
                    if (!cachedParameters[controller].ContainsKey(item.Name + i))
                        cachedParameters[controller].Add(item.Name + i, item);
                    i++;
                }
            }

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Close()
        {
            base.Close();
            menuDisplayManager.Hide();
            menuDisplayManager.menu.RemoveAll();
            if (isAsync)
                isAsync = false;
            foreach (var callback in cachedCallbacks.Values)
                callback();
            cachedCallbacks.Clear();
            foreach (var controllerKey in cachedParameters.Keys)
                cachedParameters[controllerKey].Clear();
        }

        /// <summary>
        /// Sets the label for the header of the menu.
        /// </summary>
        /// <param name="toolName"></param>
        public void SetToolName(string toolName)
        {
            menuDisplayManager.menu.Name = toolName;
        }

        #endregion
    }
}