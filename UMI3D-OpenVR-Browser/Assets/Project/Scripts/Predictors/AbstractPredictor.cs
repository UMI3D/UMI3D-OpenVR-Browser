using System.Collections.Generic;
using Unity.Barracuda;

public abstract class AbstractPredictor<T>
{
    public NNModel modelAsset;
    protected Model runtimeModel;

    protected Tensor modelInput;

    protected IWorker mainWorker;

    public AbstractPredictor(NNModel modelAsset, Model runtimeModel, Tensor modelInput, IWorker mainWorker)
    {
        this.modelAsset = modelAsset;
        this.runtimeModel = runtimeModel;
        this.modelInput = modelInput;
        this.mainWorker = mainWorker;
    }

    public AbstractPredictor(NNModel modelAsset)
    {
        this.modelAsset = modelAsset;
    }

    public virtual void Init()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        mainWorker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, runtimeModel);
    }

    protected bool isTensorFull => idNextFrame == (modelInput?.channels ?? -1); // tensor not full when not initialized
    protected int idNextFrame;

    public virtual void AddFrameInput(Tensor frame)
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

    protected virtual List<Tensor> ExecuteModel()
    {
        List<Tensor> outputs = new List<Tensor>();
        mainWorker.Execute(modelInput);

        outputs.Add(mainWorker.PeekOutput());
        return outputs;
    }

    public abstract T GetPrediction();

    public virtual void Clean()
    {
        modelInput?.Dispose();
    }
}