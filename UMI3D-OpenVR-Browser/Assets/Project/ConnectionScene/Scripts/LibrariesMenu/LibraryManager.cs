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
using umi3d.cdk;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the content of the library manager menu.
/// </summary>
public class LibraryManager : MonoBehaviour
{
    #region Fields

    /// <summary>
    /// Prefab used to represent a library in the menu.
    /// 
    /// Must have a LibraryManagerEntry script.
    /// </summary>
    public GameObject libraryItemPrefab;

    /// <summary>
    /// Library list.
    /// </summary>
    public VerticalLayoutGroup container;

    [SerializeField]
    private int nbLibraryDisplayedAtSameTime = 5;

    private List<LibraryManagerEntry> currentEntries = new List<LibraryManagerEntry>();

    private int indexOfCurrentTopEntryDisplayed = 0;

    #endregion

    #region Methods


    /// <summary>
    /// Updates the content of the library list.
    /// </summary>
    public void UpdateContent()
    {
        foreach (var entry in currentEntries)
            Destroy(entry.gameObject);
        currentEntries.Clear();

        Dictionary<string, List<UMI3DResourcesManager.DataFile>> libs = new Dictionary<string, List<UMI3DResourcesManager.DataFile>>();
        foreach (var lib in UMI3DResourcesManager.Libraries)
        {
            if (lib.applications != null)
                foreach (var app in lib.applications)
                {
                    if (!libs.ContainsKey(app)) libs[app] = new List<UMI3DResourcesManager.DataFile>();
                    libs[app].Add(lib);
                }
        }

        foreach (var app in libs)
        {
            foreach (var lib in app.Value)
            {
                // 1. Diplay lib name
                LibraryManagerEntry entry = Instantiate(libraryItemPrefab, container.transform).GetComponent<LibraryManagerEntry>();
                if (entry == null)
                    throw new System.ArgumentException("libraryItemPrefab must have a LibraryManagerEntry script");

                entry.gameObject.name = "LibraryItem_" + lib.key;
                entry.libLabel.text = lib.key;

                //2. Display environments which use this lib
                //Could be done with lib.applications if needed;

                //3. Display lib size
                //Could be done with lib.path if needed

                //4.Bind the button to unistall this lib
                entry.deleteButton.onClick.AddListener(() => {
                    if (DialogBox.Instance.IsDisplayed)
                        return;
                    DialogBox.Instance.Display("Are you sure ... ?", "This library is required for environment " + app.Key, "Yes", (b) => {
                        if (b)
                        {
                            lib.applications.Remove(app.Key);
                            UMI3DResourcesManager.RemoveLibrary(lib.key);
                            UpdateContent();
                        }
                    });
                });

                currentEntries.Add(entry);
            }
        }

        indexOfCurrentTopEntryDisplayed = 0;
        UpdateDisplay();
    }

    /// <summary>
    /// Navigates up in the library list.
    /// </summary>
    public void NavigateUp()
    {
        if (indexOfCurrentTopEntryDisplayed > 0 && !DialogBox.Instance.IsDisplayed)
        {
            indexOfCurrentTopEntryDisplayed--;
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Navigates down in the library list.
    /// </summary>
    public void NavigateDown()
    {
        if ((indexOfCurrentTopEntryDisplayed + nbLibraryDisplayedAtSameTime < currentEntries.Count - 1) && (!DialogBox.Instance.IsDisplayed))
        {
            indexOfCurrentTopEntryDisplayed++;
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Displays the libraries which have an index between [indexOfCurrentTopEntryDisplayed; indexOfCurrentTopEntryDisplayed + nbLibraryDisplayedAtSameTime].
    /// </summary>
    private void UpdateDisplay()
    {
        for (int i = 0; i < currentEntries.Count; i++)
        {
            bool display = (i >= indexOfCurrentTopEntryDisplayed) && (i <= indexOfCurrentTopEntryDisplayed + nbLibraryDisplayedAtSameTime);
            currentEntries[i].gameObject.SetActive(display);
        }
    }

    #endregion
}

