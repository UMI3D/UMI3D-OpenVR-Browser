using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AngleGizmoManager : MonoBehaviour
{
    #region Static

    /// <summary>
    /// Singleton
    /// </summary>
    public static AngleGizmoManager instance;

    /// <summary>
    /// Registers a gizmo.
    /// </summary>
    /// <param name="angleGizmo"></param>
    public static void AddGizmo(AngleGizmo angleGizmo)
    {
        instance.angleGizmos.Add(angleGizmo);
    }

    /// <summary>
    /// Unregisters a gizmo.
    /// </summary>
    /// <param name="angleGizmo"></param>
    public static void RemoveGizmo(AngleGizmo angleGizmo)
    {
        instance.angleGizmos.Remove(angleGizmo);
    }

    #endregion

    #region Fields

    /// <summary>
    /// Material to display a angle.
    /// </summary>
    [SerializeField, Tooltip("Material to display a angle")]
    private Material angleMaterial;

    [HideInInspector]
    public void AngMat(Material ang, Material circle, Material error, Material errorInside)
    {
        angleMaterial = ang;
        defaultAngleMaterial = circle;
        errorAngleMaterial = error;
        errorAngleInsideMaterial = errorInside;
    }

    /// <summary>
    /// Default material to draw a gizmo.
    /// </summary>
    [SerializeField, Tooltip("Default material to draw a gizmo")]
    private Material defaultAngleMaterial;

    /// <summary>
    /// Material to draw an line error.
    /// </summary>
    [SerializeField, Tooltip("Material to draw an line error")]
    private Material errorAngleMaterial;

    /// <summary>
    /// Material to draw a surface error.
    /// </summary>
    [SerializeField, Tooltip("Material to draw a surface error")]
    private Material errorAngleInsideMaterial;

    [SerializeField]
    private Camera cam;

    /// <summary>
    /// List of all gizmos to display.
    /// </summary>
    private HashSet<AngleGizmo> angleGizmos = new HashSet<AngleGizmo>();

    #endregion

    #region Methods

    private void Awake()
    {
        instance = this;
    }

    void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;    //
    }

    void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;    //
    }

    /// <summary>
    /// Draws gizmos.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="camera"></param>
    private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        /*if (camera != cam)
            return;*/

        foreach (var gizmo in angleGizmos)
        {
            GL.Begin(GL.LINES);

            if (!gizmo.Enabled)
                continue;

            if (gizmo.isError)
                errorAngleMaterial.SetPass(0);
            else
            {
                angleMaterial.SetPass(0);
                GL.MultMatrix(Matrix4x4.identity);
            }
            Vector3 startAngle = gizmo.startAngle.normalized * gizmo.size;
            Vector3 endAngle = Quaternion.AngleAxis(gizmo.angle, gizmo.rotationAxis) * startAngle;
            Vector3 center = gizmo.center;

            // Display angle
            GL.Vertex(center);
            GL.Vertex(center + startAngle);

            GL.Vertex(center);
            GL.Vertex(center + endAngle);

            int nbOfSections = (int)(gizmo.angle / 5f);

            Vector3 rotationAxis = gizmo.rotationAxis;
            Vector3 sectionEnd = startAngle;

            for (int i = 0; i < nbOfSections; i++)
            {
                Vector3 tmp = Quaternion.AngleAxis(5, rotationAxis) * sectionEnd;

                GL.Vertex(center + sectionEnd);
                GL.Vertex(center + tmp);

                Vector3 sectionCenter = (sectionEnd + tmp) * .5f;

                GL.Vertex(center + sectionCenter * 0.95f);
                GL.Vertex(center + sectionCenter);

                sectionEnd = tmp;
            }

            GL.Vertex(center + sectionEnd);
            GL.Vertex(center + endAngle);

            // Display 360 circle
            if (gizmo.displayTotalCircle)
            {
                GL.End();
                GL.Begin(GL.LINES);
                defaultAngleMaterial.SetPass(0);

                sectionEnd = startAngle;

                for (int i = 0; i < 720; i++)
                {
                    Vector3 tmp = Quaternion.AngleAxis(5, rotationAxis) * sectionEnd;

                    GL.Vertex(center + sectionEnd);
                    GL.Vertex(center + tmp);

                    Vector3 sectionCenter = (sectionEnd + tmp) * .5f;

                    GL.Vertex(center + sectionCenter * 0.95f);
                    GL.Vertex(center + sectionCenter);

                    sectionEnd = tmp;
                }
                GL.End();
                GL.Begin(GL.LINES);
            }

            // Label

            /*Vector3 labelCenter = Quaternion.AngleAxis(gizmo.angle / 2f, rotationAxis) * startAngle * .5f;
            Vector3 labelViewportCenter = cam.WorldToViewportPoint(center + labelCenter);

            gizmo.label.style.left = labelViewportCenter.x * Screen.width;
            gizmo.label.style.top = (1 - labelViewportCenter.y) * Screen.height;
            gizmo.label.text = Mathf.Round(gizmo.angle).ToString() + " °";*/

            GL.End();

            // Error
            if (gizmo.isError)
            {
                GL.Begin(GL.TRIANGLES);
                errorAngleInsideMaterial.SetPass(0);

                sectionEnd = startAngle;

                for (int i = 0; i < nbOfSections; i++)
                {
                    Vector3 tmp = Quaternion.AngleAxis(5, rotationAxis) * sectionEnd;

                    GL.Vertex(center);
                    GL.Vertex(center + sectionEnd);
                    GL.Vertex(center + tmp);

                    GL.Vertex(center);
                    GL.Vertex(center + tmp);
                    GL.Vertex(center + sectionEnd);

                    sectionEnd = tmp;
                }

                GL.Vertex(center);
                GL.Vertex(center + sectionEnd);
                GL.Vertex(center + endAngle);

                GL.Vertex(center);
                GL.Vertex(center + endAngle);
                GL.Vertex(center + sectionEnd);

                GL.End();
            }


        }
    }

    #endregion
}
