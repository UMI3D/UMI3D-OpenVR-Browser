using BeardedManStudios.Forge.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Valve.VR;

namespace com.inetum.eulen.recording.app
{
    public class PanelController : MonoBehaviour
    {
        public static PanelController instance;

        [Header("Feedback Panel")]
        [SerializeField] private Text uIDescription;
        [SerializeField] private string[] descriptions;
        [SerializeField] private Text uIResult;
        [SerializeField] private Transform selectedFeedback;

        public static int selectedNumPosition = 0; // 0
        private readonly Vector3[] selectedPosition = new Vector3[6];

        [Header("Scenario stuff")]
        [SerializeField] private DrawAvatar drawAvatar;
        [SerializeField] private GameObject[] tables;
        [SerializeField] private GameObject box;
        [SerializeField] private GameObject[] planes;
        [SerializeField] private Material sphere;
        [SerializeField] private Material gizmo;
        [SerializeField] private Material circle;
        [SerializeField] private Material wirebody;
        [SerializeField] private Material transparent;
        [SerializeField] private Material error;
        [SerializeField] private Material errorInside;
        [SerializeField] private GameObject[] angleTags;

        [Header("Perform stuff")]
        [SerializeField] GameObject btnPerform;
        [SerializeField] private int[] durationExcercise;
        [SerializeField] private int userRot;   // 90 != 180
        [SerializeField] private MeshCollider[] hands;
        [SerializeField] private GameObject user;
        [SerializeField] private GameObject teleport;
        [SerializeField] private TimeManager timer;
        [SerializeField] private Text timerType;
        [SerializeField] private GameObject avatarSpheres;
        [SerializeField] private LogsManager logsManager;

        [HideInInspector] public Vector3 posBoxDetached;
        private readonly string[] paths = new string[6];

        int currentAnimationFrame = 0;
        int currentFramesRecorded = 0;

        // To control what the user already see
        public static bool[] visualized = new bool[4];
        public static int exViewing;

        public delegate void OnPlayModeChangedDelegate(bool playMode);
        public event OnPlayModeChangedDelegate OnPlayModeChanged;

        private void Start()
        {
            /*uIDescription.text = descriptions[0];

            // Hardcoded :p
            // Setup 1
            selectedPosition[0] = new Vector3(0, 0.03f, 0);
            selectedPosition[1] = new Vector3(0, -0.03f, 0);
            // Setup 2
            selectedPosition[2] = new Vector3(0.05f, 0.03f, 0);
            selectedPosition[3] = new Vector3(0.05f, -0.03f, 0);
            // Setup 3
            selectedPosition[4] = new Vector3(0.1f, 0.03f, 0);
            selectedPosition[5] = new Vector3(0.1f, -0.03f, 0);

            visualized[0] = false;
            visualized[1] = false;
            visualized[2] = false;
            visualized[3] = false;

            // Specialist records
            paths[0] = ".\\Assets\\EnvironmentProject\\Data\\Track9_1.npmc";
            paths[2] = ".\\Assets\\EnvironmentProject\\Data\\Track9_2.npmc";
            paths[4] = ".\\Assets\\EnvironmentProject\\Data\\Track9_3.npmc";
            // User records
            paths[1] = ".\\Assets\\EnvironmentProject\\Data\\Track9_1_USER.npmc";
            paths[3] = ".\\Assets\\EnvironmentProject\\Data\\Track9_2_USER.npmc";
            paths[5] = ".\\Assets\\EnvironmentProject\\Data\\Track9_3_USER.npmc";

            for (int i = 0; i < avatarSpheres.transform.childCount; i++) avatarSpheres.transform.GetChild(i).gameObject.GetComponent<Renderer>().material = transparent;

            StartCoroutine(PreloadExercise());*/
        }

        /// <summary>
        /// "Preloads" the 1st PRL movement
        /// </summary>
        /// <returns></returns>
        private IEnumerator PreloadExercise()
        {
            yield return new WaitForSeconds(1f);
            UpdateInfo();
        }

