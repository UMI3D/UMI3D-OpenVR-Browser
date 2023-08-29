using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using com.inetum.addonEulen.common.dtos;
using com.inetum.addonEulen.common;
using UnityEngine.Networking;

namespace com.inetum.eulen.recording.app
{
    /// <summary>
    /// Records user's movements.
    /// </summary>
    public class FullBodyRecording : MonoBehaviour
    {
        /// <summary>
        /// File extension for recorded data.
        /// </summary>
        public const string recordFileExtension = "npmc";

        /// <summary>
        /// Mode used to record data.
        /// </summary>
        public enum RecordMode
        {
            Json,
            Text
        }

        public static FullBodyRecording instance;

        #region Fields

        [SerializeField]
        [Range(0, 120)]
        public int recordFps = 60;

        public Transform Camera;

        public List<SteamVR_Behaviour_Pose> Trackers;

        RecordMode currentRecordMode;

        public bool IsRecording { get; private set; } = false;

        string splitter = " | ";
        string subSplitter = " ; ";

        Coroutine recording;
        StreamWriter sw;

        public Transform Box;

        #endregion

        #region RecordJsonData

        private int currentRecordedMovementId = 0;

        private RecordDto recordDto;

        #endregion

        private void Awake()
        {
            instance = this;
        }

        public void StartRecording(RecordMode mode, int movementId)
        {
            if (IsRecording)
            {
                Debug.LogError("Already recording");
                return;
            }

            currentRecordMode = mode;
            IsRecording = true;
            currentRecordedMovementId = movementId;

            switch (mode)
            {
                case RecordMode.Json:
                    recording = StartCoroutine(RecordingJson());
                    break;
                case RecordMode.Text:
                    recording = StartCoroutine(RecordingText());
                    break;
                default:
                    break;
            }
        }

        private IEnumerator RecordingText()
        {
            var wait = new WaitForSecondsRealtime(1f / recordFps);

            string dataPath = "./Assets/Project/Data/FullBodyCapture " + System.DateTime.Now.ToString("yyyy-MM-dd-hhmmss") + ".npmc";

            Debug.Log(dataPath);

            if (!File.Exists(dataPath))
            {
                var fs = File.Create(dataPath);
                fs.Close();
            }

            sw = new StreamWriter(dataPath);

            while (true)
            {
                string log = "";

                log += "CAM : " + Camera.localPosition.ToString("F3") + subSplitter + Camera.localRotation.ToString("F3") + splitter;

                foreach (SteamVR_Behaviour_Pose tracker in Trackers)
                {
                    log += AppendLog(tracker);
                }

                sw.WriteLine(log);
                //Debug.Log(log);

                yield return wait;
            }

        }

        string AppendLog(SteamVR_Behaviour_Pose tracker)
        {
            string id = "" + tracker.inputSource.ToString()[0];

            if (Regex.Split(tracker.inputSource.ToString(), @"(?<!^)(?=[A-Z])").Length == 2)
                id += Regex.Split(tracker.inputSource.ToString(), @"(?<!^)(?=[A-Z])")[1][0];

            else
                id += char.ToUpper(tracker.inputSource.ToString()[1]);

            return id + " : " + tracker.transform.localPosition.ToString("F3") + subSplitter + tracker.transform.localRotation.ToString("F3") + splitter;
        }


