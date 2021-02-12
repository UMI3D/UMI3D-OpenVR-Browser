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

public class SelectorManager : MonoBehaviour
{
    public List<RaySelector> raySelectors;
    public ProximitySelector proximitySelector;

    protected int token = 0;
    protected bool raySelectorsActivated = true;

    protected virtual void Start()
    {
        token = Random.Range(int.MinValue, int.MaxValue);
    }

    protected virtual void Update()
    {
        if (raySelectorsActivated && proximitySelector.isHoveringSomething())
        {
            foreach (RaySelector raySelector in raySelectors)
                raySelector.Deactivate(token);

            raySelectorsActivated = false;
        }
        else if (!raySelectorsActivated && !proximitySelector.isHoveringSomething())
        {
            foreach (RaySelector raySelector in raySelectors)
                raySelector.Activate(token);

            raySelectorsActivated = true;
        }
    }
}
