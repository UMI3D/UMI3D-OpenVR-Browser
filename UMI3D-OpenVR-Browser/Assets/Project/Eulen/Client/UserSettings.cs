using com.inetum.addonEulen.common.dtos;
using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using Valve.VR;

/// <summary>
/// Displays a panel to set up a user before recording his movements.
/// </summary>
public class UserSettings : MonoBehaviour
{
    public static UserSettings instance = null;

    #region Fields

    /// <summary>
    /// Match between user's trackers and avatar'bones.
    /// </summary>
    public List<TrackerToBone> trackersToBones = new List<TrackerToBone>();

    /// <summary>
    /// Stores rotations to go from a tracker referential to a bone referential.
    /// </summary>
    private Dictionary<SteamVR_Input_Sources, Quaternion> trackerToBoneRotations = new Dictionary<SteamVR_Input_Sources, Quaternion>();

    /// <summary>
    /// Stores all offset between   
    /// </summary>
    private UserSettingsDto userSettings = new UserSettingsDto();

    private bool isDisplayed = true;

    public SteamVR_Action_Boolean triggerAction;

    private Action onSuccess;

    #region UI

    [SerializeField]
    private GameObject panel = null;

    [SerializeField]
    private Transform sliderValueFeedback = null;

    private float sliderValue = 0;

    private float SliderValue
    {
        get => sliderValue;
        set
        {
            sliderValue = Mathf.Clamp(value, 0, maxSliderValue);
            sliderValueFeedback.localScale = new Vector3(sliderValueFeedback.localScale.x, sliderValue / maxSliderValue, sliderValueFeedback.localScale.z);
        }
    }

    private float maxSliderValue = 100;

    #endregion

    #endregion

    #region Methods

    [ContextMenu("Display")]
    public void DisplaySettings(Action onSuccess)
    {
        if (isDisplayed)
            return;

        isDisplayed = true;

        panel.SetActive(true);

        SliderValue = 0;

        this.onSuccess = onSuccess;
    }

    [ContextMenu("Hide")]
    public void HideSettings()
    {
        if (!isDisplayed)
            return;

        isDisplayed = false;

        panel.SetActive(false);

        this.onSuccess = null;
    }