        private IEnumerator RecordingJson()
        {
            var wait = new WaitForSecondsRealtime(1f / recordFps);

            recordDto = new RecordDto { recordFps = recordFps, userSettings = UserSettings.instance.GetUserSettingsData() };

            List<RecordKeyEntryDto> entries = new List<RecordKeyEntryDto>();
            recordDto.frames = new List<RecordKeyFrameDto>();

            while (true) // While iteration represents a frame
            {
                entries.Clear();
                entries.Add(new RecordKeyEntryDto((int)SteamVR_Input_Sources.Head, Camera.position.Dto(), Camera.rotation.Dto()));

                foreach (SteamVR_Behaviour_Pose tracker in Trackers) // Right iterate on every trackers
                {
                    if (tracker.inputSource == SteamVR_Input_Sources.RightHand || tracker.inputSource == SteamVR_Input_Sources.LeftHand)
                    {
                        entries.Add(new RecordKeyEntryDto((int) tracker.inputSource,
                            (tracker.transform.position - 0.15f * tracker.transform.forward).Dto(), tracker.transform.rotation.Dto()));
                    }
                    else if (tracker.inputSource == SteamVR_Input_Sources.Head)
                    {
                        entries.Add(new RecordKeyEntryDto((int) tracker.inputSource,
                              (tracker.transform.position - 0.11f * tracker.transform.forward).Dto(), tracker.transform.rotation.Dto()));
                    }
                    else
                    {
                        entries.Add(new RecordKeyEntryDto((int)tracker.inputSource, tracker.transform.position.Dto(), tracker.transform.rotation.Dto()));
                    }

                }
                // For Box do something like entries
                // Debug.Log(SteamVR_Input_Sources.Treadmill);
                entries.Add(new RecordKeyEntryDto((int)SteamVR_Input_Sources.Treadmill, Box.position.Dto(), Box.rotation.Dto()));

                recordDto.frames.Add(new RecordKeyFrameDto(entries));

                yield return wait;
            }

        }

        public int StopRecording()
        {
            if (IsRecording)
            {
                switch (currentRecordMode)
                {
                    case RecordMode.Json:
                        try
                        {
                            StopCoroutine(recording);

                            IsRecording = false;
                            Debug.Log("Records saved  : " + recordDto.frames.Count + " frames.");

                            StartCoroutine(SendDataRecordedToServer());

                            return recordDto.frames.Count;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    case RecordMode.Text:
                        StopCoroutine(recording);
                        sw.Close();
                        IsRecording = false;
                        Debug.Log("Records saved !");
                        break;
                    default:
                        break;
                }

                return 0;
            }
            return -1;
        }

        IEnumerator SendDataRecordedToServer()
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(recordDto,
                Formatting.Indented,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None }));

            using (UnityWebRequest request = new UnityWebRequest("localhost:50043" + EulenEndPoint.postRecord + "?movementId=" + currentRecordedMovementId, UnityWebRequest.kHttpVerbPOST))
            {
                var uploadHandler = new UploadHandlerRaw(bytes);
                uploadHandler.contentType = "application/json";

                request.uploadHandler = uploadHandler;

                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(request.error + " " + request.url);
                }
                else
                {
                    Debug.Log("Record upload complete!");
                }
            }
        }

        private RecordDto GenerateFakeData()
        {
            RecordDto record = new RecordDto()
            {
                recordFps = 60,
                userSettings = new UserSettingsDto
                {
                    trackerToBoneRotations = new() { { 0, Quaternion.Euler(1, 2, 3).Dto() }, { 1, Quaternion.Euler(4, 5, 6).Dto() } },
                    boneOffsets = new() { { 0, 0f } },
                    boneLenghts = new() { { 0, 1f } }
                },
                frames = new()
                    {
                        new RecordKeyFrameDto(new List<RecordKeyEntryDto>
                        {
                            new RecordKeyEntryDto(0,Vector3.zero.Dto(), Quaternion.Euler(1, 2, 3).Dto()),
                            new RecordKeyEntryDto(1,Vector3.zero.Dto(), Quaternion.Euler(1, 2, 3).Dto()),
                            new RecordKeyEntryDto(2,Vector3.zero.Dto(), Quaternion.Euler(1, 2, 3).Dto()),
                            new RecordKeyEntryDto(3,Vector3.zero.Dto(), Quaternion.Euler(1, 2, 3).Dto()),
                            new RecordKeyEntryDto(4,Vector3.zero.Dto(), Quaternion.Euler(1, 2, 3).Dto()),
                            new RecordKeyEntryDto(5,Vector3.zero.Dto(), Quaternion.Euler(1, 2, 3).Dto()),
                        })
                    }
            };

            return record;
        }
    }
}