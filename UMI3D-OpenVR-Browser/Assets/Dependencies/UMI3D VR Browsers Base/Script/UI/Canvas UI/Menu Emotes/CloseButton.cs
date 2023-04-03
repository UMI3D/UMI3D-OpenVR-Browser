/*
Copyright 2019 - 2023 Inetum

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3dVRBrowsersBase.ui
{
    public class CloseButton
    {
        [HideInInspector]
        public Button_C Button;
        [HideInInspector]
        public Text_C Cross;

        public event System.Action Clicked;

        public CloseButton() 
        {
            Cross = new Text_C("X");
            Button = new Button_C(ButtonClicked, Cross.Go);

            Cross.TextPro.fontSize = 8f;
            Cross.TextPro.alignment = TMPro.TextAlignmentOptions.Center;
            Cross.RectT.sizeDelta = new Vector2(10f, 10f);

            Button.RectT.sizeDelta = new Vector2(10f, 10f);
            var colorBlock = Button.Button.colors;
            colorBlock.normalColor = new Color(r: 0.2995194f, g: 0.6650023f, b: 0.6698113f);
            Button.Button.colors = colorBlock;
        }

        public void ButtonClicked() => Clicked?.Invoke();
    }
}
