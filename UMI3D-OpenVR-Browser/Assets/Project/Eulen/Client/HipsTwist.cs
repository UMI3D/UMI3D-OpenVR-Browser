using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class HipsTwist : MonoBehaviour
{

    public Transform leftShoulder, rightShoulder, waist;
    public void Update()
    {
        Vector3 startAngle = leftShoulder.position - rightShoulder.position;
        Vector3 endAngle = waist.right;

        Debug.Log(Vector3.SignedAngle(startAngle, endAngle, Vector3.up));
    }
}
