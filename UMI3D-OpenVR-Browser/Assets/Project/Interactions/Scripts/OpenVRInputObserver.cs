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
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

/// <summary>
/// Observer for SteamVR buttons.
/// </summary>
public class OpenVRInputObserver : MonoBehaviour
{
    public SteamVR_Action_Boolean button;
    public SteamVR_Input_Sources controller;

    public UnityEvent onActionDown;
    public UnityEvent onAction;
    public UnityEvent onActionUp;


    private List<System.Action> subscribersUp = new List<System.Action>();
    private List<System.Action> subscribersDown = new List<System.Action>();
    private List<System.Action> subscribers = new List<System.Action>();

    public void AddOnStateUpListener(System.Action callback)
    {
        subscribersUp.Add(callback);
    }

    public void AddOnStateDownListener(System.Action callback)
    {
        subscribersDown.Add(callback);
    }

    public void AddOnStateListener(System.Action callback)
    {
        subscribers.Add(callback);
    }


    public void RemoveOnStateUpListener(System.Action callback)
    {
        subscribersUp.Remove(callback);
    }
    public void RemoveOnStateDownListener(System.Action callback)
    {
        subscribersDown.Remove(callback);
    }
    public void RemoveOnStateListener(System.Action callback)
    {
        subscribers.Remove(callback);
    }

    public bool isUsed()
    {
        return (subscribers.Count + subscribersDown.Count + subscribersUp.Count) > 0;
    }

    void Update()
    {
        if (button.GetStateUp(controller))
        {
            onActionUp.Invoke();
            foreach (System.Action action in subscribersUp)
                action.Invoke();
        }
        else if (button.GetStateDown(controller))
        {
            onActionDown.Invoke();
            foreach (System.Action action in subscribersDown)
                action.Invoke();
        }
        else if (button.GetState(controller))
        {
            onAction.Invoke();
            foreach (System.Action action in subscribers)
                action.Invoke();
        }
    }
}
