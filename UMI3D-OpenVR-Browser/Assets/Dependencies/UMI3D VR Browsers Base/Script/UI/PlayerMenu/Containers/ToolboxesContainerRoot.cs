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
using TMPro;
using umi3d.cdk.menu;
using umi3d.cdk.menu.view;
using UnityEngine;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    public partial class ToolboxesContainerRoot
    {
        /// <summary>
        /// Not implement in this class but useful in the other containers.
        /// </summary>
        public override GameObject Displayerbox { get; } = null;
        /// <summary>
        /// Number of toolboxes pinned.
        /// </summary>
        public static int PinCount { get; private set; } = 0;

        [Header("Pin")]
        [SerializeField]
        [Tooltip("Pin count text.")]
        private TextMeshProUGUI m_pinCountTMP = null;

        private HashSet<AbstractMenuItem> m_pinnedMenus = new HashSet<AbstractMenuItem>();
    }

    public partial class ToolboxesContainerRoot
    {
        private void Start()
        {
            m_pinCountTMP.text = $"{PinCount}/4";
        }

        /// <summary>
        /// Unpin all toolboxes.
        /// </summary>
        public void UnpinAll()
        {
            foreach (var container in currentDisplayers)
            {
                if (container is ToolboxesContainerDeep0 containerDeep0 && containerDeep0.IsPin)
                    containerDeep0.TogglePin();
            }
        }

        private void PinUnpin(bool value, AbstractMenuItem menu)
        {
            PinCount += (value) ? 1 : -1;
            m_pinCountTMP.text = $"{PinCount}/4";
  
            if (value)
                m_pinnedMenus.Add(menu);
            else
                m_pinnedMenus.Remove(menu);
        }
    }

    public partial class ToolboxesContainerRoot : AbstractPlayerMenuContainer
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        protected override void DisplayImp(bool forceUpdate)
            => ButtonList.SetActive(true);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void HideImp()
        {
            base.HideImp();
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="updateDisplay"></param>
        protected override void InsertImp(AbstractDisplayer element)
        {
            element.transform.SetParent(ButtonListContent.transform, false);
            element.transform.localPosition = Vector3.zero;

            if (element is ToolboxesContainerDeep0 container)
            {
                container.OnPinnedUnpinned += PinUnpin;
                if (m_pinnedMenus.Contains(container.menu))
                    container.TooglePinWithoutNotify();
            }
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
                menuContainer.parent = this;
            element.transform.SetParent(ButtonListContent.transform, false);
            element.transform.SetSiblingIndex(index);

            currentDisplayers.Insert(index, element);
            element.Hide();
            if (updateDisplay)
                Display(true);
        }
    }
}

