using System.Collections.Generic;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using umi3dVRBrowsersBase.ikManagement;
using Unity.Barracuda;
using UnityEngine;

public class FootMover : MonoBehaviour
{
    private HipsPredictor hipsPredictor;
    public NNModel hipsPredictorModel;
    public GameObject hipsPredictedMarker;

    private HipsPredictor feetPredictor;
    public NNModel feetPredictorModel;
    public GameObject lFootPredictedMarker;
    public GameObject rFootPredictedMarker;

    private VirtualObjectBodyInteraction LeftFoot;
    private VirtualObjectBodyInteraction RightFoot;
    private VirtualObjectBodyInteraction LeftHand;
    private VirtualObjectBodyInteraction RightHand;

    private UMI3DClientUserTrackingBone head;


    private void Start()
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

    protected void Update()
    {
        //LeftFoot.transform.position = new Vector3(LeftHand.transform.position.x, LeftFoot.transform.position.y, LeftHand.transform.position.z);
        //RightFoot.transform.position = new Vector3(RightHand.transform.position.x, RightFoot.transform.position.y, RightHand.transform.position.z);
        UpdateHips();
    }

    private void OnApplicationQuit()
    {
        hipsPredictor.Clean();
    }

    public void UpdateHips()
    {
        hipsPredictor.AddFrameInput(HipsPredictor.FormatInputTensor(head.transform, RightHand.transform, LeftHand.transform));
        var pred = hipsPredictor.GetPrediction();
        hipsPredictedMarker.transform.position = pred.pos;
        hipsPredictedMarker.transform.rotation = pred.rot;
    }

    public void UpdateFeet()
    {
        //hipsPredictor.AddFrameInput(HipsPredictor.FormatInputTensor(head.transform, RightHand.transform, LeftHand.transform));
        //var pred = hipsPredictor.GetPrediction();
        //lFootPredictedMarker.transform.position = pred.Item1;
        //rFootPredictedMarker.transform.position = pred.Item2;
    }
}