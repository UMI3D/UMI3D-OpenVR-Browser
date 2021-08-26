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
using System;
using System.Collections.Generic;
using umi3d.cdk.collaboration;
using umi3d.common.collaboration;
using umi3d.common.interaction;
using UnityEngine;

[CreateAssetMenu(fileName = "OculusQuestBrowserIdentifier", menuName = "UMI3D/Quest Browser Identifier")]
public class OculusQuestBrowserIdentifier : ClientIdentifierApi
{
    public Action<Action<string, string>> GetIdentityAction;
    public Action<Action<string>> GetPinAction;
    public Action<List<string>,Action<bool>> ShouldDownloadLib;
    public Action<FormDto,Action<FormAnswerDto>> GetParameters;

    public override void GetParameterDtos(FormDto parameter, Action<FormAnswerDto> callback)
    {
        GetParameters.Invoke(parameter, callback);
    }

    public override void ShouldDownloadLibraries(List<string> ids,Action<bool> callback)
    {
        ShouldDownloadLib.Invoke(ids,callback);
    }


    public override void GetIdentity(Action<UMI3DAuthenticator> callback)
    {
        callback.Invoke(new UMI3DAuthenticator(GetPin, GetLoginPassword, GetIdentity));
    }

    void GetPin(Action<string> callback)
    {
        if (GetPinAction != null)
            GetPinAction(callback);
        else
            callback?.Invoke(null);
    }

    void GetLoginPassword(Action<(string, string)> callback)
    {
        if (GetIdentityAction != null)
            GetIdentityAction((l, p) => callback((l, p)));
        else
            callback?.Invoke((null, null));
    }

    void GetIdentity(Action<IdentityDto> callback)
    {
        callback?.Invoke(UMI3DCollaborationClientServer.Identity);
    }
}