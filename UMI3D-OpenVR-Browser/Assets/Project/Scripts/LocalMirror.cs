
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

    private UMI3DClientUserTrackingBone hips;
    private GameObject HipsMirror;

    private Dictionary<string, (GameObject go, UMI3DClientUserTrackingBone bone)> mirrorLimbs 
        = new Dictionary<string, (GameObject go, UMI3DClientUserTrackingBone bone)>();

    private Dictionary<uint, AvatarIKGoal> goals = new Dictionary<uint, AvatarIKGoal>()
    {
        { BoneType.LeftHand, AvatarIKGoal.LeftHand },
        { BoneType.RightHand, AvatarIKGoal.RightHand },
        { BoneType.LeftToeBase, AvatarIKGoal.LeftFoot },
        { BoneType.RightToeBase, AvatarIKGoal.RightFoot },
    };

    private List<string> limbsToTrack = new List<string>()
    {
        "Spine",
        "Spine1",
        "Spine2",


        "LeftShoulder",
        "LeftArm",
        "LeftForeArm",
        "LeftHand",

        "RightShoulder",
        "RightArm",
        "RightForeArm",
        "RightHand",

        "Neck",
        "Head"
    };

    public void Start()
    {
        var bones = new List<UMI3DClientUserTrackingBone>(FindObjectsOfType<UMI3DClientUserTrackingBone>());
        hips = bones.Find(x => x.boneType == BoneType.Hips);
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
            if (mirrorAvatar.transform.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="mixamorig:LeftFoot") == default)
                return;

            avatar = FindObjectsOfType<UMI3DClientUserTrackingBone>()
                        .Where(x=>x.boneType == BoneType.CenterFeet)
                        .FirstOrDefault()
                        .gameObject;

            mirrorAvatar.transform.localScale = avatar.transform.localScale;
            
            GetMirrorLimbs();
            isMirrorLoaded = true;
        }
        else
        {
            MoveLimbsNoIK();
            MoveLimbs();
        }
    }

    private void GetMirrorLimbs()
    {
        string mixamoBase = "mixamorig:";
        var childrenInMirror = mirrorAvatar.GetComponentsInChildren<Transform>();
        var childrenInOriginal = avatar.GetComponentsInChildren<UMI3DClientUserTrackingBone>();
        (GameObject go, UMI3DClientUserTrackingBone bone) GetLimb(string name)
        {
            var type = childrenInOriginal.FirstOrDefault(x => x.name == mixamoBase + name);
            var go = childrenInMirror.FirstOrDefault(x => x.gameObject.name == mixamoBase + name)?.gameObject;
            return (go, type);
        }

        foreach (var limbName in limbsToTrack)
        {
            mirrorLimbs.Add(limbName, GetLimb(limbName));
        }

        HipsMirror = GetLimb("Hips").go.transform.parent.parent.gameObject;
    }

    public void MoveLimbsNoIK()
    {
        HipsMirror.transform.position = new Vector3(hips.transform.position.x, 0, hips.transform.position.z);
        HipsMirror.transform.rotation = hips.transform.rotation;
        HipsMirror.transform.position = new Vector3(hips.transform.position.x, 0, -hips.transform.position.z);
    }

    public void MoveLimbs()
    {
        if (!isMirrorLoaded)
            return;
        //HipsMirror.transform.localScale = new Vector3(1, 1, -1);
        foreach(var limbMirrored in mirrorLimbs.Values)
        {
            var (pos, rot) = Copy(limbMirrored.bone, limbMirrored.go);
            limbMirrored.go.transform.localRotation = rot;
        }
    }


    private (Vector3 pos, Quaternion rot) Copy(UMI3DClientUserTrackingBone bone, GameObject copy)
    {
        //var posInc = bone.transform.position - hips.transform.position + new Vector3(0, hips.transform.position.y, 0);
        //Vector3 pos = HipsMirror.transform.position + posInc;
        Vector3 pos = copy.transform.position;
        Quaternion rot = bone.transform.localRotation;

        return (pos, rot);
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
