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
using umi3d.cdk;
using System.Linq;
using umi3d.cdk.interaction;

public class AvaibilityHighlighter : MonoBehaviour
{
    public float radius;
    public float angle;

    private List<GameObject> highlightedObjects = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        List<GameObject> buffer = new List<GameObject>();
        foreach (Collider col in Physics.OverlapSphere(this.transform.position, radius)
            .Where(c => Vector3.Angle(c.transform.position - this.transform.position, this.transform.forward) < angle)
            .Where(c => (c.transform.GetComponent<InteractableContainer>() != null) || (c.transform.GetComponentInParent<InteractableContainer>() != null)))
        {
            //Equipment equip = col.GetComponent<Equipment>();
            //if ((equip != null) && equip.isEquiped)
            //    continue;


            buffer.Add(col.transform.gameObject);
            if (!highlightedObjects.Contains(col.transform.gameObject))
            {
                SelectionHighlight.Instance.HighlightAvailable(col.transform.gameObject);
                highlightedObjects.Add(col.transform.gameObject);
            }
            
        }

        List<GameObject> copy = new List<GameObject>(highlightedObjects);
        foreach (GameObject old in copy)
        {
            if (!buffer.Contains(old))
            {
                SelectionHighlight.Instance.DisableAvailableHighlight(old);
                highlightedObjects.Remove(old);
            }
        }
    }
}
