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

using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.common;
using umi3d.common.interaction;
using umi3dVRBrowsersBase.ui.playerMenu;
using umi3dVRBrowsersBase.ui.watchMenu;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// Manages the connection to the UMI3D environment.
    /// </summary>
    public class Connecting : SingleBehaviour<Connecting>
    {
        #region Fields

        /// <summary>
        /// Identifier specific for a browser
        /// </summary>
        public VRBrowserIdentifier identifier;

        /// <summary>
        /// Name of the launcher scene (current scene).
        /// </summary>
        public string thisScene;

        /// <summary>
        /// Name of the environement scene.
        /// </summary>
        public string environmentScene;

        /// <summary>
        /// Current data stored to join an environment.
        /// </summary>
        private AdvancedConnectionPanel.Data data;

        string url = null;

        #endregion

        #region Methods

        protected override void Awake()
        {
            base.Awake();

            identifier.ShouldDownloadLib = ShouldDlLibraries;
            identifier.GetParameters = GetParameterDtos;
        }

        private void Start()
        {
            UMI3DCollaborationClientServer.Instance.OnConnectionLost.AddListener(OnConnectionLost);
            UMI3DCollaborationClientServer.Instance.OnLeaving.AddListener(() => Destroy(UMI3DClientServer.Instance.gameObject));
        }

        /// <summary>
        /// If some librairies are required to the join the environement, ask users to download them.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="callback"></param>
        private void ShouldDlLibraries(List<string> ids, Action<bool> callback)
        {
            if (LoadingPanel.Exists)
                LoadingPanel.Instance.Hide();

            if (ids.Count == 0)
            {
                callback.Invoke(true);

                if (LoadingPanel.Exists)
                    LoadingPanel.Instance?.Display("Loading environment ...");
            }

            else DisplayAccept(ids.Count, callback);
        }

        /// <summary>
        /// If teh environement needs some information to be joined, asks this information to users.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="callback"></param>
        private void GetParameterDtos(ConnectionFormDto form, Action<FormAnswerDto> callback)
        {
            FormAsker.Instance.Display(form, callback);
        }

        protected static string FormatUrl(string ip, string port)
        {
            string url = ip + (string.IsNullOrEmpty(port) ? "" : (":" + port));

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                return "http://" + url;
            return url;
        }

        public async Task<MediaDto> GetMedia(PlayerPrefsManager.FavoriteServerData connectionData)
        {
            if (LoadingPanel.Exists)
                LoadingPanel.Instance.Display("Connecting ...");

            //this.data = data;
            LoginPasswordAsker.Instance.Hide();

            var curentUrl = FormatUrl(connectionData.serverUrl, null) + UMI3DNetworkingKeys.media;
            url = curentUrl;
            try
            {
                return await UMI3DCollaborationClientServer.GetMedia(url, (e) => url == curentUrl && e.count < 3);
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Tries to connect to the server.
        /// </summary>
        /// <param name="data"></param>
        public void Connect(AdvancedConnectionPanel.Data connectionData)
        {
            if (LoadingPanel.Exists)
                LoadingPanel.Instance.Display("Connecting ...");

            this.data = connectionData;
            LoginPasswordAsker.Instance.Hide();

            var baseUrl = FormatUrl(connectionData.ip, connectionData.port);
            var curentUrl = baseUrl + UMI3DNetworkingKeys.media;
            url = curentUrl;

            GetMediaSucces(new MediaDto() { url = baseUrl, name =/* connectionData.environmentName ??*/ connectionData.ip }, (s) => GetMediaFailed(s));
        }

        /// <summary>
        /// Called if the connection failed.
        /// </summary>
        /// <param name="error"></param>
        private void GetMediaFailed(string error)
        {
            DialogBox.Instance.Display($"Server Unreachable", error, "Leave", () =>
            {
                Leave();
                DialogBox.Instance.Hide();
            });
        }

        /// <summary>
        /// Called if the connection succeded.
        /// </summary>
        private void GetMediaSucces(MediaDto media, System.Action<string> failed)
        {
            PlayerMenuManager.Instance.MenuHeader.SetEnvironmentName(media);
            UMI3DCollaborationClientServer.Connect(media, failed);

        }

        /// <summary>
        /// Asks users a login and a password.
        /// </summary>
        private void DisplayLoginPassword(Action<string, string> callback = null)
        {
            if (LoadingPanel.Exists)
                LoadingPanel.Instance.Hide();

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
        private void DisplayPassword(Action<string> callback)
        {
            if (LoadingPanel.Exists)
                LoadingPanel.Instance.Hide();

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

        /// <summary>
        /// Displays pop up to accept librairies downloading.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="callback"></param>
        private void DisplayAccept(int count, Action<bool> callback)
        {
            LibraryAsker.Instance.AskForDownload(count, (b) =>
            {
                callback.Invoke(b);
                if (!b)
                    Leave();
            });
        }

        /// <summary>
        /// Tries to reconnect or leave when connection is lost.
        /// </summary>
        private void OnConnectionLost()
        {
            Action<bool> callback = (b) =>
            {
                if (b) Connect(data);
                else Leave();
            };
            OnConnectionLost(callback);
        }

        /// <summary>
        /// Asks if users prefer to try to reconnect or leave when connection is lost.
        /// </summary>
        /// <param name="callback"></param>
        private void OnConnectionLost(Action<bool> callback)
        {
            DialogBox.Instance.Display($"Connection to the server lost", "Leave to the connection menu or try again ?", "Try again", (b) =>
            {
                callback.Invoke(b);

                if (LoadingPanel.Exists)
                {
                    if (b)
                        LoadingPanel.Instance.Display("Connecting ...");
                    else
                        LoadingPanel.Instance.Display("Loading ...");
                }
                DialogBox.Instance.Hide();
            });
        }

        /// <summary>
        /// Leaves current UMI3D Environment.
        /// </summary>
        public void Leave()
        {
            var mainThreadDispatcher = MainThreadDispatcher.UnityMainThreadDispatcher.Instance() as CustomMainThreadDispatcher;
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
            UMI3DCollaborationClientServer.Logout();
            umi3dVRBrowsersBase.DontDestroyOnLoad.DestroyAllInstances();

            WatchMenu.UnPinAllMenus();

            StartCoroutine(LoadConnectionScene());
        }

        /// <summary>
        /// Loads asynchronously the connection scene. 
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadConnectionScene()
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(thisScene, LoadSceneMode.Single);
            yield return new WaitUntil(() => async.isDone);
            Destroy(this.gameObject);
        }

        #endregion
    }
}