using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualObjectBodyInteraction : MonoBehaviour
{
    public enum BodyInteractionType { Interaction = 0, Intent = 1 };
    public BodyInteractionType interactionType;
    public AvatarIKGoal goal;

    void OnDrawGizmosSelected()
    {
        DrawLine(transform);
    }

    public void DrawLine(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Draws a blue line from this transform to the target
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(parent.position, child.position);
            DrawLine(child);
        }
    }
}
