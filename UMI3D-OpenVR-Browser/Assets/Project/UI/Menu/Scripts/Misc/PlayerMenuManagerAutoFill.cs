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
using UnityEditor;
using System.Reflection;

#if UNITY_EDITOR

/// <summary>
/// You are welcome... I know i know...
/// </summary>
public class PlayerMenuManagerAutoFill : MonoBehaviour
{
    public SerializedObject serializedObject;

    [System.Serializable]
    public class MaterialField
    {
        public string fieldName;
        public string materialFilePrefix;
    }

    public PlayerMenuManager playerMenuManager;
    public List<MaterialField> fields;
    public List<string> materialsSuffixes;
    public List<Material> materialsToPickFrom;


    [ContextMenu("AutoFill")]
    public void AutoFill()
    {


        foreach (MaterialField matField in fields)
        {
            foreach(string suffix in materialsSuffixes)
            {
                Material material = materialsToPickFrom.Find(m => m.name.ToLower().Contains(matField.materialFilePrefix.ToLower()) && m.name.ToLower().Contains(suffix.ToLower()));
                if (material != null)
                {
                    FieldInfo materialGroup = new List<FieldInfo>(typeof(PlayerMenuManager).GetFields()).Find(f => f.Name.ToLower().Contains(matField.fieldName.ToLower()));
                    if (materialGroup != null)
                    {
                        FieldInfo materialField = new List<FieldInfo>(materialGroup.FieldType.GetFields()).Find(f => f.Name.ToLower().Contains(suffix.ToLower()));
                        if (materialField != null)
                            materialField.SetValue(materialGroup.GetValue(playerMenuManager), material);
                        else
                            Debug.Log("No sub field found for suffix");
                    }
                    else Debug.Log("no group field found");
                }
                else
                    Debug.Log("No Material found");
            }
        }

        (new SerializedObject(playerMenuManager)).ApplyModifiedProperties();


    }

}

#endif
