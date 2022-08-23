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
using UnityEngine;

namespace umi3dVRBrowsersBase.ui.displayers.watchMenu
{
    /// <summary>
    /// Base <see cref="AbstractMenuContainer"/> for the <see cref="ui.watchMenu.WatchMenu"/>.
    /// </summary>
    public abstract class AbstractWatchMenuContainer : AbstractMenuContainer, IWatchDisplayerTransform
    {
        #region Fields

        /// <summary>
        /// Transorm of the element which really display the information.
        /// </summary>
        [SerializeField]
        private DefaultClickableButtonElement displayerButton;

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Display(bool forceUpdate = false)
        {
            base.Display(forceUpdate);
            DisplayDisplayer(true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Select()
        {
            if (isExpanded)
            {
                Collapse(true);
                Back();
            }
            else
            {
                base.Select();
                //displayerButton?.Select();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Collapse(bool forceUpdate = false)
        {
            base.Collapse(forceUpdate);
            //displayerButton?.UnSelect();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Expand(bool forceUpdate = false)
        {
            base.Expand(forceUpdate);
            ExpandAs(this);

            //displayerButton?.Select();
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
                (disp as AbstractWatchDisplayer)?.SetDepth(GetDepth());

                disp.Display();
                disp.transform.SetParent(this.transform);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            //displayerButton?.UnSelect();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="updateDisplay"></param>
        public override void Insert(AbstractDisplayer element, bool updateDisplay = true)
        {
            base.Insert(element, updateDisplay);

            if (element is AbstractWatchDisplayer watchDisplayer)
                watchDisplayer.SetParent(this);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="index"></param>
        /// <param name="updateDisplay"></param>
        public override void Insert(AbstractDisplayer element, int index, bool updateDisplay = true)
        {
            base.Insert(element, index, updateDisplay);

            if (element is AbstractWatchDisplayer watchDisplayer)
                watchDisplayer.SetParent(this);
        }

        #region IWatchDisplayerTransform

        /// <summary>
        /// Rotates locally the element which displays the menu.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public virtual void SetDisplayerRotation(float x, float y, float z)
        {
            if (displayerButton != null)
            {
                displayerButton.transform.localRotation = Quaternion.Euler(x, y, z);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public virtual void SetDisplayerPosition(float x, float y, float z)
        {
            if (displayerButton != null)
            {
                displayerButton.transform.localPosition = new Vector3(x, y, z);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="b"></param>
        public void DisplayDisplayer(bool b)
        {
            if (displayerButton != null)
            {
                displayerButton.gameObject.SetActive(b);
            }
        }

        #endregion

        #endregion
    }
}