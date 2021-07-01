using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualObjectBodyInteractionEndBone : MonoBehaviour
{

    void OnDrawGizmosSelected()
    {
        VirtualObjectBodyInteraction gizmos = GetComponentInParent<VirtualObjectBodyInteraction>();
        gizmos.DrawLine(gizmos.transform);
    }
}
