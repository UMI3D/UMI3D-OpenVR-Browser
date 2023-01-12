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
        LeftHandMirror = GetLimb("LeftHand");
        RightFootMirror = GetLimb("RightFoot");
        RightHandMirror = GetLimb("RightHand");
    }

    private void MoveLimbs()
    {
        Copy(LeftFoot, LeftFootMirror);
        Copy(LeftHand, LeftHandMirror);
        Copy(RightFoot, RightFootMirror);
        Copy(RightHand, RightHandMirror);
    }

    private void Copy(VirtualObjectBodyInteraction original, GameObject copy)
    {
        var (pos, rot) = Mirror(original.transform);
        animator.SetIKPosition(original.goal, pos);
        animator.SetIKRotation(original.goal, rot);
        copy.transform.position = pos;
        copy.transform.rotation = rot;
    }

    private (Vector3 pos, Quaternion rot) Mirror(Transform transform)
    {
        (Vector3 pos, Quaternion rot) result;

        result.pos = transform.position;
        result.pos.z = -transform.position.z;
        result.rot = transform.rotation;
        result.rot = Quaternion.Euler(transform.rotation.eulerAngles.x, -transform.rotation.eulerAngles.y + 180, transform.rotation.eulerAngles.z);
        return result;
    }
}
