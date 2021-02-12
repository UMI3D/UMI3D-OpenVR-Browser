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
using System.Linq;
using UnityEngine;

public class HemicircularPathConstraint : MonoBehaviour
{
    public List<Transform> constraintedObjects = new List<Transform>();

    /// <summary>
    /// Circle radius.
    /// </summary>
    public float radius = 1;

    /// <summary>
    /// Circle center in local frame.
    /// </summary>
    public Vector3 circleCenter = Vector3.zero;

    public int numberOfItemsInCircle = 4;

    public float angleDelta = 0;

    private int cursor = 0;

    /// <summary>
    /// Displacement amount along the hemicircle.
    /// </summary>
    public int Cursor
    {
        get => cursor;
        set
        {
            cursor = value;
            UpdatePosition();
        }
    }

    public bool activated = true;

    public void Activate()
    {
        activated = true;
    }

    public void Deactivate()
    {
        activated = false;
    }

    protected virtual void UpdatePosition()
    {
        constraintedObjects = constraintedObjects.Where(obj => obj != null).ToList(); //Sometimes, some transform are null because they were destroyed in MenuDisplayManager.Clear().

        int n = constraintedObjects.Count;

        for (int i=0; i<n; i++)
        {
            constraintedObjects[i].position = this.transform.TransformPoint(IndexToPosition(Cursor - i) + circleCenter);
            constraintedObjects[i].LookAt(2 * constraintedObjects[i].position - this.transform.TransformPoint(circleCenter), this.transform.up);
            constraintedObjects[i].Rotate(Vector3.up, angleDelta, Space.Self);

            if (activated)
            {
                if ((Cursor - i >= numberOfItemsInCircle) || (Cursor - i < 0))
                {
                    if (constraintedObjects[i].gameObject.activeSelf)
                        constraintedObjects[i].gameObject.SetActive(false);
                }
                else if (!constraintedObjects[i].gameObject.activeSelf)
                    constraintedObjects[i].gameObject.SetActive(true);
            }
            else
            {
                if (constraintedObjects[i].gameObject.activeSelf)
                    constraintedObjects[i].gameObject.SetActive(false);
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < numberOfItemsInCircle - 1; i++)
        {
            Gizmos.DrawLine(
                this.transform.TransformPoint(IndexToPosition(i) + circleCenter),
                this.transform.TransformPoint(IndexToPosition(i + 1) + circleCenter));
        }
    }

    public Vector3 IndexToPosition(int index)
    {
        float x = 0, z = 0;

        if (index >= numberOfItemsInCircle)
        {
            x = -radius;
            z = -1;
        }
        else if (index < 0)
        {
            x = radius;
            z = -1;
        }
        else
        {
            x = radius * Mathf.Cos(((float)index+ .53f) / ((float)numberOfItemsInCircle) * Mathf.PI); //why .53f ?
            z = radius * Mathf.Sin(((float)index+ .53f) / ((float)numberOfItemsInCircle) * Mathf.PI); //idk ...
        }

        return new Vector3(x, 0, z);
    }
}
