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

using System.Collections.Generic;
using umi3d.cdk.menu.view;
using UnityEngine;

namespace umi3dVRBrowsersBase.ui.displayers.watchMenu
{
    /// <summary>
    /// Container for second depth of the <see cref="<see cref="ui.watchMenu.WatchMenu"/> "/>.
    /// </summary>
    public class ToolWatchMenuContainer : AbstractWatchMenuContainer
    {
        #region Fields

        [Header("Scroll view parameter")]

        [SerializeField]
        private WatchScrollViewmode mode = WatchScrollViewmode.Radial;

        /// <summary>
        /// Distance from the center of the menu and the children of this container if <see cref="mode"/> is set to <see cref="WatchScrollViewmode.Radial"/>.
        /// </summary>
        [SerializeField]
        private float radiusOffset = 0.02f;

        /// <summary>
        /// Distance between children of this container if <see cref="mode"/> is set to <see cref="WatchScrollViewmode.Horizontal"/>.
        /// </summary>
        [SerializeField]
        private float horizontalOffset = 0.035f;

        [SerializeField]
        private float defaultHorizontalOffset = 0.06f;

        /// <summary>
        /// Only relevant if  <see cref="mode"/> is set to <see cref="WatchScrollViewmode.Horizontal"/>.
        /// </summary>
        [SerializeField]
        private Transform horizontalTransformRoot;

        /// <summary>
        /// Buttons to navigate through elements if they are more than <see cref="maxElementsDisplayed"/>.
        /// </summary>
        public List<DefaultClickableButton> navigationButtons = new List<DefaultClickableButton>();

        [SerializeField]
        private GameObject navigationButtonRoot;

        private int currentLeftElementDisplayed = 0;

        #endregion

        private void Start()
        {
            Debug.Assert(navigationButtons.Count == 2, "Two and only two navigation buttons are required for this element");

            foreach (DefaultClickableButton btn in navigationButtons)
                btn.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            navigationButtons[0].OnClicked.AddListener(NavigatePrevious);
            navigationButtons[1].OnClicked.AddListener(NavigateNext);
        }

        private void OnDisable()
        {
            navigationButtons[0].OnClicked.RemoveListener(NavigatePrevious);
            navigationButtons[1].OnClicked.RemoveListener(NavigateNext);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="forceUpdate"></param>
        public override void ExpandAs(AbstractMenuDisplayContainer container, bool forceUpdate = false)
        {
            base.ExpandAs(container, forceUpdate);

            currentLeftElementDisplayed = 0;

            UpdateDisplay();
        }

        /// <summary>
        /// Moves right in the menu.
        /// </summary>
        public void NavigateNext()
        {
            if (virtualContainer.Count() <= maxElementsDisplayed)
                return;

            if (currentLeftElementDisplayed + maxElementsDisplayed < virtualContainer.Count())
            {
                currentLeftElementDisplayed++;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Moves left in the menu.
        /// </summary>
        public void NavigatePrevious()
        {
            if (virtualContainer.Count() <= maxElementsDisplayed)
                return;

            if (currentLeftElementDisplayed > 0)
            {
                currentLeftElementDisplayed--;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Displays only dispkayers that must be displayed.
        /// </summary>
        private void UpdateDisplay()
        {
            for (int i = 0; i < virtualContainer.Count(); i++)
            {
                AbstractDisplayer disp = virtualContainer[i];

                if (i >= currentLeftElementDisplayed && i < maxElementsDisplayed + currentLeftElementDisplayed)
                {
                    disp.Display();

                    switch (mode)
                    {
                        case WatchScrollViewmode.Radial:
                            float angleOffset = 180f / maxElementsDisplayed;
                            angleOffset = angleOffset * (i - currentLeftElementDisplayed) - 48;
                            if (disp is IWatchDisplayerTransform watchElt)
                                watchElt.SetDisplayerRotation(0, angleOffset, 0);
                            break;
                        case WatchScrollViewmode.Horizontal:
                            disp.transform.SetParent(horizontalTransformRoot);
                            disp.transform.localPosition = Vector3.zero;
                            if (disp is IWatchDisplayerTransform watchElt1)
                                watchElt1.SetDisplayerPosition(0, 0, -(i - currentLeftElementDisplayed) * horizontalOffset - defaultHorizontalOffset);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (disp is IWatchDisplayerTransform watchElement)
                        watchElement.DisplayDisplayer(false);
                    else
                        disp.Hide();
                }
            }

            if (currentLeftElementDisplayed > 0)
                navigationButtons[0].gameObject.SetActive(true);
            else
                navigationButtons[0].gameObject.SetActive(false);

            if (currentLeftElementDisplayed + maxElementsDisplayed < virtualContainer.Count())
                navigationButtons[1].gameObject.SetActive(true);
            else
                navigationButtons[1].gameObject.SetActive(false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Collapse(bool forceUpdate = false)
        {
            base.Collapse(forceUpdate);

            foreach (DefaultClickableButton btn in navigationButtons)
                btn.gameObject.SetActive(false);
        }
    }

    internal enum WatchScrollViewmode { Radial, Horizontal };
}
