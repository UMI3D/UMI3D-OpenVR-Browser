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
    public class EmoteButton
    {
        [HideInInspector]
        public Button_C Button;
        [HideInInspector]
        public Image_C Emote;
        [HideInInspector]
        public Text_C Name;
        [HideInInspector]
        public Vbox_C Box;

        public event System.Action Clicked;

        public EmoteButton() 
        {
            Emote = new Image_C();
            Name = new Text_C();
            Box = new Vbox_C(0f, Emote.Go, Name.Go);
            Button = new Button_C(EmoteClicked, Box.Go);

            Button.RectTBehaviour.RectTransformChanged += () =>
            {
                var newSize = Button.RectT.sizeDelta;
                newSize /= 1.5f;
                Emote.RectT.sizeDelta = newSize;
                Emote.RectT.localPosition = Vector3.up * 3f;

                newSize = Button.RectT.sizeDelta;
                newSize.y = 8f;
                Name.RectT.sizeDelta = newSize;
                Name.RectT.localPosition = Vector3.up * -13f;
                Name.TextPro.fontSize = 7f;
                Name.TextPro.alignment = TMPro.TextAlignmentOptions.Center;
            };
            Button.RectT.sizeDelta = Vector2.one * 35f;
            Box.RectT.sizeDelta = Vector2.one * 35f;
        }

        public void EmoteClicked() => Clicked?.Invoke();
    }
}
