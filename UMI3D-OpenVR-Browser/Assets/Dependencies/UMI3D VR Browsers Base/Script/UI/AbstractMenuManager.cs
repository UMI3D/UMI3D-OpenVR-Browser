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

using umi3d.cdk.menu.view;
using UnityEngine;

namespace umi3dVRBrowsersBase.ui
{
    /// <summary>
    /// Base class to handle menus for VR UMI3D Browsers.
    /// </summary>
    public abstract class AbstractMenuManager : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Menu manager reponsible for displaying the data.
        /// </summary>
        [SerializeField]
        protected MenuDisplayManager menuDisplayManager;

        /// <summary>
        /// Is the menu open ?
        /// </summary>
        public bool IsOpen { get; private set; } = false;

        #endregion

        #region

        /// <summary>
        /// Opens the menu.
        /// </summary>
        public virtual void Open()
        {
            if (!IsOpen)
            {
                IsOpen = true;
            }
        }

        /// <summary>
        /// Closes the menu
        /// </summary>
        public virtual void Close()
        {
            if (IsOpen)
            {
                IsOpen = false;
            }
        }

        #endregion
    }

}

