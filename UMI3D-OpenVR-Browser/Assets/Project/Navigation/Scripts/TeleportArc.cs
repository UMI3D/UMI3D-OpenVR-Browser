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
using BrowserQuest.Navigation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TeleportArc : MonoBehaviour
{
    public GameObject stepDisplayerPrefab;
    public GameObject impactPoint;
    public GameObject errorPoint;
    public Material defaultMaterial;
    public Material pointingAtTpArea;
    public Material pointingAtObstacle;

    public Transform rayStartPoint;
    public float raySpeed = 6;
    public float maxVerticalAngle;

    /// <summary>
    /// Arc discrete subdivision length.
    /// </summary>
    public float stepLength = 0.1f;

    public float arcMaxLength = 100;

    /// <summary>
    /// Time spent between each update (in seconds).
    /// </summary>
    public float updateRate = 0.1f;

    public LayerMask navmeshLayer;


    private Coroutine updateRoutine = null;
    private List<GameObject> displayers = new List<GameObject>();

    protected virtual void Awake()
    {
        for (int i = 0; i < arcMaxLength/stepLength + 1; i++)
        {
            GameObject disp = Instantiate(stepDisplayerPrefab, this.transform);
            disp.SetActive(false);
            displayers.Add(disp);
        }
    }

    [ContextMenu("Display")]
    public void Display()
    {
        if (updateRoutine != null)
            return;

        updateRoutine = StartCoroutine(UpdateArc());

    }

    [ContextMenu("Hide")]
    public void Hide()
    {
        if (updateRoutine == null)
            return;

        impactPoint.SetActive(false);
        errorPoint.SetActive(false);
        foreach (GameObject g in displayers)
            g.SetActive(false);

        StopCoroutine(updateRoutine);
        updateRoutine = null;
    }

    public Vector3? GetPointedPoint()
    {
        if (impactPoint.activeSelf)
            return impactPoint.transform.position;
        else
            return null;
    }


    private Vector3 GetArcPoint(float distanceFromStartAlongArc)
    {
        return Physics.gravity * distanceFromStartAlongArc * distanceFromStartAlongArc
            + rayStartPoint.forward * raySpeed * distanceFromStartAlongArc
            + rayStartPoint.position;
    }

   


    private IEnumerator UpdateArc()
    {
        while (true)
        {
            Vector3 previousArcPoint = rayStartPoint.position;
            int stepCount;

            /* 
             * 0:found nothing, 
             * 1:found tp area, 
             * 2:found obstacle
             */
            int state = 0;
            for (stepCount = 1; stepCount < arcMaxLength / stepLength; stepCount++)
            {
                Vector3 point = GetArcPoint(stepCount * stepLength);
                Vector3 nextPoint = GetArcPoint((stepCount + 1) * stepLength);

                RaycastHit hit;
                if (Physics.Raycast(previousArcPoint, point - previousArcPoint, out hit, (nextPoint - point).magnitude, navmeshLayer))
                {
                    TeleportArea area = hit.transform.GetComponent<TeleportArea>();
                    TeleportObstacle obstacle = hit.transform.GetComponent<TeleportObstacle>();
                    if (area != null)
                    {
                        impactPoint.SetActive(true);
                        impactPoint.transform.position = hit.point;
                        impactPoint.transform.LookAt(hit.point + hit.normal);
                        errorPoint.SetActive(false);
                        TeleportArea.Instances.ForEach(a => a.Highlight());
                        state = 1;
                    } 
                    else if (obstacle != null)
                    {
                        errorPoint.SetActive(true);
                        errorPoint.transform.position = hit.point;
                        errorPoint.transform.LookAt(hit.point + hit.normal);
                        impactPoint.SetActive(false);
                        TeleportArea.Instances.ForEach(a => a.DisableHighlight());
                        state = 2;
                    }
                    else
                    {
                        TeleportArea.Instances.ForEach(a => a.DisableHighlight());
                        state = 0;
                        Debug.LogWarning("Teleport arc hit something that is neither a TeleportArea nor an TeleportObstacle, check your layers' physics.");
                    }
                    break;
                } 
                else
                {
                    impactPoint.SetActive(false);
                    errorPoint.SetActive(false);
                    TeleportArea.Instances.ForEach(area => area.DisableHighlight());
                    state = 0;
                }

                GameObject disp = displayers[stepCount];
                disp.transform.position = point;
                if (!disp.activeSelf)
                {
                    disp.SetActive(true);
                }
                previousArcPoint = point;                
            }

            if (state > 0)
            {
                for (int i = stepCount; i < arcMaxLength / stepLength; i++)
                {
                    displayers[i].SetActive(false);
                }
            }
            else
            {
                impactPoint.SetActive(false);
                errorPoint.SetActive(false);
            }
            displayers.ForEach(
                disp => disp.GetComponentsInChildren<MeshRenderer>().ToList().ForEach(
                    rnd => rnd.material =
                        (state == 0) ?
                        defaultMaterial :
                        (state == 1) ?
                        pointingAtTpArea :
                        pointingAtObstacle
                    ));

            yield return new WaitForSeconds(updateRate);
        }
    }

}
