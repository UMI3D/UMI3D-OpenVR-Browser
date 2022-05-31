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
        #region Firlds

        public Action<Action<string, string>> GetIdentityAction;
        public Action<Action<string>> GetPinAction;
        public Action<List<string>, Action<bool>> ShouldDownloadLib;
        public Action<FormDto, Action<FormAnswerDto>> GetParameters;

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="callback"></param>
        public override void GetParameterDtos(FormDto parameter, Action<FormAnswerDto> callback)
        {
            GetParameters.Invoke(parameter, callback);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="callback"></param>
        public override void ShouldDownloadLibraries(List<string> ids, Action<bool> callback)
        {
            ShouldDownloadLib.Invoke(ids, callback);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="callback"></param>
        public override void GetIdentity(Action<UMI3DAuthenticator> callback)
        {
            callback.Invoke(new UMI3DAuthenticator(GetPin, GetLoginPassword, GetIdentity));
        }

        /// <summary>
        /// Gets environement pin.
        /// </summary>
        /// <param name="callback"></param>
        private void GetPin(Action<string> callback)
        {
            if (GetPinAction != null)
                GetPinAction(callback);
            else
                callback?.Invoke(null);
        }

        /// <summary>
        /// Gets environment login and password.
        /// </summary>
        /// <param name="callback"></param>
        private void GetLoginPassword(Action<(string, string)> callback)
        {
            if (GetIdentityAction != null)
                GetIdentityAction((l, p) => callback((l, p)));
            else
                callback?.Invoke((null, null));
        }

        /// <summary>
        /// Gets identity.
        /// </summary>
        /// <param name="callback"></param>
        private void GetIdentity(Action<IdentityDto> callback)
        {
            callback?.Invoke(UMI3DCollaborationClientServer.Identity);
        }

        #endregion
    }
}