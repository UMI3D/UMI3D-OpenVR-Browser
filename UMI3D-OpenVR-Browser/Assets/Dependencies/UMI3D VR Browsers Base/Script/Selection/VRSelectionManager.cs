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

using System.Collections.Generic;
using System.Linq;
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
        public ElementVRSelector elementSelector;

        [Header("Cursor")]
        public AbstractCursor pointingCursor;

        public AbstractCursor grabCursor;

        public SelectionData LastSelectedInfo;

        private ISelector LastSelectorUsed;

        public void Update()
        {
            // Retrieves propositions from selectors
            var possibleSelec = new List<SelectionData>();
            if (interactableSelector.activated)
            {
                possibleSelec.AddRange(interactableSelector.Detect());
            }
            if (selectableSelector.activated)
            {
                possibleSelec.AddRange(selectableSelector.Detect());
            }
            if (elementSelector.activated)
            {
                possibleSelec.AddRange(elementSelector.Detect());
            }

            if (possibleSelec.Count == 0 && LastSelectedInfo != null)
            {
                LastSelectorUsed?.Deselect(LastSelectedInfo);
            }

            if (TrySelect(possibleSelec, DetectionOrigin.PROXIMITY))
                return;
            else if (TrySelect(possibleSelec, DetectionOrigin.POINTING))
                return;

        }

        private bool TrySelect(List<SelectionData> possibleSelec, DetectionOrigin origin)
        {
            var possibleSelecPointed = possibleSelec.Where(x => x.detectionOrigin == origin);
            if (possibleSelecPointed.Any())
            {
                if (possibleSelecPointed.Count() == 1)
                    StartSelection(possibleSelecPointed.First());
                else
                {
                    possibleSelecPointed.ToList().Sort(Compare);
                    StartSelection(possibleSelecPointed.Last());
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Commands the selection with the appropriate
        /// </summary>
        /// <param name="preferedObjectData"></param>
        private void StartSelection(SelectionData preferedObjectData)
        {
            ISelector appropriateSelector;
            // Disambiguation already occured
            switch (preferedObjectData.objectType)
            {
                case SelectableObjectType.SELECTABLE:
                    appropriateSelector = selectableSelector;
                    break;
                case SelectableObjectType.CLIENT_ELEMENT:
                    appropriateSelector = elementSelector;
                    break;
                case SelectableObjectType.INTERACTABLE:
                    appropriateSelector = interactableSelector;
                    break;
                default:
                    appropriateSelector = null;
                    break;
            }

            // selector switching, deselection is normally handled when the selector remains the sam
            if (LastSelectedInfo != null && LastSelectorUsed != appropriateSelector)
                LastSelectorUsed.Deselect(LastSelectedInfo);

            appropriateSelector.Select(preferedObjectData);
            LastSelectorUsed = appropriateSelector;
            LastSelectedInfo = preferedObjectData;

        }

        /// <summary>
        /// Compare selection data between them from a same detection origin.<br/>
        /// Preference: Client Element > Selectable > Interactable
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <returns></returns>
        public int Compare(SelectionData data1, SelectionData data2)
        {
            //! This method could be improved by comparing the distances, but it would assume the detector used
            if (data1.objectType == SelectableObjectType.CLIENT_ELEMENT)
                return 1;
            else if (data2.objectType == SelectableObjectType.CLIENT_ELEMENT) //if data1 is not c_element
                return -1;
            else if (data1.objectType == SelectableObjectType.SELECTABLE)
                return 1;
            else if (data2.objectType == SelectableObjectType.SELECTABLE)
                return -1;
            else if (data1.objectType == SelectableObjectType.INTERACTABLE)
                return 1;
            else if (data2.objectType == SelectableObjectType.INTERACTABLE)
                return -1;
            return 0;
        }
    }
}