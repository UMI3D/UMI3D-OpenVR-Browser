using Unity.Barracuda;
using UnityEngine;

public class FootPredictor : AbstractPredictor<(Vector3 posLFoot, Vector3 posRFoot)>
{
    protected readonly int NB_PARAMETERS = 7;
    protected readonly int NB_TRACKED_LIMBS = 4;
    protected const int NB_FRAMES_MAX = 45;



    public FootPredictor(NNModel modelAsset) : base(modelAsset)
    { }

    protected override void Init()
    {
        base.Init();
        modelInput = new Tensor(1, 1, NB_PARAMETERS * NB_TRACKED_LIMBS, NB_FRAMES_MAX); //initializing
    }


    public virtual Tensor FormatInputTensor(Transform head, Transform rHand, Transform lHand, Transform hips)
    {
        Tensor frameTensor = new Tensor(1, 1, NB_PARAMETERS * NB_TRACKED_LIMBS, 1);

        void FillUp(int startIndex, Transform go, out int endIndex)
        {
            int index = startIndex;
            frameTensor[0, 0, index, 0] = go.position.x;
            frameTensor[0, 0, ++index, 0] = go.position.y;
            frameTensor[0, 0, ++index, 0] = go.position.z;

            frameTensor[0, 0, ++index, 0] = go.rotation.x;
            frameTensor[0, 0, ++index, 0] = go.rotation.y;
            frameTensor[0, 0, ++index, 0] = go.rotation.z;
            frameTensor[0, 0, ++index, 0] = go.rotation.w;

            endIndex = index;
        }

        // head
        int index = 0;
        FillUp(index, head, out index);
        FillUp(index, rHand, out index);
        FillUp(index, lHand, out index);
        FillUp(index, hips, out _);

        return frameTensor;
    }

    public override (Vector3 posLFoot, Vector3 posRFoot) GetPrediction()
    {
        var output = ExecuteModel();

        int index=0;
        Vector3 posLFoot = new Vector3(output[0][0, 0, 0, index++],
                                       output[0][0, 0, 0, index++],
                                       output[0][0, 0, 0, index++]);

        Vector3 posRFoot = new Vector3(output[0][0, 0, 0, index++],
                                       output[0][0, 0, 0, index++],
                                       output[0][0, 0, 0, index++]);


        (Vector3 posLFoot, Vector3 posRFoot) result = (posLFoot, posRFoot);
        return result;
    }
}