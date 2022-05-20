﻿/*
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
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.displayers
{
    /// <summary>
    /// 2D Displayer for  <see cref="FloatRangeInputMenuItem"/>.
    /// </summary>
    public class FloatRangeMenuInputDisplayer : AbstractRangeInputDisplayer, IDisplayerUIGUI
    {
        #region Fields

        /// <summary>
        /// Label to display associated item name.
        /// </summary>
        public Text label;

        /// <summary>
        /// Slider to edit item's value.
        /// </summary>
        public Slider slider;

        /// <summary>
        /// Label to display <see cref="slider"/> value.
        /// </summary>
        public Text value;

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="forceUpdate"></param>
        public override void Display(bool forceUpdate = false)
        {
            this.gameObject.SetActive(true);
            if (menuItem.continuousRange)
            {
                slider.minValue = menuItem.min;
                slider.maxValue = menuItem.max;
                slider.wholeNumbers = false;
            }
            else
            {
                slider.minValue = 0;
                slider.maxValue = (menuItem.max - menuItem.min) / menuItem.increment;
            }

            label.text = menuItem.Name;
            value.text = menuItem.value.ToString("F1");

            slider.value = menuItem.GetValue();

            slider.onValueChanged.AddListener(v =>
            {
                NotifyValueChange(v);
                value.text = v.ToString("F1");
            });

            menuItem.Subscribe(v => slider.normalizedValue = Mathf.InverseLerp(menuItem.min, menuItem.max, menuItem.value));
        }

        public override void Hide()
        {
            this.gameObject.SetActive(false);
            slider.onValueChanged.RemoveAllListeners();
        }

        #endregion
    }
}