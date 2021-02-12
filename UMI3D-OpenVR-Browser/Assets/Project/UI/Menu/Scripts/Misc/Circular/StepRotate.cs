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

public class StepRotate : MonoBehaviour
{
    /// <summary>
    /// Time spent for one rotation (in seconds).
    /// </summary>
    public float duration = 0.5f;

    public Vector3 axis = Vector3.up;

    public float angle = 90;

    /// <summary>
    /// If true, <see cref="RotateLeft"/> and <see cref="RotateRight"/> called while rotating will be registered and executed after the current rotation.
    /// IF false, the calls will be ingnored.
    /// </summary>
    public bool enqueueRotationRequests = false;


    private bool isRotating;

    /// <summary>
    /// Rotation request queue, true means left and false means right. 
    /// </summary>
    private Queue<bool> rotationRequestsQueue = new Queue<bool>();

    /// <summary>
    /// Routine enqueuing the rotation requests (if any).
    /// </summary>
    private Coroutine queueUpdate = null;

    /// <summary>
    /// Make one rotation anti-clockwise.
    /// </summary>
    [ContextMenu("Rotate Left")]
    public void RotateLeft()
    {
        if (!isRotating || enqueueRotationRequests)
        {
            if (isRotating)
            {
                rotationRequestsQueue.Enqueue(true);
                if (queueUpdate == null)
                    queueUpdate = StartCoroutine(DequeueRequests());
            }
            else
            {
                StartCoroutine(RotateLeftInternal());
            }
        }
    }


    /// <summary>
    /// Make one rotation clockwise.
    /// </summary>
    [ContextMenu("Rotate Right")]
    public void RotateRight()
    {
        if (!isRotating || enqueueRotationRequests)
        {
            if (isRotating)
            {
                rotationRequestsQueue.Enqueue(false);
                if (queueUpdate == null)
                    queueUpdate = StartCoroutine(DequeueRequests());
            }
            else
            {
                StartCoroutine(RotateRightInternal());
            }
        }
    }

    IEnumerator RotateLeftInternal()
    {
        float clock = 0;
        Quaternion finalRot = this.transform.rotation * Quaternion.Euler(-angle * axis);
        isRotating = true;

        while (clock < duration)
        {
            this.transform.Rotate(axis, -angle * Time.deltaTime / duration, Space.Self);

            clock += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        this.transform.rotation = finalRot;
        isRotating = false;
    }

    IEnumerator RotateRightInternal()
    {
        float clock = 0;
        Quaternion finalRot = this.transform.rotation * Quaternion.Euler(angle * axis);
        isRotating = true;

        while (clock < duration)
        {
            this.transform.Rotate(axis, angle * Time.deltaTime / duration, Space.Self);

            clock += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        this.transform.rotation = finalRot;
        isRotating = false;
    }

    IEnumerator DequeueRequests()
    {
        while (true)
        {
            if (rotationRequestsQueue.Count == 0)
            {
                StopCoroutine(queueUpdate);
                queueUpdate = null;
            }
            else if (!isRotating)
            {
                bool newRequest = rotationRequestsQueue.Dequeue();
                if (newRequest)
                {
                    RotateLeft();
                }
                else
                {
                    RotateRight();
                }
            }

            yield return null;
        }
    }
}
