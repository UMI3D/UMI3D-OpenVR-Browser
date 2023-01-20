using System.Collections.Generic;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using Unity.Barracuda;
using UnityEngine;

public class LegsMover : MonoBehaviour
{
    // HIPS
    private HipsPredictor hipsPredictor;

    [Header("Hips prediction")]
    public NNModel hipsPredictorModelV1;

    public NNModel hipsPredictorModelV3;
    public GameObject hipsPredictedMarker;

    public enum HipsModelToUse
    { V1, V3 }

    public HipsModelToUse hipsModelToUse;

    private (Vector3 pos, Quaternion rot) hipsPredicted;

    // LEGS

    [Header("LoBSTr")]
    public bool shouldPredictLegs;

    private FootPredictor legsPredictor;
    public NNModel legsPredictorModel;
    public GameObject lFootPredictedMarker;
    public GameObject rFootPredictedMarker;

    private (float rightfoot, float leftfoot) contact = new ();

    // references to skeleton joints
    private Dictionary<HumanBodyBones, UMI3DClientUserTrackingBone> jointReferences = new();

    private void Start()
    {
        // gather all bones and reference them by Unity bonetype (for use in a dictionnary)
        var bones = new List<UMI3DClientUserTrackingBone>(FindObjectsOfType<UMI3DClientUserTrackingBone>());
        foreach (var bone in bones)
        {
            var boneType = BoneTypeConverter.ConvertToBoneType(bone.boneType);

            if (boneType.HasValue)
                jointReferences.Add(boneType.Value, bone);
        }

        // Init hips predictor
        if (hipsModelToUse == HipsModelToUse.V1)
            hipsPredictor = new HipsPredictor(hipsPredictorModelV1);
        else
            hipsPredictor = new HipsPredictorV3(hipsPredictorModelV3);

        hipsPredictor.Init();

        // Init legs predictor
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
        if (hipsPredictor.isTensorFull)
            UpdateFeet();

        static string PrintRotPos(string name, Transform t) => $"{name} \t | \t pos: {t.position}, \t rot {t.rotation.eulerAngles}," +
                                                                $" \t posRel: {t.localPosition}, \t rotRel: {t.localRotation.eulerAngles}\n";

        var message = "=== INPUT === \n" +
                      PrintRotPos("HEAD", jointReferences[HumanBodyBones.Head].transform) +
                      PrintRotPos("R_HAND", jointReferences[HumanBodyBones.RightHand].transform) +
                      PrintRotPos("L_HAND", jointReferences[HumanBodyBones.LeftHand].transform) +
                      "=== OUTPUT === \n" +
                      PrintRotPos("HIPS", hipsPredictedMarker.transform) +
                      PrintRotPos("R_FOOT", rFootPredictedMarker.transform) +
                      PrintRotPos("L_FOOT", lFootPredictedMarker.transform) +
                      $"R_FOOT_CONTACT {contact.rightfoot} \n" +
                      $"L_FOOT_CONTACT {contact.leftfoot} \n";

        Debug.Log(message);
    }

    /// <summary>
    /// Compute Hips prediction and place marker
    /// </summary>
    public void UpdateHips()
    {
        // get prediction of hips rotatio through hips predictor
        hipsPredictor.AddFrameInput(hipsPredictor.FormatInputTensor(jointReferences[HumanBodyBones.Head].transform,
                                                                    jointReferences[HumanBodyBones.RightHand].transform,
                                                                    jointReferences[HumanBodyBones.LeftHand].transform,
                                                                    jointReferences[HumanBodyBones.Hips].transform));

        if (!hipsPredictor.isTensorFull) // force to wait 45 frames
            return;

        hipsPredicted = hipsPredictor.GetPrediction();

        // apply predicted hips global rotation
        hipsPredictedMarker.transform.rotation = hipsPredicted.rot;
        hipsPredictedMarker.transform.position = hipsPredicted.pos; //! should be relative ? Try in (0,0,0) | maybe process data with a reference point at the projected position of the headset onto the ground
    }

    /// <summary>
    /// Order in which to apply foward kinematics
    /// </summary>
    private readonly HumanBodyBones[] orderToApplyFK = new HumanBodyBones[8]
    {
        HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, HumanBodyBones.RightToes,
        HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes,
    };

    /// <summary>
    /// Compute Legs prediction and place marker
    /// </summary>
    public void UpdateFeet()
    {
        // get prediction of position of legs joints from LoBSTr
        legsPredictor.AddFrameInput(legsPredictor.FormatInputTensor(jointReferences[HumanBodyBones.Head].transform,
                                                                    jointReferences[HumanBodyBones.RightHand].transform,
                                                                    jointReferences[HumanBodyBones.LeftHand].transform,
                                                                    hipsPredicted));
        if (!legsPredictor.isTensorFull) // force to wait 45 frames
            return;

        var (rotations, contact) = legsPredictor.GetPrediction();
        this.contact = contact; 

        // apply global positoon and hips offset (forward kinematics)
        foreach (var joint in orderToApplyFK)
            jointReferences[joint].transform.localRotation = rotations[joint];

        lFootPredictedMarker.transform.position = jointReferences[HumanBodyBones.LeftToes].transform.position;
        rFootPredictedMarker.transform.position = jointReferences[HumanBodyBones.RightToes].transform.position;

    }

    private void OnApplicationQuit()
    {
        hipsPredictor?.Clean();
        legsPredictor?.Clean();
    }
}