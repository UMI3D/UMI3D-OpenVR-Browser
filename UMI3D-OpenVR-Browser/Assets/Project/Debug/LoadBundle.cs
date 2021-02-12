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

public class LoadBundle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var myLoadedAssetBundle = AssetBundle.LoadFromFile("C:\\Users\\Mamadou\\Downloads\\aqua_v1.prefab.bundle");
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            return;
        }
        foreach(var s in myLoadedAssetBundle.GetAllAssetNames())
        {
            var prefab = myLoadedAssetBundle.LoadAsset<GameObject>(s);
            var o = Instantiate(prefab);
            o.transform.position = new Vector3(-688, 1.04f, 4.38f);
        }


        myLoadedAssetBundle.Unload(false);
    }

}
