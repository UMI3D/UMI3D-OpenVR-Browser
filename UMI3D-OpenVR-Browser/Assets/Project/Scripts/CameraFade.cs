using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class CameraFade : MonoBehaviour
{
	[SerializeField]
	private Material fadeMaterial = null;

	[SerializeField]
	private Color fadeColor = Color.black;

	MeshFilter fadeMeshFilter;
	MeshRenderer fadeMeshRenderer;

    private void Awake()
    {
		fadeMeshFilter = gameObject.AddComponent<MeshFilter>();

		var mesh = new Mesh();
		fadeMeshFilter.mesh = mesh;

		// 1. Create a screen object
		Vector3[] vertices = new Vector3[4];
		vertices[0] = new Vector3(-4, -4, 1);
		vertices[1] = new Vector3(4, -4, 1);
		vertices[2] = new Vector3(-4, 4, 1);
		vertices[3] = new Vector3(4, 4, 1);

		mesh.vertices = vertices;

		int[] tri = new int[6];

		tri[0] = 0;
		tri[1] = 2;
		tri[2] = 1;

		tri[3] = 2;
		tri[4] = 3;
		tri[5] = 1;

		mesh.triangles = tri;

		Vector3[] normals = new Vector3[4];

		normals[0] = -Vector3.forward;
		normals[1] = -Vector3.forward;
		normals[2] = -Vector3.forward;
		normals[3] = -Vector3.forward;

		mesh.normals = normals;

		//2. Add renderer
		fadeMeshRenderer = gameObject.AddComponent<MeshRenderer>();
		fadeMeshRenderer.material = fadeMaterial;
		fadeMeshRenderer.enabled = false;
	}

    private void OnDestroy()
    {
		Object.Destroy(fadeMeshFilter);
		Object.Destroy(fadeMeshRenderer);
    }

    public void Fade()
    {
        StartCoroutine(FadeCoroutine());
    }

    IEnumerator FadeCoroutine()
    {
		fadeMeshRenderer.enabled = true;

		float t = 0;
		float fadeDuration = .2f;

		Color color;

		while (t < fadeDuration)
        {
			color = Color.Lerp(fadeColor, Color.clear, t / fadeDuration);


			fadeMaterial.SetColor("_BaseColor", color);

			t += Time.deltaTime;

			yield return null;
        }

		fadeMeshRenderer.enabled = false;
	}
}
