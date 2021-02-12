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
using UnityEngine.Events;

public class JoystickSelectorTarget : MonoBehaviour
{
    private bool enableHoverEvent { get; set; } = true;

    public UnityEvent onHoverEnter = new UnityEvent();
    public UnityEvent onHoverExit = new UnityEvent();
    public UnityEvent onSelect = new UnityEvent();
    public UnityEvent onDeselect = new UnityEvent();

    public virtual void NotifyHoverEnter()
    {
        if (enableHoverEvent)
            onHoverEnter.Invoke();
    }

    public virtual void NotifyHoverExit()
    {
        if (enableHoverEvent)
            onHoverExit.Invoke();
    }


    public virtual void Select()
    {
        onSelect.Invoke();
    }

    public virtual void OnDeselect()
    {
        onDeselect.Invoke();
    }
}