        /// <summary>
        /// Panel control arrows manager
        /// </summary>
        /// <param name="arrow"></param>
        private void MoveFeedback(int arrow)
        {
            /*int auxPos = selectedNumPosition;

            FullBodyRecording.instance.StopRecording();

            switch (arrow)
            {
                case 0:     // Down
                    if (selectedNumPosition % 2 == 0) selectedNumPosition += 1;
                    break;
                case 1:     // Up
                    if (selectedNumPosition % 2 != 0) selectedNumPosition -= 1;
                    break;
                case 2:     // Right
                    if (selectedNumPosition < 4) selectedNumPosition += 2;
                    break;
                case 3:     // Left
                    if (selectedNumPosition > 1) selectedNumPosition -= 2;
                    break;
            }

            if (selectedNumPosition > 5) selectedNumPosition = 5;
            else if (selectedNumPosition < 0) selectedNumPosition = 0;

            LockSystem(auxPos);
            selectedFeedback.localPosition = selectedPosition[selectedNumPosition];
            UpdateInfo();*/
        }

        /// <summary>
        /// Update the feedback panel and preloads the animations
        /// </summary>
        private void UpdateInfo()
        {
            if (selectedNumPosition <= 1) uIDescription.text = descriptions[0];
            else if (selectedNumPosition <= 3) uIDescription.text = descriptions[1];
            else uIDescription.text = descriptions[2];
            uIResult.text = "";
            PlayExercise();
            exViewing = 3;
            avatarSpheres.GetComponent<DrawAvatar>().StopReplay();
        }

        /// <summary>
        /// Unlock excersices as the user completes them
        /// </summary>
        /// <param name="prevPos"></param>
        private void LockSystem(int prevPos)
        {
            /*if (!visualized[0])   
            {
                Debug.LogWarning("You must see the excersice 1 first!!!!! ");
                selectedNumPosition = prevPos;
            }
            else if ((selectedNumPosition == 3 || selectedNumPosition == 4) && !visualized[1])
            {
                Debug.LogWarning("You must see the excersice 2 first!!!!! ");
                selectedNumPosition = prevPos;
            }
            else if (selectedNumPosition == 5 && !visualized[2])
            {
                Debug.LogWarning("You must see the excersice 3 first!!!!! ");
                selectedNumPosition = prevPos;
            }*/

            if (selectedNumPosition <= 2)
            {
                if (!visualized[0])     // In case you didn't visualize the exercise you can't advance
                {
                    Debug.LogWarning("You must see the excersice 1 first! ");
                    selectedNumPosition = prevPos;
                }
                else if (visualized[0] && logsManager.LogAttemptsAll[0] == 0 && selectedNumPosition == prevPos + 1)    // In case you didn't perform the exercise you can't see your replay
                {
                    Debug.LogWarning("You must perform the excersice 1 first! ");
                    selectedNumPosition = prevPos;
                }
            }
            else if (selectedNumPosition <= 4)
            {
                if (!visualized[1])     // In case you didn't visualize the exercise you can't advance
                {
                    Debug.LogWarning("You must see the excersice 2 first! ");
                    selectedNumPosition = prevPos;
                }
                else if (visualized[1] && logsManager.LogAttemptsAll[1] == 0 && selectedNumPosition == prevPos + 1)    // In case you didn't perform the exercise you can't see your replay
                {
                    Debug.LogWarning("You must perform the excersice 2 first! ");
                    selectedNumPosition = prevPos;
                }
            }
            else if (selectedNumPosition == 5)
            {
                if (!visualized[2])     // In case you didn't visualize the exercise you can't advance
                {
                    Debug.LogWarning("You must see the excersice 2 first! ");
                    selectedNumPosition = prevPos;
                }
                else if (visualized[2] && logsManager.LogAttemptsAll[2] == 0 && selectedNumPosition == prevPos + 1)    // In case you didn't perform the exercise you can't see your replay
                {
                    Debug.LogWarning("You must perform the excersice 2 first! ");
                    selectedNumPosition = prevPos;
                }
            }
        }

        public void ButtonDown()
        {
            MoveFeedback(0);
        }
        public void ButtonUp()
        {
            MoveFeedback(1);
        }
        public void ButtonRight()
        {
            MoveFeedback(2);
        }
        public void ButtonLeft()
        {
            MoveFeedback(3);
        }

