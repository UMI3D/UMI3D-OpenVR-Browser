/*
Copyright 2019 - 2023 Inetum

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

namespace umi3d.browser.utils
{
    public static class ObjectExtension
    {
        public static void GetOrAddComponent<T>(this Object go, out T component)
            where T : UnityEngine.Component
        {
#if UNITY_2021_3_15_OR_NEWER
            component = go.GetComponent<T>();
            if (component == null) component = go.AddComponent<T>();
#else
            T GetComponent()
            {
                if (go is GameObject)
                {
                    return ((GameObject)go).GetComponent<T>();
                }
                else if (go is Component)
                {
                    return ((Component)go).GetComponent<T>();
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
            T AddComponent()
            {
                if (go is GameObject)
                {
                    return ((GameObject)go).AddComponent<T>();
                }
                else if (go is Component)
                {
                    return ((Component)go).gameObject.AddComponent<T>();
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
            component = GetComponent();
            if (component == null) component = AddComponent();
#endif
        }

        public static void FindOrCreate(this GameObject parent, string name, out GameObject child)
        {
            child = parent.transform.Find(name)?.gameObject;
            if (child == null) child = new GameObject(name);
            parent.Add(child);
        }

        public static void FindOrCreatePrefab(this GameObject parent, string name, out GameObject child, GameObject prefab)
        {
            child = parent.transform.Find(name)?.gameObject;
            if (prefab == null) return;
            if (child == null) child = GameObject.Instantiate(prefab);
            parent.Add(child);
        }

        public static bool Find(this GameObject parent, string name, out GameObject child)
        {
            child = parent.transform.Find(name)?.gameObject;
            if (child == null) return false;
            else return true;
        }

        public static void Add(this GameObject parent, GameObject child)
        {
            if (child == null || parent == null) return;
            child.transform.parent = parent.transform;
        }
    }
}
