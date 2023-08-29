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
        /// Avtar to model/
        /// </summary>
        public GameObject avatar;

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

        [Header("Bones")]
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
        public GameObject angKneeR;
        public GameObject angKneeL;
        public GameObject angElbowR;
        public GameObject angElbowL;
        public GameObject angHips;
        public GameObject angWaist;
        public GameObject auxWaist;


        [Header("")]
        public Transform BoxReplay;
        [SerializeField] private LogsManager logsManager;
        [SerializeField] private GameObject screenResults;
        [SerializeField] private Text[] res;

        /// <summary>
        /// Save the box position to compare as previous frame
        /// </summary>
        private Vector3 boxPosAux;
        /// <summary>
        /// Is the box "attached to the user"
        /// </summary>
        private bool boxAttached = false;

        int replayNum = 0;
        private bool isRotationSetup = false;

        /// <summary>
        /// Is replay playing
        /// </summary>
        public bool IsPlaying { get; private set; }

        public delegate void OnFrameChangedDelegate(int frame);
        public event OnFrameChangedDelegate OnFrameChanged;

        #region Display options

        /// <summary>
        /// Are tracking spheres displayed ?
        /// </summary>
        private bool areSphereDisplayed = false;

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

        [HideInInspector]
        public bool shouldDisplayHipsAngle, shouldDisplayRightKneeAngle, shouldDisplayLeftKneeAngle, shouldDisplayRightElbowAngle, shouldDisplayLeftElbowAngle, shouldDisplayWaistAngle;    //W

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
            shouldDisplayHipsAngle = true;
            shouldDisplayRightKneeAngle = true;
            shouldDisplayLeftKneeAngle = true;
            shouldDisplayRightElbowAngle = true;
            shouldDisplayLeftElbowAngle = true;
            shouldDisplayWaistAngle = true; //W

            foreach (var tracker in trackers)
            {
                trackersDico[tracker.source] = tracker.trans;
            }

            foreach (var tracker in UserSettings.instance.trackersToBones)
            {
                bonesDico[tracker.source] = tracker.bone;
            }

            waistTracker = trackersDico[SteamVR_Input_Sources.Waist];
            rightKneeTracker = trackersDico[SteamVR_Input_Sources.RightKnee];
            leftKneeTracker = trackersDico[SteamVR_Input_Sources.LeftKnee];

            rightKneeGizmo = new AngleGizmo { size = .1f };
            leftKneeGizmo = new AngleGizmo { size = .1f };
            leftElbowGizmo = new AngleGizmo { size = .1f };
            rightElbowGizmo = new AngleGizmo { size = .1f };
            waistGizmo = new AngleGizmo { size = .1f };
            hipsGizmo = new AngleGizmo { size = .1f, displayTotalCircle = true };
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
            RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
        }

        void OnDisable()
        {
            RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
        }

        private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            // Debug.Log("On Post Render " + camera.name + " " + camera.GetInstanceID());
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

            GL.PopMatrix();
        }

        #endregion

        #region Replay

        /// <summary>
        /// Replays a whole tracking record.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public void Replay(RecordDto data, bool displayAvatar, int offset = 0)
        {
            if (IsPlaying)
                StopReplay();

            if (!IsPlaying)
            {
                if (replayCoroutine != null)
                    StopCoroutine(replayCoroutine);

                avatar.SetActive(displayAvatar);
                DisplayWireBody = !displayAvatar;

                replayNum = PanelController.selectedNumPosition;
                replayCoroutine = StartCoroutine(ReplayCoroutine(data, offset));
            }
        }

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

                var wait = new WaitForSeconds(1f / data.recordFps); // 1f Normal fps replay
                // var hold = new WaitForSeconds(4f / data.recordFps); // Less fps replay

                for (int i = offset; i < data.frames.Count; i++)
                {
                    SetFramePose(data.frames[i], data.userSettings, i);                             // A: Replay the whole exercise in the same speed (even with errors)
                    /*if (!SetFramePose(data.frames[i], data.userSettings, i))                      // B: Replay the exercise and stops if there found any error
                        break;*/
                    /*if (!SetFramePose(data.frames[i], data.userSettings, i)) yield return hold;   // C: Replay the exercise normal and, if there are errors, replay that segments more slowly
                    else yield return wait;*/
                    yield return wait;
                }

                StopReplay();

                if (replayNum == 1 || replayNum == 3 || replayNum == 5)
                {
                    if (screenResults.GetComponent<Text>().text.Length > 1) screenResults.GetComponent<Text>().text = screenResults.GetComponent<Text>().text.Trim();   // Trim the last line break
                    else screenResults.GetComponent<Text>().text = "Ejercicio realizado correctamente";

                    logsManager.UpdateResultLogs(replayNum, screenResults.GetComponent<Text>().text);           // Save the log in the corresponding history
                    UpdateResultScreen();
                }
            }
        }

        private void UpdateResultScreen()
        {
            switch (replayNum)
            {
                case 1: res[0].text = logsManager.IsWellDone[0] ? "1. Correcto" : "1. Incorrecto"; break;
                case 3: res[1].text = logsManager.IsWellDone[1] ? "2. Correcto" : "2. Incorrecto"; break;
                case 5: res[2].text = logsManager.IsWellDone[2] ? "3. Correcto" : "3. Incorrecto"; break;
                default: break;
            }
        }

        /// <summary>
        /// Display a specific record frame.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="trackersToBones"></param>
        /// <param name="i"></param>
        public bool SetFramePose(RecordKeyFrameDto frame, UserSettingsDto userSettings, int i)
        {
            SetAvatarHeight(userSettings.cameraHeight, userSettings.boneLenghts);

            DisplayTrackerSpheres();

            // 1. Sets wire body
            Vector3 _rightFoot = Vector3.zero, _rightKnee = Vector3.zero;

            foreach (var entry in frame.entries)
            {
                trackersDico[(SteamVR_Input_Sources) entry.source].position = entry.position.Struct();
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
                    // trackersDico[entry.source].position -= trackersDico[entry.source].forward * userSettings.boneOffsets[entry.source];
                    if (replayNum == 0 || replayNum == 2 || replayNum == 4) trackersDico[(SteamVR_Input_Sources)entry.source].position -= trackersDico[(SteamVR_Input_Sources)entry.source].forward * userSettings.boneOffsets[entry.source];
                    else trackersDico[(SteamVR_Input_Sources)entry.source].position -= trackersDico[(SteamVR_Input_Sources)entry.source].forward * waistOffset;
                }
                else if ((SteamVR_Input_Sources)entry.source == SteamVR_Input_Sources.LeftKnee)//
                {
                    if (replayNum == 1 || replayNum == 3 || replayNum == 5) trackersDico[(SteamVR_Input_Sources)entry.source].position += trackersDico[(SteamVR_Input_Sources)entry.source].right * kneeOffset;
                }
                else if ((SteamVR_Input_Sources)entry.source == SteamVR_Input_Sources.RightKnee)//
                {
                    if (replayNum == 1 || replayNum == 3 || replayNum == 5) trackersDico[(SteamVR_Input_Sources)entry.source].position -= trackersDico[(SteamVR_Input_Sources)entry.source].up * kneeOffset;
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
            if (replayNum == 1 || replayNum == 3 || replayNum == 5) return validator.Validate(rightKneeGizmo, leftKneeGizmo, hipsGizmo, waistGizmo, leftElbowGizmo, rightElbowGizmo, boxAttached, screenResults, trackersDico[SteamVR_Input_Sources.LeftFoot], trackersDico[SteamVR_Input_Sources.RightFoot]);
            else return true;
            // return true;
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
            hips.rotation = Quaternion.LookRotation(neck.transform.position - waistTracker.position, -waistTracker.transform.forward);
            hips.Rotate(hipsOffset);

            // 2.1. Legs
            rightUpLegBone.transform.rotation = Quaternion.LookRotation(rightKneeTracker.position - waistTracker.transform.position, -rightKneeTracker.forward);
            rightUpLegBone.transform.Rotate(upLegOffset);
            leftUpLegBone.transform.rotation = Quaternion.LookRotation(leftKneeTracker.position - waistTracker.transform.position, -leftKneeTracker.forward);
            leftUpLegBone.transform.Rotate(upLegOffset);

            // 2.2. Knees
            rightKneeBone.transform.rotation = Quaternion.LookRotation(rightFootTracker.position - rightKneeTracker.position, -rightFootTracker.forward);
            rightKneeBone.transform.Rotate(rightKneeOffset);

            leftKneeBone.transform.rotation = Quaternion.LookRotation(leftFootTracker.position - leftKneeTracker.position, -leftFootTracker.forward);
            leftKneeBone.transform.Rotate(leftKneeOffset);

            // Move spin for a better movement if neccessay 

            spine.transform.localRotation = Quaternion.identity;
            spine1.transform.localRotation = Quaternion.identity;

            Vector3 backBone = neck.transform.position - waistTracker.position;
            var bendAngle = Vector3.SignedAngle(Vector3.up, backBone, trackersDico[SteamVR_Input_Sources.RightShoulder].position - trackersDico[SteamVR_Input_Sources.LeftShoulder].position);
            float spineTorsionAngle = Vector3.Angle(trackersDico[SteamVR_Input_Sources.RightShoulder].position - trackersDico[SteamVR_Input_Sources.LeftShoulder].position,
                spine.transform.right);

            spine.transform.Rotate(0, -0.2f * spineTorsionAngle, 0);
            spine1.transform.Rotate(0, -0.6f * spineTorsionAngle, 0);

            float bendingFactor = bendAngle > 0 ? Mathf.Sqrt(bendAngle / 90f) : 0;

            // 1 Code qui fonctionne
            hips.Translate(0, -.2f * bendingFactor, 0);
            spine.transform.Rotate(30 * bendingFactor, 0, 0);

            // 2.5. Shoulders

            rightShoulderBone.transform.rotation = Quaternion.LookRotation(trackersDico[SteamVR_Input_Sources.RightElbow].position - trackersDico[SteamVR_Input_Sources.RightShoulder].position,
                trackersDico[SteamVR_Input_Sources.RightElbow].up);
            rightShoulderBone.transform.Rotate(rightShoulderOffset);

            leftShoulderBone.transform.rotation = Quaternion.LookRotation(trackersDico[SteamVR_Input_Sources.LeftElbow].position - trackersDico[SteamVR_Input_Sources.LeftShoulder].position,
                trackersDico[SteamVR_Input_Sources.LeftElbow].up);
            leftShoulderBone.transform.Rotate(leftShoulderOffset);


            // 2.6. Elbow
            var rightElbowBone = bonesDico[SteamVR_Input_Sources.RightElbow];
            rightElbowBone.transform.rotation = Quaternion.LookRotation(trackersDico[SteamVR_Input_Sources.RightHand].position - trackersDico[SteamVR_Input_Sources.RightElbow].position,
                trackersDico[SteamVR_Input_Sources.RightHand].up);
            rightElbowBone.transform.Rotate(rightElbowOffset);

            var leftElbowBone = bonesDico[SteamVR_Input_Sources.LeftElbow];
            leftElbowBone.transform.rotation = Quaternion.LookRotation(trackersDico[SteamVR_Input_Sources.LeftHand].position - trackersDico[SteamVR_Input_Sources.LeftElbow].position,
                trackersDico[SteamVR_Input_Sources.LeftHand].up);
            leftElbowBone.transform.Rotate(leftElbowOffset);


            // 7. Head
            var head = bonesDico[SteamVR_Input_Sources.Head];
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
                rightHand.rotation = trackersDico[SteamVR_Input_Sources.RightHand].rotation * trackersToBones[(int)SteamVR_Input_Sources.RightHand].Quaternion();
                rightHand.Rotate(rightHandOffset);
                leftHand.rotation = trackersDico[SteamVR_Input_Sources.LeftHand].rotation * trackersToBones[(int)SteamVR_Input_Sources.LeftHand].Quaternion();
                leftHand.Rotate(leftHandOffset);
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

            float d1 = Vector3.Distance(hips.position, spine.transform.position);
            float d2 = Vector3.Distance(spine.transform.position, neckBone.transform.position);
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
            rightKneeGizmo.Enabled = shouldDisplayRightKneeAngle;
            leftKneeGizmo.Enabled = shouldDisplayLeftKneeAngle;
            hipsGizmo.Enabled = shouldDisplayHipsAngle;
            leftElbowGizmo.Enabled = shouldDisplayLeftElbowAngle;
            rightElbowGizmo.Enabled = shouldDisplayRightElbowAngle;
            waistGizmo.Enabled = shouldDisplayRightElbowAngle;

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
            // endAngle = trackersDico[SteamVR_Input_Sources.Waist].forward + new Vector3(-177.636f, 3.838013f, 60.44f);
            //endAngle = trackersDico[SteamVR_Input_Sources.LeftFoot].position - trackersDico[SteamVR_Input_Sources.RightFoot].position;

            Debug.Log("TODO");
            /*endAngle = trackersDico[SteamVR_Input_Sources.Waist].position - auxWaist.GetComponent<FollowGizmo>().GetChild().position;
            rotationAxis = Vector3.Cross(startAngle, endAngle);
            hipsGizmo.startAngle = startAngle;*/


            //hipsGizmo.rotationAxis = rotationAxis;
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
            Debug.Log("TODO");
            /*angKneeR.GetComponent<FollowGizmo>().UpdateTagPosition(cameraHead.transform, rightKneeGizmo.angle);
            angKneeL.GetComponent<FollowGizmo>().UpdateTagPosition(cameraHead.transform, leftKneeGizmo.angle);
            angElbowR.GetComponent<FollowGizmo>().UpdateTagPosition(cameraHead.transform, rightElbowGizmo.angle);
            angElbowL.GetComponent<FollowGizmo>().UpdateTagPosition(cameraHead.transform, leftElbowGizmo.angle);
            angHips.GetComponent<FollowGizmo>().UpdateTagPosition(cameraHead.transform, hipsGizmo.angle);
            angWaist.GetComponent<FollowGizmo>().UpdateTagPosition(cameraHead.transform, waistGizmo.angle);

            auxWaist.GetComponent<FollowGizmo>().UpdateAuxPosition();*/

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
                IsPlaying = false;

                // Mark the excercise as viewed
                PanelController.visualized[PanelController.exViewing] = true;
                // Log for each time you COMPLETLY watch the exercise

                Debug.Log("TODO");
                return;

                switch (PanelController.exViewing)
                {
                    case 0:
                        logsManager.LogViewsAll[0]++;
                        break;
                    case 1:
                        logsManager.LogViewsAll[1]++;
                        break;
                    case 2:
                        logsManager.LogViewsAll[2]++;
                        break;
                }
            }
        }

        #endregion

        /// <summary>
        /// Scales avatar globally and some of its bones to fit users' bodies.
        /// </summary>
        /// <param name="cameraHeight"></param>
        /// <param name="boneLenghts"></param>
        private void SetAvatarHeight(float cameraHeight, Dictionary<int, float> boneLenghts)
        {
            avatar.transform.localScale = Vector3.one * (1 / 1.87f) * cameraHeight * 1.06f;

            float lenght, factor;

            foreach (var bone in boneLenghts.Keys)
            {
                switch (bone)
                {
                    case (int)SteamVR_Input_Sources.LeftShoulder:
                        leftShoulderBone.transform.localScale = Vector3.one;
                        lenght = Vector3.Distance(leftShoulderBone.transform.position, leftElbowBone.transform.position);
                        factor = (boneLenghts[bone]) / lenght;
                        leftShoulderBone.transform.localScale = new Vector3(1, factor, 1);
                        break;
                    case (int)SteamVR_Input_Sources.RightShoulder:
                        rightShoulderBone.transform.localScale = Vector3.one;
                        lenght = Vector3.Distance(rightShoulderBone.transform.position, rightElbowBone.transform.position);
                        factor = (boneLenghts[bone]) / lenght;
                        rightShoulderBone.transform.localScale = new Vector3(1, factor, 1);
                        break;
                    case (int)SteamVR_Input_Sources.LeftElbow:
                        leftElbowBone.transform.localScale = Vector3.one;
                        lenght = Vector3.Distance(leftElbowBone.transform.position, leftHandBone.transform.position);
                        factor = boneLenghts[bone] / lenght;
                        leftElbowBone.transform.localScale = new Vector3(1, factor, 1);
                        break;
                    case (int)SteamVR_Input_Sources.RightElbow:
                        rightElbowBone.transform.localScale = Vector3.one;
                        lenght = Vector3.Distance(rightElbowBone.transform.position, rightHandBone.transform.position);
                        factor = boneLenghts[bone] / lenght;
                        rightElbowBone.transform.localScale = new Vector3(1, factor, 1);
                        break;
                    default:
                        break;
                }
            }
        }

        private void DisplayTrackerSpheres()
        {
            if (DisplayWireBody && !areSphereDisplayed)
            {
                foreach (var tracker in trackers)
                {
                    if (tracker.source == SteamVR_Input_Sources.Camera || tracker.source == SteamVR_Input_Sources.Treadmill)
                        continue;

                    areSphereDisplayed = true;
                    tracker.trans.gameObject.SetActive(true);
                }

                neck.SetActive(true);
            }
            else if (!DisplayWireBody && areSphereDisplayed)
            {
                foreach (var tracker in trackers)
                {
                    if (tracker.source == SteamVR_Input_Sources.Camera || tracker.source == SteamVR_Input_Sources.Treadmill)
                        continue;

                    areSphereDisplayed = false;
                    tracker.trans.gameObject.SetActive(false);
                }

                neck.SetActive(false);
            }
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