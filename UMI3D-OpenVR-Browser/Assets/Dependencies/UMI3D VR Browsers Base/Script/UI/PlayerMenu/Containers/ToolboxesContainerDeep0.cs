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
using umi3d.cdk.menu;
using umi3d.cdk.menu.view;
using umi3dVRBrowsersBase.ui.displayers;
using umi3dVRBrowsersBase.ui.watchMenu;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    public partial class ToolboxesContainerDeep0
    {
        /// <summary>
        /// Is this container a tool (containes displayers) or toolbox (containes other toolboxes).
        /// </summary>
        public bool IsTool { get; set; } = false;
        /// <summary>
        /// Is this container pinned.
        /// </summary>
        public bool IsPin { get; set; } = false;
        public event Action<bool, AbstractMenuItem> OnPinnedUnpinned;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override GameObject Displayerbox
        {
            get
            {
                if (m_displayerbox == null)
                {
                    m_displayerbox = new GameObject();
                    m_displayerbox.name = "Displayerbox" + "-" + menu.Name;
                    RectTransform rectTransform = m_displayerbox.AddComponent<RectTransform>();
                    rectTransform.sizeDelta = new Vector2(350f, rectTransform.sizeDelta.y);
                    VerticalLayoutGroup layout = m_displayerbox.AddComponent<VerticalLayoutGroup>();
                    layout.childControlWidth = true;
                    layout.childControlHeight = false;
                    ContentSizeFitter contentSize = m_displayerbox.AddComponent<ContentSizeFitter>();
                    contentSize.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
                return m_displayerbox;
            }
        }

        [Header("Button, Dropdown")]
        [SerializeField]
        [Tooltip("Dropdown button.")]
        protected ActiveIdleExpandButton m_dropdown = null;
        [SerializeField]
        [Tooltip("Pin button.")]
        protected OnOffButton m_pinButton = null;

        [Header("Containers")]
        [SerializeField]
        [Tooltip("Containers containing Toolboxes, Displayerboxes and DeepNPlus containers.")]
        protected GameObject m_containers = null;
        [SerializeField]
        [Tooltip("Toolboxes container")]
        protected GameObject m_toolboxesContainer = null;
        [SerializeField]
        [Tooltip("Displayerboxes container")]
        protected GameObject m_displayerboxesContainer = null;
        [SerializeField]
        [Tooltip("Containers of deep N > 0 container")]
        protected GameObject m_deepNPlusContainer = null;

        protected GameObject m_displayerbox = null;
    }

    public partial class ToolboxesContainerDeep0 : AbstractPlayerMenuContainer
    {
        protected override void Awake()
        {
            base.Awake();
            isExpanded = true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        protected override void DisplayImp(bool forceUpdate)
        {
            this.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="element"></param>
        protected override void InsertImp(AbstractDisplayer element)
        {
            if (element is ToolboxesContainerDeep1 container)
            {
                IsTool = false;
                AddButtonToList(container);
                AddBoxInContainer(container);
                SetChildTransform(container);
                //container.transform.SetParent(m_deepNPlusContainer.transform, false);
                //AddChildrenToContainer(container);
            }
            else if (element is IDisplayerUIGUI displayer)
            {
                ButtonList.SetActive(false);
                Displayerbox.transform.SetParent(m_displayerboxesContainer.transform, false);

                IsTool = true;
                AddDisplayerToBox(displayer);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="container"></param>
        protected override void ExpandAsImp(AbstractMenuDisplayContainer container)
        {
            base.ExpandAsImp(container);
            m_containers.SetActive(true);
            ForceUpdateLayoutFrom(m_containers);
            m_dropdown.Expand(true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        protected override void CollapseImp()
        {
            base.CollapseImp();
            m_containers.SetActive(false);
            ForceUpdateLayoutFrom(m_containers);
            m_dropdown.Expand(false);
        }
    }

    public partial class ToolboxesContainerDeep0
    {
        /// <summary>
        /// Pin or unpin the toolbox.
        /// </summary>
        public void TogglePin()
        {
            if (!m_pinButton.IsOn && ToolboxesContainerRoot.PinCount > 3)
                return;
            TooglePinWithoutNotify();
            OnPinnedUnpinned.Invoke(IsPin, menu);
        }

        public void TooglePinWithoutNotify()
        {
            IsPin = !m_pinButton.IsOn;
            m_pinButton.Toggle(IsPin);
            m_dropdown.SetActive(IsPin);
            if (IsPin)
                WatchMenu.PinMenu((Menu)menu);
            else
                WatchMenu.UnPinMenu((Menu)menu);
        }

        public void AddBoxInContainer(ToolboxesContainerDeep1 container)
        {
            if (container.IsTool)
            {
                container.Displayerbox.transform.SetParent(m_displayerboxesContainer.transform, false);
                container.Toolbox.SetActive(false);
            }
            else
            {
                container.Toolbox.transform.SetParent(m_toolboxesContainer.transform, false);
                container.Displayerbox.SetActive(false);
            }
        }

        public void SetChildTransform(ToolboxesContainerDeep1 container)
            => container.transform.SetParent(m_deepNPlusContainer.transform, false);
    }
}