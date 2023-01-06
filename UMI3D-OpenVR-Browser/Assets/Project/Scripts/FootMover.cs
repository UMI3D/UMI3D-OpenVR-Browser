using System.Collections;
using System.Collections.Generic;
using System.Data;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using umi3dVRBrowsersBase.ikManagement;
using Unity.Barracuda;
using UnityEngine;

public class FootMover : MonoBehaviour
{
    public NNModel hipsPredictorModel;

    private VirtualObjectBodyInteraction LeftFoot;
    private VirtualObjectBodyInteraction RightFoot;
    private VirtualObjectBodyInteraction LeftHand;
    private VirtualObjectBodyInteraction RightHand;

    private UMI3DClientUserTrackingBone head;

    HipsPredictor hipsPredictor;

    public GameObject hipsPredictedMarker;

    void Start()
    {
        var objects = new List<VirtualObjectBodyInteraction>(FindObjectsOfType<VirtualObjectBodyInteraction>());
        var bones = new List<UMI3DClientUserTrackingBone>(FindObjectsOfType<UMI3DClientUserTrackingBone>());
        LeftFoot = objects.Find(x => x.goal == AvatarIKGoal.LeftFoot);
        RightFoot = objects.Find(x => x.goal == AvatarIKGoal.RightFoot);
        LeftHand = objects.Find(x => x.goal == AvatarIKGoal.LeftHand);
        RightHand = objects.Find(x => x.goal == AvatarIKGoal.RightHand);
        head = bones.Find(x => x.boneType == BoneType.Head);

        hipsPredictor = new HipsPredictor(hipsPredictorModel);

        //var frequency = 1f / 30f;
        //InvokeRepeating(nameof(UpdateHips), 0, frequency);
    }

    void Update()
    {
        //LeftFoot.transform.position = new Vector3(LeftHand.transform.position.x, LeftFoot.transform.position.y, LeftHand.transform.position.z);
        //RightFoot.transform.position = new Vector3(RightHand.transform.position.x, RightFoot.transform.position.y, RightHand.transform.position.z);
        UpdateHips();
    }

    void OnApplicationQuit()
    {
        hipsPredictor.Clean();
    }

    public void UpdateHips()
    {
        hipsPredictor.AddFrameInput(HipsPredictor.FormatInputTensor(head.transform, RightHand.transform, RightFoot.transform));        
        var pred = hipsPredictor.GetPrediction();
        hipsPredictedMarker.transform.rotation = pred;
    }


}
