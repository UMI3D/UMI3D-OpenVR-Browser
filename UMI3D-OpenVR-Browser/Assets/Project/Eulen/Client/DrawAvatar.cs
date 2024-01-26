using com.inetum.addonEulen.common.dtos;
using inetum.unityUtils;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Valve.VR;

namespace com.inetum.eulen.recording.app
{
    public class DrawAvatar : SingleBehaviour<DrawAvatar>
    {
        #region Fields

        /// <summary>
        /// Avatar to model.
        /// </summary>
        public GameObject[] avatar;
        private char genre;

        [SerializeField] private GameObject cameraHead;

        /// <summary>
        /// Material to draw lines.
        /// </summary>
        public Material lineMat;

        /// <summary>
        /// Every sphere trackers.
        /// </summary>
        public List<TrackerGizmo> trackers = new List<TrackerGizmo>();

        [Header("Sphere")]
        /// <summary>
        /// Estimated bones
        /// </summary>
        public GameObject rightUpLegSphere;
        public GameObject leftUpLegSphere;
        public GameObject neck;

        [Header("Bones Male")]
        public GameObject leftShoulderBone;
        public GameObject rightShoulderBone;
        public GameObject rightUpLegBone;
        public GameObject leftUpLegBone;
        public GameObject leftElbowBone;
        public GameObject leftHandBone;
        public GameObject rightElbowBone;
        public GameObject rightHandBone;
        public GameObject spine;
        public GameObject spine1;
        public GameObject neckBone;

        [Header("Bones Female")]
        public GameObject leftShoulderBoneF;
        public GameObject rightShoulderBoneF;
        public GameObject rightUpLegBoneF;
        public GameObject leftUpLegBoneF;
        public GameObject leftElbowBoneF;
        public GameObject leftHandBoneF;
        public GameObject rightElbowBoneF;
        public GameObject rightHandBoneF;
        public GameObject spineF;
        public GameObject spine1F;
        public GameObject neckBoneF;

        [Header("Rotation offsets")]
        public Vector3 hipsOffset;
        public Vector3 rightShoulderOffset;
        public Vector3 leftShoulderOffset;
        public Vector3 rightElbowOffset, leftElbowOffset;
        public Vector3 upLegOffset, rightKneeOffset, leftKneeOffset;
        public Vector3 rightHandOffset, leftHandOffset;
        public Vector3 headRotationOffset;

        [Header("Position offsets")]
        public float kneeOffset = .05f;
        public float waistOffset = 0.05f;
        public float chestOffset = 0.075f;
        public float feetOffset = 0.05f;
        public float headOffset = 0.05f;

        [Header("Angle Tags")]
        public AngleTag angKneeR;
        public AngleTag angKneeL;
        public AngleTag angElbowR;
        public AngleTag angElbowL;
        public AngleTag angHips;
        public AngleTag angWaist;

        [Space(16f)]
        public Transform BoxReplay;
        [SerializeField] private LogsManager logsManager;

        /// <summary>
        /// Save the box position to compare as previous frame
        /// </summary>
        private Vector3 boxPosAux;
        /// <summary>
        /// Is the box "attached to the user"
        /// </summary>
        private bool boxAttached = false;

        private bool isRotationSetup = false;

        /// <summary>
        /// Is replay playing
        /// </summary>
        public bool IsPlaying { get; private set; }

        public delegate void OnFrameChangedDelegate(int frame);
        public event OnFrameChangedDelegate OnFrameChanged;

        #region Display options

        private bool displayWireBody = false;

        /// <summary>
        /// Display wire body avatar ?
        /// </summary>
        public bool DisplayWireBody
        {
            get => displayWireBody;
            set
            {
                displayWireBody = value;

                DisplayTrackerSpheres();
            }
        }

        #endregion

        #region Private Data

        /// <summary>
        /// Stores all gameobjects which represents Vive trackers.
        /// </summary>
        Dictionary<SteamVR_Input_Sources, Transform> trackersDico = new Dictionary<SteamVR_Input_Sources, Transform>();

        /// <summary>
        /// Direct acces of some transforms of <see cref="trackersDico"/>.
        /// </summary>
        Transform waistTracker, rightKneeTracker, leftKneeTracker;

        Dictionary<SteamVR_Input_Sources, Transform> bonesDico = new Dictionary<SteamVR_Input_Sources, Transform>();

        Coroutine replayCoroutine;

        /// <summary>
        /// Gizmos to display angles.
        /// </summary>
        private AngleGizmo rightKneeGizmo, leftKneeGizmo, hipsGizmo, leftElbowGizmo, rightElbowGizmo, waistGizmo;   //W

        [SerializeField]
        private MovementValidator validator = null;

        #endregion

        #endregion

        #region Methods

        #region Monobehavior callbacks

