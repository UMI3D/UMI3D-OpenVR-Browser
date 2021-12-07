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
using umi3d.cdk;
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoadingScreen : Singleton<LoadingScreen>
{
    //public Slider slider;
    public GameObject panel;

    public Text message;

    /// <summary>
    /// Objects to hide while environment is loading.
    /// </summary>
    public GameObject[] objectsToHide;

    /// <summary>
    /// Sphere used to make the screen black;
    /// </summary>
    public GameObject loadingSphere;

    public static UnityEvent OnLoadingEnvironmentStart = new UnityEvent();

    public static UnityEvent OnLoadingEnvironmentFinish = new UnityEvent();

    private void Start()
    {
        loadingSphere.SetActive(false);
        StartCoroutine(WaitForLoader());
        Hide();
    }

    IEnumerator WaitForLoader()
    {
        while (!UMI3DEnvironmentLoader.Exists)
            yield return null;

        UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(HideLoadingScreen);
        UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(() =>
        {
            OnLoadingEnvironmentFinish?.Invoke();
            AudioListener.volume = 1.0f;
        });
        UMI3DEnvironmentLoader.Instance.onProgressChange.AddListener(v =>
        {
            if (v == 0)
            {
                OnLoadingEnvironmentStart?.Invoke();
                AudioListener.volume = 0.0f;
            }
        });
    }

    public void SetLoadingScreen()
    {
        ConnectionMenuManager.instance.HideNextNavigationButton();
        ConnectionMenuManager.instance.HidePreviousNavigationButton();

        SetLightningSettings.ResetLightningSettings();
        loadingSphere.SetActive(true);
        foreach (var o in objectsToHide)
            o.SetActive(false);
    }

    public void HideLoadingScreen()
    {
        loadingSphere.SetActive(false);
    }

    /// <summary>
    /// Displays a loading string with a message.
    /// </summary>
    /// <param name="message"></param>
    public void Display(string message)
    {
        panel?.SetActive(true);
        if (this.message != null)
            this.message.text = message;
    }

    /// <summary>
    /// Hides the loading screen.
    /// </summary>
    public void Hide()
    {
        panel?.SetActive(false);
    }


}

