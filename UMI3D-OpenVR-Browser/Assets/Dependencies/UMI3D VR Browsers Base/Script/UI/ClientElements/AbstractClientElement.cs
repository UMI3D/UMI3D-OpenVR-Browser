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

using umi3dVRBrowsersBase.interactions;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.ui
{
    public abstract class AbstractClientInteractableElement : MonoBehaviour, IInteractableElement, ISelectableElement
    {
        public UnityEvent OnSelected { get; private set; } = new UnityEvent();

        public UnityEvent OnDeselected { get; private set; } = new UnityEvent();

        public abstract void Interact(VRController controller);

        public abstract void Select(VRController controller);

        public abstract void Deselect(VRController controller);

        protected bool isSelected;

        public bool IsSelected() => isSelected;

    }
}