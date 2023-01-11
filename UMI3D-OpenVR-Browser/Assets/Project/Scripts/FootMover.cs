using System.Collections.Generic;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using umi3dVRBrowsersBase.ikManagement;
using Unity.Barracuda;
using UnityEngine;

public class FootMover : MonoBehaviour
{
    private HipsPredictor hipsPredictor;
    [Header("Hips prediction")]
    public NNModel hipsPredictorModelV1;
    public NNModel hipsPredictorModelV3;
    public GameObject hipsPredictedMarker;

    public enum ModelToUse
    {
        V1,
        V3
    }

    public ModelToUse modelToUse;

    private HipsPredictor feetPredictor;
    [Header("LoBSTr")]
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

        if (modelToUse == ModelToUse.V1)
            hipsPredictor = new HipsPredictor(hipsPredictorModelV1);
        else
            hipsPredictor = new HipsPredictorV3(hipsPredictorModelV3);

        

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
        hipsPredictor.AddFrameInput(hipsPredictor.FormatInputTensor(head.transform, RightHand.transform, LeftHand.transform));
        var pred = hipsPredictor.GetPrediction();
        //hipsPredictedMarker.transform.position = pred.pos;
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