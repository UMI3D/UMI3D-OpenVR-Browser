
using System.Collections.Generic;
using umi3d.cdk.userCapture;
using umi3dVRBrowsersBase.ikManagement;
using Unity.Barracuda;
using UnityEngine;

public class HipsPredictor : AbstractPredictor<Quaternion>
{

    public HipsPredictor(NNModel modelAsset) : base(modelAsset)
    { }

    protected override void Init()
    {
        base.Init();
        modelInput = new Tensor(1, 1, 21, 45); //initializing
    }

    public static Tensor FormatInputTensor(Transform head, Transform rHand, Transform lHand)
    {
        Tensor frameTensor = new Tensor(1, 1, 21, 1);

        // head

        frameTensor[0, 0, 0, 0] = head.position.x;
        frameTensor[0, 0, 1, 0] = head.position.y;
        frameTensor[0, 0, 2, 0] = head.position.z;

        frameTensor[0, 0, 3, 0] = head.rotation.x;
        frameTensor[0, 0, 4, 0] = head.rotation.y;
        frameTensor[0, 0, 5, 0] = head.rotation.z;
        frameTensor[0, 0, 6, 0] = head.rotation.w;

        // rightHand

        frameTensor[0, 0, 7, 0] = rHand.position.x;
        frameTensor[0, 0, 8, 0] = rHand.position.y;
        frameTensor[0, 0, 9, 0] = rHand.position.z;

        frameTensor[0, 0, 10, 0] = rHand.rotation.x;
        frameTensor[0, 0, 11, 0] = rHand.rotation.y;
        frameTensor[0, 0, 12, 0] = rHand.rotation.z;
        frameTensor[0, 0, 13, 0] = rHand.rotation.w;

        // leftHand

        frameTensor[0, 0, 14, 0] = lHand.position.x;
        frameTensor[0, 0, 15, 0] = lHand.position.y;
        frameTensor[0, 0, 16, 0] = lHand.position.z;

        frameTensor[0, 0, 17, 0] = lHand.rotation.x;
        frameTensor[0, 0, 18, 0] = lHand.rotation.y;
        frameTensor[0, 0, 19, 0] = lHand.rotation.z;
        frameTensor[0, 0, 20, 0] = lHand.rotation.w;

        return frameTensor;
    }

    private bool isTensorFull => idNextFrame == modelInput.channels;
    private int idNextFrame;

    public override void AddFrameInput(Tensor frame)
    {
        if (isTensorFull)
        {
            for (int i = 0; i < modelInput.channels - 1; i++)
            {
                // move a frame to the left
                for (int j = 0; j < modelInput.width; j++)
                    modelInput[0, 0, j, i] = modelInput[0, 0, j, i + 1];
            }
            // replace the last frame
            for (int j = 0; j < modelInput.width; j++)
                modelInput[0, 0, j, modelInput.channels - 1] = frame[0, 0, j, 0];
        }
        else
        {
            for (int i = 0; i < modelInput.width; i++)
                modelInput[0, 0, i, idNextFrame] = frame[0, 0, i, 0];

            idNextFrame++;
        }
    }

    protected List<Tensor> ExecuteModel()
    {
        List<Tensor> outputs = new List<Tensor>();
        mainWorker.Execute(modelInput);

        outputs.Add(mainWorker.PeekOutput());
        return outputs;
    }

    public override Quaternion GetPrediction()
    {
        var output = ExecuteModel();
        return new Quaternion(output[0][0, 0, 0, 3],
                              output[0][0, 0, 0, 4],
                              output[0][0, 0, 0, 5],
                              output[0][0, 0, 0, 6]);
    }
}