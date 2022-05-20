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

using UnityEngine;

namespace umi3dVRBrowsersBase.ui.keyboard
{
    /// <summary>
    /// Defines a letter key for <see cref="Keyboard"/>.
    /// </summary>
    public class KeyboardLetter : KeyboardKey
    {
        /// <summary>
        /// Character associated to this key.
        /// </summary>
        public string symbol;

        protected override void Start()
        {
            base.Start();

            Debug.Assert(!string.IsNullOrEmpty(symbol), name + " keyboard letter has no symbol");
            Debug.Assert(button != null, name + " keyboard letter has no button");
        }
    }
}
