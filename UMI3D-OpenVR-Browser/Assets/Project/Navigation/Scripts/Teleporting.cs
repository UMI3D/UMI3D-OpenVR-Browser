﻿/*
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

public class Teleporting : MonoBehaviour
{
    public GameObject teleportingObject;
    public GameObject vrCamera;
    public TeleportArc arc;

    [ContextMenu("Teleport")]
    public void Teleport()
    {
        Vector3? position = arc.GetPointedPoint();

        if (position.HasValue)
        {
            teleportingObject.transform.position = new Vector3(position.Value.x - vrCamera.transform.localPosition.x, 
                                                                position.Value.y, 
                                                                position.Value.z - vrCamera.transform.localPosition.z);
        }
    }

}
