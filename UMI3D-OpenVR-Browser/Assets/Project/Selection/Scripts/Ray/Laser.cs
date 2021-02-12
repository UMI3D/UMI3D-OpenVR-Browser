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

/// <summary>
/// Display a laser using a cylinder (has to be configured directly in the scene).
/// </summary>
public class Laser : MonoBehaviour
{
    public bool hovering { get { return tokens.Count > 0; } }


    public GameObject laserObject;
    public GameObject impactPoint;

    public Color defaultColor;
    public Color hoverColor;

    private Renderer laserObjectRenderer;
    private Renderer impactPointRenderer;

    private List<int> tokens = new List<int>();

    /// <summary>
    /// Notify that the component indentified by the given token is hovering something.
    /// </summary>
    /// <param name="token"></param>
    public void OnHoverEnter(int token)
    {
        if (tokens.Contains(token))
        {
            Debug.LogWarning("This token has already been given.");
            return;
        }
        else if (!hovering)
            HoverEnterInternal();

        tokens.Add(token);
    }

    /// <summary>
    /// Notify that the component indentified by the given token is not hovering anything anymore.
    /// </summary>
    /// <param name="token"></param>
    public void OnHoverExit(int token)
    {
        if (!tokens.Contains(token))
        {
            return;
        }
        tokens.Remove(token);

        if (tokens.Count == 0)
            HoverExitInternal();
    }


    protected void HoverEnterInternal()
    {
        if (impactPointRenderer != null)
            impactPointRenderer.material.color = hoverColor;

        if (laserObjectRenderer != null)
            laserObjectRenderer.material.color = hoverColor;
    }

    protected void HoverExitInternal()
    {
        if (impactPointRenderer != null)
            impactPointRenderer.material.color = defaultColor;
        if (laserObjectRenderer != null)
            laserObjectRenderer.material.color = defaultColor;
        SetImpactPoint(this.transform.position + this.transform.forward * 500, false);
    }

    public void SetImpactPoint(Vector3 point, bool displayImpact = true)
    {
        impactPoint.transform.position = point;
        laserObject.transform.localScale = new Vector3(1, Vector3.Distance(this.transform.position, point), 1);
        impactPoint.SetActive(displayImpact);
    }


    protected virtual void Awake()
    {
        laserObjectRenderer = laserObject.GetComponent<Renderer>();
        impactPointRenderer = impactPoint.GetComponent<Renderer>();

        if(impactPointRenderer != null)
            impactPointRenderer.material.color = defaultColor;

        if (laserObjectRenderer != null)
            laserObjectRenderer.material.color = defaultColor;

        SetImpactPoint(this.transform.position + this.transform.forward * 500, false);
    }


}
