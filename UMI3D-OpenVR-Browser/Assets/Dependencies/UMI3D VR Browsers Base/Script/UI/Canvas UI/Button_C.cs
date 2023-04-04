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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui
{
    public class Button_C
    {
        [HideInInspector]
        public GameObject Go;
        [HideInInspector]
        public RectTransform RectT;
        [HideInInspector]
        public RectTBehaviour RectTBehaviour;
        [HideInInspector]
        public BoxCollider BoxCollider;
        [HideInInspector]
        public Button Button;
        [HideInInspector]
        public Image Teint;

        [HideInInspector]
        public GameObject Content;

        protected Button_C() { }

        public Button_C(string text, UnityAction clicked) : this(clicked, new Text_C(text).Go)
        {
        }

        public Button_C(UnityAction clicked, GameObject content)
        {
            Go = new GameObject("Button");
            Button = Go.AddComponent<Button>();
            Teint = Go.AddComponent<Image>();
            RectT = Go.GetComponent<RectTransform>();
            RectTBehaviour = Go.AddComponent<RectTBehaviour>();
            BoxCollider = Go.AddComponent<BoxCollider>();

            Button.targetGraphic = Teint;
            var colorBlock = Button.colors;
            colorBlock.normalColor = new Color { a = 0f };
            colorBlock.highlightedColor = new Color { a = 0.3f };
            colorBlock.pressedColor = new Color { g = 0.4047148f, b = 1f, a = 1f };
            colorBlock.selectedColor = new Color { a = .6f };
            Button.colors = colorBlock;

            Content = content;
            Go.Add(content);
            Button.onClick.AddListener(clicked);

            RectTBehaviour.RectTransformChanged += () =>
            {
                BoxCollider.size = new Vector3(RectT.sizeDelta.x, RectT.sizeDelta.y, 1f);
            };
        }
    }
}
