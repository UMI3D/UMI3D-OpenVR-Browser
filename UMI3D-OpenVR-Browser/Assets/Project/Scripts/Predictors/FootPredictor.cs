using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class FootPredictor : AbstractPredictor<(Dictionary<HumanBodyBones, Vector3> positions, (float, float) contact)>
{
    protected readonly int NB_PARAMETERS = 7;
    protected readonly int NB_TRACKED_LIMBS = 4;
    protected const int NB_FRAMES_MAX = 45;

    public FootPredictor(NNModel modelAsset) : base(modelAsset)
    { }

    protected override void Init()
    {
        base.Init();
        modelInput = new Tensor(1, 1, w: NB_PARAMETERS * NB_TRACKED_LIMBS, c: NB_FRAMES_MAX); //initializing
    }

    public virtual Tensor FormatInputTensor(Transform head, Transform rHand, Transform lHand, Transform hips)
    {
        Tensor frameTensor = new Tensor(1, 1, w: NB_PARAMETERS * NB_TRACKED_LIMBS, 1);

        void FillUp(int startIndex, Transform go, out int endIndex)
        {
            int index = startIndex;
            frameTensor[0, 0, index++, 0]   = go.position.x;
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
        FillUp(index, lHand, out index);
        FillUp(index, hips, out _);

        return frameTensor;
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
}