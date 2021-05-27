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
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the advanced connection panel.
/// </summary>
public class SignalingAsker : MonoBehaviour
{
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
        ip.text = PlayerPrefsManager.GetUmi3dIp();
        port.text = PlayerPrefsManager.GetUmi3DPort();

        Hide();
    }


    public void Run()
    {
        LoadingScreen.Instance.Display("Connection ...");

        if ((ip != null) && (ip.text != null) && (ip.text != ""))
        {
            PlayerPrefsManager.SaveUmi3dIp(ip.text);
        }
        if ((port != null) && (port.text != null) && (port.text != ""))
        {
            PlayerPrefsManager.SaveUmi3dPort(port.text);
        }

        Hide();

        ConnectionMenuManager.instance.ConnectToUmi3DEnvironement(ip.text, port.text);
    }

    public void Display()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