        /// <summary>
        /// Plays the selected exercise
        /// </summary>
        public void PlayExercise()
        {
            /*currentAnimationFrame = 0;

            switch (selectedNumPosition)
            {
                // PRL Movements
                case 0:
                    UserPRL(true);
                    ScenarioSetup(1);
                    currentFramesRecorded = FullBodyRecording.instance.LoadFile(paths[0]);
                    exViewing = 0;
                    break;
                case 2:
                    UserPRL(true);
                    ScenarioSetup(2);
                    currentFramesRecorded = FullBodyRecording.instance.LoadFile(paths[2]);
                    exViewing = 1;
                    break;
                case 4:
                    UserPRL(true);
                    ScenarioSetup(3);
                    currentFramesRecorded = FullBodyRecording.instance.LoadFile(paths[4]);
                    exViewing = 2;
                    break;
                // USER Movements
                case 1:
                    UserPRL(false);
                    ScenarioSetup(1);
                    currentFramesRecorded = FullBodyRecording.instance.LoadFile(paths[1]);
                    break;
                case 3:
                    UserPRL(false);
                    ScenarioSetup(2);
                    currentFramesRecorded = FullBodyRecording.instance.LoadFile(paths[3]);
                    break;
                case 5:
                    UserPRL(false);
                    ScenarioSetup(3);
                    currentFramesRecorded = FullBodyRecording.instance.LoadFile(paths[5]);
                    break;
            }
            OnPlayModeChanged?.Invoke(true);
            uIResult.text = "";
            drawAvatar.Replay(FullBodyRecording.instance.GetRecordData(), currentAnimationFrame);*/
        }

        /// <summary>
        /// Setup the avatar and info deppending on the replay
        /// PRL Replay: only displays the avatar
        /// USER Replay: Won't display the avatar, but the wirebody, angles and angle tags
        /// </summary>
        /// <param name="isPRL"></param>
        private void UserPRL(bool isPRL)
        {
            /*if (isPRL)  // PRL
            {
                drawAvatar.DisplayAvatar = true;
                drawAvatar.DisplayWireBody = false;
                AngleGizmoManager.instance.AngMat(transparent, transparent, transparent, transparent);
                for (int i = 0; i < avatarSpheres.transform.childCount; i++)
                {
                    avatarSpheres.transform.GetChild(i).gameObject.GetComponent<Renderer>().material = transparent;
                }
                foreach (var item in angleTags) item.SetActive(false);
            }
            else        // USER
            {
                drawAvatar.DisplayAvatar = false;
                drawAvatar.lineMat = wirebody;
                drawAvatar.DisplayWireBody = true;
                AngleGizmoManager.instance.AngMat(gizmo, circle, error, errorInside);
                for (int i = 0; i < avatarSpheres.transform.childCount; i++)
                {
                    if (i == 4 || i == 5 || i == 9 || i == 15) continue;
                    avatarSpheres.transform.GetChild(i).gameObject.GetComponent<Renderer>().material = sphere;
                }
                foreach (var item in angleTags) item.SetActive(true);
            }*/
        }

        /// <summary>
        /// Allows the user perform the exercise
        /// </summary>
        public void PerformExercise()
        {
            /*if (UnlockPerformExercise())
            {

                int time;
                // Teleport to the scene and disable the teleport action
                user.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0, userRot, 0));
                teleport.SetActive(false);

                // Hide avatar and disable collider hands 
                drawAvatar.DisplayAvatar = false;
                avatarSpheres.SetActive(false);
                AngleGizmoManager.instance.AngMat(transparent, transparent, transparent, transparent);
                foreach (var item in angleTags) item.SetActive(false);
                hands[0].GetComponent<MeshCollider>().enabled = false;
                hands[1].GetComponent<MeshCollider>().enabled = false;

                // Setup scenario
                if (selectedNumPosition <= 1) { ScenarioSetup(1); time = durationExcercise[0]; logsManager.LogAttemptsAll[0]++; }
                else if (selectedNumPosition <= 3) { ScenarioSetup(2); time = durationExcercise[1]; logsManager.LogAttemptsAll[1]++; }
                else { ScenarioSetup(3); time = durationExcercise[2]; logsManager.LogAttemptsAll[2]++; }
                uIResult.text = "";

                // Enable setup (Sync trackers with avatar skeleton) and start a countdown (5sec)
                timerType.text = "Iniciando";
                UserSettings.instance.DisplaySettings(() => CountDown(8, time));
            }*/
        }

