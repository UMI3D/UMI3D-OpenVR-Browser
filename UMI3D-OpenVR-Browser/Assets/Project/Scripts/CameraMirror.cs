using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CameraMirror : MonoBehaviour
{
    private GameObject cameraAnchor;
    private Camera cameraFront;
    private Camera cameraTop;

    private bool isCameraSetUp;

    public Canvas display;
    public RawImage displayFront;
    public RawImage displayTop;

    void Start()
    {
        
    }

    void Update()
    {
        if (cameraAnchor == null)
        {
            cameraAnchor = GameObject.Find("Camera Anchors");
            if (cameraAnchor != null)
            {
                cameraFront = cameraAnchor.transform.Find("Camera Front").gameObject.AddComponent<Camera>();
                cameraTop = cameraAnchor.transform.Find("Camera Top").gameObject.AddComponent<Camera>();

                var renders = display.GetComponentsInChildren<RawImage>();
                displayFront = renders[0];
                displayTop = renders[1];

                cameraFront.targetTexture = displayFront.texture as RenderTexture;
                cameraTop.targetTexture = displayTop.texture as RenderTexture;

                isCameraSetUp = true;
            }
        }
           
    }
}
