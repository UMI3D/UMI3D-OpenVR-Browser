using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using UnityEngine;

public class LocalMirror : MonoBehaviour
{
    private GameObject avatar;

    private GameObject mirrorAvatar;

    private UMI3DClientUserTrackingBone hips;
    private GameObject HipsMirror;

    private Dictionary<string, (GameObject go, UMI3DClientUserTrackingBone bone)> mirrorLimbs
        = new Dictionary<string, (GameObject go, UMI3DClientUserTrackingBone bone)>();

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
            if (mirrorAvatar.transform.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "mixamorig:LeftFoot") == default)
                return;

            avatar = FindObjectsOfType<UMI3DClientUserTrackingBone>()
                        .Where(x => x.boneType == BoneType.CenterFeet)
                        .FirstOrDefault()
                        .gameObject;

            mirrorAvatar.transform.localScale = avatar.transform.localScale;

            GetMirrorLimbs();
            isMirrorLoaded = true;
        }
        else
            MoveLimbs();
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

    public void MoveLimbs()
    {
        if (!isMirrorLoaded)
            return;

        HipsMirror.transform.position = new Vector3(hips.transform.position.x, 0, hips.transform.position.z);
        HipsMirror.transform.rotation = hips.transform.rotation;
        HipsMirror.transform.position = new Vector3(hips.transform.position.x, 0, -hips.transform.position.z);

        foreach (var limbMirrored in mirrorLimbs.Values)
            limbMirrored.go.transform.localRotation = limbMirrored.bone.transform.localRotation;
    }
}