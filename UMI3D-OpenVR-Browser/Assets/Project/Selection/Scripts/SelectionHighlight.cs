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
using umi3d;
using umi3d.cdk;
using umi3d.common;
using UnityEngine;

public class SelectionHighlight : Singleton<SelectionHighlight>
{

    public Material availableMaterial;
    public Material hoverMaterial;
    public Material selectedMaterial;

    private Dictionary<int, List<GameObject>> ghosts = new Dictionary<int, List<GameObject>>();
    private List<int> availableObjectsIds = new List<int>();
    private List<int> hoveredObjectsIds = new List<int>();
    private List<int> selectedObjectsIds = new List<int>();
    private List<int> equipedObjectsIds = new List<int>();


    protected virtual void Start()
    {

        //throw new System.NotImplementedException(); //todo
        Debug.Log("<color=red>TODO : SelectionHighlight.cs not implemented</color>" + name);

        //UMI3DEquipablesManager.Instance.onEquip.AddListener(equip =>
        //{
        //    HighlightEquiped(equip.gameObject);
        //});
        //UMI3DEquipablesManager.Instance.onUnequip.AddListener(equip =>
        //{
        //    DisableEquipedHighlight(equip.gameObject);
        //});
    }

    public void HighlightAvailable(GameObject target)
    {
        int id = target.GetInstanceID();

        if (availableObjectsIds.Contains(id) || hoveredObjectsIds.Contains(id) || selectedObjectsIds.Contains(id) || equipedObjectsIds.Contains(id))
            return;

        if (ghosts.ContainsKey(id))
            return;

        CreateGhost(id, target.gameObject, availableMaterial);
        availableObjectsIds.Add(id);
    }

    public void HighlightHover(GameObject target)
    {
        int id = target.GetInstanceID();

        if (hoveredObjectsIds.Contains(id) || selectedObjectsIds.Contains(id) || equipedObjectsIds.Contains(id))
            return;

        if (ghosts.TryGetValue(id, out List<GameObject> oldGhosts))
        {
            foreach (GameObject g in oldGhosts)
                Destroy(g);
            ghosts.Remove(id);
        }

        CreateGhost(id, target.gameObject, hoverMaterial);
        hoveredObjectsIds.Add(id);
    }

    public void HighlightSelect(GameObject target)
    {
        int id = target.GetInstanceID();

        if (selectedObjectsIds.Contains(id) || equipedObjectsIds.Contains(id))
            return;

        if (ghosts.TryGetValue(id, out List<GameObject> oldGhosts))
        {
            foreach (GameObject g in oldGhosts)
                Destroy(g);
            ghosts.Remove(id);
        }

        CreateGhost(id, target.gameObject, selectedMaterial);
        selectedObjectsIds.Add(id);
    }

    public void HighlightEquiped(GameObject target)
    {
        int id = target.GetInstanceID();

        if (equipedObjectsIds.Contains(id))
            return;

        if (ghosts.TryGetValue(id, out List<GameObject> oldGhosts))
        {
            foreach (GameObject g in oldGhosts)
                Destroy(g);
            ghosts.Remove(id);
        }

        equipedObjectsIds.Add(id);
    }


    private void CreateGhost(int id, GameObject target, Material material)
    {
        List<GameObject> objectGhosts = new List<GameObject>();
        foreach (MeshFilter filter in target.GetComponentsInChildren<MeshFilter>())
        {
            GameObject ghost = new GameObject();
            ghost.transform.parent = filter.transform;
            ghost.transform.localPosition = Vector3.zero;
            ghost.transform.localRotation = Quaternion.Euler(Vector3.zero);
            ghost.transform.localScale = Vector3.one;

            MeshFilter filterClone = ghost.AddComponent<MeshFilter>();
            filterClone.mesh = filter.mesh;

            MeshRenderer renderer = ghost.AddComponent<MeshRenderer>();
            renderer.material = material;

            objectGhosts.Add(ghost);
        }

        ghosts.Add(id, objectGhosts);
    }


    public void DisableAvailableHighlight(GameObject target)
    {
        int id = target.GetInstanceID();

        if (!availableObjectsIds.Contains(id))
            return;

        availableObjectsIds.Remove(id);

        if (!hoveredObjectsIds.Contains(id) && !selectedObjectsIds.Contains(id) && !equipedObjectsIds.Contains(id))
        {
            if (ghosts.TryGetValue(id, out List<GameObject> objectGhosts))
            {
                foreach (GameObject g in objectGhosts)
                    Destroy(g);

                ghosts.Remove(target.GetInstanceID());
            }
        }
    }

    public void DisableHoverHighlight(GameObject target)
    {
        int id = target.GetInstanceID();

        if (!hoveredObjectsIds.Contains(id))
            return;

        hoveredObjectsIds.Remove(id);

        if (!selectedObjectsIds.Contains(id) && !equipedObjectsIds.Contains(id))
        {
            if (ghosts.TryGetValue(id, out List<GameObject> objectGhosts))
            {
                foreach (GameObject g in objectGhosts)
                    Destroy(g);

                ghosts.Remove(target.GetInstanceID());
            }
        }
        
        if (availableObjectsIds.Contains(id))
        {
            CreateGhost(id, target.gameObject, availableMaterial);
        }
    }

    public void DisableSelectionHighlight(GameObject target)
    {
        int id = target.GetInstanceID();

        if (!selectedObjectsIds.Contains(id))
            return;

        selectedObjectsIds.Remove(id);

        if (!equipedObjectsIds.Contains(id))
        {
            if (ghosts.TryGetValue(id, out List<GameObject> objectGhosts))
            {
                foreach (GameObject g in objectGhosts)
                    Destroy(g);

                ghosts.Remove(target.GetInstanceID());
            }
        }
        
        if (hoveredObjectsIds.Contains(id))
        {
            CreateGhost(id, target.gameObject, hoverMaterial);
        }
        else if (availableObjectsIds.Contains(id))
        {
            CreateGhost(id, target.gameObject, availableMaterial);
        }
    }

    public void DisableEquipedHighlight(GameObject target)
    {
        int id = target.GetInstanceID();

        if (!equipedObjectsIds.Contains(id))
            return;

        equipedObjectsIds.Remove(id);      

        if (selectedObjectsIds.Contains(id))
        {
            CreateGhost(id, target.gameObject, selectedMaterial);
        }
        else if (hoveredObjectsIds.Contains(id))
        {
            CreateGhost(id, target.gameObject, hoverMaterial);
        }
        else if (availableObjectsIds.Contains(id))
        {
            CreateGhost(id, target.gameObject, availableMaterial);
        }

    }
}
