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
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SignalingAsker : MonoBehaviour
{
    public string environmentLoadingScene;
    public string thisScene;

    public GameObject panel;
    public InputField ip;
    public InputField port;

    public class Data
    {
        public string ip;
        public string port;
    }

    private void Awake()
    {
        if (PlayerPrefs.HasKey("umi3dip"))
            ip.text = PlayerPrefs.GetString("umi3dip");
        if (PlayerPrefs.HasKey("umi3dport"))
            port.text = PlayerPrefs.GetString("umi3dport");
    }


    public void Run()
    {
        LoadingScreen.Instance.Display("Connection ...");

        if ((ip != null) && (ip.text != null) && (ip.text != ""))
        {
            PlayerPrefs.SetString("umi3dip", ip.text);
        }
        if ((port != null) && (port.text != null) && (port.text != ""))
        {
            PlayerPrefs.SetString("umi3dport", port.text);
        }

        panel.SetActive(false);
        
        StartCoroutine(WaitReady(new Data() { ip = ip.text, port = port.text }));
    }

    IEnumerator WaitReady(Data data)
    {
        //SceneManager.LoadScene(environmentLoadingScene, LoadSceneMode.Additive);
        while (!Connecting.Exists && !UMI3DEnvironmentLoader.Exists)
            yield return new WaitForEndOfFrame();

        Connecting.Instance.Connect(data);

        while (!UMI3DEnvironmentLoader.Exists)
            yield return new WaitForEndOfFrame();
        UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(() => SceneManager.UnloadSceneAsync(thisScene));
    }
}
