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
        Init();
    }

    protected virtual void Init()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        mainWorker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, runtimeModel);
    }

    public abstract void AddFrameInput(Tensor tensor);

    public abstract T GetPrediction();

    public virtual void Clean()
    {
        modelInput?.Dispose();
    }
}