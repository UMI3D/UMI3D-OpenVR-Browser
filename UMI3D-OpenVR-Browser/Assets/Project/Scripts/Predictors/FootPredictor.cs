using Unity.Barracuda;
using UnityEngine;

public class FootPredictor : AbstractPredictor<(Vector3, Vector3)>
{
    public FootPredictor(NNModel modelAsset) : base(modelAsset)
    { }

    public override void AddFrameInput(Tensor tensor)
    {
        throw new System.NotImplementedException();
    }

    public override (Vector3, Vector3) GetPrediction()
    {
        throw new System.NotImplementedException();
    }
}