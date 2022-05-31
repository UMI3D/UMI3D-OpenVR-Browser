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
using umi3d.cdk.menu.view;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.displayers
{
    /// <summary>
    /// 2D Container
    /// </summary>
    public class PanelContainer : AbstractMenuDisplayContainer
    {
        #region Fields

        /// <summary>
        /// Where all children displayed will be created.
        /// </summary>
        public GameObject viewport;

        /// <summary>
        /// Label to display associated menu item name.
        /// </summary>
        public Text label;

        /// <summary>
        /// Button go navigate back.
        /// </summary>
        public Button backButton;

        /// <summary>
        /// Button to select associated menu item.
        /// </summary>
        public Button selectButton;

        /// <summary>
        /// List of all current children displayers.
        /// </summary>
        private List<AbstractDisplayer> containedDisplayers = new List<AbstractDisplayer>();

        /// <summary>
        /// Reference to associated virtual container.
        /// </summary>
        private AbstractMenuDisplayContainer virtualContainer;

        /// <summary>
        /// Getter for <see cref="virtualContainer"/>.
        /// </summary>
        protected AbstractMenuDisplayContainer VirtualContainer
        {
            get => virtualContainer; set
            {
                if (virtualContainer != null)
                    backButton?.onClick.RemoveListener(virtualContainer.backButtonPressed.Invoke);
                virtualContainer = value;
                bool display = virtualContainer?.parent != null;
                if (backButton != null && backButton.gameObject.activeSelf != display) backButton.gameObject.SetActive(display);
                if (display)
                    backButton?.onClick.AddListener(virtualContainer.backButtonPressed.Invoke);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override AbstractDisplayer this[int i] { get => containedDisplayers[i]; set { RemoveAt(i); Insert(value, i); } }

        #endregion

        #region Methods

        /// <summary>
        /// Hides <see cref="backButton"/>.
        /// </summary>
        private void HideBackButton()
        {
            backButton?.gameObject.SetActive(false);
            if (virtualContainer != null)
                backButton?.onClick.RemoveListener(virtualContainer.backButtonPressed.Invoke);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            foreach (AbstractDisplayer displayer in containedDisplayers)
                displayer.Clear();
            HideBackButton();
            selectButton?.onClick.RemoveAllListeners();
            RemoveAll();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public override bool Contains(AbstractDisplayer element)
        {
            return containedDisplayers.Contains(element);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Display(bool forceUpdate = false)
        {
            if (isDisplayed && !forceUpdate)
            {
                return;
            }
            selectButton?.onClick.AddListener(Select);
            this.gameObject.SetActive(true);
            if (VirtualContainer != null) VirtualContainer = this;
            if (label)
                label.text = menu.Name;
            foreach (AbstractDisplayer disp in containedDisplayers)
            {
                disp.Display();
            }
            isDisplayed = true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public override int GetIndexOf(AbstractDisplayer element)
        {
            return element.transform.GetSiblingIndex();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<AbstractDisplayer> GetDisplayers()
        {
            return containedDisplayers;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Hide()
        {
            foreach (AbstractDisplayer disp in containedDisplayers)
            {
                disp.Hide();
            }
            HideBackButton();
            selectButton?.onClick.RemoveListener(Select);
            this.gameObject.SetActive(false);
            isDisplayed = false;
        }

        /// <summary>
        /// Insert an element in the display container.
        /// </summary>
        /// <param name="element">Element to insert</param>
        /// <param name="updateDisplay">Should update the display (default is true)</param>
        public override void Insert(AbstractDisplayer element, bool updateDisplay = true)
        {
            element.transform.SetParent(viewport.transform, false);

            containedDisplayers.Add(element);
            if (updateDisplay)
                Display();
        }

        /// <summary>
        /// Insert a collection of elements in the display container.
        /// </summary>
        /// <param name="elements">Elements to insert</param>
        /// <param name="updateDisplay">Should update the display (default is true)</param>
        public override void InsertRange(IEnumerable<AbstractDisplayer> elements, bool updateDisplay = true)
        {
            foreach (AbstractDisplayer e in elements)
            {
                Insert(e, false);
            }
            if (updateDisplay)
                Display();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="index"></param>
        /// <param name="updateDisplay"></param>
        public override void Insert(AbstractDisplayer element, int index, bool updateDisplay = true)
        {
            element.transform.SetParent(viewport.transform, false);
            element.transform.SetSiblingIndex(index);

            containedDisplayers.Add(element);
            if (updateDisplay)
                Display();
        }

        /// <summary>
        /// Remove an object from the display container.
        /// </summary>
        /// <param name="element">Element to remove</param>
        /// <param name="updateDisplay">Should update the display (default is true)</param>
        public override bool Remove(AbstractDisplayer element, bool updateDisplay = true)
        {
            if (element == null) return false;
            bool ok = containedDisplayers.Remove(element);
            if (updateDisplay)
                Display();
            return ok;
        }

        /// <summary>
        /// Remove all elements from the display container.
        /// </summary>
        public override int RemoveAll()
        {
            var elements = new List<AbstractDisplayer>(containedDisplayers);
            int count = 0;
            foreach (AbstractDisplayer element in elements)
                if (Remove(element, false)) count++;
            return count;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="updateDisplay"></param>
        /// <returns></returns>
        public override bool RemoveAt(int index, bool updateDisplay = true)
        {
            return Remove(containedDisplayers?[index], updateDisplay);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Expand(bool forceUpdate = false)
        {
            if (!isDisplayed)
            {
                Display(forceUpdate);
            }
            if (isExpanded && !forceUpdate)
            {
                return;
            }

            if (VirtualContainer != null && VirtualContainer != this)
            {
                label.text = menu.Name;
                foreach (AbstractDisplayer displayer in VirtualContainer)
                {
                    displayer.Hide();
                    displayer.transform.SetParent((VirtualContainer as PanelContainer)?.viewport?.transform);
                }
            }

            viewport.SetActive(true);
            VirtualContainer = this;
            selectButton?.onClick.RemoveListener(Select);
            foreach (AbstractDisplayer displayer in this)
            {
                displayer.Display();
            }
            isExpanded = true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Collapse(bool forceUpdate = false)
        {
            if (!isExpanded && !forceUpdate)
            {
                return;
            }
            if (VirtualContainer != null && VirtualContainer != this)
            {
                label.text = menu.Name;
                foreach (AbstractDisplayer displayer in VirtualContainer)
                {
                    displayer.Hide();
                    displayer.transform.SetParent((VirtualContainer as PanelContainer)?.viewport?.transform);
                }
                VirtualContainer = this;
            }
            HideBackButton();
            viewport.SetActive(false);
            backButton?.gameObject.SetActive(false);
            selectButton?.onClick.AddListener(Select);
            foreach (AbstractDisplayer displayer in this)
            {
                displayer.Hide();
            }
            isExpanded = false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="Container"></param>
        /// <param name="forceUpdate"></param>
        public override void ExpandAs(AbstractMenuDisplayContainer Container, bool forceUpdate = false)
        {
            if (isExpanded && !forceUpdate)
            {
                return;
            }
            if (VirtualContainer != null && VirtualContainer != Container)
            {
                backButton?.gameObject.SetActive(false);
                selectButton?.onClick.AddListener(Select);
                foreach (AbstractDisplayer displayer in this)
                {
                    displayer.Hide();
                }
            }

            VirtualContainer = Container;
            label.text = Container.menu.Name;
            viewport.SetActive(true);
            selectButton?.onClick.RemoveListener(Select);
            foreach (AbstractDisplayer displayer in VirtualContainer)
            {
                displayer.transform.SetParent(viewport.transform);
                displayer.Display();
            }
            isExpanded = true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override AbstractMenuDisplayContainer CurrentMenuDisplayContainer()
        {
            return VirtualContainer;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public override int IsSuitableFor(umi3d.cdk.menu.AbstractMenuItem menu)
        {
            return (menu is umi3d.cdk.menu.Menu) ? 1 : 0;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int Count()
        {
            return containedDisplayers.Count;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void ExpandImp()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="container"></param>
        protected override void ExpandAsImp(AbstractMenuDisplayContainer container)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void CollapseImp()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}