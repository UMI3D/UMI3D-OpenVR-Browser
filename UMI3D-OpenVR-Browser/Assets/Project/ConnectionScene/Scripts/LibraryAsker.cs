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
using umi3d.common;
using UnityEngine;
using UnityEngine.UI;

public class LibraryAsker : Singleton<LibraryAsker>
{
    public GameObject panel;
    public Text info;
    public Button accept;
    public Button deny;

    public void AskForDownload(int count, Action<bool> callback)
    {
        panel.SetActive(true);
        accept.onClick.RemoveAllListeners();
        deny.onClick.RemoveAllListeners();

        info.text = count + " Libraries are required to join the environment.";
        accept.onClick.AddListener(() =>
        {
            callback(true);
            LoadingScreen.Instance?.Display("Downloading libraries ...");
            Hide();
        });
        deny.onClick.AddListener(() =>
        {
            callback(false);
            Hide();
        });
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
