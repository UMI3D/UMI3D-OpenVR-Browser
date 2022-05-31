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
using System.Linq;
using umi3d.cdk.menu.view;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.displayers.watchMenu
{
    /// <summary>
    /// Base displayer for all <see cref="AbstractDisplayer"/> used in the <see cref="WatchMenu"/>.
    /// </summary>
    public abstract class AbstractWatchDisplayer : AbstractDisplayer, IWatchDisplayerTransform
    {
        #region Fields

        /// <summary>
        /// The n-eme element of this list will be used to display this element when its depth will be equals to n.
        /// </summary>
        public List<DisplayerPrefab> prefabPerDepth = new List<DisplayerPrefab>();

        /// <summary>
        /// The n-eme text of this list will be used to display the name of the associated item.
        /// </summary>
        private List<Text> labelsPerPrefab = new List<Text>();

        /// <summary>
        /// Current prefab created to display the element.
        /// </summary>
        private DisplayerPrefab prefabCreated;

        /// <summary>
        /// Parent of this element.
        /// </summary>
        protected AbstractWatchMenuContainer parentContainer;

        #endregion

        #region Methods

        private void Awake()
        {
            foreach (DisplayerPrefab prefab in prefabPerDepth)
                labelsPerPrefab.Add(prefab.label);
        }

        /// <summary>
        /// Sets the menu depth of this element.
        /// </summary>
        /// <param name="i"></param>
        public void SetDepth(int i)
        {
            i = Mathf.Clamp(i, 0, prefabPerDepth.Count - 1);

            DisplayerPrefab prefabToChoose = prefabPerDepth[i];

            if (prefabCreated == null)
                prefabCreated = prefabToChoose;
            else if (prefabCreated != prefabToChoose)
                prefabCreated.obj.gameObject.SetActive(false);

            prefabCreated = prefabToChoose;
            prefabCreated.obj.gameObject.SetActive(true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public virtual void SetDisplayerRotation(float x, float y, float z)
        {
            if (prefabCreated != null)
            {
                prefabCreated.obj.transform.localRotation = Quaternion.Euler(x, y, z);
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
            if (prefabCreated != null)
            {
                prefabCreated.obj.transform.localPosition = new Vector3(x, y, z);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="b"></param>
        public void DisplayDisplayer(bool b)
        {
            if (prefabCreated != null)
            {
                prefabCreated.obj.gameObject.SetActive(b);
            }
        }

        /// <summary>
        /// Setter for <see cref="parentContainer"/>.
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(AbstractWatchMenuContainer parent)
        {
            this.parentContainer = parent;
        }

        #region AbstractDisplayer

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Display(bool forceUpdate = false)
        {
            name = menu.Name + "-" + GetType().ToString().Split('.').Last();

            foreach (Text label in labelsPerPrefab)
                if (label != null)
                    label.text = menu.Name;

            if (menu.icon2D != null && prefabCreated != null && prefabCreated.iconRenderer != null)
            {
                try
                {
                    prefabCreated.iconRenderer.material.SetTexture("_Map", menu.icon2D);
                }
                catch
                {
                    Debug.LogError("Impossible to set icon of displayer for " + menu.Name);
                }
            }

            gameObject.SetActive(true);
            DisplayDisplayer(true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Hide()
        {
            gameObject.SetActive(false);
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Class to describe a prefab than be used by <see cref="AbstractWatchDisplayer"/>.
    /// </summary>
    [System.Serializable]
    public class DisplayerPrefab
    {
        public DefaultClickableButton obj;

        public Renderer iconRenderer;

        public Text label;
    }
}
