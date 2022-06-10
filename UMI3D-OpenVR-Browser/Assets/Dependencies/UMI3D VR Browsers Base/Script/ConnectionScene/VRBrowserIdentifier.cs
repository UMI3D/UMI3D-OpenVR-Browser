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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.cdk.collaboration;
using umi3d.common.collaboration;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// Identifier for VR UMI3D browsers;
    /// </summary>
    [CreateAssetMenu(fileName = "VRBrowserIdentifier", menuName = "UMI3D/VR Browser Identifier")]
    public class VRBrowserIdentifier : ClientIdentifierApi
    {
        #region Fields

        public Action<List<string>, Action<bool>> ShouldDownloadLib;
        public Action<FormDto, Action<FormAnswerDto>> GetParameters;

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="callback"></param>
        public override async Task<FormAnswerDto> GetParameterDtos(FormDto parameter)
        {
            bool b = true;
            FormAnswerDto form = null;
            Action<FormAnswerDto> callback = (f) => { form = f; b = false; };

            GetParameters.Invoke(parameter, callback);
            while (b)
                await Task.Yield();
            return form;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="callback"></param>
        public override async Task<bool> ShouldDownloadLibraries(List<string> LibrariesId)
        {
            bool b = true;
            bool form = false;
            Action<bool> callback = (f) => { form = f; b = false; };

            ShouldDownloadLib.Invoke(LibrariesId, callback);
            while (b)
                await Task.Yield();
            return form;
        }

        #endregion
    }
}