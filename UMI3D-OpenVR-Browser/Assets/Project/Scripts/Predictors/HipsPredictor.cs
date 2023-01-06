using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class HipsPredictor : AbstractPredictor<(Vector3 pos, Quaternion rot)>
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

        void FillUp(int startIndex, Transform go, out int endIndex)
        {
            frameTensor[0, 0, startIndex, 0] = go.position.x;
            frameTensor[0, 0, ++startIndex, 0] = go.position.y;
            frameTensor[0, 0, ++startIndex, 0] = go.position.z;

            frameTensor[0, 0, ++startIndex, 0] = go.rotation.x;
            frameTensor[0, 0, ++startIndex, 0] = go.rotation.y;
            frameTensor[0, 0, ++startIndex, 0] = go.rotation.z;
            frameTensor[0, 0, ++startIndex, 0] = go.rotation.w;

            endIndex = startIndex;
        }

        // head
        int index = 0;
        FillUp(index, head, out index);
        FillUp(index, rHand, out index);
        FillUp(index, lHand, out _);

        return frameTensor;
    }

    protected bool isTensorFull => idNextFrame == modelInput.channels;
    protected int idNextFrame;

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

    public override (Vector3 pos, Quaternion rot) GetPrediction()
    {
        var output = ExecuteModel();

        (Vector3 pos, Quaternion rot) results = (
                                                new Vector3(output[0][0, 0, 0, 0],
                                                  output[0][0, 0, 0, 1],
                                                  output[0][0, 0, 0, 2]),
                                                new Quaternion(output[0][0, 0, 0, 3],
                                                  output[0][0, 0, 0, 4],
                                                  output[0][0, 0, 0, 5],
                                                  output[0][0, 0, 0, 6]));

        return results;
    }
}

public class HipsPredictorV3 : HipsPredictor
{
    public HipsPredictorV3(NNModel modelAsset) : base(modelAsset)
    {
    }

    public new static Tensor FormatInputTensor(Transform head, Transform rHand, Transform lHand)
    {
        var frameTensor = new Tensor(1, 1, 27, 1);

        void FillUp(int startIndex, Transform go, out int endIndex)
        {
            frameTensor[0, 0, startIndex, 0] = go.position.x;
            frameTensor[0, 0, ++startIndex, 0] = go.position.y;
            frameTensor[0, 0, ++startIndex, 0] = go.position.z;

            var rightNormalized = go.right.normalized;
            var upNormalized = go.up.normalized;
            frameTensor[0, 0, ++startIndex, 0] = rightNormalized.x;
            frameTensor[0, 0, ++startIndex, 0] = rightNormalized.y;
            frameTensor[0, 0, ++startIndex, 0] = rightNormalized.z;
            frameTensor[0, 0, ++startIndex, 0] = upNormalized.x;
            frameTensor[0, 0, ++startIndex, 0] = upNormalized.y;
            frameTensor[0, 0, ++startIndex, 0] = upNormalized.z;

            endIndex = startIndex;
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

        Vector3 pos = new Vector3(output[0][0, 0, 0, 0],
                                  output[0][0, 0, 0, 1],
                                  output[0][0, 0, 0, 2]);

        var zAxis = new Vector3(output[0][0, 0, 0, 3], output[0][0, 0, 0, 4], output[0][0, 0, 0, 5]).normalized;

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