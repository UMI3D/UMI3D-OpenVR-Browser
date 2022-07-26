/*
Copyright 2019 - 2021 Inetum
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

using umi3d.cdk.interaction.selection.cursor;
using UnityEngine;

namespace umi3dVRBrowsersBase.interactions.selection
{
    public class VRSelectionManager : MonoBehaviour
    {
        /// <summary>
        /// Controller to manage selection for
        /// </summary>
        [Header("Controller")]
        public VRController controller;

        [Header("Selectors")]
        public InteractableVRSelector interactableSelector;
        public SelectableVRSelector selectableSelector;

        [Header("Cursor")]
        public AbstractCursor pointingCursor;
        public AbstractCursor grabCursor;
    }
}
