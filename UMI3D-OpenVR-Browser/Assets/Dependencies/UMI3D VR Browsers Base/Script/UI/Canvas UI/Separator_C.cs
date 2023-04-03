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
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui
{
    public class Separator_C
    {
        [HideInInspector]
        public GameObject Go;
        [HideInInspector]
        public RectTransform RectT;
        [HideInInspector]
        public RectTBehaviour RectTBehaviour;
        [HideInInspector]
        public Image Image;

        public float width;

        public Sprite Sprite
        {
            get => Image.sprite;
            set => Image.sprite = value;
        }

        public Separator_C()
        {
            Go = new GameObject("Separator");
            Image = Go.AddComponent<Image>();
            RectT = Go.GetComponent<RectTransform>();
            RectTBehaviour = Go.AddComponent<RectTBehaviour>();
        }
    }
}
