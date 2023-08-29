using com.inetum.addonEulen.common;
using com.inetum.addonEulen.common.dtos;
using com.inetum.eulen.recording.app;
using System.Threading.Tasks;
using umi3d.cdk.collaboration;
using umi3d.common;
using UnityEngine;

[CreateAssetMenu(fileName = "EulenLoadingParameters", menuName = "Eulen/Eulen Loading Parameters")]
public class EulenCollaborationLoadingParameters : UMI3DCollabLoadingParameters
{
    public override Task UnknownOperationHandler(uint operationId, ByteContainer container)
    {
        switch (operationId)
        {
            case EulenPropertyKeys.playRecord:
                bool displayAvatar = UMI3DSerializer.Read<bool>(container);
                RecordDto record = UMI3DSerializer.Read<RecordDto>(container);
                Debug.Log(displayAvatar + " " + record);

                DrawAvatar.Instance.Replay(record, displayAvatar);

                return Task.CompletedTask;
            case EulenPropertyKeys.startRecord:
                int movementId = UMI3DSerializer.Read<int>(container);

                Debug.Log("Start record " + movementId);
                DrawAvatar.Instance.StopReplay();

                FullBodyRecording.instance.StartRecording(FullBodyRecording.RecordMode.Json, movementId);
                return Task.CompletedTask;
            case EulenPropertyKeys.stopRecord:
                Debug.Log("Stop record " + UMI3DSerializer.Read<int>(container));

                FullBodyRecording.instance.StopRecording();
                return Task.CompletedTask;
            default:
                break;
        }
        return base.UnknownOperationHandler(operationId, container);
    }
}