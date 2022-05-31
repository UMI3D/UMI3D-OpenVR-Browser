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

using umi3d.cdk.menu.view;
using umi3dVRBrowsersBase.ui.displayers;
using UnityEngine;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    public class ToolParametersContainer : AbstractMenuContainer
    {
        #region Fields

        [SerializeField]
        private Transform scrollViewContent;

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="updateDisplay"></param>
        public override void Insert(AbstractDisplayer element, bool updateDisplay = true)
        {
            if (element is AbstractMenuDisplayContainer menuContainer)
            {
                menuContainer.parent = this;
            }

            element.transform.SetParent(this.scrollViewContent);
            element.transform.localPosition = Vector3.zero;

            currentDisplayers.Add(element);

            element.Hide();

            if (updateDisplay)
                Display(true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="index"></param>
        /// <param name="updateDisplay"></param>
        public override void Insert(AbstractDisplayer element, int index, bool updateDisplay = true)
        {
            if (element is AbstractMenuDisplayContainer menuContainer)
            {
                menuContainer.parent = this;
            }

            element.transform.SetParent(this.scrollViewContent);
            element.transform.SetSiblingIndex(index);

            currentDisplayers.Insert(index, element);
            element.Hide();

            if (updateDisplay)
                Display(true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="forceUpdate"></param>
        public override void ExpandAs(AbstractMenuDisplayContainer container, bool forceUpdate = false)
        {
            base.ExpandAs(container, forceUpdate);

            foreach (AbstractDisplayer disp in virtualContainer)
            {
                disp.Display();
                disp.transform.SetParent(this.scrollViewContent);
            }

            isExpanded = true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Expand(bool forceUpdate = false)
        {
            base.Expand(forceUpdate);
            ExpandAs(this, forceUpdate);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void ExpandImp()
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="container"></param>
        protected override void ExpandAsImp(AbstractMenuDisplayContainer container)
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void CollapseImp()
        {
        }

        #endregion
    }
}
