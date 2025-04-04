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
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad;

    public LoadSceneMode loadSceneMode;

    public bool setNewSceneAsActive = true;

    // Start is called before the first frame update
    void Start()
    {
        if (setNewSceneAsActive)
        {
            StartCoroutine(LoadScene());
        }
        else
            SceneManager.LoadScene(sceneToLoad, loadSceneMode);
    }

    IEnumerator LoadScene()
    {
        var indicator = SceneManager.LoadSceneAsync(sceneToLoad, loadSceneMode);

        yield return new WaitUntil(() => indicator.isDone);

;       SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
    }
}
