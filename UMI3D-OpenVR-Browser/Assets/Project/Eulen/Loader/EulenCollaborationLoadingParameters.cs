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
                char genre = UMI3DSerializer.Read<char>(container);
                bool displayAvatar = UMI3DSerializer.Read<bool>(container);
                RecordDto record = UMI3DSerializer.Read<RecordDto>(container);
                Debug.Log(displayAvatar + " " + record);

                DrawAvatar.Instance.Replay(record, displayAvatar, genre);

                return Task.CompletedTask;
            case EulenPropertyKeys.startRecord:
                int movementId = UMI3DSerializer.Read<int>(container);

                Debug.Log("Start record " + movementId);
                DrawAvatar.Instance.StopReplay();

                FullBodyRecording.instance.StartRecording(movementId);
                return Task.CompletedTask;
            case EulenPropertyKeys.stopRecord:
                Debug.Log("Stop record " + UMI3DSerializer.Read<int>(container));

                FullBodyRecording.instance.StopRecording();
                return Task.CompletedTask;
            case EulenPropertyKeys.stopReplay:
                DrawAvatar.Instance.StopReplay();
                DrawAvatar.Instance.HideReplay();
                return Task.CompletedTask;
            case EulenPropertyKeys.sendSummary:
                try
                {
                    SummaryGenerator.Generate(container);

                } catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }

                return Task.CompletedTask;
            default:
                break;
        }
        return base.UnknownOperationHandler(operationId, container);
    }
}