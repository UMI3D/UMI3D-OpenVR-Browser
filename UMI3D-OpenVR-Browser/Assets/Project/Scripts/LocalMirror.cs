using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using umi3dVRBrowsersBase.ikManagement;
using UnityEditor;
using UnityEngine;


public class LocalMirror : MonoBehaviour
{
    private GameObject mirrorAvatar;
    private Animator animator;

    private VirtualObjectBodyInteraction LeftFoot;
    private VirtualObjectBodyInteraction RightFoot;
    private VirtualObjectBodyInteraction LeftHand;
    private VirtualObjectBodyInteraction RightHand;

    private UMI3DClientUserTrackingBone head;

    private GameObject LeftFootMirror;
    private GameObject RightFootMirror;
    private GameObject LeftHandMirror;
    private GameObject RightHandMirror;

    public void Start()
    {
        var objects = new List<VirtualObjectBodyInteraction>(FindObjectsOfType<VirtualObjectBodyInteraction>());
        var bones = new List<UMI3DClientUserTrackingBone>(FindObjectsOfType<UMI3DClientUserTrackingBone>());
        LeftFoot = objects.Find(x => x.goal == AvatarIKGoal.LeftFoot);
        RightFoot = objects.Find(x => x.goal == AvatarIKGoal.RightFoot);
        LeftHand = objects.Find(x => x.goal == AvatarIKGoal.LeftHand);
        RightHand = objects.Find(x => x.goal == AvatarIKGoal.RightHand);
        head = bones.Find(x => x.boneType == BoneType.Head);

    }

    private bool isMirrorLoaded;

    public void Update()
    {
        if (!isMirrorLoaded)
        {
            if (mirrorAvatar == null)
                mirrorAvatar = GameObject.Find("Player mirror");
            if (mirrorAvatar == null)
                return;
            if (animator == null)
                animator = mirrorAvatar.GetComponentInChildren<Animator>();
            if (animator == null)
                return;
            isMirrorLoaded = true;
            GetMirrorLimbs();
        }
        else
        {
            MoveLimbs();
        }
    }

    private void GetMirrorLimbs()
    {
        string mixamoBase = "mixamorig:";
        var children = mirrorAvatar.GetComponentsInChildren<Transform>();
        GameObject GetLimb(string name)
        {
            return children.FirstOrDefault(x => x.gameObject.name == mixamoBase + name)?.gameObject;
        }
        LeftFootMirror = GetLimb("LeftFoot");
        Debug.Log(LeftFootMirror);
        LeftHandMirror = GetLimb("LeftHand");
    }

    private void MoveLimbs()
    {
        Copy(LeftFoot, LeftFootMirror);
        Copy(LeftHand, LeftHandMirror);
    }

    private void Copy(VirtualObjectBodyInteraction original, GameObject copy)
    {
        animator.SetIKPosition(original.goal, copy.transform.position);
        animator.SetIKRotation(original.goal, copy.transform.rotation);
        copy.transform.position = original.transform.position;
        copy.transform.rotation = original.transform.rotation;
    }
}
