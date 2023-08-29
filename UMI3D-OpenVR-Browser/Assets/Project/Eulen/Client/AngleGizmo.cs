using UnityEngine;

public class AngleGizmo
{
    /// <summary>
    /// World center of gizmo
    /// </summary>
    public Vector3 center = Vector3.zero;

    /// <summary>
    /// Defines start angle, from center (world position).
    /// </summary>
    public Vector3 startAngle = Vector3.zero;

    /// <summary>
    /// Rotation axis (world position).
    /// </summary>
    public Vector3 rotationAxis = Vector3.zero;

    /// <summary>
    /// Angle to rotate (degrees) from <see cref="startAngle"/> along <see cref="rotationAxis"/>.
    /// </summary>
    public float angle;

    /// <summary>
    /// Gizmo size (radius).
    /// </summary>
    public float size = 1;

    private bool enable = true;

    /// <summary>
    /// Display a 360 circle behing angle ? 
    /// </summary>
    public bool displayTotalCircle = false;

    /// <summary>
    /// Display angle with error shader ?
    /// </summary>
    public bool isError = false;

    public bool Enabled
    {
        get => enable;
        set
        {
            if (enable == value) return;

            enable = value;
        }
    }
}