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

public class FPSDisplayer : MonoBehaviour
{
   public UnityEngine.UI.Text displayText;

    public float refreshRate = 0.2f;

    double timer = 0;

    // Update is called once per frame
    void Update()
    {
        if (Time.unscaledTime > timer + refreshRate)
        {
            timer = Time.unscaledTime;
            displayText.text = "FPS " + ((int)(1f / Time.unscaledDeltaTime)).ToString();
        }
        
    }
}
