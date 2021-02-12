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
using umi3d.common;
using umi3d.cdk.menu;
using UnityEngine;
using UnityEngine.Events;
using umi3d.common.interaction;
using umi3d.cdk.interaction;

public class HoldableButtonMenuItem : MenuItem, IObservable<bool>
{
    /// <summary>
    /// If true, the button will stay pressed on selection.
    /// </summary>
    public bool Holdable = false;

    /// <summary>
    /// This menu item can store an interaction.
    /// </summary>
    public EventDto associatedInteractionDto;

    /// <summary>
    /// If associatedIntearctionDto is not null stores the associated tool id.
    /// </summary>
    public string toolId;

    /// <summary>
    /// If associatedIntearctionDto is not null stores the hovered object id.
    /// </summary>
    public string hoveredObjectId;

    /// <summary>
    /// UMI3DInput associated.
    /// </summary>
    public AbstractUMI3DInput associatedInput;

    /// <summary>
    /// Image to display (if any).
    /// </summary>
    public Sprite sprite;

    private bool buttonDown = false;

    /// <summary>
    /// Subscribers on value change
    /// </summary>
    private List<UnityAction<bool>> subscribers = new List<UnityAction<bool>>();



    /// <summary>
    /// Subscribe a callback for button press.
    /// </summary>
    /// <param name="callback">Callback to invoke on button press</param>
    public virtual void Subscribe(UnityAction<bool> callback)
    {
        if (!subscribers.Contains(callback))
        {
            subscribers.Add(callback);
        }
    }

    /// <summary>
    /// Unsubscribe a callback from the value change.
    /// </summary>
    /// <param name="callback"></param>
    public virtual void UnSubscribe(UnityAction<bool> callback)
    {
        subscribers.Remove(callback);
    }

    public override string ToString()
    {
        return Name;
    }

    public virtual bool GetValue()
    {
        return buttonDown;
    }

    public virtual void NotifyValueChange(bool down)
    {
        buttonDown = down;
        if (Holdable || buttonDown)
        {
            foreach (UnityAction<bool> sub in subscribers)
            {
                sub.Invoke(buttonDown);
            }
        }

    }
}
