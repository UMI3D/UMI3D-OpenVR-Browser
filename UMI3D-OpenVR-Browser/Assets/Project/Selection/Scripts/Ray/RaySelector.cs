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
using UnityEngine;
using System;
using umi3d.cdk;
using umi3d.cdk.interaction;
using System.Collections.Generic;

/// <summary>
/// Parametric class for object ray selection.
/// </summary>
/// <typeparam name="T">Objects to pick type</typeparam>
public abstract class RaySelector<T> : RaySelector
{
    /// <summary>
    /// Return all currently pointed objects (any type) as an array of RaycastHit.
    /// </summary>
    /// <returns></returns>
    protected RaycastHit[] GetAllPointedObjects()
    {
        return umi3d.common.Physics.RaycastAll(this.transform.position, this.transform.forward, 100);
    }

    /// <summary>
    /// Return all currently pointed objects of type T as an array of RaycastHit.
    /// </summary>
    /// <returns></returns>
    protected RaycastHit[] GetPointedObjects(bool checkInParent = false)
    {
        RaycastHit[] hits = GetAllPointedObjects();

        RaycastHit[] hitsT = Array.FindAll(hits, x => {
            bool hasTComponent = (x.transform.GetComponent<T>() != null);
            if (!hasTComponent)
                hasTComponent = (x.transform.GetComponentInParent<T>() != null);

            return hasTComponent;
        });

        return hitsT;
    }

    /// <summary>
    /// Return closest currently pointed object of type T as a RaycastHit.
    /// </summary>
    /// <returns></returns>
    protected RaycastHit? GetClosestPointedObject(bool checkInParent = false)
    {
        RaycastHit[] hits = GetPointedObjects(checkInParent);
        if (typeof(T) == typeof(Interactable))
        {
            hits = SortInteractable(hits);
        }
        if (hits.Length > 0)
        {
            return hits[0];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Return closest currently pointed object (any type) as a RaycastHit.
    /// </summary>
    /// <returns></returns>
    protected RaycastHit? GetClosestOfAllPointedObject()
    {
        RaycastHit[] hits = GetAllPointedObjects();
        if (typeof(T) == typeof(Interactable))
        {
            hits = SortInteractable(hits);
        }
        if (hits.Length > 0)
        {
            return hits[0];
            
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Return true if the closest of all pointed object (if any) is of type T, false otherwise
    /// </summary>
    /// <returns></returns>
    protected bool isClosestPointedObjectOfTypeT()
    {
        RaycastHit? c = GetClosestOfAllPointedObject();
        return c.HasValue ? (c.Value.transform.GetComponent<T>() != null) : false;
    }

    private RaycastHit[] SortInteractable(RaycastHit[] source)
    {
        List<RaycastHit> hitsList = new List<RaycastHit>(source);
        List<(RaycastHit, Interactable)> interactables = new List<(RaycastHit, Interactable)>();

        foreach (RaycastHit hit in source)
        {
            InteractableContainer container = hit.transform.GetComponent<InteractableContainer>();

            if (container == null)
            {
                container = hit.transform.GetComponentInParent<InteractableContainer>();
            }

            if (container != null)
            {
                interactables.Add((hit, container.Interactable));
            }
        }

        Transform t = Camera.main.transform;

        interactables.Sort(delegate ((RaycastHit, Interactable) x, (RaycastHit, Interactable) y)
        {
            if (x.Item2.Active && !y.Item2.Active)
                return -1;
            else if (!x.Item2.Active && y.Item2.Active)
                return 1;
            else if (x.Item2.HasPriority && !y.Item2.HasPriority)
                return -1;
            else if (!x.Item2.HasPriority && y.Item2.HasPriority)
                return 1;
            else
            {
                if (Vector3.Distance(t.position, x.Item1.point) >= Vector3.Distance(t.position, y.Item1.point))
                    return 1;
                else
                    return -1;
            }
        });

        List<RaycastHit> res = new List<RaycastHit>();
        foreach (var e in interactables)
            res.Add(e.Item1);

        return res.ToArray();
    }

    /// <summary>
    /// Update the ray displayer depending on the object pointed.
    /// </summary>
    protected virtual void UpdateRayDisplayer()
    {

        RaycastHit? hit = GetClosestOfAllPointedObject();
        if (hit != null)
        {
            laser.SetImpactPoint(hit.Value.point);
        }
    }


    
    protected virtual void Update()
    {
        UpdateRayDisplayer();
    }

}

public abstract class RaySelector : AbstractSelector
{
    /// <summary>
    /// Ray displayer
    /// </summary>
    public Laser laser;

    protected override void ActivateInternal()
    {
        base.ActivateInternal();
        laser.gameObject.SetActive(true);
        laser.SetImpactPoint(this.transform.position + this.transform.forward * 500, false);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();
        laser.OnHoverExit(this.gameObject.GetInstanceID());
        laser.gameObject.SetActive(false);        
    }      
}