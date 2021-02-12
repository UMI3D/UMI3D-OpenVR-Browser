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
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using umi3d.cdk.menu.view;
using UnityEditor.SceneManagement;



[CustomEditor(typeof(CustomContainerSelector))]
public class ContainerSelectorEditor : Editor
{
    /// <summary>
    /// Target object edited.
    /// </summary>
    private CustomContainerSelector containerSelector;

    /// <summary>
    /// Container to add a a new exception.
    /// </summary>
    private AbstractMenuDisplayContainer newExcepContainer;

    /// <summary>
    /// Container to add a a new typed exception.
    /// </summary>
    private AbstractMenuDisplayContainer newTypedExcepContainer;

    /// <summary>
    /// Name of the menu of the new exception to add.
    /// </summary>
    private string newExcepName;

    /// <summary>
    /// Name of the menu of the new typed exception to add.
    /// </summary>
    private string newTypeExcepName;

    private void OnEnable()
    {
        containerSelector = target as CustomContainerSelector;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.Label("Container by height", EditorStyles.boldLabel);
        //DrawDefaultInspector();
        for (int i = 0; i < containerSelector.containerPrefabs.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(i.ToString(), GUILayout.ExpandWidth(false));
            containerSelector.containerPrefabs[i] =
                EditorGUILayout.ObjectField(containerSelector.containerPrefabs[i], typeof(AbstractMenuDisplayContainer), true) as AbstractMenuDisplayContainer;
            if (GUILayout.Button("Remove"))
            {
                containerSelector.containerPrefabs.Remove(containerSelector.containerPrefabs[i]);
                break;
            }
            GUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Add"))
        {
            containerSelector.containerPrefabs.Add(null);
        }

        GUILayout.Space(50);

        #region named exceptions

        GUILayout.Label("Exceptions", EditorStyles.boldLabel);
        if (containerSelector.exceptions == null)
            containerSelector.exceptions = new ContainerDictionary();
        if (containerSelector.exceptions.Count == 0)
            GUILayout.Label("No exceptions set yet");

        foreach (KeyValuePair<string, AbstractMenuDisplayContainer> excep in containerSelector.exceptions)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(excep.Key);
            if (excep.Value)
                GUILayout.Label(excep.Value.gameObject.name);
            else
                GUILayout.Label("none");
            if (GUILayout.Button("Remove"))
            {
                containerSelector.exceptions.Remove(excep.Key);
                break;
            }
            GUILayout.EndHorizontal();
        }


        GUILayout.Space(10);
        GUILayout.Label("Add exception", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        newExcepName = EditorGUILayout.TextField("Menu name", newExcepName);
        newExcepContainer = EditorGUILayout.ObjectField(newExcepContainer, typeof(AbstractMenuDisplayContainer), true) as AbstractMenuDisplayContainer;

        GUILayout.EndHorizontal();
        if (GUILayout.Button("Add"))
        {
            if ((newExcepName != null) && (newExcepName.Length > 0) && (newExcepContainer != null))
                containerSelector.exceptions.Add(newExcepName, newExcepContainer);
            else
                EditorUtility.DisplayDialog("Exception error", "Please ensure that the exception name and the associated container are valid", "OK");
        }

        #endregion

        GUILayout.Space(50);

        #region type exceptions

        GUILayout.Label("Typed exceptions", EditorStyles.boldLabel);
        if (containerSelector.typedException == null)
            containerSelector.typedException = new ContainerDictionary();
        if (containerSelector.typedException.Count == 0)
            GUILayout.Label("No exceptions set yet");

        foreach (KeyValuePair<string, AbstractMenuDisplayContainer> excep in containerSelector.typedException)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(excep.Key);
            if (excep.Value)
                GUILayout.Label(excep.Value.gameObject.name);
            else
                GUILayout.Label("none");
            if (GUILayout.Button("Remove"))
            {
                containerSelector.typedException.Remove(excep.Key);
                break;
            }
            GUILayout.EndHorizontal();
        }


        GUILayout.Space(10);
        GUILayout.Label("Add exception", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Type name");
        newTypeExcepName = EditorGUILayout.TextField(newTypeExcepName);
        newTypedExcepContainer = EditorGUILayout.ObjectField(newTypedExcepContainer, typeof(AbstractMenuDisplayContainer), true) as AbstractMenuDisplayContainer;

        if (GUILayout.Button("Add"))
        {
            if ((newTypeExcepName != null) && (newTypeExcepName.Length > 0) && (newTypedExcepContainer != null))
            {
                containerSelector.typedException.Add(newTypeExcepName, newTypedExcepContainer);
                newTypeExcepName = "";
                newTypedExcepContainer = null;
            }
            else
                EditorUtility.DisplayDialog("Exception error", "Please ensure that the exception name and the associated container are valid", "OK");
        }

        #endregion



        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(containerSelector);
            EditorSceneManager.MarkSceneDirty(containerSelector.gameObject.scene);
        }
    }
}

#endif