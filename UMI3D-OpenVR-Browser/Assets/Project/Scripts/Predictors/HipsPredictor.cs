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

    protected override void Init()
    {
        base.Init();
        modelInput = new Tensor(1, 1, NB_PARAMETERS * NB_TRACKED_LIMBS, NB_FRAMES_MAX); //initializing
    }

    public virtual Tensor FormatInputTensor(Transform head, Transform rHand, Transform lHand)
    {
        Tensor frameTensor = new Tensor(1, 1, NB_PARAMETERS * NB_TRACKED_LIMBS, 1);

        void FillUp(int startIndex, Transform go, out int endIndex)
        {
            int index = startIndex;
            frameTensor[0, 0, index++, 0] = go.position.x;
            frameTensor[0, 0, index++, 0] = go.position.y;
            frameTensor[0, 0, index++, 0] = go.position.z;

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

    public override Tensor FormatInputTensor(Transform head, Transform rHand, Transform lHand)
    {
        var frameTensor = new Tensor(1, 1,  NB_PARAMETERS * NB_TRACKED_LIMBS, 1);

        void FillUp(int startIndex, Transform go, out int endIndex)
        {
            int index = startIndex;
            frameTensor[0, 0, index++, 0] = go.position.x;
            frameTensor[0, 0, index++, 0] = go.position.y;
            frameTensor[0, 0, index++, 0] = go.position.z;

            var rightNormalized = go.right.normalized;
            var upNormalized = go.up.normalized;
            frameTensor[0, 0, index++, 0] = rightNormalized.x;
            frameTensor[0, 0, index++, 0] = rightNormalized.y;
            frameTensor[0, 0, index++, 0] = rightNormalized.z;
            frameTensor[0, 0, index++, 0] = upNormalized.x;
            frameTensor[0, 0, index++, 0] = upNormalized.y;
            frameTensor[0, 0, index++, 0] = upNormalized.z;

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

        int i = 0;
        Vector3 pos = new Vector3(output[0][0, 0, 0, i++],
                                  output[0][0, 0, 0, i++],
                                  output[0][0, 0, 0, i++]);

        var zAxis = new Vector3(output[0][0, 0, 0, i++],
                                output[0][0, 0, 0, i++],
                                output[0][0, 0, 0, i++]).normalized;

        /*
         * Simpler alternative :
         *  Quaternion rot = Quaternion.FromToRotation(Vector3.up, zAxis);
         */

        Vector3 zXZ = ProjOnPlan(zAxis, Vector3.up);
        float angleY = Vector3.SignedAngle(Vector3.forward, zXZ, Vector3.up);
        float angleX = Vector3.SignedAngle(zXZ, zAxis, Vector3.Cross(Vector3.up, zXZ));
        Matrix4x4 mat = RotationMatrix(angleX, angleY, 0f);
        var yAxis = (mat * Vector3.up).normalized;

        var xAxis = Vector3.Cross(yAxis, zAxis);

        Quaternion rot = MatrixToQuaternion(LocalRotationMatrix(xAxis, yAxis, zAxis));

        (Vector3 pos, Quaternion rot) result = (pos, rot);
        return result;
    }

    /// <summary>
    /// compute the projected value of a vector onto a plane
    /// </summary>
    private Vector3 ProjOnPlan(Vector3 u, Vector3 n)
    {
        float num = Vector3.Dot(u, n);
        float den = Mathf.Pow(VecNorm(n), 2f);
        Vector3 vertComp = (num / den) * n;

        Vector3 proj = u - vertComp;
        return proj;
    }

    private Matrix4x4 RotationMatrix(float rX, float rY, float rZ)
    {
        Matrix4x4 matZ = Matrix4x4.identity;
        matZ[0, 0] = Mathf.Cos(rZ * Mathf.Deg2Rad); matZ[0, 1] = -Mathf.Sin(rZ * Mathf.Deg2Rad);
        matZ[1, 0] = Mathf.Sin(rZ * Mathf.Deg2Rad); matZ[1, 1] = Mathf.Cos(rZ * Mathf.Deg2Rad);

        Matrix4x4 matX = Matrix4x4.identity;
        matX[1, 1] = Mathf.Cos(rX * Mathf.Deg2Rad); matX[1, 2] = -Mathf.Sin(rX * Mathf.Deg2Rad);
        matX[2, 1] = Mathf.Sin(rX * Mathf.Deg2Rad); matX[2, 2] = Mathf.Cos(rX * Mathf.Deg2Rad);

        Matrix4x4 matY = Matrix4x4.identity;
        matY[0, 0] = Mathf.Cos(rY * Mathf.Deg2Rad); matY[0, 2] = Mathf.Sin(rY * Mathf.Deg2Rad);
        matY[2, 0] = -Mathf.Sin(rY * Mathf.Deg2Rad); matY[2, 2] = Mathf.Cos(rY * Mathf.Deg2Rad);

        return matY * matX * matZ;
    }

    private float VecNorm(Vector3 v)
    {
        return Mathf.Sqrt(Mathf.Pow(v.x, 2f) + Mathf.Pow(v.y, 2f) + Mathf.Pow(v.z, 2f));
    }

    private Matrix4x4 LocalRotationMatrix(Vector3 x, Vector3 y, Vector3 z)
    {
        Matrix4x4 mat = Matrix4x4.identity;

        mat[0, 0] = x.x;
        mat[1, 0] = x.y;
        mat[2, 0] = x.z;

        mat[0, 1] = y.x;
        mat[1, 1] = y.y;
        mat[2, 1] = y.z;

        mat[0, 2] = z.x;
        mat[1, 2] = z.y;
        mat[2, 2] = z.z;

        return mat;
    }

    private Quaternion MatrixToQuaternion(Matrix4x4 mat)
    {
        Quaternion quat = new Quaternion();
        float c = mat[0, 0], a = mat[0, 1], d = mat[0, 2];
        float e = mat[1, 0], f = mat[1, 1], g = mat[1, 2];
        float h = mat[2, 0], k = mat[2, 1], b = mat[2, 2];

        float l = c + f + b;
        if (0 < l)
        {
            c = 0.5f / Mathf.Sqrt(l + 1);
            quat.x = (k - g) * c;
            quat.y = (d - h) * c;
            quat.z = (e - a) * c;
            quat.w = 0.25f / c;
        }
        else if (c > f && c > b)
        {
            c = 2 * Mathf.Sqrt(1 + c - f - b);
            quat.x = 0.25f * c;
            quat.y = (a + e) / c;
            quat.z = (d + h) / c;
            quat.w = (k - g) / c;
        }
        else if (f > b)
        {
            c = 2 * Mathf.Sqrt(1 + f - c - b);
            quat.x = (a + e) / c;
            quat.y = 0.25f * c;
            quat.z = (g + k) / c;
            quat.w = (d - h) / c;
        }
        else
        {
            c = 2 * Mathf.Sqrt(1 + b - c - f);
            quat.x = (d + h) / c;
            quat.y = (g + k) / c;
            quat.z = 0.25f * c;
            quat.w = (e - a) / c;
        }

        return quat;
    }
}