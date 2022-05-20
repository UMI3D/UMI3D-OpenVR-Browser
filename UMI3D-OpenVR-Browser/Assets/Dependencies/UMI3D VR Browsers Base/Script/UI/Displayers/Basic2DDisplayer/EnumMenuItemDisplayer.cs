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
using umi3d.cdk.menu;
using umi3d.cdk.menu.view;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.displayers
{
    /// <summary>
    /// 2D Displayer for an enum parameter.
    /// </summary>
    public class EnumMenuItemDisplayer : AbstractDropDownInputDisplayer, IDisplayerUIGUI
    {
        #region Fields

        [Tooltip("Label to display associated item name")]
        public Text label;

        [Tooltip("Maximum number of element visible at the same time")]
        public int displayedAmount = 4;

        [Tooltip("Template to instanciate to display an element")]
        public GameObject itemTemplate;

        [Tooltip("Element where all element are created")]
        public GameObject viewport;

        [Tooltip("Background to set show this element is selected")]
        public Sprite selectedSprite;

        [Tooltip("Background to set show this element is not selected")]
        public Sprite unselectedSprite;

        /// <summary>
        /// Is this element selected ?
        /// </summary>
        private bool isDisplayed = false;

        /// <summary>
        /// List of all current displayers.
        /// </summary>
        private List<GameObject> itemDisplayers = new List<GameObject>();

        /// <summary>
        /// Index of first left element displayed.
        /// </summary>
        private int cursor = 0;

        /// <summary>
        /// Class to represent an entry.
        /// </summary>
        private class EnumItem
        {
            /// <summary>
            /// Image of the entry.
            /// </summary>
            public Image image;

            /// <summary>
            /// Label to display the name of an entry.
            /// </summary>
            public Text text;

            /// <summary>
            /// Sprite to show this element is selected.
            /// </summary>
            public Sprite selectedSprite;

            /// <summary>
            /// Sprite to show this element is not selected.
            /// </summary>
            public Sprite unselectedSprite;

            /// <summary>
            /// Selects this element.
            /// </summary>
            public void Select()
            {
                image.sprite = selectedSprite;
            }

            /// <summary>
            /// Unselects this element.
            /// </summary>
            public void UnSelect()
            {
                image.sprite = unselectedSprite;
            }
        }

        /// <summary>
        /// Current element selected.
        /// </summary>
        private EnumItem currentSelectedChoice;

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Display(bool forceUpdate = false)
        {
            if (isDisplayed && !forceUpdate)
                return;

            if (!isDisplayed)
            {
                foreach (string s in menuItem.options)
                {
                    GameObject item = Instantiate(itemTemplate, viewport.transform);
                    item.SetActive(true);
                    Text text = item.GetComponentInChildren<Text>();
                    text.text = s;
                    item.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        if (s != menuItem.GetValue())
                        {
                            menuItem.NotifyValueChange(s);

                            currentSelectedChoice?.UnSelect();
                            currentSelectedChoice = new EnumItem { image = item.GetComponentInChildren<Image>(), text = text, selectedSprite = selectedSprite, unselectedSprite = unselectedSprite };
                            currentSelectedChoice.Select();
                        }
                    });

                    itemDisplayers.Add(item);

                    if (s == menuItem.GetValue())
                    {
                        currentSelectedChoice = new EnumItem { image = item.GetComponentInChildren<Image>(), text = text, selectedSprite = selectedSprite, unselectedSprite = unselectedSprite };
                        currentSelectedChoice.Select();
                    }
                }
            }

            this.gameObject.SetActive(true);

            label.text = menuItem.Name;
            ArrangeDisplayers();
            isDisplayed = true;
        }

        /// <summary>
        /// Moves right in the carousel.
        /// </summary>
        public void Next()
        {
            cursor++;
            cursor = Mathf.Min(cursor, itemDisplayers.Count - displayedAmount);
            ArrangeDisplayers();
        }

        /// <summary>
        /// Moves left in the carousel.
        /// </summary>
        public void Previous()
        {
            cursor--;
            cursor = Mathf.Max(cursor, 0);
            ArrangeDisplayers();
        }

        /// <summary>
        /// Displays only diplayers which must be visible.
        /// </summary>
        private void ArrangeDisplayers()
        {
            for (int i = 0; i < itemDisplayers.Count; i++)
            {
                GameObject item = itemDisplayers[i];
                bool display = i >= cursor && i < cursor + displayedAmount;
                item.SetActive(display);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Hide()
        {
            foreach (GameObject g in itemDisplayers) Destroy(g);
            itemDisplayers.Clear();
            this.gameObject.SetActive(false);
            isDisplayed = false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public override int IsSuitableFor(AbstractMenuItem menu)
        {
            return (menu is AbstractEnumInputMenuItem<string>) ? 2 : 0;
        }

        #endregion
    }
}