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
using umi3d.cdk.menu;
using umi3d.cdk.menu.view;
using umi3dVRBrowsersBase.ui.displayers;
using UnityEngine;
using UnityEngine.UI;
using static umi3dVRBrowsersBase.ui.playerMenu.Tool_ToolboxButton;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    public partial class ToolboxesContainerDeep1
    {
        /// <summary>
        /// The button of the container.
        /// </summary>
        public GameObject Button => m_button;
        /// <summary>
        /// The toolbox (list of buttons sub-tools or sub-toolboxes).
        /// </summary>
        public GameObject Toolbox => m_buttonList;
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

        /// <summary>
        /// Is this container a tool (containes displayers) or toolbox (containes other toolboxes).
        /// </summary>
        public bool IsTool { get; set; } = false;
    }

    public partial class ToolboxesContainerDeep1
    {
        [Header("Button")]
        [SerializeField]
        [Tooltip("The button of this container.")]
        protected GameObject m_button = null;

        protected GameObject m_displayerbox = null;
    }

    public partial class ToolboxesContainerDeep1 : AbstractPlayerMenuContainer
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="menu"></param>
        public override void SetMenuItem(AbstractMenuItem menu)
        {
            base.SetMenuItem(menu);
            m_button.name = $"{menu.Name}-ButtonDeep1";
            Toolbox.name = $"{menu.Name}-ToolboxDeep1";
            Tool_ToolboxButton button = m_button.GetComponent<Tool_ToolboxButton>();
            if (menu.icon2D != null && menu.icon2D.width > 0f && menu.icon2D.height > 0f)
                button.SetCustomIcon(menu.icon2D);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        protected override void DisplayImp(bool forceUpdate)
            => Button.SetActive(true);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void HideImp()
        {
            base.HideImp();
            Button.SetActive(false);
            DisplayHideBox(false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="container"></param>
        protected override void ExpandAsImp(AbstractMenuDisplayContainer container)
        {
            base.ExpandAsImp(container);
            DisplayHideBox(true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        protected override void CollapseImp()
        {
            base.CollapseImp();
            DisplayHideBox(false);
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
            }
            else if (element is IDisplayerUIGUI displayer)
            {
                IsTool = true;
                AddDisplayerToBox(displayer);
            }
            FindDeep0AndAddBoxInContainer();
        }
    }

    public partial class ToolboxesContainerDeep1
    {
        /// <summary>
        /// Display of hide displayerbox or toolbox
        /// </summary>
        /// <param name="value"></param>
        private void DisplayHideBox(bool value)
        {
            if (IsTool)
            {
                Displayerbox.SetActive(value);
                ForceUpdateLayoutFrom(Displayerbox);
            }
            else
            {
                Toolbox.SetActive(value);
                ForceUpdateLayoutFrom(Toolbox);
            }
        }

        private void FindDeep0AndAddBoxInContainer()
        {
            var parent = this.parent;
            while (parent != null && !(parent is ToolboxesContainerDeep0))
                parent = parent.parent;

            if (parent == null)
                throw new System.Exception("Parent null, not ToolboxesContainerDeep0");

            (parent as ToolboxesContainerDeep0).AddBoxInContainer(this);
            (parent as ToolboxesContainerDeep0).SetChildTransform(this);
        }
    }
}