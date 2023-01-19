using System.Collections.Generic;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using umi3dVRBrowsersBase.ikManagement;
using Unity.Barracuda;
using UnityEngine;

public class FootMover : MonoBehaviour
{
    // HIPS
    private HipsPredictor hipsPredictor;
    [Header("Hips prediction")]
    public NNModel hipsPredictorModelV1;
    public NNModel hipsPredictorModelV3;
    public GameObject hipsPredictedMarker;

    public enum HipsModelToUse { V1, V3 }
    public HipsModelToUse hipsModelToUse;

    private (Vector3 pos, Quaternion rot) hipsPredicted;

    // LEGS

    [Header("LoBSTr")]
    public bool shouldPredictLegs;
    private FootPredictor legsPredictor;
    public NNModel legsPredictorModel;
    public GameObject lFootPredictedMarker;
    public GameObject rFootPredictedMarker;


    private Dictionary<HumanBodyBones, UMI3DClientUserTrackingBone> jointReferences = new();

    private void Start()
    {
        var bones = new List<UMI3DClientUserTrackingBone>(FindObjectsOfType<UMI3DClientUserTrackingBone>());

        foreach (var bone in bones)
        {
            var boneType = BoneTypeConverter.ConvertToBoneType(bone.boneType);

            if (boneType.HasValue)
                 jointReferences.Add(boneType.Value, bone);
        }

        if (hipsModelToUse == HipsModelToUse.V1)
            hipsPredictor = new HipsPredictor(hipsPredictorModelV1);
        else
            hipsPredictor = new HipsPredictorV3(hipsPredictorModelV3);

        hipsPredictor.Init();

        if (shouldPredictLegs)
        {
            legsPredictor = new FootPredictor(legsPredictorModel);
            legsPredictor.Init();
        }

        /* to use to lower the frequency of updates, at the cost of not being synchronized with Unity lifecycle */
        //var frequency = 1f / 30f;
        //InvokeRepeating(nameof(UpdateHips), 0, frequency);
    }

    protected void Update()
    {
        UpdateHips();
        UpdateFeet();

        var message = $"HEAD pos: {jointReferences[HumanBodyBones.Head].transform.position}, rot {jointReferences[HumanBodyBones.Head].transform.rotation.eulerAngles} \n" +
            $"HIPS pred pos: {hipsPredicted.pos}, rot {hipsPredicted.rot.eulerAngles} \n" +
            $"RFoot pred pos: {rFootPredictedMarker.transform.position} \n" +
            $"RFoot comp pos: {jointReferences[HumanBodyBones.RightFoot].transform.position} \n" +
            $"LFoot pred pos: {lFootPredictedMarker.transform.position} \n" +
            $"LFoot comp pos: {jointReferences[HumanBodyBones.LeftFoot].transform.position}";
        Debug.Log(message);
    }

    private void OnApplicationQuit()
    {
        hipsPredictor?.Clean();
        legsPredictor?.Clean();
    }

    public void UpdateHips()
    {
        // get prediction of hips rotatio through hips predictor
        hipsPredictor.AddFrameInput(hipsPredictor.FormatInputTensor(jointReferences[HumanBodyBones.Head].transform,
                                                                    jointReferences[HumanBodyBones.RightHand].transform,
                                                                    jointReferences[HumanBodyBones.LeftHand].transform));
        hipsPredicted = hipsPredictor.GetPrediction();

        // apply predicted hips global rotation
        hipsPredictedMarker.transform.rotation = hipsPredicted.rot;
    }

    /// <summary>
    /// Order in which to apply foward kinematics
    /// </summary>
    private readonly HumanBodyBones[] orderToApply = new HumanBodyBones[6] 
    {
        HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot,
        HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot
    };

    public void UpdateFeet()
    {
        // get prediction of position of legs joints from LoBSTr
        legsPredictor.AddFrameInput(legsPredictor.FormatInputTensor(jointReferences[HumanBodyBones.Head].transform,
                                                                    jointReferences[HumanBodyBones.RightHand].transform,
                                                                    jointReferences[HumanBodyBones.LeftHand].transform,
                                                                    hipsPredicted));
        var lowerBodyPredictionPosition = legsPredictor.GetPrediction();

        // get local rotations from position through foward kinematics
        var lowerBodyPredictionRotation = legsPredictor.ComputeForwardKinematics(hipsPredicted.pos, lowerBodyPredictionPosition.positions);

        // apply local rotations
        foreach (var joint in orderToApply)
            jointReferences[joint].transform.localRotation = lowerBodyPredictionRotation[joint];

        lFootPredictedMarker.transform.position = lowerBodyPredictionPosition.positions[HumanBodyBones.LeftFoot];
        rFootPredictedMarker.transform.position = lowerBodyPredictionPosition.positions[HumanBodyBones.RightFoot];
    }
}