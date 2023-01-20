using Unity.Barracuda;
using UnityEngine;

public class HipsPredictor : AbstractPredictor<(Vector3 pos, Quaternion rot)>
{
    protected readonly int NB_PARAMETERS = 7;
    protected readonly int NB_TRACKED_LIMBS = 3;
    protected const int NB_FRAMES_MAX = 45;

    public HipsPredictor(NNModel modelAsset) : base(modelAsset)
    { }

    public HipsPredictor(NNModel modelAsset, int nbParameters, int nbTrackedLimbs) : base(modelAsset)
    {
        NB_PARAMETERS = nbParameters;
        NB_TRACKED_LIMBS = nbTrackedLimbs;
    }

    public override void Init()
    {
        base.Init();
        modelInput = new Tensor(1, 1, NB_PARAMETERS * NB_TRACKED_LIMBS, NB_FRAMES_MAX); //initializing
    }

    public virtual Tensor FormatInputTensor(Transform head, Transform rHand, Transform lHand, Transform referencePoint)
    {
        Tensor frameTensor = new Tensor(1, 1, NB_PARAMETERS * NB_TRACKED_LIMBS, 1);

        void FillUp(int startIndex, Transform go, out int endIndex)
        {
            int index = startIndex;

            // global position for each tracked limb
            var pos = go.position; // - referencePoint.position;
            frameTensor[0, 0, index++, 0] = pos.x;
            frameTensor[0, 0, index++, 0] = pos.y;
            frameTensor[0, 0, index++, 0] = pos.z;

            // global rotation as quaternion coordinates for each tracked limb
            frameTensor[0, 0, index++, 0] = go.rotation.x;
            frameTensor[0, 0, index++, 0] = go.rotation.y;
            frameTensor[0, 0, index++, 0] = go.rotation.z;
            frameTensor[0, 0, index++, 0] = go.rotation.w;

            endIndex = index;
        }

        // head
        int index = 0;
        FillUp(index, head, out index);
        FillUp(index, rHand, out index);
        FillUp(index, lHand, out _);

        return frameTensor;
    }

    public override (Vector3 pos, Quaternion rot) GetPrediction()
    {
        var output = ExecuteModel();

        int idx = 0;
        (Vector3 pos, Quaternion rot) results = (
                                                new Vector3(output[0][0, 0, 0, idx++],
                                                            output[0][0, 0, 0, idx++],
                                                            output[0][0, 0, 0, idx++]),
                                                new Quaternion(output[0][0, 0, 0, idx++],
                                                               output[0][0, 0, 0, idx++],
                                                               output[0][0, 0, 0, idx++],
                                                               output[0][0, 0, 0, idx++]));

        return results;
    }
}

public class HipsPredictorV3 : HipsPredictor
{
    public HipsPredictorV3(NNModel modelAsset) : base(modelAsset, nbParameters: 9, nbTrackedLimbs: 3)
    { }

    public override Tensor FormatInputTensor(Transform head, Transform rHand, Transform lHand, Transform referencePoint)
    {
        var frameTensor = new Tensor(1, 1, NB_PARAMETERS * NB_TRACKED_LIMBS, 1);

        void FillUp(int startIndex, Transform go, out int endIndex)
        {
            int index = startIndex;

            // global position for each tracked limb
            var pos = go.position; // - referencePoint.position;
            frameTensor[0, 0, index++, 0] = pos.x;
            frameTensor[0, 0, index++, 0] = pos.y;
            frameTensor[0, 0, index++, 0] = pos.z;

            // global rotation as rotation matrix columns for each tracked limb
            var rightNormalized = go.right.normalized;
            var upNormalized = go.up.normalized;
            frameTensor[0, 0, index++, 0] = rightNormalized.x; //first column of the rotation matrix
            frameTensor[0, 0, index++, 0] = rightNormalized.y;
            frameTensor[0, 0, index++, 0] = rightNormalized.z;
            frameTensor[0, 0, index++, 0] = upNormalized.x; //second column of the rotation matrix
            frameTensor[0, 0, index++, 0] = upNormalized.y;
            frameTensor[0, 0, index++, 0] = upNormalized.z;

            endIndex = index;
        }

        int index = 0;
        FillUp(index, head, out index);
        FillUp(index, rHand, out index);
        FillUp(index, lHand, out _);

        return frameTensor;
    }

    public override (Vector3 pos, Quaternion rot) GetPrediction()
    {
        var output = ExecuteModel();

        int i = 0;
        Vector3 pos = new Vector3(output[0][0, 0, 0, i++],
                                  output[0][0, 0, 0, i++],
                                  output[0][0, 0, 0, i++]);

        var zAxis = new Vector3(output[0][0, 0, 0, i++],
                                output[0][0, 0, 0, i++],
                                output[0][0, 0, 0, i++]).normalized;

        /*
         * Simpler alternative :
         *  Quaternion rot = Quaternion.FromToRotation(Vector3.foward, zAxis);
         *  Quaternion rot = Quaternion.LookRotation(zAxis, Vector3.up);
         */

        Vector3 zXZ = PredictorUtils.ProjOnPlan(zAxis, Vector3.up);
        float angleY = Vector3.SignedAngle(Vector3.forward, zXZ, Vector3.up);
        float angleX = Vector3.SignedAngle(zXZ, zAxis, Vector3.Cross(Vector3.up, zXZ));
        Matrix4x4 mat = PredictorUtils.RotationMatrix(angleX, angleY, 0f);
        var yAxis = (mat * Vector3.up).normalized;

        var xAxis = Vector3.Cross(yAxis, zAxis);

        Quaternion rot = PredictorUtils.MatrixToQuaternion(PredictorUtils.LocalRotationMatrix(xAxis, yAxis, zAxis));

        (Vector3 pos, Quaternion rot) result = (pos, rot);
        return result;
    }
}