        /// <summary>
        /// Check if the user has seen the whole movement repetition to allows perform it
        /// </summary>
        /// <returns></returns>
        private bool UnlockPerformExercise()
        {
            bool unlock = false;

            switch (selectedNumPosition)
            {
                case 0:
                    if (visualized[0]) unlock = true;
                    break;
                case 2:
                    if (visualized[1]) unlock = true;
                    break;
                case 4:
                    if (visualized[2]) unlock = true;
                    break;
                default:
                    unlock = true;
                    break;
            }
            return unlock;
        }

        /// <summary>
        /// Reconfigure the scenario for the different situations
        /// </summary>
        /// <param name="scenario"></param>
        private void ScenarioSetup(int scenario)
        {
            // Set up the different scenarios
            switch (scenario)
            {
                case 1:     // Floor to Table
                    tables[0].SetActive(true);
                    tables[1].SetActive(false);

                    planes[0].SetActive(true);
                    planes[1].SetActive(true);
                    planes[2].SetActive(false);
                    planes[3].SetActive(false);

                    box.transform.position = new Vector3(0, 0.01f, 1.2f);
                    planes[0].GetComponent<Collider>().enabled = true;
                    break;
                case 2:     // Floor, Walk to table
                    tables[0].SetActive(true);
                    tables[1].SetActive(false);

                    planes[0].SetActive(true);
                    planes[1].SetActive(false);
                    planes[2].SetActive(true);
                    planes[3].SetActive(false);

                    box.transform.position = new Vector3(0, 0.01f, -0.2f);
                    planes[0].GetComponent<Collider>().enabled = true;
                    break;
                case 3:     // Table to table
                    tables[0].SetActive(true);
                    tables[1].SetActive(true);

                    planes[0].SetActive(true);
                    planes[1].SetActive(false);
                    planes[2].SetActive(false);
                    planes[3].SetActive(true);

                    box.transform.position = new Vector3(0, 0.9060006f, 1.85f);
                    planes[0].GetComponent<Collider>().enabled = false;
                    planes[3].GetComponent<Collider>().enabled = true;
                    break;
            }
            box.transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        /// <summary>
        /// Timer to prepare for exercise
        /// </summary>
        /// <param name="seg"></param>
        /// <param name="excerciseTime"></param>
        private void CountDown(int seg, int excerciseTime)
        {
            /*user.transform.SetPositionAndRotation(new Vector3(0, 0, 1), Quaternion.Euler(0, userRot - 180, 0));
            timer.SetDuration(seg, excerciseTime).Begin(0);
            OnPlayModeChanged?.Invoke(false);*/
        }

        /// <summary>
        /// Stop the excercise if the users complete it or exceeds the time
        /// User complete the excercise: Saves the exercise
        /// User exceeds the time: Doesn't save the exercise
        /// </summary>
        /// <param name="save"></param>
        public void StopSetupAndSave(bool save)
        {
            /*string path;
            Vector3 initialPos = new(5.014f, -0.29f, 1.789f);
            Quaternion initialRot = Quaternion.Euler(0, 180, 0); // 180 != 270

            avatarSpheres.SetActive(true);
            // Stops the recording
            FullBodyRecording.instance.StopRecording();
            // Enables teleport action and teleport to the panel controller
            teleport.SetActive(true);
            user.transform.SetPositionAndRotation(initialPos, initialRot);

            hands[0].GetComponent<MeshCollider>().enabled = true;
            hands[1].GetComponent<MeshCollider>().enabled = true;

            if (selectedNumPosition <= 1) path = ".\\Assets\\EnvironmentProject\\Data\\Track9_1_USER.npmc";
            else if (selectedNumPosition <= 3) path = ".\\Assets\\EnvironmentProject\\Data\\Track9_2_USER.npmc";
            else path = ".\\Assets\\EnvironmentProject\\Data\\Track9_3_USER.npmc";

            // Save the recording
            if (save) { FullBodyRecording.instance.Save(path); uIResult.text = "Revisa la repetición"; }
            else uIResult.text = "Has excedido el límite de tiempo para realizar el ejercicio, vuelve a intentarlo";*/
        }

        /*public void BoxPositionDetached()
        {
            posBoxDetached = box.transform.position;
        }*/

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}