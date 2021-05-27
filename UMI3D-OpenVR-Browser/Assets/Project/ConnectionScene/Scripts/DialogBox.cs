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
using UnityEngine.UI;
using UnityEngine;
using umi3d.common;

using UnityEngine.Events;

public class DialogBox : Singleton<DialogBox>
{
    public Camera cam;
    public float distanceFromCam;
    public Text title;
    public Text info;
    public Button optionA;
    public Button optionB;
    public GameObject dialogueBoxPopUp;

    public GameObject[] lasers;

    public static UnityEvent OnClose = new UnityEvent();

    public bool IsDisplayed { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Hide();
    }

    /// <summary>
    /// Displays a pop up with two choices.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="info"></param>
    /// <param name="optionA"></param>
    /// <param name="optionB"></param>
    /// <param name="callback"></param>
    public void Display(string title, string info, string optionA, Action<bool> callback)
    {
        dialogueBoxPopUp.SetActive(true);

        this.optionA.onClick.AddListener(() => { callback(true); Hide(); });
        this.optionB.onClick.AddListener(() => { callback(false); Hide(); });
        this.optionA.GetComponentInChildren<Text>().text = optionA;
        this.title.text = title;
        this.info.text = info;
        IsDisplayed = true;

        foreach (var laser in lasers)
            laser.SetActive(true);
    }

    /// <summary>
    /// Displays a pop up with only one choice.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="info"></param>
    /// <param name="optionA"></param>
    /// <param name="optionB"></param>
    /// <param name="callback"></param>
    public void Display(string title, string info, string option, Action callback)
    {
        dialogueBoxPopUp.SetActive(true);
        optionA.gameObject.SetActive(true);
        optionB.gameObject.SetActive(false);

        this.optionA.onClick.AddListener(() => { callback(); Hide(); });
        this.optionA.GetComponentInChildren<Text>().text = option;
        this.title.text = title;
        this.info.text = info;
        IsDisplayed = true;
    }

    public void Hide()
    {
        this.optionA.onClick.RemoveAllListeners();
        this.optionB.onClick.RemoveAllListeners();
        dialogueBoxPopUp.SetActive(false);
        IsDisplayed = false;

        OnClose?.Invoke();
    }

    /// <summary>
    /// Updates the pop up position to always face users.
    /// </summary>
    protected virtual void Update()
    {
        (this.transform as RectTransform).SetPositionAndRotation(cam.transform.position + distanceFromCam * cam.transform.forward, Quaternion.identity);
        transform.LookAt(cam.transform);
        transform.Rotate(0, 180, 0, Space.Self);
    }
}

