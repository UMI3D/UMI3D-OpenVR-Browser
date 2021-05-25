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
using System.Collections.Generic;
using umi3d.cdk.interaction;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class MenuOpenner : MonoBehaviour
{
    public SteamVR_Action_Boolean button;
    public SteamVR_Input_Sources controller;

    public PlayerMenuManager playerMenuManager;

    public GameObject laser;

    public UnityEvent onMenuOpenned;
    public UnityEvent onMenuClosed;

    public static List<MenuOpenner> instances { get; protected set; } = new List<MenuOpenner>();
    public static MenuOpenner FindInstanceAssociatedToController(AbstractController controller)
    {
        foreach (var i in instances)
        {
            if (i.playerMenuManager.controller == controller)
                return i;
        }
        return null;
    }

    private void Awake()
    {
        instances.Add(this);
    }

    private void OnDestroy()
    {
        instances.Remove(this);
    }

    void Start()
    {
        Close();

        DialogBox.OnClose.AddListener(() =>
        {
            if (playerMenuManager.IsDisplaying)
                DisplayLaser(false);
        });

        LoadingScreen.OnLoadingEnvironmentStart.AddListener(() =>
        {
            DisplayLaser(false);
        });
        LoadingScreen.OnLoadingEnvironmentFinish.AddListener(() =>
        {
            DisplayLaser(true);
        });
    }

    void Update()
    {
        if (button.GetStateDown(controller))
        {
            if (playerMenuManager != null)
            {
                if (!playerMenuManager.IsDisplaying)
                {

                    Open();
                }
                else
                {
                    Close();
                }
            }
        }
    }


    [ContextMenu("Open")]
    public void Open()
    {
        playerMenuManager.Display();

        onMenuOpenned.Invoke();
        if (playerMenuManager.toolParametersMenuDisplay.menu.Count > 0 || playerMenuManager.toolboxesAndToolsMenuDisplay.menu.Count > 0)
            DisplayLaser(false);
    }


    [ContextMenu("Close")]
    public void Close()
    {
        playerMenuManager.Hide();
        onMenuClosed.Invoke();
        DisplayLaser(true);
    }

    private void DisplayLaser(bool display)
    {
        laser.SetActive(display);
    }
}
