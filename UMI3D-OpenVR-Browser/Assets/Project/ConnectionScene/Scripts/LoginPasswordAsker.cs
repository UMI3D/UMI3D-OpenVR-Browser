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
using UnityEngine;
using umi3d.common;
using UnityEngine.UI;

public class LoginPasswordAsker : Singleton<LoginPasswordAsker>
{
    [SerializeField] private GameObject panel;
    [SerializeField] private InputField loginField;
    [SerializeField] private InputField passwordField;
    [SerializeField] private Button submitButton;

    [SerializeField]
    private RectTransform loginPanel;

    [Tooltip("When the password is played, yLoginOffset is applied to thge login panel to position it above the password input")]
    [SerializeField]
    int yLoginOffset = 130;

    [SerializeField]
    private RectTransform passwordPanel;

    public void Register(System.Action<string, string> callback)
    {
        submitButton.onClick.AddListener(() => callback(loginField.text, passwordField.text));
    }

    public void UnregisterAll()
    {
        submitButton.onClick.RemoveAllListeners();
    }



    public void Display(bool displayLoginInput = true)
    {
        panel.SetActive(true);

        if (displayLoginInput)
        {
            loginPanel.gameObject.SetActive(true);
            passwordPanel.localPosition = new Vector3(loginPanel.localPosition.x, yLoginOffset, loginPanel.localPosition.z);
        }
        else
        {
            loginPanel.gameObject.SetActive(false);
            passwordPanel.localPosition = new Vector3(loginPanel.localPosition.x, 0, loginPanel.localPosition.z);
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
