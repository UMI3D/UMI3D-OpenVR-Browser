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
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the connection to the UMI3D environment.
/// </summary>
public class Connecting : Singleton<Connecting>
{
    #region Fields

    public OculusQuestBrowserIdentifier identifier;

    /// <summary>
    /// Name of the launcher scene (current scene).
    /// </summary>
    public string thisScene;

    /// <summary>
    /// Name of the environement scene.
    /// </summary>
    public string environmentScene;

    SignalingAsker.Data data;

    #endregion

    #region Methods

    protected override void Awake()
    {
        base.Awake();
        identifier.GetIdentityAction = DisplayLoginPassword;
        identifier.GetPinAction = DisplayPassword;
        identifier.ShouldDownloadLib = ShouldDlLibraries;
        identifier.GetParameters = GetParameterDtos;
    }

    private void Start()
    {
        UMI3DCollaborationClientServer.Instance.OnConnectionLost.AddListener(OnConnectionLost);
    }

    /// <summary>
    /// If some librairies are required to the join the environement, ask users to download them.
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="callback"></param>
    void ShouldDlLibraries(List<string> ids, Action<bool> callback)
    {
        LoadingScreen.Instance.Hide();

        if (ids.Count == 0)
        {
            callback.Invoke(true);
            LoadingScreen.Instance?.Display("Loading environment ...");
        }
        else DisplayAccept(ids.Count, callback);
    }

    /// <summary>
    /// If teh environement needs some information to be joined, asks this information to users.
    /// </summary>
    /// <param name="form"></param>
    /// <param name="callback"></param>
    void GetParameterDtos(FormDto form, Action<FormAnswerDto> callback)
    {
        FormAsker.Instance.Display(form, callback);
    }

    /// <summary>
    /// Tries to connect to the server.
    /// </summary>
    /// <param name="data"></param>
    public void Connect(SignalingAsker.Data data)
    {
        LoadingScreen.Instance.Display("Connecting ...");
        this.data = data;
        LoginPasswordAsker.Instance.Hide();
        string url = "http://" + data.ip + ":" + data.port + UMI3DNetworkingKeys.media;
        UMI3DCollaborationClientServer.GetMedia(url, GetMediaSucces, GetMediaFailed, e => false);
    }

    /// <summary>
    /// Called if the connection failed.
    /// </summary>
    /// <param name="error"></param>
    void GetMediaFailed(string error)
    {
        DialogBox.Instance.Display($"Server Unreachable", error, "Leave", () => {
            Leave();
            DialogBox.Instance.Hide();
        });
    }

    /// <summary>
    /// Called if the connection succeded.
    /// </summary>
    void GetMediaSucces(MediaDto media)
    {
        UMI3DCollaborationClientServer.Connect();
    }

    /// <summary>
    /// Asks users a login and a password.
    /// </summary>
    void DisplayLoginPassword(Action<string, string> callback = null)
    {
        LoadingScreen.Instance.Hide();

        LoginPasswordAsker.Instance.Display();
        LoginPasswordAsker.Instance.UnregisterAll();

        ConnectionMenuManager.instance.ShowPreviousNavigationButton(() =>
        {
            LoginPasswordAsker.Instance.Hide();
            ConnectionMenuManager.instance.DisplayHome();
        });

        if (callback != null)
        {
            LoginPasswordAsker.Instance.Register((log, pwd) =>
            {
                LoginPasswordAsker.Instance.Hide();
                callback(log, pwd);
            });
        }
    }

    /// <summary>
    /// Asks users a password.
    /// </summary>
    void DisplayPassword(Action<string> callback)
    {
        LoadingScreen.Instance.Hide();

        LoginPasswordAsker.Instance.Display(false);
        LoginPasswordAsker.Instance.UnregisterAll();

        ConnectionMenuManager.instance.ShowPreviousNavigationButton(() =>
        {
            LoginPasswordAsker.Instance.Hide();
            ConnectionMenuManager.instance.DisplayHome();
        });

        if (callback != null)
        {
            LoginPasswordAsker.Instance.Register((log, pwd) =>
            {
                LoginPasswordAsker.Instance.Hide();
                callback(pwd);
            });
        }
    }

    void DisplayAccept(int count, Action<bool> callback)
    {
        LibraryAsker.Instance.AskForDownload(count, (b) =>
        {
            callback.Invoke(b);
            if (!b)
                Leave();
        });
    }

    void OnConnectionLost()
    {
        Action<bool> callback = (b) => {
            if (b) Connect(data);
            else Leave();
        };
        OnConnectionLost(callback);
    }

    void OnConnectionLost(Action<bool> callback)
    {
        DialogBox.Instance.Display($"Connection to the server lost", "Leave to the connection menu or try again ?", "Try again", (b) => {
            callback.Invoke(b);
            if (b)
                LoadingScreen.Instance.Display("Connecting ...");
            else
                LoadingScreen.Instance.Display("Loading ...");
            DialogBox.Instance.Hide();
        });
    }

    public void Leave()
    {
        CustomMainThreadDispatcher mainThreadDispatcher = MainThreadDispatcher.UnityMainThreadDispatcher.Instance() as CustomMainThreadDispatcher;
        if (mainThreadDispatcher != null)
        {
            mainThreadDispatcher.StopAllCoroutines();
            mainThreadDispatcher.ClearQueue();
        }
        else
        {
            Debug.LogError("MainTheadDispatcher should not be null");
        }

        UMI3DEnvironmentLoader.Clear();
        UMI3DResourcesManager.Instance.ClearCache();
        UMI3DCollaborationClientServer.Logout(() => { Destroy(UMI3DClientServer.Instance.gameObject); }, null);
        QuestBrowser.Helper.DontDestroyOnLoad.DestroyAllInstances();

        StartCoroutine(LoadConnectionScene());
        //UMI3DCollaborationClientServer.Identity = new umi3d.common.collaboration.IdentityDto();
    }

    IEnumerator LoadConnectionScene()
    {
        var async = SceneManager.LoadSceneAsync(thisScene, LoadSceneMode.Single);
        yield return new WaitUntil(() => async.isDone);
        Destroy(this.gameObject);
    }

    #endregion
}
