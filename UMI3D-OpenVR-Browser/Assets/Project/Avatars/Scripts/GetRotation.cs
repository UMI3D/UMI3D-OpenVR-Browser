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

public class GetRotation : MonoBehaviour
{
    public GameObject RotParent;

    public bool X;
    public bool Y;
    public bool Z;

    private float rX = 0;
    private float rY = 0;
    private float rZ = 0;

    // Update is called once per frame
    void Update()
    {
        if (X)
        {
            rX = RotParent.transform.eulerAngles.x;
        }

        if (Y)
        {
            rY = RotParent.transform.eulerAngles.y;
        }

        if (Z)
        {
            rZ = RotParent.transform.eulerAngles.z;
        }

        transform.rotation = Quaternion.identity;
        transform.Rotate(new Vector3(rX, rY, rZ));
    }
}
