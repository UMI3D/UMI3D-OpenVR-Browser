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
using umi3d.cdk.interaction;
using UnityEngine;



public abstract class ProximitySelector<T> : ProximitySelector where T : Component
{
    protected class Closest
    {
        public T component;
        public Collider collider;
        public Vector3 localHoverPosition;
        public Vector3 localHoverNormal;
    }

    /// <summary>
    /// Wraper for neighbour storage.
    /// </summary>
    protected class Neighbour
    {
        public T obj;

        /// <summary>
        /// Collider used from selection.
        /// </summary>
        public Collider collider;
    }

    protected List<Neighbour> neighbours = new List<Neighbour>();

    protected virtual void OnTriggerEnter(Collider other)
    {
        T neighbour = other.GetComponent<T>();
        if (neighbour == null)
            neighbour = other.GetComponentInParent<T>();

        if (neighbour != null)
            neighbours.Add(new Neighbour()
            {
                obj = neighbour,
                collider = other
            });
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        T neighbour = other.GetComponent<T>();
        if (neighbour == null)
            neighbour = other.GetComponentInParent<T>();

        if (neighbour != null)
            neighbours.RemoveAll(n => n.collider.Equals(other));
    }

    protected virtual Closest GetClosest()
    {
        if (neighbours.Count == 0)
            return null;

        Neighbour closestObj = neighbours[0];
        float minDist = float.MaxValue;

        List<Neighbour> buffer = new List<Neighbour>(neighbours);
        foreach (Neighbour n in buffer)
        {
            if ((n == null) || (n.collider == null) || (n.obj == null))
            {
                neighbours.Remove(n);
                continue;
            }

            float distance = Vector3.Distance(this.transform.position, n.collider.ClosestPoint(this.transform.position));
            if (distance < minDist)
            {
                minDist = distance;
                closestObj = n;
            }
        }

        Vector3 hoverLocalPosition = closestObj.collider.ClosestPoint(this.transform.position);
        Closest closest = new Closest()
        {
            component = closestObj.obj,
            collider = closestObj.collider,
            localHoverPosition = closestObj.obj.transform.InverseTransformPoint(hoverLocalPosition),
            localHoverNormal = closestObj.obj.transform.InverseTransformDirection((this.transform.position - hoverLocalPosition).normalized)
        };
        return closest;
    }

    public override bool isHoveringSomething()
    {
        return neighbours.Count > 0;
    }
}


/// <summary>
/// Selection by proximity.
/// </summary>
public abstract class ProximitySelector : AbstractSelector
{
    public abstract bool isHoveringSomething();   
}
