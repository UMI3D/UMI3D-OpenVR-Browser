using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class HipsPredictor : AbstractPredictor<(Vector3 pos, Quaternion rot)?>
{
    protected readonly int NB_PARAMETERS = 7;
    protected readonly int NB_TRACKED_LIMBS = 3;
    protected const int NB_FRAMES_MAX = 45;

    public HipsPredictor(NNModel modelAsset, MonoBehaviour ownerMono) : base(modelAsset, ownerMono)
    { }

    public HipsPredictor(NNModel modelAsset, int nbParameters, int nbTrackedLimbs, MonoBehaviour mono) : base(modelAsset, mono)
    {
        NB_PARAMETERS = nbParameters;
        NB_TRACKED_LIMBS = nbTrackedLimbs;
    }

    public override void Init()
    {
        base.Init();
        modelInput = new Tensor(1, 1, NB_PARAMETERS * NB_TRACKED_LIMBS, NB_FRAMES_MAX); //initializing
    }

    public virtual float[] FormatInputTensor(Transform head, Transform rHand, Transform lHand, Transform referencePoint)
    {
        float[] frameTensor = new float[NB_PARAMETERS * NB_TRACKED_LIMBS];

        void FillUp(int startIndex, Transform go, out int endIndex)
        {
            int index = startIndex;

            // global position for each tracked limb
            var pos = go.position; // - referencePoint.position;
            frameTensor[index++] = pos.x;
            frameTensor[index++] = pos.y;
            frameTensor[index++] = pos.z;

            // global rotation as quaternion coordinates for each tracked limb
            frameTensor[index++] = go.rotation.x;
            frameTensor[index++] = go.rotation.y;
            frameTensor[index++] = go.rotation.z;
            frameTensor[index++] = go.rotation.w;

            endIndex = index;
        }

        // head
        int index = 0;
        FillUp(index, head, out index);
        FillUp(index, rHand, out index);
        FillUp(index, lHand, out _);

        return frameTensor;
    }

    public override (Vector3 pos, Quaternion rot)? GetPrediction(bool isAsync = false)
    {
        List<Tensor> output = isAsync ? ExecuteModelAsync() : ExecuteModel();
        if (output == null)
        {
            Debug.LogWarning("Prediction of hips requested before available");
            return null;
        }

        int idx = 0;
        (Vector3 pos, Quaternion rot) results = (
                                                new Vector3(output[0][0, 0, 0, idx++],
                                                            output[0][0, 0, 0, idx++],
                                                            output[0][0, 0, 0, idx++]),
                                                new Quaternion(output[0][0, 0, 0, idx++],
                                                               output[0][0, 0, 0, idx++],
                                                               output[0][0, 0, 0, idx++],
                                                               output[0][0, 0, 0, idx++]));

        foreach (var tensor in output)
            tensor.Dispose();

        return results;
    }
}

public class HipsPredictorV3 : HipsPredictor
{
    public HipsPredictorV3(NNModel modelAsset, MonoBehaviour ownerMono) : base(modelAsset, 9, 3, ownerMono)
    { }

    public override float[] FormatInputTensor(Transform head, Transform rHand, Transform lHand, Transform referencePoint)
    {
        float[] frameTensor = new float[NB_PARAMETERS * NB_TRACKED_LIMBS];

        void FillUp(int startIndex, Transform go, out int endIndex)
        {
            int index = startIndex;

            // global position for each tracked limb
            var pos = go.position - referencePoint.position; // - referencePoint.position;
            frameTensor[index++] = pos.x;
            frameTensor[index++] = pos.y;
            frameTensor[index++] = pos.z;

            // global rotation as rotation matrix columns for each tracked limb
            var rightNormalized = go.right.normalized;
            var upNormalized = go.up.normalized;
            frameTensor[index++] = rightNormalized.x; //first column of the rotation matrix
            frameTensor[index++] = rightNormalized.y;
            frameTensor[index++] = rightNormalized.z;
            frameTensor[index++] = upNormalized.x; //second column of the rotation matrix
            frameTensor[index++] = upNormalized.y;
            frameTensor[index++] = upNormalized.z;

            endIndex = index;
        }

        int index = 0;
        FillUp(index, head, out index);
        FillUp(index, rHand, out index);
        FillUp(index, lHand, out _);

        return frameTensor;
    }

    public override (Vector3 pos, Quaternion rot)? GetPrediction(bool isAsync = false)
    {
        List<Tensor> output = isAsync ? ExecuteModelAsync() : ExecuteModel();
        if (output == null)
        {
            Debug.LogWarning("Prediction of hips requested before available");
            return null;
        }

        int i = 0;
        Vector3 pos = new Vector3(output[0][0, 0, 0, i++],
                                  output[0][0, 0, 0, i++],
                                  output[0][0, 0, 0, i++]);

        var zAxis = new Vector3(output[0][0, 0, 0, i++],
                                output[0][0, 0, 0, i++],
                                output[0][0, 0, 0, i++]).normalized;

        /*
         * Simpler alternative :
         *  Quaternion rot = Quaternion.LookRotation(zAxis, Vector3.up);
         */

        Vector3 zXZ = PredictorUtils.ProjOnPlan(zAxis, Vector3.up);
        float angleY = Vector3.SignedAngle(Vector3.forward, zXZ, Vector3.up);
        float angleX = Vector3.SignedAngle(zXZ, zAxis, Vector3.Cross(Vector3.up, zXZ));
        Matrix4x4 mat = PredictorUtils.RotationMatrix(angleX, angleY, 0f);
        var yAxis = (mat * Vector3.up).normalized;

        Quaternion rot = Quaternion.LookRotation(zAxis, yAxis);

        (Vector3 pos, Quaternion rot) result = (pos, rot);

        foreach (var tensor in output)
            tensor.Dispose();

        return result;
    }
}