    #region Monobehavior callback

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        HideSettings();
    }

    private void Update()
    {
        if (isDisplayed)
        {
            if (triggerAction.GetState(SteamVR_Input_Sources.Any))
            {
                SliderValue += 50f * Time.deltaTime;

            }

            if (Mathf.Abs(sliderValue - maxSliderValue) < 1f)
            {
                Init();
            }
        }
    }

    #endregion

    /// <summary>
    /// Inits <see cref="userSettings"/>.
    /// </summary>
    private void Init()
    {
        Debug.Log("Init");

        userSettings = new UserSettingsDto();

        trackerToBoneRotations.Clear();

        // 1. Compute rotations from tracker frame of reference to bone frame.
        foreach (var entry in trackersToBones)
        {
            Quaternion offset = Quaternion.Inverse(entry.tracker.rotation) * entry.bone.rotation;
            trackerToBoneRotations[entry.source] = offset;

            Debug.Log(entry.source + " -> " + offset.eulerAngles);

            if (entry.source == SteamVR_Input_Sources.Head)
            {
                userSettings.cameraHeight = entry.tracker.position.y;
                Debug.Log("HEIGHT " + entry.tracker.position.y);
            }
            else if (entry.source == SteamVR_Input_Sources.RightFoot)
            {
                userSettings.offsetFromGround = entry.tracker.position.y;
                Debug.Log("GROUND OFFSET " + userSettings.offsetFromGround);
            }
        }

        var copy = new Dictionary<int, Vector4Dto>();
        foreach (var item in trackerToBoneRotations)
        {
            copy[(int)item.Key] = item.Value.Dto();
        }

        userSettings.trackerToBoneRotations = copy;

        // 2. Compute offsets
        userSettings.boneOffsets = GetBoneOffsets();

        userSettings.boneLenghts = GetBonesLenghts();

        onSuccess?.Invoke();

        HideSettings();
    }

    /// <summary>
    /// Compute offsets between trackers and "real bone centers".
    /// </summary>
    /// <returns></returns>
    private Dictionary<int, float> GetBoneOffsets()
    {
        var boneOffsets = new Dictionary<int, float>();

        /*var waist = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.Waist).tracker.transform;
        var rightKnee = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.RightKnee).tracker.transform;
        var rightFoot = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.RightFoot).tracker.transform;
        var chest = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.Chest).tracker.transform;

        var kneeToWaist = rightKnee.position - waist.position;
        kneeToWaist = Vector3.ProjectOnPlane(kneeToWaist, Vector3.up);
        var footToWaist = rightFoot.position - waist.position;
        footToWaist = Vector3.ProjectOnPlane(footToWaist, Vector3.up);
        var chestToWaist = chest.position - waist.position;
        chestToWaist = Vector3.ProjectOnPlane(chestToWaist, Vector3.up);

        var average = (kneeToWaist.z + footToWaist.z + chestToWaist.z) / 4f;

        Debug.Log("AVERAGE " + average);
        Debug.Log("Knee offset" + (kneeToWaist.z - average));
        Debug.Log("Feet offset" + (footToWaist.z - average));
        Debug.Log("Chest offset" + (chestToWaist.z - average));
        Debug.Log("waist offset" + (average));

        boneOffsets[SteamVR_Input_Sources.RightKnee] = boneOffsets[SteamVR_Input_Sources.LeftKnee] = Mathf.Abs(kneeToWaist.z - average);
        boneOffsets[SteamVR_Input_Sources.RightFoot] = boneOffsets[SteamVR_Input_Sources.LeftFoot] = Mathf.Abs(footToWaist.z - average);
        boneOffsets[SteamVR_Input_Sources.Chest]  = Mathf.Abs(chestToWaist.z - average);
        boneOffsets[SteamVR_Input_Sources.Waist]  = Mathf.Abs( average);*/

        var waist = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.Waist).tracker.transform;
        var rightKnee = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.RightKnee).tracker.transform;
        var rightFoot = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.RightFoot).tracker.transform;
        var rightShoulder = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.RightShoulder).tracker.transform;
        var leftShoulder = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.LeftShoulder).tracker.transform;

        var kneeToWaist = rightKnee.position - waist.position;
        kneeToWaist = Vector3.ProjectOnPlane(kneeToWaist, Vector3.up);
        var footToWaist = rightFoot.position - waist.position;
        footToWaist = Vector3.ProjectOnPlane(footToWaist, Vector3.up);
        var rightShoulderToWaist = rightShoulder.position - waist.position;
        rightShoulderToWaist = Vector3.ProjectOnPlane(rightShoulderToWaist, Vector3.up);
        var leftShoulderToWaist = leftShoulder.position - waist.position;
        leftShoulderToWaist = Vector3.ProjectOnPlane(leftShoulderToWaist, Vector3.up);

        var average = (rightShoulderToWaist.z + leftShoulderToWaist.z) / 2f;

        /*Debug.Log("AVERAGE " + average);
        Debug.Log("Knee offset" + (kneeToWaist.z - average));
        Debug.Log("Feet offset" + (footToWaist.z - average));
        Debug.Log("waist offset" + (average));*/

        boneOffsets[(int)SteamVR_Input_Sources.RightKnee] = boneOffsets[(int)SteamVR_Input_Sources.LeftKnee] = Mathf.Abs(kneeToWaist.z - average);
        boneOffsets[(int)SteamVR_Input_Sources.RightFoot] = boneOffsets[(int)SteamVR_Input_Sources.LeftFoot] = Mathf.Abs(footToWaist.z - average);
        boneOffsets[(int)SteamVR_Input_Sources.Waist] = Mathf.Abs(average);

        return boneOffsets;
    }

    /// <summary>
    /// Compute bone lenghts if their scale must be different from one.
    /// </summary>
    /// <returns></returns>
    private Dictionary<int, float> GetBonesLenghts()
    {
        var boneLenghts = new Dictionary<int, float>();

        var rightShoulder = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.RightShoulder).tracker.transform;
        var rightElbow = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.RightElbow).tracker.transform;
        var rightHand = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.RightHand).tracker.transform;
        var leftShoulder = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.LeftShoulder).tracker.transform;
        var leftElbow = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.LeftElbow).tracker.transform;
        var leftHand = trackersToBones.Find(e => e.source == SteamVR_Input_Sources.LeftHand).tracker.transform;

        boneLenghts[(int)SteamVR_Input_Sources.RightShoulder] = 0.69f * Vector3.Distance(rightShoulder.transform.position, rightElbow.transform.position);
        boneLenghts[(int)SteamVR_Input_Sources.RightElbow] = 0.64f * Vector3.Distance(rightHand.transform.position, rightElbow.transform.position);
        boneLenghts[(int)SteamVR_Input_Sources.LeftShoulder] = 0.69f * Vector3.Distance(leftShoulder.transform.position, leftElbow.transform.position);
        boneLenghts[(int)SteamVR_Input_Sources.LeftElbow] = 0.64f * Vector3.Distance(leftHand.transform.position, leftElbow.transform.position);

        Debug.Log("Right arm " + boneLenghts[(int)SteamVR_Input_Sources.RightShoulder]);
        Debug.Log("Left arm " + boneLenghts[(int)SteamVR_Input_Sources.LeftShoulder]);

        Debug.Log("Right forearm " + boneLenghts[(int)SteamVR_Input_Sources.RightElbow]);
        Debug.Log("Left forearm " + boneLenghts[(int)SteamVR_Input_Sources.LeftElbow]);

        return boneLenghts;
    }

    /// <summary>
    /// Returns rotation to go from a tracker referential to a bone referential.
    /// bone_rotation = tracker_rotation * [res].
    /// </summary>
    /// <param name="tracker"></param>
    /// <returns></returns>
    public Quaternion GetTrackerToBoneRotation(SteamVR_Input_Sources tracker)
    {
        return trackerToBoneRotations.ContainsKey(tracker) ? trackerToBoneRotations[tracker] : Quaternion.identity;
    }

    public UserSettingsDto GetUserSettingsData()
    {
        return userSettings;
    }

    #endregion
}

[Serializable]
public class TrackerToBone
{
    public SteamVR_Input_Sources source;

    public Transform tracker;

    public Transform bone;
}