
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using umi3dVRBrowsersBase.ikManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.XR;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.Image;


public class LocalMirror : MonoBehaviour
{
    private GameObject avatar;

    private GameObject mirrorAvatar;
    private Animator animator;

    private UMI3DClientUserTrackingBone LeftFoot;
    private UMI3DClientUserTrackingBone RightFoot;
    private UMI3DClientUserTrackingBone LeftHand;
    private UMI3DClientUserTrackingBone RightHand;

    private UMI3DClientUserTrackingBone head;
    private UMI3DClientUserTrackingBone hips;

    private GameObject LeftFootMirror;
    private GameObject RightFootMirror;
    private GameObject HipsMirror;
    private GameObject LeftHandMirror;
    private GameObject RightHandMirror;

    private Dictionary<uint, AvatarIKGoal> goals = new Dictionary<uint, AvatarIKGoal>()
    {
        { BoneType.LeftHand, AvatarIKGoal.LeftHand },
        { BoneType.RightHand, AvatarIKGoal.RightHand },
        { BoneType.LeftToeBase, AvatarIKGoal.LeftFoot },
        { BoneType.RightToeBase, AvatarIKGoal.RightFoot },
    };

    public void Start()
    {
        var bones = new List<UMI3DClientUserTrackingBone>(FindObjectsOfType<UMI3DClientUserTrackingBone>());
        head = bones.Find(x => x.boneType == BoneType.Head);
        hips = bones.Find(x => x.boneType == BoneType.Hips);
        LeftFoot = bones.Find(x => x.boneType == BoneType.LeftToeBase);
        RightFoot = bones.Find(x => x.boneType == BoneType.RightToeBase);
        LeftHand = bones.Find(x => x.boneType == BoneType.LeftHand);
        RightHand = bones.Find(x => x.boneType == BoneType.RightHand);

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
            //animator.enabled = false;
            avatar = FindObjectsOfType<UMI3DClientUserTrackingBone>()
                        .Where(x=>x.boneType == BoneType.CenterFeet)
                        .FirstOrDefault()
                        .gameObject;
            mirrorAvatar.transform.localScale = avatar.transform.localScale;
            var IkRedirector = animator.gameObject.AddComponent<IKRedirector>();
            IkRedirector.mirrorhandler = this;
            isMirrorLoaded = true;
            GetMirrorLimbs();
        }
        else
        {
            MoveLimbsNoIK();
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
        LeftHandMirror = GetLimb("LeftHand");
        RightHandMirror = GetLimb("RightHand");
        HipsMirror = GetLimb("Hips").transform.parent.parent.gameObject;
    }

    public void MoveLimbsNoIK()
    {
        HipsMirror.transform.position = new Vector3(hips.transform.position.x, 0, hips.transform.position.z);
        HipsMirror.transform.rotation = hips.transform.rotation;

        HipsMirror.transform.position = new Vector3(hips.transform.position.x, 0, -hips.transform.position.z);

        Copy(LeftHand, LeftHandMirror);
        Copy(RightHand, RightHandMirror);
    }

    public void MoveLimbs()
    {
        if (!isMirrorLoaded)
            return;



        //HipsMirror.transform.localScale = new Vector3(1, 1, -1);
        SetIK(LeftHand, LeftHandMirror);
        SetIK(RightHand, RightHandMirror);
    }

    private void SetIK(UMI3DClientUserTrackingBone original, GameObject copy)
    {
        var (pos, rot) = (copy.transform.position, copy.transform.rotation);

        animator.SetIKPositionWeight(goals[original.boneType], 1);
        animator.SetIKPosition(goals[original.boneType], pos);
        //animator.SetIKRotationWeight(goals[original.boneType], 1);
        //animator.SetIKRotation(goals[original.boneType], rot);
    }

    //private void Copy(UMI3DClientUserTrackingBone bone, GameObject copy)
    //{
    //    var (pos, rot) = Mirror(bone.transform);
    //    copy.transform.SetPositionAndRotation(pos, rot);
    //}

    private void Copy(UMI3DClientUserTrackingBone bone, GameObject copy)
    {
        //var (pos, rot) = Mirror(bone.transform);
        var posInc = bone.transform.position - hips.transform.position + new Vector3(0, hips.transform.position.y, 0);
        //var rotInc =  Quaternion.Inverse(hips.transform.rotation) * bone.transform.rotation;
        copy.transform.position = HipsMirror.transform.position + posInc;
        copy.transform.rotation = bone.transform.rotation;
        //copy.transform.SetPositionAndRotation(copy.transform.position + posInc, bone.transform.rotation);
    }

    private (Vector3 pos, Quaternion rot) Mirror(Transform transform)
    {
        (Vector3 pos, Quaternion rot) result;

        result.pos = transform.position;
        result.pos.z = -transform.position.z;
        result.rot = transform.rotation;
        //result.rot = Quaternion.Euler(transform.rotation.eulerAngles.x -90, transform.rotation.eulerAngles.y , transform.rotation.eulerAngles.z);
        //transform.localScale = new Vector3(1, 1, -1);
        return result;
    }
}
