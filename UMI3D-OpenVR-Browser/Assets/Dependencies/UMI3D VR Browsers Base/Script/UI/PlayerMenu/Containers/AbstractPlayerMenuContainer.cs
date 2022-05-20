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
using TMPro;
using umi3d.cdk.menu;
using umi3d.cdk.menu.view;
using umi3dVRBrowsersBase.ui.displayers;
using UnityEngine;
using UnityEngine.UI;
using static umi3dVRBrowsersBase.ui.playerMenu.Tool_ToolboxButton;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    public abstract partial class AbstractPlayerMenuContainer
    {
        /// <summary>
        /// Where the sub tool[box]s buttons are displayed.
        /// </summary>
        public GameObject ButtonListContent => m_buttonListContent;
        /// <summary>
        /// The whole game object where sub-tools buttons are displayed.
        /// </summary>
        public GameObject ButtonList => m_buttonList;
        /// <summary>
        /// Where displayers are displayed
        /// </summary>
        public abstract GameObject Displayerbox { get; }

        [SerializeField]
        [Tooltip("Where the name of the menu will be displayed.")]
        protected TextMeshProUGUI m_label = null;

        [Header("Sub-tools buttons list.")]
        [SerializeField]
        [Tooltip("The whole game object where sub-tools buttons are displayed.")]
        protected GameObject m_buttonList = null;
        [SerializeField]
        [Tooltip("Where the sub tool[box]s buttons are displayed.")]
        protected GameObject m_buttonListContent = null;
    }

    public abstract partial class AbstractPlayerMenuContainer : AbstractMenuContainer
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="menu"></param>
        public override void SetMenuItem(AbstractMenuItem menu)
        {
            base.SetMenuItem(menu);
            if (m_label != null)
                m_label.text = menu.Name;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Display(bool forceUpdate = false)
        {
            if (isDisplayed && !forceUpdate) return;
            isDisplayed = true;
            DisplayImp(forceUpdate);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Hide()
        {
            if (!isDisplayed) return;
            isDisplayed = false;
            HideImp();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="updateDisplay"></param>
        public override void Insert(AbstractDisplayer element, bool updateDisplay = true)
        {
            if (element is AbstractMenuDisplayContainer menuContainer)
                menuContainer.parent = this;

            InsertImp(element);

            currentDisplayers.Add(element);
            element.Hide();
            if (updateDisplay)
                Display(true);
        }
    }

    public abstract partial class AbstractPlayerMenuContainer
    {
        /// <summary>
        /// Implementation of the Display methode.
        /// </summary>
        /// <param name="forceUpdate"></param>
        protected abstract void DisplayImp(bool forceUpdate);

        /// <summary>
        /// Implementation of the Display methode.
        /// </summary>
        protected virtual void HideImp()
        {
            foreach (AbstractDisplayer disp in virtualContainer)
                disp.Hide();
        }

        /// <summary>
        /// Implementation of the Insert method.
        /// </summary>
        /// <param name="element"></param>
        protected abstract void InsertImp(AbstractDisplayer element);

        protected void ForceUpdateLayoutFrom(GameObject child)
        {
            RectTransform[] rectTransforms = child.transform.GetComponentsInParent<RectTransform>();
            foreach (RectTransform rectTransform in rectTransforms)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        protected void AddButtonToList(ToolboxesContainerDeep1 container)
        {
            container.Button.transform.SetParent(ButtonListContent.transform, false);
            Tool_ToolboxButton button = container.Button.GetComponent<Tool_ToolboxButton>();
            bool isContainerATool = container.IsTool;
            button.SetButtonType((isContainerATool) ? ToolboxButtonType.Tool : ToolboxButtonType.Toolbox);
        }

        protected void AddDisplayerToBox(IDisplayerUIGUI displayer)
        {
            ((MonoBehaviour)displayer).transform.SetParent(Displayerbox.transform, false);
            if (displayer is EnumMenuItemDisplayer enumDisplayer)
                enumDisplayer.displayedAmount = 1;
        }
    }
}
