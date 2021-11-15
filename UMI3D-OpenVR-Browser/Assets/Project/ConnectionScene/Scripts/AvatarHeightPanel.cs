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

/// <summary>
/// This class is responsible for asling users to set up their avatar heights.
/// </summary>
public class AvatarHeightPanel : MonoBehaviour
{
    [SerializeField]
    GameObject panel;

    [SerializeField]
    UnityEngine.UI.Button validateButton;


    /// <summary>
    /// Has user set his height.
    /// </summary>
    public static bool isSetup = false;

    System.Action validationCallBack;

    private void Start()
    {
        validateButton.onClick.AddListener(() =>
        {
            var setUp = GameObject.FindObjectOfType<SetUpAvatarHeight>();
            Debug.Assert(setUp != null, "No avatar found to set up height. Should not happen");
            StartCoroutine(setUp.SetUpAvatar());
            Hide();
            validationCallBack?.Invoke();
            isSetup = true;
        });
    }

    public void Display(System.Action validationCallBack)
    {
        panel.SetActive(true);
        this.validationCallBack = validationCallBack;
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}