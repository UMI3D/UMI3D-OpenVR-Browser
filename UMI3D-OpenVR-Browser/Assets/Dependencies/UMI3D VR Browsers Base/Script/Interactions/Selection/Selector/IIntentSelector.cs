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

using System.Collections.Generic;
using umi3dBrowsers.interaction.selection.intentdetector;

namespace umi3dBrowsers.interaction.selection.selector
{
    /// <summary>
    /// Interface for Intent Selectors
    /// </summary>
    public interface IIntentSelector
    {
        /// <summary>
        /// Get selection intent info from the <see cref="AbstractDetector"/> attached to the intent selector
        /// </summary>
        /// <returns></returns>
        IEnumerable<SelectionIntentData> GetIntentDetections();

        /// <summary>
        /// Set as selection intent target
        /// </summary>
        /// <param name="data"></param>
        void Select(SelectionIntentData data);

        /// <summary>
        /// Unset as selection intent target
        /// </summary>
        /// <param name="data"></param>
        void Deselect(SelectionIntentData data);

        /// <summary>
        /// True if the selector is currently selecting an object
        /// </summary>
        /// <returns></returns>
        bool IsSelecting();
    }
}