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
using System.Linq;


namespace BrowserQuest.Navigation
{
    public class TeleportArea : MonoBehaviour
    {
        public static List<TeleportArea> Instances = new List<TeleportArea>();

        protected virtual void Awake() => Instances.Add(this);
        protected virtual void OnDestroy() => Instances.Remove(this);

        public void Highlight()
        {
            this.GetComponentsInChildren<MeshRenderer>().ToList().ForEach(rnd => rnd.material = NavmeshManager.Instance.tpAreaHighlight);
        }

        public void DisableHighlight()
        {
            this.GetComponentsInChildren<MeshRenderer>().ToList().ForEach(rnd => rnd.material = NavmeshManager.Instance.tpAreaDefault);
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        public void Display()
        {
            DisableHighlight();
            this.gameObject.SetActive(true);
        }
    }
}