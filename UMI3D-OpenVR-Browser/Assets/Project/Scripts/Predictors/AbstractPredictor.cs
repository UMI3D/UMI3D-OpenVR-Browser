using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public abstract class AbstractPredictor<T>
{
    public NNModel modelAsset;
    protected Model runtimeModel;

    protected Tensor modelInput;

    protected IWorker mainWorker;

    protected MonoBehaviour coroutineOwnerMonoBehaviour;

    public AbstractPredictor(NNModel modelAsset, Model runtimeModel, Tensor modelInput, IWorker mainWorker, MonoBehaviour ownerMono)
    {
        this.modelAsset = modelAsset;
        this.runtimeModel = runtimeModel;
        this.modelInput = modelInput;
        this.mainWorker = mainWorker;
        this.coroutineOwnerMonoBehaviour = ownerMono;
    }

    public AbstractPredictor(NNModel modelAsset, MonoBehaviour mono)
    {
        this.modelAsset = modelAsset;
        this.coroutineOwnerMonoBehaviour = mono;
    }

    public virtual void Init()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        mainWorker = WorkerFactory.CreateWorker(WorkerFactory.Type.Compute, runtimeModel);
    }

    public bool IsTensorFull => idNextFrame == (modelInput?.channels ?? -1); // tensor not full when not initialized
    protected int idNextFrame;

    /// <summary>
    /// Add a new frame state to the registered state
    /// </summary>
    /// <param name="frame"></param>
    public virtual void AddFrameInput(float[] frame)
    {
        if (IsTensorFull)
        {
            // move all frames to the left
            for (int i = 0; i < modelInput.channels - 1; i++)
            {
                // move a frame to the left
                for (int j = 0; j < modelInput.width; j++)
                    modelInput[0, 0, j, i] = modelInput[0, 0, j, i + 1];
            }
            // replace the last frame
            for (int j = 0; j < modelInput.width; j++)
                modelInput[0, 0, j, modelInput.channels - 1] = frame[j];
        }
        else
        {
            for (int i = 0; i < modelInput.width; i++)
                modelInput[0, 0, i, idNextFrame] = frame[i];

            idNextFrame++;
        }
    }

    protected virtual List<Tensor> ExecuteModel()
    {
        List<Tensor> outputs = new();
        mainWorker.Execute(modelInput);

        outputs.Add(mainWorker.PeekOutput());
        return outputs;
    }

    protected bool isModelRunning;
    protected bool isOutputReady;
    protected List<Tensor> lastPrediction;

    protected virtual List<Tensor> ExecuteModelAsync(int framesMaxBeforeSync = 5)
    {
        if (isOutputReady)
        {
            if (lastPrediction != null)
            {
                foreach (var tensor in lastPrediction)
                    tensor.Dispose(); //free unused tensors
            }
            lastPrediction = new()
            {
                mainWorker.CopyOutput()
            };
            Debug.Log("Got results");
            isOutputReady = false;
        }
        if (!isModelRunning)
        {
            var executor = mainWorker.StartManualSchedule(modelInput);

            IEnumerator ExecuteModelOnSeveralFrames()
            {
                isModelRunning = true;
                while (executor.MoveNext())
                {
                    yield return new WaitForEndOfFrame();
                }

                isModelRunning = false;
                isOutputReady = true;
            }

            coroutineOwnerMonoBehaviour.StartCoroutine(ExecuteModelOnSeveralFrames());
        }
        return lastPrediction;
    }

    public abstract T GetPrediction(bool isAsync = false);

    public virtual void Clean()
    {
        // should dispose tensors and workers to avoid memory leaks
        mainWorker?.Dispose();
        modelInput?.Dispose();
        modelInput?.FlushCache(false);
    }
}