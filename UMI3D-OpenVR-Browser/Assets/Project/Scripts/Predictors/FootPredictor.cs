using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;


public class FootPredictor : AbstractPredictor<FootPredictionResult>
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

        public enum RotationFlattenMode
        {
            QUATERNION,
            EULER,
            MATRIX2,
            MATRIX3
        }

        public List<float> Flatten(bool serializePos = true, bool serializeRot = true,
                                    RotationFlattenMode rotationFlattenMode = RotationFlattenMode.QUATERNION)
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
                switch (rotationFlattenMode)
                {
                    case RotationFlattenMode.QUATERNION:
                        data.Add(rot.x);
                        data.Add(rot.y);
                        data.Add(rot.z);
                        data.Add(rot.w);
                        break;

                    case RotationFlattenMode.EULER:
                        data.Add(rot.eulerAngles.x);
                        data.Add(rot.eulerAngles.y);
                        data.Add(rot.eulerAngles.z);
                        break;

                    case RotationFlattenMode.MATRIX2:
                        var right = rot * Vector3.right;
                        var up = rot * Vector3.up;
                        data.Add(right.x);
                        data.Add(right.y);
                        data.Add(right.z);
                        data.Add(up.x);
                        data.Add(up.y);
                        data.Add(up.z);
                        break;

                    case RotationFlattenMode.MATRIX3:
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

        public List<float> Flattent()
        {
            var data = new List<float>();

            var memberList = new List<PosRot>() { head, rHand, lHand, hips };

            foreach (var posRot in memberList)
                data.AddRange(posRot.Flatten());

            return data;
        }
    }

    private List<LoBSTrFrameData> recordedFrames = new();
    private List<List<float>> recordedFramesSerialized = new();
    private List<Vector3> recordedjointRefPositions = new();
    private List<Quaternion> recordedjointRefRotations = new();

    public virtual Tensor FormatInputTensor(PosRot head, PosRot rHand, PosRot lHand, PosRot hips)
    {
        Tensor frameTensor = new Tensor(1, 1, w: NbChannels, 1);

        // recording joint reference
        var jointRefPos = hips.pos;
        var jointRefRot = PredictorUtils.ComputeRefJointRot(hips.rot * Vector3.forward);
        recordedjointRefPositions.Add(jointRefPos);
        recordedjointRefRotations.Add(jointRefRot);

        // recording frame
        recordedFrames.Add(new LoBSTrFrameData()
        {
            head = (head.pos, head.rot), //! changed from local pos to global (correction form the original paper)
            rHand = (rHand.pos, rHand.rot),
            lHand = (lHand.pos, lHand.rot),
            hips = hips
        });
        recordedFramesSerialized.Add(recordedFrames[^1].Flattent());

        // if number or recorded frames too long, discard
        if (recordedFrames.Count > NB_FRAMES_MAX)
        {
            recordedFrames.RemoveAt(0);
            recordedFramesSerialized.RemoveAt(0);
            recordedjointRefPositions.RemoveAt(0);
            recordedjointRefRotations.RemoveAt(0);
        }

        // computing full input with velocities
        var fullInput = PredictorUtils.ComputeVelocities(recordedjointRefPositions, recordedjointRefRotations, recordedFramesSerialized, frameTensor);

        return fullInput;
    }

    protected override List<Tensor> ExecuteModel()
    {
        List<Tensor> outputs = new List<Tensor>();
        mainWorker.Execute(modelInput);

        outputs.Add(mainWorker.PeekOutput("Lfk"));
        outputs.Add(mainWorker.PeekOutput("footProba"));
        //outputs.Add(mainWorker.PeekOutput("Lleft")); // previous version
        //outputs.Add(mainWorker.PeekOutput("Lright"));
        return outputs;
    }

    public override (Dictionary<HumanBodyBones, Quaternion> rotations, (float leftOnFloor, float rightOnFloor) contact) GetPrediction()
    {
        var output = ExecuteModel();

        Quaternion ExtractRotation(int startIndex, out int endIndex)
        {
            int index = startIndex;
            Vector3 right = new Vector3(output[0][0, 0, 0, index++],
                                        output[0][0, 0, 0, index++],
                                        output[0][0, 0, 0, index++]).normalized;
            Vector3 up = new Vector3(output[0][0, 0, 0, index++],
                                     output[0][0, 0, 0, index++],
                                     output[0][0, 0, 0, index++]).normalized;
            endIndex = index;
            return PredictorUtils.MatrixToQuaternion(right, up);
        }

        int index = 0;

        var result = new FootPredictionResult()
        {
            rotations = new Dictionary<HumanBodyBones, Quaternion>() // matrice columns right and up in rotation
                {
                    { HumanBodyBones.RightUpperLeg, ExtractRotation(index, out index) },
                    { HumanBodyBones.RightLowerLeg, ExtractRotation(index, out index) },
                    { HumanBodyBones.RightFoot,     ExtractRotation(index, out index) },
                    { HumanBodyBones.RightToes,     ExtractRotation(index, out index) },

                    { HumanBodyBones.LeftUpperLeg,  ExtractRotation(index, out index) },
                    { HumanBodyBones.LeftLowerLeg,  ExtractRotation(index, out index) },
                    { HumanBodyBones.LeftFoot,      ExtractRotation(index, out index) },
                    { HumanBodyBones.LeftToes,      ExtractRotation(index, out _) },
                },
            contact  = (output[1][0, 0, 0, 0], output[1][0, 0, 0, 1])
        };

        return result;
    }


}

public struct FootPredictionResult
{
    public Dictionary<HumanBodyBones, Quaternion> rotations;
    public (float leftOnFloor, float rightOnFloor) contact;

    public void Deconstruct(out Dictionary<HumanBodyBones, Quaternion> rotations, out (float leftOnFloor, float rightOnFloor) contact)
    {
        rotations = this.rotations;
        contact = this.contact;
    }
}