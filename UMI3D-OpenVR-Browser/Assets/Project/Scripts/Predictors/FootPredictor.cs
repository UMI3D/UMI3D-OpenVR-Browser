using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class FootPredictor : AbstractPredictor<(Dictionary<HumanBodyBones, Vector3> positions, (float, float) contact)>
{
    protected readonly int NB_PARAMETERS = 9;
    protected readonly int NB_TRACKED_LIMBS = 3;
    protected const int NB_FRAMES_MAX = 45;

    //tracked limbs (head, lhand, rhand) in rotation (9 each) and predicted hips, +1 for the height
    protected int NbChannels => NB_PARAMETERS * (NB_TRACKED_LIMBS + 1) + 1;

    public FootPredictor(NNModel modelAsset) : base(modelAsset)
    { }

    public override void Init()
    {
        base.Init();
        modelInput = new Tensor(1, 1, w: NbChannels, c: NB_FRAMES_MAX); //initializing
    }

    public class PosRot
    {
        public Vector3 pos;
        public Quaternion rot;

        public static implicit operator PosRot(Transform t)
        {
            return new PosRot()
            {
                pos = t.position,
                rot = t.rotation
            };
        }

        public static implicit operator PosRot((Vector3 pos, Quaternion rot) ts)
        {
            return new PosRot()
            {
                pos = ts.pos,
                rot = ts.rot
            };
        }

        public enum RotationSerializationMode
        {
            QUATERNION,
            EULER,
            MATRIX2,
            MATRIX3
        }

        public List<float> Serialize(bool serializePos= true, bool serializeRot= true, 
                                    RotationSerializationMode rotationSerializationMode = RotationSerializationMode.QUATERNION)
        {
            var data = new List<float>();

            if (serializePos)
            {
                data.Add(pos.x);
                data.Add(pos.y);
                data.Add(pos.z);
            }
            if (serializeRot)
            {
                switch (rotationSerializationMode)
                {
                    case RotationSerializationMode.QUATERNION:
                        data.Add(rot.x);
                        data.Add(rot.y);
                        data.Add(rot.z);
                        data.Add(rot.w);
                        break;
                    case RotationSerializationMode.EULER:
                        data.Add(rot.eulerAngles.x);
                        data.Add(rot.eulerAngles.y);
                        data.Add(rot.eulerAngles.z);
                        break;
                    case RotationSerializationMode.MATRIX2:
                        var right = rot * Vector3.right;
                        var up = rot * Vector3.up;
                        data.Add(right.x);
                        data.Add(right.y);
                        data.Add(right.z);
                        data.Add(up.x);
                        data.Add(up.y);
                        data.Add(up.z);
                        break;
                    case RotationSerializationMode.MATRIX3:
                        var forward = rot * Vector3.forward;
                        right = rot * Vector3.right;
                        up = rot * Vector3.up;
                        data.Add(forward.x);
                        data.Add(forward.y);
                        data.Add(forward.z);
                        data.Add(right.x);
                        data.Add(right.y);
                        data.Add(right.z);
                        data.Add(up.x);
                        data.Add(up.y);
                        data.Add(up.z);
                        break;
                }
            }
            return data;
        }
    }

    public class LoBSTrFrameData
    {
        public PosRot head;
        public PosRot rHand;
        public PosRot lHand;
        public PosRot hips;

        public List<float> Serialize()
        {
            var data = new List<float>();

            var memberList = new List<PosRot>() { head, rHand, lHand, hips };

            foreach (var posRot in memberList)
            {
                data.AddRange(posRot.Serialize());
            }
            return data;
        }
    }

    private List<LoBSTrFrameData> recordedFrames = new List<LoBSTrFrameData>();
    private List<List<float>> recordedFramesSerialized = new List<List<float>>();
    private List<Vector3> recordedjointRefPositions = new();
    private List<Quaternion> recordedjointRefRotations = new();

    public virtual Tensor FormatInputTensor(PosRot head, PosRot rHand, PosRot lHand, PosRot hips)
    {
        Tensor frameTensor = new Tensor(1, 1, w: NbChannels, 1);

        // recording frame
        recordedFrames.Add(new LoBSTrFrameData()
        {
            head = head,
            rHand = rHand,
            lHand = lHand,
            hips = hips
        });
        recordedFramesSerialized.Add(recordedFrames[^1].Serialize());

        // recording joint reference
        var jointRefRot = PredictorUtils.ComputeRefJointRot(hips.rot * Vector3.forward);
        recordedjointRefPositions.Add(hips.pos);
        recordedjointRefRotations.Add(jointRefRot);

        // computing full input with velocities
        var fullInput = PredictorUtils.ComputeVelocities(recordedjointRefPositions, recordedjointRefRotations, recordedFramesSerialized, frameTensor);

        return fullInput;
    }

    public override (Dictionary<HumanBodyBones, Vector3> positions, (float, float) contact) GetPrediction()
    {
        var output = ExecuteModel();

        int index = 0;
        var positions = new Dictionary<HumanBodyBones, Vector3>()
        {
            { HumanBodyBones.RightUpperLeg, new Vector3(output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++])
            },
            { HumanBodyBones.RightLowerLeg, new Vector3(output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++])
            },
            { HumanBodyBones.RightFoot,     new Vector3(output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++])
            },
            { HumanBodyBones.RightToes,     new Vector3(output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++])
            },
            { HumanBodyBones.LeftUpperLeg,  new Vector3(output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++])
            },
            { HumanBodyBones.LeftLowerLeg,  new Vector3(output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++])
            },
            { HumanBodyBones.LeftFoot,      new Vector3(output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++])
            },
            { HumanBodyBones.LeftToes,      new Vector3(output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++],
                                                        output[0][0, 0, 0, index++])
            }
        };

        (float rightOnFloor, float leftOnFloot) contact = (output[0][0, 0, 0, index++], output[0][0, 0, 0, index++]);

        (Dictionary<HumanBodyBones, Vector3> positions, (float, float) contact) result = (positions, contact);
        return result;
    }

    public Dictionary<HumanBodyBones, Quaternion> ComputeForwardKinematics(Vector3 hips, Dictionary<HumanBodyBones, Vector3> positions)
    {
         //not necessary with Unity parenting. Needs to be parented in the right order though.

        var rotations = new Dictionary<HumanBodyBones, Quaternion>()
        {
            { HumanBodyBones.RightUpperLeg, Quaternion.FromToRotation(hips,                                     positions[HumanBodyBones.RightUpperLeg]) },
            { HumanBodyBones.RightLowerLeg, Quaternion.FromToRotation(positions[HumanBodyBones.RightUpperLeg],  positions[HumanBodyBones.RightLowerLeg]) },
            { HumanBodyBones.RightFoot,     Quaternion.FromToRotation(positions[HumanBodyBones.RightLowerLeg],  positions[HumanBodyBones.RightFoot]) },
            { HumanBodyBones.RightToes,     Quaternion.FromToRotation(positions[HumanBodyBones.RightFoot],      positions[HumanBodyBones.RightToes]) },

            { HumanBodyBones.LeftUpperLeg,  Quaternion.FromToRotation(hips,                                     positions[HumanBodyBones.LeftUpperLeg]) },
            { HumanBodyBones.LeftLowerLeg,  Quaternion.FromToRotation(positions[HumanBodyBones.LeftUpperLeg],   positions[HumanBodyBones.LeftLowerLeg]) },
            { HumanBodyBones.LeftFoot,      Quaternion.FromToRotation(positions[HumanBodyBones.LeftLowerLeg],   positions[HumanBodyBones.LeftFoot]) },
            { HumanBodyBones.LeftToes,      Quaternion.FromToRotation(positions[HumanBodyBones.LeftFoot],       positions[HumanBodyBones.LeftToes]) },
        };

        return rotations;
    }
}