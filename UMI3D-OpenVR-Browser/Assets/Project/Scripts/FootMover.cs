using System.Collections;
using System.Collections.Generic;
using System.Data;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using umi3dVRBrowsersBase.ikManagement;
using Unity.Barracuda;
using UnityEngine;

public class FootMover : MonoBehaviour
{
    private VirtualObjectBodyInteraction LeftFoot;
    private VirtualObjectBodyInteraction RightFoot;
    private VirtualObjectBodyInteraction LeftHand;
    private VirtualObjectBodyInteraction RightHand;

    private UMI3DClientUserTrackingBone head;


    public NNModel modelAsset;
    private Model runtimeModel;

    private Tensor modelInput;

    private IWorker mainWorker;

    public GameObject hipsPredicted;

    // Start is called before the first frame update
    void Start()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        mainWorker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, runtimeModel);
        modelInput = new Tensor(1, 1, 21, 45); //initializing

        var objects = new List<VirtualObjectBodyInteraction>(FindObjectsOfType<VirtualObjectBodyInteraction>());
        var bones = new List<UMI3DClientUserTrackingBone>(FindObjectsOfType<UMI3DClientUserTrackingBone>());
        LeftFoot = objects.Find(x => x.goal == AvatarIKGoal.LeftFoot);
        RightFoot = objects.Find(x => x.goal == AvatarIKGoal.RightFoot);
        LeftHand = objects.Find(x => x.goal == AvatarIKGoal.LeftHand);
        RightHand = objects.Find(x => x.goal == AvatarIKGoal.RightHand);
        head = bones.Find(x => x.boneType == BoneType.Head);

        var frequency = 1 / 30f;
        InvokeRepeating(nameof(UpdateHips), 0, frequency);
    }

    // Update is called once per frame
    void Update()
    {
        LeftFoot.transform.position = new Vector3(LeftHand.transform.position.x, LeftFoot.transform.position.y, LeftHand.transform.position.z);
        RightFoot.transform.position = new Vector3(RightHand.transform.position.x, RightFoot.transform.position.y, RightHand.transform.position.z);
    }


    private Tensor GetHipsInputs()
    {
        Tensor frameTensor = new Tensor(1, 1, 21, 1);

        // head
        Vector3 posHead = head.transform.position;
        Quaternion rotHead = head.transform.rotation;

        frameTensor[0, 0, 0, 0] = posHead.x;
        frameTensor[0, 0, 1, 0] = posHead.y;
        frameTensor[0, 0, 2, 0] = posHead.z;

        frameTensor[0, 0, 3, 0] = rotHead.x;
        frameTensor[0, 0, 4, 0] = rotHead.y;
        frameTensor[0, 0, 5, 0] = rotHead.z;
        frameTensor[0, 0, 6, 0] = rotHead.w;

        // rightHand
        Vector3 posRHand = RightHand.transform.position;
        Quaternion rotRHand = RightHand.transform.rotation;

        frameTensor[0, 0, 7, 0] = posRHand.x;
        frameTensor[0, 0, 8, 0] = posRHand.y;
        frameTensor[0, 0, 9, 0] = posRHand.z;

        frameTensor[0, 0, 10, 0] = rotRHand.x;
        frameTensor[0, 0, 11, 0] = rotRHand.y;
        frameTensor[0, 0, 12, 0] = rotRHand.z;
        frameTensor[0, 0, 13, 0] = rotRHand.w;

        // leftHand
        Vector3 posLHand = LeftHand.transform.position;
        Quaternion rotLHand = LeftHand.transform.rotation;

        frameTensor[0, 0, 14, 0] = posLHand.x;
        frameTensor[0, 0, 15, 0] = posLHand.y;
        frameTensor[0, 0, 16, 0] = posLHand.z;

        frameTensor[0, 0, 17, 0] = rotLHand.x;
        frameTensor[0, 0, 18, 0] = rotLHand.y;
        frameTensor[0, 0, 19, 0] = rotLHand.z;
        frameTensor[0, 0, 20, 0] = rotLHand.w;
        
        return frameTensor;
    }

    private bool isTensorFull => idNextFrame == modelInput.channels;
    private int idNextFrame;

    public void AddFrame(Tensor frame)
    {
        if (isTensorFull)
        {
            for (int i = 0; i < modelInput.channels-1; i++)
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

    public List<Tensor> ExecuteModel()
    {
        List<Tensor> outputs = new List<Tensor>();
        mainWorker.Execute(modelInput);

        outputs.Add(mainWorker.PeekOutput());
        return outputs;
    }

    public Quaternion GetPrediction()
    {
        var output = ExecuteModel();
        return new Quaternion(output[0][0, 0, 0, 3], output[0][0, 0, 0, 4], output[0][0, 0, 0, 5],
                    output[0][0, 0, 0, 6]);
    }

    void OnApplicationQuit()
    {
        modelInput?.Dispose();
    }

    public void UpdateHips()
    {
        hipsPredicted.transform.rotation = GetPrediction();
    }

}
