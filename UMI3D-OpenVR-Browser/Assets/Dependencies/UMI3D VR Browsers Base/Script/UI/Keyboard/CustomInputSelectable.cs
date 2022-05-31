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

using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.keyboard
{
    /// <summary>
    /// Override of the default Unity <see cref="InputField"/> to make it raise OnSelect() and OnDeselect() events.
    /// </summary>
    public class CustomInputSelectable : InputField
    {
        /// <summary>
        /// Event raised when the input is selected.
        /// </summary>
        public UnityEvent OnSelectEvent = new UnityEvent();

        /// <summary>
        /// Event raised when the input is deselected.
        /// </summary>
        public UnityEvent OnDeSelectEvent = new UnityEvent();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            OnSelectEvent?.Invoke();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            OnDeSelectEvent?.Invoke();
        }
    }
}