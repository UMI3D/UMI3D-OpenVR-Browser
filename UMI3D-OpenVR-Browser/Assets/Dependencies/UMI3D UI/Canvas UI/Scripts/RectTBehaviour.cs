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
using UnityEngine.EventSystems;

namespace umi3dVRBrowsersBase.ui
{
    public class RectTBehaviour : UIBehaviour
    {
        [HideInInspector]
        public RectTransform RectT;

        public event System.Action RectTransformChanged;

        protected override void Awake()
        {
            RectT = gameObject.GetComponent<RectTransform>();
            if (RectT == null ) RectT = gameObject.AddComponent<RectTransform>();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            RectTransformChanged?.Invoke();
        }
    }
}