        private void Start()
        {
            foreach (var tracker in trackers)
            {
                trackersDico[tracker.source] = tracker.trans;
            }

            /*foreach (var tracker in UserSettings.instance.trackersToBones)
            {
                bonesDico[tracker.source] = tracker.bone;
            }*/

            waistTracker = trackersDico[SteamVR_Input_Sources.Waist];
            rightKneeTracker = trackersDico[SteamVR_Input_Sources.RightKnee];
            leftKneeTracker = trackersDico[SteamVR_Input_Sources.LeftKnee];

            rightKneeGizmo = new AngleGizmo { size = .1f, name = "RK" };
            leftKneeGizmo = new AngleGizmo { size = .1f, name = "LK" };
            leftElbowGizmo = new AngleGizmo { size = .1f, name = "LE" };
            rightElbowGizmo = new AngleGizmo { size = .1f, name = "RE" };
            waistGizmo = new AngleGizmo { size = .1f, name = "W" };
            hipsGizmo = new AngleGizmo { size = .1f, displayTotalCircle = true, name = "H" };
            rightKneeGizmo.Enabled = leftKneeGizmo.Enabled = hipsGizmo.Enabled = leftElbowGizmo.Enabled = rightElbowGizmo.Enabled = waistGizmo.Enabled = false; //W

            AngleGizmoManager.AddGizmo(rightKneeGizmo);
            AngleGizmoManager.AddGizmo(leftKneeGizmo);
            AngleGizmoManager.AddGizmo(hipsGizmo);
            AngleGizmoManager.AddGizmo(rightElbowGizmo);
            AngleGizmoManager.AddGizmo(leftElbowGizmo);
            AngleGizmoManager.AddGizmo(waistGizmo); //W
        }

