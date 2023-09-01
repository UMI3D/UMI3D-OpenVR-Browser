using com.inetum.addonEulen.common.dtos;
using com.inetum.addonEulen.common;
using inetum.unityUtils;
using System.Collections;
using System.Text.RegularExpressions;
using umi3d.cdk.collaboration;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class EulenMessagesSender : SingleBehaviour<EulenMessagesSender>
{

    public void SendDataRecordedToServer(RecordDto recordDto, int movementId)
    {
        string url = Regex.Replace(UMI3DCollaborationClientServer.Instance.environementHttpUrl + EulenEndPoint.postRecord, ":param", movementId.ToString());
        StartCoroutine(SendMessagesToServerCoroutine(recordDto, movementId, url));
    }

    public void SendMovementValidation(MovementValidationDto validationDto, int movementId)
    {
        string url = UMI3DCollaborationClientServer.Instance.environementHttpUrl + EulenEndPoint.postValidation;
        StartCoroutine(SendMessagesToServerCoroutine(validationDto, movementId, url));
    }

    private IEnumerator SendMessagesToServerCoroutine(object obj, int movementId, string url)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(obj,
            Formatting.Indented,
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None }));

        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
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
                Debug.Log("Record upload complete ! " + request.uri);
            }
        }
    }
}
