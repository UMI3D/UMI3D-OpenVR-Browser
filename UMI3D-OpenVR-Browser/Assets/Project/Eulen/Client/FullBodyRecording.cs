using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System;
using com.inetum.addonEulen.common.dtos;
using com.inetum.addonEulen.common;
using UnityEngine.Networking;
using umi3d.cdk.collaboration;
using System.Linq;
using umi3d.cdk;
using System.Threading.Tasks;

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

        public static FullBodyRecording instance;

        #region Fields

        [SerializeField]
        [Range(0, 120)]
        public int recordFps = 60;

        public Transform Camera;

        public List<SteamVR_Behaviour_Pose> Trackers;

        public bool IsRecording { get; private set; } = false;

        private Transform box;

        public Transform boxReplay;

        #endregion

        #region RecordJsonData

        private int currentRecordedMovementId = 0;

        private RecordDto recordDto;

        private Coroutine recordCoroutine;

        #endregion

        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            UMI3DEnvironmentClient.EnvironementLoaded.AddListener(OnEnvironmentLoaded);      
        }

        private void OnDisable()
        {
            UMI3DEnvironmentClient.EnvironementLoaded.RemoveListener(OnEnvironmentLoaded);
            box = null;
        }

        private void OnEnvironmentLoaded()
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

            watch.Start();
            Debug.Log("Resources box 0: " + Resources.FindObjectsOfTypeAll<Transform>().Where(o => o.name == "BoxTraining").Count());
            box = Resources.FindObjectsOfTypeAll<Transform>().Where(o => o.name == "BoxTraining" && o.parent != UMI3DResourcesManager.Instance.transform && o != boxReplay)?.FirstOrDefault();
            watch.Stop();

            // About 3 ms
            Debug.Log("BOX FOUND " + (box != null) + " " + watch.ElapsedMilliseconds + " Parent: " + box?.parent.name);
        }

        public void StartRecording(int movementId)
        {
            if (IsRecording)
            {
                Debug.LogError("Already recording");
                return;
            }

            IsRecording = true;
            currentRecordedMovementId = movementId;

            recordCoroutine = StartCoroutine(RecordingJson());

            Debug.Assert(box != null, "Box should not be null");
        }

        private IEnumerator RecordingJson()
        {
            var wait = new WaitForSecondsRealtime(1f / recordFps);

            // As we only replay with a wireframe avatar for now, user settings are empty.
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

                if (box != null)
                {
                    entries.Add(new RecordKeyEntryDto((int)SteamVR_Input_Sources.Treadmill, box.position.Dto(), box.rotation.Dto()));
                }
                recordDto.frames.Add(new RecordKeyFrameDto(entries));

                yield return wait;
            }

        }

        public async Task<int> StopRecording()
        {
            if (IsRecording)
            {
                try
                {
                    StopCoroutine(recordCoroutine);

                    IsRecording = false;
                    Debug.Log("Records saved  : " + recordDto.frames.Count + " frames.");

                    EulenMessagesSender.Instance.SendDataRecordedToServer(recordDto, currentRecordedMovementId);
                    await UMI3DAsyncManager.Delay(3000);
                    DrawAvatar.Instance.ValidateCompleteMovement(recordDto, currentRecordedMovementId);

                    return recordDto.frames.Count;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

            }
            return -1;
        }

       
    }
}