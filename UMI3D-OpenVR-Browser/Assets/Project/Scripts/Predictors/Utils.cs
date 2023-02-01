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

    public static Quaternion MatrixToQuaternion(Vector3 right, Vector3 up)
    {
        var forward = Vector3.Cross(right.normalized, up.normalized);
        if (forward == Vector3.zero || up == Vector3.zero)
            throw new System.Exception($"Impossible to compute rotation. Input {(up == Vector3.zero ? "Up" : "Right")} is zero.");
        return Quaternion.LookRotation(forward.normalized, up.normalized);
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

    public static Tensor ComputeVelocities(List<Vector3> jointRefPositions, List<Quaternion> jointRefRotations, List<List<float>> trackedJoints, Tensor frame)
    {
        // REFERENCE JOINT
        Vector3 posRef = jointRefPositions[^1];
        Quaternion rotRef = jointRefRotations[^1];

        Vector3 posRefBef = new Vector3();
        Quaternion rotRefBef = new Quaternion();

        Vector3 velocityRef;
        Quaternion rotVelocityRef;

        if (jointRefPositions.Count == 1)
        {
            velocityRef = Quaternion.Inverse(rotRef) * (posRef - posRef); //! optimizable but normal by choice
            rotVelocityRef = (Quaternion.Inverse(rotRef) * Quaternion.Inverse(rotRef)) * rotRef; //! optimizable but normal by choice
        }
        else
        {
            posRefBef = jointRefPositions[^2];
            rotRefBef = jointRefRotations[^2];
            velocityRef = Quaternion.Inverse(rotRef) * (posRef - posRefBef);
            rotVelocityRef = (Quaternion.Inverse(rotRef) * Quaternion.Inverse(rotRefBef)) * rotRef;
        }

        int index = 0;
        frame[0, 0, w: index++, 0] = velocityRef.x;
        frame[0, 0, w: index++, 0] = velocityRef.y;
        frame[0, 0, w: index++, 0] = velocityRef.z;

        var rotRefRight = rotVelocityRef * Vector3.right;
        var rotRefUp = rotVelocityRef * Vector3.up;
        frame[0, 0, w: index++, 0] = rotRefRight.x;
        frame[0, 0, w: index++, 0] = rotRefRight.y;
        frame[0, 0, w: index++, 0] = rotRefRight.z;
        frame[0, 0, w: index++, 0] = rotRefUp.x;
        frame[0, 0, w: index++, 0] = rotRefUp.y;
        frame[0, 0, w: index++, 0] = rotRefUp.z;

        // TRACKED JOINTS
        const int NB_PARAMETER = 7; // because its pos and rot in quaternions
        for (int i = 0; i < 3; i++)
        {
            Vector3 posJoint = new Vector3(trackedJoints[^1][0 + (i * NB_PARAMETER)],
                                           trackedJoints[^1][1 + (i * NB_PARAMETER)],
                                           trackedJoints[^1][2 + (i * NB_PARAMETER)]);
            Quaternion rotJoint = new Quaternion(trackedJoints[^1][3 + (i * NB_PARAMETER)],
                                                 trackedJoints[^1][4 + (i * NB_PARAMETER)],
                                                 trackedJoints[^1][5 + (i * NB_PARAMETER)],
                                                 trackedJoints[^1][6 + (i * NB_PARAMETER)]);

            Vector3 velocityJoint;
            Quaternion rotVelocityJoint;

            if (jointRefPositions.Count == 1)
            {
                Vector3 positionDeltaInReference = Quaternion.Inverse(rotRef) * (posJoint - posRef); // p prime in paper
                velocityJoint = positionDeltaInReference - positionDeltaInReference; //! optimizable but normal by choice

                Quaternion rotationDeltaJoint = Quaternion.Inverse(rotRef) * rotJoint; // q prime in paper
                rotVelocityJoint = Quaternion.Inverse(rotationDeltaJoint) * rotationDeltaJoint; //! optimizable but normal by choice
            }
            else
            {
                Vector3 posJointBefore = new Vector3(trackedJoints[^2][0 + (i * NB_PARAMETER)],
                                                     trackedJoints[^2][1 + (i * NB_PARAMETER)],
                                                     trackedJoints[^2][2 + (i * NB_PARAMETER)]);
                Quaternion rotJointBefore = new Quaternion(trackedJoints[^2][3 + (i * NB_PARAMETER)],
                                                           trackedJoints[^2][4 + (i * NB_PARAMETER)],
                                                           trackedJoints[^2][5 + (i * NB_PARAMETER)],
                                                           trackedJoints[^2][6 + (i * NB_PARAMETER)]);

                Vector3 positionDeltaInReference = Quaternion.Inverse(rotRef) * (posJoint - posRef);
                Vector3 positionDeltaInReferenceBefore = Quaternion.Inverse(rotRefBef) * (posJointBefore - posRefBef);
                velocityJoint = positionDeltaInReference - positionDeltaInReferenceBefore;

                Quaternion rotationDeltaJoint = Quaternion.Inverse(rotRef) * rotJoint;
                Quaternion rotationDeltaJointBefore = Quaternion.Inverse(rotRefBef) * rotJointBefore;
                rotVelocityJoint = Quaternion.Inverse(rotationDeltaJointBefore) * rotationDeltaJoint;
            }

            frame[0, 0, w: index++, 0] = velocityJoint.x;
            frame[0, 0, w: index++, 0] = velocityJoint.y;
            frame[0, 0, w: index++, 0] = velocityJoint.z;

            var right = rotVelocityJoint * Vector3.right;
            var up = rotVelocityJoint * Vector3.up;
            frame[0, 0, w: index++, 0] = right.x;
            frame[0, 0, w: index++, 0] = right.y;
            frame[0, 0, w: index++, 0] = right.z;
            frame[0, 0, w: index++, 0] = up.x;
            frame[0, 0, w: index++, 0] = up.y;
            frame[0, 0, w: index++, 0] = up.z;
        }
        frame[0, 0, w: index++, 0] = jointRefPositions[^1].y;
        return frame;
    }
}