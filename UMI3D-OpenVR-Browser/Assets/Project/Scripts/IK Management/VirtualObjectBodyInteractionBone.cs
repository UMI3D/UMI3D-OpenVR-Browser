using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualObjectBodyInteractionBone : MonoBehaviour
{
    public HumanBodyBones bone;

    void OnDrawGizmosSelected()
    {
        VirtualObjectBodyInteraction gizmos = GetComponentInParent<VirtualObjectBodyInteraction>();
        gizmos.DrawLine(gizmos.transform);
    }
}
