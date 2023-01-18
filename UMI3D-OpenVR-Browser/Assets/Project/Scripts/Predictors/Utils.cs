

using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public static class PredictorUtils
{
    /// <summary>
    /// compute the projected value of a vector onto a plane
    /// </summary>
    public static Vector3 ProjOnPlan(Vector3 u, Vector3 n)
    {
        float num = Vector3.Dot(u, n);
        float den = Mathf.Pow(VecNorm(n), 2f);
        Vector3 vertComp = (num / den) * n;

        Vector3 proj = u - vertComp;
        return proj;
    }

    public static Matrix4x4 RotationMatrix(float rX, float rY, float rZ)
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

    public static float VecNorm(Vector3 v)
    {
        return Mathf.Sqrt(Mathf.Pow(v.x, 2f) + Mathf.Pow(v.y, 2f) + Mathf.Pow(v.z, 2f));
    }

    public static Matrix4x4 LocalRotationMatrix(Vector3 x, Vector3 y, Vector3 z)
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

    public static Quaternion MatrixToQuaternion(Matrix4x4 mat)
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

    public static Matrix4x4 QuaternionToMatrix(Quaternion quat)
    {
        Matrix4x4 mat = new Matrix4x4();
        float c = quat.x, d = quat.y, e = quat.z, f = quat.w;
        float g = c + c, h = d + d, k = e + e, a = c * g;
        float l = c * h; c = c * k;
        float m = d * h; d = d * k;
        e = e * k; g = f * g;
        h = f * h; f = f * k;
        mat[0, 0] = 1 - (m + e); mat[0, 1] = l - f; mat[0, 2] = c + h; mat[0, 3] = 0;
        mat[1, 0] = l + f; mat[1, 1] = 1 - (a + e); mat[1, 2] = d - g; mat[1, 3] = 0;
        mat[2, 0] = c - h; mat[2, 1] = d + g; mat[2, 2] = 1 - (a + m); mat[2, 3] = 0;
        mat[3, 0] = 0; mat[3, 1] = 0; mat[3, 2] = 0; mat[3, 3] = 1;
        return mat;
    }

    public static Quaternion ComputeRefJointRot(Matrix4x4 rot)
    {
        Vector3 hipsZ = new Vector3(rot[0, 2], rot[1, 2], rot[2, 2]);
        Vector3 refZ = ProjOnPlan(hipsZ.normalized, Vector3.up);
        Vector3 refX = Vector3.Cross(Vector3.up, refZ);
        Matrix4x4 matRefRot = LocalRotationMatrix(refX.normalized, Vector3.up, refZ.normalized);
        Quaternion res = MatrixToQuaternion(matRefRot);
        return res;
    }

    public static Quaternion ComputeRefJointRot(Vector3 hipsZ)
    {
        Vector3 refZ = ProjOnPlan(hipsZ.normalized, Vector3.up);
        Vector3 refX = Vector3.Cross(Vector3.up, refZ);
        Matrix4x4 matRefRot = LocalRotationMatrix(refX.normalized, Vector3.up, refZ.normalized);
        Quaternion res = MatrixToQuaternion(matRefRot);
        return res;
    }

    public static Tensor ComputeVelocities(List<Vector3> jointPositions, List<Quaternion> jointRotations, List<List<float>> otherInputs)
    {
        Tensor frame = new Tensor(1, 1, 37, 1);
        int lastFrameIndex = jointPositions.Count - 1;
        Vector3 posRef = jointPositions[lastFrameIndex];
        Quaternion rotRef = jointRotations[lastFrameIndex];
        Vector3 posRefBef = new Vector3();
        Quaternion rotRefBef = new Quaternion();

        Vector3 vRef;
        Quaternion wRef;

        Vector3 posJoint;
        Quaternion rotJoint;

        Vector3 posJointBef;
        Quaternion rotJointBef;

        Vector3 pp;
        Quaternion qp;

        Vector3 ppBef;
        Quaternion qpBef;

        Vector3 vJoint;
        Quaternion wJoint;

        Matrix4x4 matRef;
        Matrix4x4 matJoint;

        if (jointPositions.Count == 1)
        {
            vRef = MultQuatVec(InvQuat(rotRef), posRef - posRef);
            wRef = MultQuat(MultQuat(InvQuat(rotRef), InvQuat(rotRef)), rotRef);
        }
        else
        {
            posRefBef = jointPositions[lastFrameIndex - 1];
            rotRefBef = jointRotations[lastFrameIndex - 1];
            vRef = MultQuatVec(InvQuat(rotRef), posRef - posRefBef);
            wRef = MultQuat(MultQuat(InvQuat(rotRef), InvQuat(rotRefBef)), rotRef);
        }

        int index = 0;
        frame[0, 0, w: index++, 0] = vRef.x;
        frame[0, 0, w: index++, 0] = vRef.y;
        frame[0, 0, w: index++, 0] = vRef.z;
        matRef = QuaternionToMatrix(wRef);
        frame[0, 0, w: index++, 0] = matRef[0, 0];
        frame[0, 0, w: index++, 0] = matRef[1, 0];
        frame[0, 0, w: index++, 0] = matRef[2, 0];
        frame[0, 0, w: index++, 0] = matRef[0, 1];
        frame[0, 0, w: index++, 0] = matRef[1, 1];
        frame[0, 0, w: index++, 0] = matRef[2, 1];

        for (int i = 0; i < 3; i++)
        {
            posJoint = new Vector3(otherInputs[lastFrameIndex][0 + (i * 7)], otherInputs[lastFrameIndex][1 + (i * 7)], otherInputs[lastFrameIndex][2 + (i * 7)]);
            rotJoint = new Quaternion(otherInputs[lastFrameIndex][3 + (i * 7)], otherInputs[lastFrameIndex][4 + (i * 7)], otherInputs[lastFrameIndex][5 + (i * 7)],
                otherInputs[lastFrameIndex][6 + (i * 7)]);
            if (jointPositions.Count == 1)
            {
                pp = MultQuatVec(InvQuat(rotRef), posJoint - posRef);
                vJoint = pp - pp;
                qp = MultQuat(InvQuat(rotRef), rotJoint);
                wJoint = MultQuat(InvQuat(qp), qp);
            }
            else
            {
                posJointBef = new Vector3(otherInputs[lastFrameIndex - 1][0 + (i * 7)], otherInputs[lastFrameIndex - 1][1 + (i * 7)], otherInputs[lastFrameIndex - 1][2 + (i * 7)]);
                rotJointBef = new Quaternion(otherInputs[lastFrameIndex - 1][3 + (i * 7)], otherInputs[lastFrameIndex - 1][4 + (i * 7)], otherInputs[lastFrameIndex - 1][5 + (i * 7)],
                    otherInputs[lastFrameIndex - 1][6 + (i * 7)]);
                pp = MultQuatVec(InvQuat(rotRef), posJoint - posRef);
                ppBef = MultQuatVec(InvQuat(rotRefBef), posJointBef - posRefBef);
                vJoint = pp - ppBef;
                qp = MultQuat(InvQuat(rotRef), rotJoint);
                qpBef = MultQuat(InvQuat(rotRefBef), rotJointBef);
                wJoint = MultQuat(InvQuat(qpBef), qp);
            }

            frame[0, 0, w: index++, 0] = vJoint.x;
            frame[0, 0, w: index++, 0] = vJoint.y;
            frame[0, 0, w: index++, 0] = vJoint.z;
            matJoint = QuaternionToMatrix(wJoint);
            frame[0, 0, w: index++, 0] = matJoint[0, 0];
            frame[0, 0, w: index++, 0] = matJoint[1, 0];
            frame[0, 0, w: index++, 0] = matJoint[2, 0];
            frame[0, 0, w: index++, 0] = matJoint[0, 1];
            frame[0, 0, w: index++, 0] = matJoint[1, 1];
            frame[0, 0, w: index++, 0] = matJoint[2, 1];
        }
        frame[0, 0, w: index++, 0] = jointPositions[lastFrameIndex].y;
        return frame;
    }

    public static Vector3 MultQuatVec(Quaternion q, Vector3 v)
    {
        Vector3 qXYZ = new Vector3(q.x, q.y, q.z);
        Vector3 prod = v + 2 * q.w * Vector3.Cross(qXYZ, v) + 2 * Vector3.Cross(qXYZ, Vector3.Cross(qXYZ, v));
        return prod;
    }

    public static Quaternion ConjQuat(Quaternion q)
    {
        return new Quaternion(-q.x, -q.y, -q.z, q.w);
    }

    public static Quaternion InvQuat(Quaternion q)
    {
        Quaternion conj = ConjQuat(q);
        float magn = q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w; // no Mathf.Pow() 'cause of the sqr
        if (magn == 0f)
        {
            magn = 1f;
        }
        return new Quaternion(conj.x / magn, conj.y / magn, conj.z / magn, conj.w / magn);
    }

    public static Quaternion MultQuat(Quaternion q1, Quaternion q2)
    {
        Quaternion mult = new Quaternion();
        mult.x = q2.w * q1.x + q2.x * q1.w - q2.y * q1.z + q2.z * q1.y;
        mult.y = q2.w * q1.y + q2.x * q1.z + q2.y * q1.w - q2.z * q1.x;
        mult.z = q2.w * q1.z - q2.x * q1.y + q2.y * q1.x + q2.z * q1.w;
        mult.w = q2.w * q1.w - q2.x * q1.x - q2.y * q1.y - q2.z * q1.z;
        return mult;
    }
}