        void OnEnable()
        {
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        void OnDisable()
        {
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            OnPostRender();
        }

        /// <summary>
        /// Renders wire body.
        /// </summary>
        void OnPostRender()
        {
            if (!DisplayWireBody)
                return;

            if (!lineMat)
            {
                Debug.LogError("Please Assign a material on the inspector");
                return;
            }

            GL.PushMatrix();

            lineMat.SetPass(0);
            GL.MultMatrix(Matrix4x4.identity);

            GL.Begin(GL.LINES);
            GL.Color(Color.red);

            GL.Vertex(trackersDico[SteamVR_Input_Sources.RightFoot].position);
            GL.Vertex(rightKneeTracker.position);

            GL.Vertex(waistTracker.position);
            GL.Vertex(rightKneeTracker.position);

            GL.Vertex(trackersDico[SteamVR_Input_Sources.LeftFoot].position);
            GL.Vertex(leftKneeTracker.position);

            GL.Vertex(waistTracker.position);
            GL.Vertex(leftKneeTracker.position);

            GL.Vertex(waistTracker.position);
            GL.Vertex(neck.transform.position);

            GL.Vertex(neck.transform.position);
            GL.Vertex(trackersDico[SteamVR_Input_Sources.RightShoulder].position);

            GL.Vertex(trackersDico[SteamVR_Input_Sources.RightShoulder].position);
            GL.Vertex(trackersDico[SteamVR_Input_Sources.RightElbow].position);

            GL.Vertex(trackersDico[SteamVR_Input_Sources.RightHand].position);
            GL.Vertex(trackersDico[SteamVR_Input_Sources.RightElbow].position);

            GL.Vertex(neck.transform.position);
            GL.Vertex(trackersDico[SteamVR_Input_Sources.LeftShoulder].position);

            GL.Vertex(trackersDico[SteamVR_Input_Sources.LeftShoulder].position);
            GL.Vertex(trackersDico[SteamVR_Input_Sources.LeftElbow].position);

            GL.Vertex(trackersDico[SteamVR_Input_Sources.LeftHand].position);
            GL.Vertex(trackersDico[SteamVR_Input_Sources.LeftElbow].position);

            GL.Vertex(neck.transform.position);
            GL.Vertex(trackersDico[SteamVR_Input_Sources.Head].position);

            GL.End();

            AngleGizmoManager.instance.OnEndCameraRendering();

            GL.PopMatrix();
        }

        #endregion

        #region Replay

        /// <summary>
        /// Replays a whole tracking record.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public void Replay(RecordDto data, bool displayAvatar, char genre, int offset = 0)
        {
            if (IsPlaying)
                StopReplay();

            if (!IsPlaying)
            {
                if (replayCoroutine != null)
                    StopCoroutine(replayCoroutine);

                this.genre = genre;
                if (genre == 'm')
                {
                    avatar[0].SetActive(displayAvatar);
                    avatar[1].SetActive(false);
                    foreach (var tracker in UserSettings.instance.trackersToBones) bonesDico[tracker.source] = tracker.bone;
                }
                else
                {
                    avatar[1].SetActive(displayAvatar);
                    avatar[0].SetActive(false);
                    foreach (var tracker in UserSettings.instance.trackersToBones) bonesDico[tracker.source] = tracker.boneF;
                }

                BoxReplay.gameObject.SetActive(true);
                DisplayWireBody = !displayAvatar;

                rightKneeGizmo.Enabled = leftKneeGizmo.Enabled = hipsGizmo.Enabled = leftElbowGizmo.Enabled = rightElbowGizmo.Enabled = waistGizmo.Enabled = !displayAvatar;
                replayCoroutine = StartCoroutine(ReplayCoroutine(data, offset));
            }
        }

        private int errorFrames = 30; // 30 frames = 1 seg

        /// <summary>
        /// Replays the exercise (PRL - USER)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private IEnumerator ReplayCoroutine(RecordDto data, int offset = 0)
        {
            if (offset >= data.frames.Count && offset >= 0)
            {
                Debug.LogError("Offset too high for this data : " + offset + ", there are only " + data.frames.Count + " frames");
            }
            else
            {
                Debug.Log("Estimated height " + (data.userSettings.cameraHeight * 1.06f));

                IsPlaying = true;

                Debug.Log("Start playing " + data.frames.Count + " frames at " + offset + " frame");

                var wait = new WaitForSeconds(1f / data.recordFps); // 1 sec -> 30 Frames
                bool rightPerform;
                bool wasError = false;

                Debug.Log($"FPS: {data.recordFps}");
                for (int i = offset; i < data.frames.Count; i++)
                {
                    // If it's PRL, replay will be true
                    rightPerform = SetFramePose(data.frames[i], data.userSettings, i); // Replay the whole exercise

                    // Test
                    for (int j = 0; j < MovementCondition.wrongGizmos.Length; j++)
                    {
                        if (MovementCondition.wrongGizmos[j] >= errorFrames)
                        {
                            errorUser = true;
                            errorUserAux[j] = true;
                            if (MovementCondition.wrongGizmos[j] == errorFrames) Debug.Log($"<color=#77ffaa> Error on gizmo: {MovementCondition.gizmosAux[j].name} detected :) \nFrame: {i} </color>");
                        }
                        else
                        {
                            if (errorUserAux[j]) { wasError = true; Debug.Log($"Gizmo now ok: {MovementCondition.gizmosAux[j].name} :) \nFrame: {i}"); }

                            errorUser = false;
                            errorUserAux[j] = false;
                        }

                        // Check if it was an error to show the error few seconds more
                        if ((!errorUser && !errorUserAux[j]) && wasError)
                        {
                            Debug.Log("Displaying the error an additional time...");
                            StartCoroutine(ShowErrorMoreTime(j));

                            wasError = false;
                        }
                    }

                    yield return wait;
                }

                StopReplay();
            }
        }

        /// <summary>
        /// Display a specific record frame.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="trackersToBones"></param>
        /// <param name="i"></param>
        public bool SetFramePose(RecordKeyFrameDto frame, UserSettingsDto userSettings, int i, MovementValidationDto validationDto = null)
        {
            SetAvatarHeight(userSettings.cameraHeight, userSettings.boneLenghts);

            // 1. Sets wire body
            Vector3 _rightFoot = Vector3.zero, _rightKnee = Vector3.zero;

            foreach (var entry in frame.entries)
            {
                trackersDico[(SteamVR_Input_Sources)entry.source].position = entry.position.Struct();
                trackersDico[(SteamVR_Input_Sources)entry.source].rotation = entry.rotation.Quaternion();

                if (userSettings.boneOffsets.ContainsKey(entry.source))
                {
                    trackersDico[(SteamVR_Input_Sources)entry.source].position -= trackersDico[(SteamVR_Input_Sources)entry.source].forward * userSettings.boneOffsets[entry.source];
                }
                if ((SteamVR_Input_Sources)entry.source == SteamVR_Input_Sources.Head)
                {
                    trackersDico[(SteamVR_Input_Sources)entry.source].position -= trackersDico[(SteamVR_Input_Sources)entry.source].forward * headOffset;
                }
                else if ((SteamVR_Input_Sources)entry.source == SteamVR_Input_Sources.Waist)
                {
                    trackersDico[(SteamVR_Input_Sources)entry.source].position -= trackersDico[(SteamVR_Input_Sources)entry.source].forward * userSettings.boneOffsets[entry.source];
                    /*if (replayNum == 0 || replayNum == 2 || replayNum == 4) trackersDico[(SteamVR_Input_Sources)entry.source].position -= trackersDico[(SteamVR_Input_Sources)entry.source].forward * userSettings.boneOffsets[entry.source];
                    else trackersDico[(SteamVR_Input_Sources)entry.source].position -= trackersDico[(SteamVR_Input_Sources)entry.source].forward * waistOffset;*/
                }
                else if ((SteamVR_Input_Sources)entry.source == SteamVR_Input_Sources.LeftKnee)//
                {
                    /*if (replayNum == 1 || replayNum == 3 || replayNum == 5)*/
                    trackersDico[(SteamVR_Input_Sources)entry.source].position += trackersDico[(SteamVR_Input_Sources)entry.source].right * kneeOffset;
                }
                else if ((SteamVR_Input_Sources)entry.source == SteamVR_Input_Sources.RightKnee)//
                {
                    /* if (replayNum == 1 || replayNum == 3 || replayNum == 5)*/
                    trackersDico[(SteamVR_Input_Sources)entry.source].position -= trackersDico[(SteamVR_Input_Sources)entry.source].up * kneeOffset;
                }
                else if ((SteamVR_Input_Sources)entry.source == SteamVR_Input_Sources.Treadmill)
                {
                    BoxReplay.position = trackersDico[SteamVR_Input_Sources.Treadmill].position;
                    BoxReplay.rotation = trackersDico[SteamVR_Input_Sources.Treadmill].rotation;
                }
            }

            Vector3 middle = (trackersDico[SteamVR_Input_Sources.RightShoulder].transform.position +
                trackersDico[SteamVR_Input_Sources.LeftShoulder].transform.position) / 2f;
            neck.transform.position = middle;

            Vector3 backBone = (neck.transform.position - trackersDico[SteamVR_Input_Sources.Waist].transform.position).normalized;
            trackersDico[SteamVR_Input_Sources.RightShoulder].transform.position -= backBone * 0.05f;
            trackersDico[SteamVR_Input_Sources.LeftShoulder].transform.position -= backBone * 0.05f;

            Vector3 shoulderLine = (trackersDico[SteamVR_Input_Sources.RightShoulder].transform.position - trackersDico[SteamVR_Input_Sources.LeftShoulder].transform.position).normalized;
            trackersDico[SteamVR_Input_Sources.RightShoulder].transform.position += shoulderLine * 0.08f;
            trackersDico[SteamVR_Input_Sources.LeftShoulder].transform.position -= shoulderLine * 0.08f;

            AnimateAvatar(userSettings.trackerToBoneRotations, userSettings.offsetFromGround);

            var bendAngle = Vector3.SignedAngle(Vector3.up, backBone, trackersDico[SteamVR_Input_Sources.RightShoulder].position - trackersDico[SteamVR_Input_Sources.LeftShoulder].position);

            if (bendAngle > 0)
            {
                //Compensate offset added when avatar bended
                var projectedBackBone = Vector3.ProjectOnPlane(backBone, Vector3.up).normalized;

                trackersDico[SteamVR_Input_Sources.RightKnee].Translate(projectedBackBone * -.2f * (bendAngle / 90f), Space.World);
                trackersDico[SteamVR_Input_Sources.LeftKnee].Translate(projectedBackBone * -.2f * (bendAngle / 90f), Space.World);
                trackersDico[SteamVR_Input_Sources.RightFoot].Translate(projectedBackBone * -.2f * (bendAngle / 90f), Space.World);
                trackersDico[SteamVR_Input_Sources.LeftFoot].Translate(projectedBackBone * -.2f * (bendAngle / 90f), Space.World);
            }

            UpdateGizmos(i);

            OnFrameChanged?.Invoke(i + 1);

            // USER Replays needs to validate, PRL no need it
            if (DisplayWireBody || validationDto != null)
            {
                validationDto ??= validDtoAux;
                return validator.Validate(rightKneeGizmo, leftKneeGizmo, hipsGizmo, waistGizmo, leftElbowGizmo, rightElbowGizmo, boxAttached, trackersDico[SteamVR_Input_Sources.LeftFoot], trackersDico[SteamVR_Input_Sources.RightFoot], validationDto);
            }
            else return true;
        }

        /// <summary>
        /// Animates character's avatar with data recorded.
        /// </summary>
        /// <param name="trackersToBones"></param>
        /// <param name="groundOffset"></param>
        /// <param name="shouldersLine"></param>
        private void AnimateAvatar(Dictionary<int, Vector4Dto> trackersToBones, float groundOffset)
        {
            // 2 Draw avatar
            var rightKneeBone = bonesDico[SteamVR_Input_Sources.RightKnee];
            var leftKneeBone = bonesDico[SteamVR_Input_Sources.LeftKnee];

            var rightFootTracker = trackersDico[SteamVR_Input_Sources.RightFoot];
            var leftFootTracker = trackersDico[SteamVR_Input_Sources.LeftFoot];

            // 2.0. Waist
            var hips = bonesDico[SteamVR_Input_Sources.Waist];
            hips.position = waistTracker.position;
            if (Vector3.zero != neck.transform.position - waistTracker.position)
                hips.rotation = Quaternion.LookRotation(neck.transform.position - waistTracker.position, -waistTracker.transform.forward);
            hips.Rotate(hipsOffset);

            // 2.1. Legs
            if (Vector3.zero != rightKneeTracker.position - waistTracker.transform.position)
                if (genre == 'm')
                {
                    rightUpLegBone.transform.rotation = Quaternion.LookRotation(rightKneeTracker.position - waistTracker.transform.position, -rightKneeTracker.forward);
                    rightUpLegBone.transform.Rotate(upLegOffset);
                }
                else
                {
                    rightUpLegBoneF.transform.rotation = Quaternion.LookRotation(rightKneeTracker.position - waistTracker.transform.position, -rightKneeTracker.forward);
                    rightUpLegBoneF.transform.Rotate(upLegOffset);
                }
            if (Vector3.zero != leftKneeTracker.position - waistTracker.transform.position)
                if (genre == 'm')
                {
                    leftUpLegBone.transform.rotation = Quaternion.LookRotation(leftKneeTracker.position - waistTracker.transform.position, -leftKneeTracker.forward);
                    leftUpLegBone.transform.Rotate(upLegOffset);
                }
                else
                {
                    leftUpLegBoneF.transform.rotation = Quaternion.LookRotation(leftKneeTracker.position - waistTracker.transform.position, -leftKneeTracker.forward);
                    leftUpLegBoneF.transform.Rotate(upLegOffset);
                }

            // 2.2. Knees
            if (Vector3.zero != rightFootTracker.position - rightKneeTracker.position)
                rightKneeBone.transform.rotation = Quaternion.LookRotation(rightFootTracker.position - rightKneeTracker.position, -rightFootTracker.forward);
            rightKneeBone.transform.Rotate(rightKneeOffset);
            if (Vector3.zero != leftFootTracker.position - leftKneeTracker.position)
                leftKneeBone.transform.rotation = Quaternion.LookRotation(leftFootTracker.position - leftKneeTracker.position, -leftFootTracker.forward);
            leftKneeBone.transform.Rotate(leftKneeOffset);

            // Move spin for a better movement if neccessay 
            if (genre == 'm')
            {
                spine.transform.localRotation = Quaternion.identity;
                spine1.transform.localRotation = Quaternion.identity;
            }
            else
            {
                spineF.transform.localRotation = Quaternion.identity;
                spine1F.transform.localRotation = Quaternion.identity;
            }
            Vector3 backBone = neck.transform.position - waistTracker.position;
            var bendAngle = Vector3.SignedAngle(Vector3.up, backBone, trackersDico[SteamVR_Input_Sources.RightShoulder].position - trackersDico[SteamVR_Input_Sources.LeftShoulder].position);
            float spineTorsionAngle;

            if (genre == 'm')
            {
                spineTorsionAngle = Vector3.Angle(trackersDico[SteamVR_Input_Sources.RightShoulder].position - trackersDico[SteamVR_Input_Sources.LeftShoulder].position, spine.transform.right);
                spine.transform.Rotate(0, -0.2f * spineTorsionAngle, 0);
                spine1.transform.Rotate(0, -0.6f * spineTorsionAngle, 0);
            }
            else
            {
                spineTorsionAngle = Vector3.Angle(trackersDico[SteamVR_Input_Sources.RightShoulder].position - trackersDico[SteamVR_Input_Sources.LeftShoulder].position, spineF.transform.right);
                spineF.transform.Rotate(0, -0.2f * spineTorsionAngle, 0);
                spine1F.transform.Rotate(0, -0.6f * spineTorsionAngle, 0);
            }
            float bendingFactor = bendAngle > 0 ? Mathf.Sqrt(bendAngle / 90f) : 0;

            // 1 Code qui fonctionne
            hips.Translate(0, -.2f * bendingFactor, 0);
            if (genre == 'm')
            {
                spine.transform.Rotate(30 * bendingFactor, 0, 0);
            }
            else
            {
                spineF.transform.Rotate(30 * bendingFactor, 0, 0);
            }

            // 2.5. Shoulders

            if (Vector3.zero != trackersDico[SteamVR_Input_Sources.RightElbow].position - trackersDico[SteamVR_Input_Sources.RightShoulder].position)
                if (genre == 'm')
                {
                    rightShoulderBone.transform.rotation = Quaternion.LookRotation(trackersDico[SteamVR_Input_Sources.RightElbow].position - trackersDico[SteamVR_Input_Sources.RightShoulder].position, trackersDico[SteamVR_Input_Sources.RightElbow].up);
                    rightShoulderBone.transform.Rotate(rightShoulderOffset);
                }
                else
                {
                    rightShoulderBoneF.transform.rotation = Quaternion.LookRotation(trackersDico[SteamVR_Input_Sources.RightElbow].position - trackersDico[SteamVR_Input_Sources.RightShoulder].position, trackersDico[SteamVR_Input_Sources.RightElbow].up);
                    rightShoulderBoneF.transform.Rotate(rightShoulderOffset);
                }

            if (Vector3.zero != trackersDico[SteamVR_Input_Sources.LeftElbow].position - trackersDico[SteamVR_Input_Sources.LeftShoulder].position)
                if (genre == 'm')
                {
                    leftShoulderBone.transform.rotation = Quaternion.LookRotation(trackersDico[SteamVR_Input_Sources.LeftElbow].position - trackersDico[SteamVR_Input_Sources.LeftShoulder].position, trackersDico[SteamVR_Input_Sources.LeftElbow].up);
                    leftShoulderBone.transform.Rotate(leftShoulderOffset);
                }
                else
                {
                    leftShoulderBoneF.transform.rotation = Quaternion.LookRotation(trackersDico[SteamVR_Input_Sources.LeftElbow].position - trackersDico[SteamVR_Input_Sources.LeftShoulder].position, trackersDico[SteamVR_Input_Sources.LeftElbow].up);
                    leftShoulderBoneF.transform.Rotate(leftShoulderOffset);
                }


            // 2.6. Elbow
            var rightElbowBone = bonesDico[SteamVR_Input_Sources.RightElbow];
            if (Vector3.zero != trackersDico[SteamVR_Input_Sources.RightHand].position - trackersDico[SteamVR_Input_Sources.RightElbow].position)
                rightElbowBone.transform.rotation = Quaternion.LookRotation(trackersDico[SteamVR_Input_Sources.RightHand].position - trackersDico[SteamVR_Input_Sources.RightElbow].position,
                trackersDico[SteamVR_Input_Sources.RightHand].up);
            rightElbowBone.transform.Rotate(rightElbowOffset);

            var leftElbowBone = bonesDico[SteamVR_Input_Sources.LeftElbow];
            if (Vector3.zero != trackersDico[SteamVR_Input_Sources.LeftHand].position - trackersDico[SteamVR_Input_Sources.LeftElbow].position)
                leftElbowBone.transform.rotation = Quaternion.LookRotation(trackersDico[SteamVR_Input_Sources.LeftHand].position - trackersDico[SteamVR_Input_Sources.LeftElbow].position,
                trackersDico[SteamVR_Input_Sources.LeftHand].up);
            leftElbowBone.transform.Rotate(leftElbowOffset);


            // 7. Head
            var head = bonesDico[SteamVR_Input_Sources.Head];
            if (Vector3.zero != trackersDico[SteamVR_Input_Sources.Head].position - neck.transform.position)
                head.rotation = Quaternion.LookRotation(trackersDico[SteamVR_Input_Sources.Head].position - neck.transform.position, trackersDico[SteamVR_Input_Sources.Head].forward);
            head.transform.Rotate(headRotationOffset);

            // 2.3.Foot an hands
            // var rightHand = bonesDico[SteamVR_Input_Sources.RightHand];
            // rightHand.rotation = trackersDico[SteamVR_Input_Sources.RightHand].rotation * trackersToBones[SteamVR_Input_Sources.RightHand];
            // rightHand.Rotate(rightHandOffset);
            // var leftHand = bonesDico[SteamVR_Input_Sources.LeftHand];
            // leftHand.rotation = trackersDico[SteamVR_Input_Sources.LeftHand].rotation * trackersToBones[SteamVR_Input_Sources.LeftHand];
            // leftHand.Rotate(leftHandOffset);
            if (!isRotationSetup)
            {
                var rightHand = bonesDico[SteamVR_Input_Sources.RightHand];
                var leftHand = bonesDico[SteamVR_Input_Sources.LeftHand];
                if (trackersToBones.ContainsKey((int)SteamVR_Input_Sources.RightHand))
                {
                    rightHand.rotation = trackersDico[SteamVR_Input_Sources.RightHand].rotation * trackersToBones[(int)SteamVR_Input_Sources.RightHand].Quaternion();
                    rightHand.Rotate(rightHandOffset);
                }
                if (trackersToBones.ContainsKey((int)SteamVR_Input_Sources.LeftHand))
                {
                    leftHand.rotation = trackersDico[SteamVR_Input_Sources.LeftHand].rotation * trackersToBones[(int)SteamVR_Input_Sources.LeftHand].Quaternion();
                    leftHand.Rotate(leftHandOffset);
                }
                isRotationSetup = true;
            }


            if (rightFootTracker.position.y < leftFootTracker.position.y)
            {
                leftFootTracker.localRotation = Quaternion.Euler(63.864f, -1.634f, -2.477f);
            }
            else
            {
                rightFootTracker.localRotation = Quaternion.Euler(63.864f, 1.634f, 2.477f);
            }

            var rightFootBone = bonesDico[SteamVR_Input_Sources.RightFoot];
            var leftFootBone = bonesDico[SteamVR_Input_Sources.LeftFoot];

            rightFootBone.rotation = Quaternion.LookRotation(Vector3.up, rightKneeTracker.forward) * Quaternion.Euler(-25, 0, 0);
            leftFootBone.rotation = Quaternion.LookRotation(Vector3.up, rightKneeTracker.forward) * Quaternion.Euler(-25, 0, 0);

            float d1;
            float d2;

            if (genre == 'm')
            {
                d1 = Vector3.Distance(hips.position, spine.transform.position);
                d2 = Vector3.Distance(spine.transform.position, neckBone.transform.position);
            }
            else
            {
                d1 = Vector3.Distance(hips.position, spineF.transform.position);
                d2 = Vector3.Distance(spineF.transform.position, neckBoneF.transform.position);
            }
            float d4 = Vector3.Distance(hips.position, neck.transform.position);



            if (d4 < d1 + d2)
            {
                float beta = Mathf.Acos((-d4 * d4 + d1 * d1 + d2 * d2) / (2 * d1 * d2)) * Mathf.Rad2Deg;
                float alpha = 180 - Mathf.Acos((-d2 * d2 + d1 * d1 + d4 * d4) / (2 * d1 * d4)) * Mathf.Rad2Deg - beta;

                /*Debug.Log(alpha + " " + beta + " d1 " + d1 + " d2 " + d2 + " d4 " + d4);

                Debug.DrawRay(hips.position, Quaternion.Euler(-14, 0, 0) * (spine.transform.position- hips.position), Color.blue, 10);
                Debug.DrawRay(hips.position, Quaternion.Euler(-90, 0, 0) * (spine.transform.position - hips.position), Color.red, 10);
                Debug.DrawLine(hips.position, neck.transform.position, Color.green, 10);
                hips.Rotate(alpha, 0, 0);
                spine.transform.Rotate(beta, 0, 0);*/
            }

            var offset = groundOffset - Mathf.Min(rightFootBone.position.y, leftFootBone.position.y);
            hips.Translate(0, offset, 0, Space.World);
        }

        /// <summary>
        /// Update gizmos positions.
        /// </summary>
        private void UpdateGizmos(int i)
        {
            // Right Knee Gizmo
            rightKneeGizmo.center = rightKneeTracker.position;
            Vector3 startAngle = waistTracker.position - rightKneeTracker.position;
            Vector3 endAngle = trackersDico[SteamVR_Input_Sources.RightFoot].transform.position - rightKneeTracker.position;
            Vector3 rotationAxis = Vector3.Cross(startAngle, endAngle);
            rightKneeGizmo.startAngle = startAngle;
            rightKneeGizmo.rotationAxis = rotationAxis;
            rightKneeGizmo.angle = Vector3.Angle(startAngle, endAngle);

            // Left Knee Gizmo
            leftKneeGizmo.center = leftKneeTracker.position;
            startAngle = waistTracker.position - leftKneeTracker.position;
            endAngle = trackersDico[SteamVR_Input_Sources.LeftFoot].position - leftKneeTracker.position;
            rotationAxis = Vector3.Cross(startAngle, endAngle);
            leftKneeGizmo.startAngle = startAngle;
            leftKneeGizmo.rotationAxis = rotationAxis;
            leftKneeGizmo.angle = Vector3.Angle(startAngle, endAngle);

            /*chestAngleGizmo.center = chestTracker.position;
            startAngle = trackersDico[SteamVR_Input_Sources.Head].position - chestTracker.position;
            endAngle = waistTracker.position - chestTracker.position;
            rotationAxis = Vector3.Cross(startAngle, endAngle);
            chestAngleGizmo.startAngle = startAngle;
            chestAngleGizmo.rotationAxis = rotationAxis;
            chestAngleGizmo.angle = Vector3.Angle(startAngle, endAngle);*/

            // Hips Gizmo (Back Twisting)
            hipsGizmo.center = (neck.transform.position + waistTracker.position) / 2f;
            startAngle = trackersDico[SteamVR_Input_Sources.LeftShoulder].position - trackersDico[SteamVR_Input_Sources.RightShoulder].position;
            endAngle = trackersDico[SteamVR_Input_Sources.Waist].right;
            hipsGizmo.startAngle = startAngle;
            hipsGizmo.rotationAxis = Vector3.up;
            hipsGizmo.angle = Vector3.SignedAngle(startAngle, endAngle, Vector3.up);

            /*Quaternion waistToChest = chestTracker.rotation * Quaternion.Inverse(waistTracker.rotation);
            waistToChest.ToAngleAxis(out float waistToChestAngle, out Vector3 waistToChestAxis);
            float waistToChestProjectedAngle = Vector3.Project(waistToChestAxis.normalized * waistToChestAngle, waistTracker.position - chestTracker.position).magnitude;

            backBoneAngleGizmo.center = (chestTracker.position + waistTracker.position) / 2f;
            startAngle = chestTracker.forward;
            rotationAxis = waistTracker.position - chestTracker.position;
            backBoneAngleGizmo.startAngle = startAngle;
            backBoneAngleGizmo.rotationAxis = rotationAxis;
            backBoneAngleGizmo.angle = waistToChestProjectedAngle;*/

            // Waist Gizmo (Back Inclination)
            Quaternion waistToNeck = neck.transform.rotation * Quaternion.Inverse(waistTracker.rotation);

            waistToNeck.ToAngleAxis(out float waistToNeckAngle, out Vector3 waistToNeckAxis);

            waistGizmo.center = waistTracker.position;
            startAngle = neck.transform.position - trackersDico[SteamVR_Input_Sources.Waist].position;
            endAngle = -Vector3.up;
            rotationAxis = Vector3.Cross(startAngle, endAngle);
            waistGizmo.startAngle = startAngle;
            waistGizmo.rotationAxis = rotationAxis;
            waistGizmo.angle = Vector3.Angle(startAngle, endAngle);

            // Right Elbow Gizmo
            rightElbowGizmo.center = trackersDico[SteamVR_Input_Sources.RightElbow].position;
            startAngle = trackersDico[SteamVR_Input_Sources.RightHand].position - trackersDico[SteamVR_Input_Sources.RightElbow].position;
            endAngle = trackersDico[SteamVR_Input_Sources.RightShoulder].position - trackersDico[SteamVR_Input_Sources.RightElbow].position;
            rotationAxis = Vector3.Cross(startAngle, endAngle);
            rightElbowGizmo.startAngle = startAngle;
            rightElbowGizmo.rotationAxis = rotationAxis;
            rightElbowGizmo.angle = Vector3.Angle(startAngle, endAngle);

            // Left Elbow gizmo
            leftElbowGizmo.center = trackersDico[SteamVR_Input_Sources.LeftElbow].position;
            startAngle = trackersDico[SteamVR_Input_Sources.LeftHand].position - trackersDico[SteamVR_Input_Sources.LeftElbow].position;
            endAngle = trackersDico[SteamVR_Input_Sources.LeftShoulder].position - trackersDico[SteamVR_Input_Sources.LeftElbow].position;
            rotationAxis = Vector3.Cross(startAngle, endAngle);
            leftElbowGizmo.startAngle = startAngle;
            leftElbowGizmo.rotationAxis = rotationAxis;
            leftElbowGizmo.angle = Vector3.Angle(startAngle, endAngle);

            // Angle Tags
            angKneeR.UpdateTag(rightKneeGizmo);
            angKneeL.UpdateTag(leftKneeGizmo);
            angElbowR.UpdateTag(rightElbowGizmo);
            angElbowL.UpdateTag(leftElbowGizmo);
            angHips.UpdateTag(hipsGizmo);
            angWaist.UpdateTag(waistGizmo);

            //Debug.Log("TODO");
            /*auxWaist.GetComponent<FollowGizmo>().UpdateAuxPosition();*/

            // Box  (Used to compare the box position with the previous frame to check if the box has moved, so if the box has moved means the user has picked up)
            if (i != 0 && BoxReplay.position != boxPosAux) boxAttached = true;
            else if (i != 0 && BoxReplay.position == boxPosAux) boxAttached = false;
            boxPosAux = BoxReplay.position;
        }

        /// <summary>
        /// Stops playing replay.
        /// </summary>
        public void StopReplay()
        {
            if (IsPlaying)
            {
                if (replayCoroutine != null)
                    StopCoroutine(replayCoroutine);

                replayCoroutine = null;
            }
            IsPlaying = false;
        }

        public void HideReplay()
        {
            DisplayWireBody = false;
            if (genre == 'm') avatar[0].SetActive(false);
            else avatar[1].SetActive(false);
            BoxReplay.gameObject.SetActive(false);

            rightKneeGizmo.Enabled = leftKneeGizmo.Enabled = hipsGizmo.Enabled = leftElbowGizmo.Enabled = rightElbowGizmo.Enabled = waistGizmo.Enabled = false;
        }

        #region Validations

        [HideInInspector] public static bool errorUser = false;
        private MovementValidationDto validDtoAux;

        [HideInInspector] public static bool[] errorUserAux = new bool[6];
        [HideInInspector] public static bool isExtraTime = false;
        [HideInInspector] public static bool[] isGizmoErrorExtraTime = new bool[6];

        /// <summary>
        /// Once the exercise is completed immediately validates the movement
        /// </summary>
        /// <param name="data"></param>
        /// <param name="movementId"></param>
        public void ValidateCompleteMovement(RecordDto data, int movementId)
        {
            bool rightPerform;
            MovementValidationDto validationDto = new() { movementId = movementId, isValid = true };
            validDtoAux = validationDto;
            int auxWrongCounterFrame = 0;

            for (int i = 0; i < data.frames.Count; i++)
            {
                rightPerform = SetFramePose(data.frames[i], data.userSettings, i, validationDto);
                Debug.Log($"Frame: {i} RightPerform: {rightPerform}");

                if (!rightPerform) { auxWrongCounterFrame++; Debug.Log($"Wrong Frames: {auxWrongCounterFrame}"); }
                else auxWrongCounterFrame = 0;

                if (auxWrongCounterFrame >= errorFrames)
                {
                    errorUser = true;
                    Debug.Log("Error user :)");
                }
                else errorUser = false;

            }

            if (validationDto.isValid)
                validationDto.logMessages = new List<string> { "Ha realizado correctamente el ejercicio" };

            Debug.Log("Send validation " + validationDto.isValid);

            EulenMessagesSender.Instance.SendMovementValidation(validationDto, movementId);
        }

        /// <summary>
        /// Displays the error for an additional time 
        /// </summary>
        /// <param name="gizmo"></param>
        /// <returns></returns>
        private IEnumerator ShowErrorMoreTime(int gizmoPos)
        {
            int extraTime = 4;

            isExtraTime = true;
            Debug.Log($"Error gizmo: {MovementCondition.gizmosAux[gizmoPos].name}");

            isGizmoErrorExtraTime[gizmoPos] = true;

            yield return new WaitForSeconds(extraTime);

            isExtraTime = false;

            isGizmoErrorExtraTime[gizmoPos] = false;
            Debug.Log("Ya no hay error :v");
        }
        #endregion

        #endregion

        /// <summary>
        /// Scales avatar globally and some of its bones to fit users' bodies.
        /// </summary>
        /// <param name="cameraHeight"></param>
        /// <param name="boneLenghts"></param>
        private void SetAvatarHeight(float cameraHeight, Dictionary<int, float> boneLenghts)
        {
            if (genre == 'm') avatar[0].transform.localScale = Vector3.one * (1 / 1.87f) * cameraHeight * 1.06f;
            else avatar[1].transform.localScale = Vector3.one * (1 / 1.87f) * cameraHeight * 1.06f;

            float lenght, factor;

            foreach (var bone in boneLenghts.Keys)
            {
                switch (bone)
                {
                    case (int)SteamVR_Input_Sources.LeftShoulder:
                        if (genre == 'm')
                        {
                            leftShoulderBone.transform.localScale = Vector3.one;
                            lenght = Vector3.Distance(leftShoulderBone.transform.position, leftElbowBone.transform.position);
                            factor = (boneLenghts[bone]) / lenght;
                            leftShoulderBone.transform.localScale = new Vector3(1, factor, 1);
                        }
                        else
                        {
                            leftShoulderBoneF.transform.localScale = Vector3.one;
                            lenght = Vector3.Distance(leftShoulderBoneF.transform.position, leftElbowBoneF.transform.position);
                            factor = (boneLenghts[bone]) / lenght;
                            leftShoulderBoneF.transform.localScale = new Vector3(1, factor, 1);
                        }
                        break;
                    case (int)SteamVR_Input_Sources.RightShoulder:
                        if (genre == 'm')
                        {
                            rightShoulderBone.transform.localScale = Vector3.one;
                            lenght = Vector3.Distance(rightShoulderBone.transform.position, rightElbowBone.transform.position);
                            factor = (boneLenghts[bone]) / lenght;
                            rightShoulderBone.transform.localScale = new Vector3(1, factor, 1);
                        }
                        else
                        {
                            rightShoulderBoneF.transform.localScale = Vector3.one;
                            lenght = Vector3.Distance(rightShoulderBoneF.transform.position, rightElbowBoneF.transform.position);
                            factor = (boneLenghts[bone]) / lenght;
                            rightShoulderBoneF.transform.localScale = new Vector3(1, factor, 1);
                        }
                        break;
                    case (int)SteamVR_Input_Sources.LeftElbow:
                        if (genre == 'm')
                        {
                            leftElbowBone.transform.localScale = Vector3.one;
                            lenght = Vector3.Distance(leftElbowBone.transform.position, leftHandBone.transform.position);
                            factor = boneLenghts[bone] / lenght;
                            leftElbowBone.transform.localScale = new Vector3(1, factor, 1);
                        }
                        else
                        {
                            leftElbowBoneF.transform.localScale = Vector3.one;
                            lenght = Vector3.Distance(leftElbowBoneF.transform.position, leftHandBoneF.transform.position);
                            factor = boneLenghts[bone] / lenght;
                            leftElbowBoneF.transform.localScale = new Vector3(1, factor, 1);
                        }
                        break;
                    case (int)SteamVR_Input_Sources.RightElbow:
                        if (genre == 'm')
                        {
                            rightElbowBone.transform.localScale = Vector3.one;
                            lenght = Vector3.Distance(rightElbowBone.transform.position, rightHandBone.transform.position);
                            factor = boneLenghts[bone] / lenght;
                            rightElbowBone.transform.localScale = new Vector3(1, factor, 1);
                        }
                        else
                        {
                            rightElbowBoneF.transform.localScale = Vector3.one;
                            lenght = Vector3.Distance(rightElbowBoneF.transform.position, rightHandBoneF.transform.position);
                            factor = boneLenghts[bone] / lenght;
                            rightElbowBoneF.transform.localScale = new Vector3(1, factor, 1);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void DisplayTrackerSpheres()
        {
            if (DisplayWireBody)
            {
                foreach (var tracker in trackers)
                {
                    tracker.trans.gameObject.SetActive(false);
                }

                neck.SetActive(true);
            }
            else
            {
                foreach (var tracker in trackers)
                {
                    tracker.trans.gameObject.SetActive(false);
                }

                neck.SetActive(false);
            }
        }
        // Hide angle tags
        public void HideAngleTags()
        {
            angKneeR.gameObject.SetActive(false);
            angKneeL.gameObject.SetActive(false);
            angElbowR.gameObject.SetActive(false);
            angElbowL.gameObject.SetActive(false);
            angHips.gameObject.SetActive(false);
            angWaist.gameObject.SetActive(false);
        }

        [System.Serializable]
        public class TrackerGizmo
        {
            public SteamVR_Input_Sources source;

            public Transform trans;
        }

        #endregion
    }
}