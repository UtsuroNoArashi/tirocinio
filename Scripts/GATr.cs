using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using TMPro;
using System.Text;

public class Gatr : MonoBehaviour
{
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private TMP_Text outputText;

    private Model runtimeModel;
    private Worker worker;
    Tensor<float> inputTensor;

    private const int k_LayersPerFrame = 5;
    private bool m_Started = false;
    private IEnumerator scheduler;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // Uses StringBuilder to build debug messages
        StringBuilder sb = new StringBuilder();

        // Loads assets with debug
        if (modelAsset == null)
        {
            sb.AppendLine("ModelAsset not assigned!");
            Debug.LogError("ModelAsset not assigned!");
        }

        if (outputText == null)
        {
            sb.AppendLine("TMP_Text not assigned!");
            Debug.LogError("TMP_Text not assigned!");
        }
        else
        {
            sb.AppendLine("Loading ...");
        }

        // Loads the model
        runtimeModel = ModelLoader.Load(modelAsset);

        // ModelQuantizer.QuantizeWeights(QuantizationType.Float16, ref runtimeModel);

        if (runtimeModel == null)
        {
            sb.AppendLine("Error: model not loaded.");
            Debug.LogError("Error: model not loaded.");
        }
        else
        {
            sb.AppendLine("Model loaded");
        }

        List<Model.Input> inputs = runtimeModel.inputs;

        // Logs the inputs of the model
        foreach (var input in inputs)
        {
            sb.AppendLine($"input.name = {input.name}");
            sb.AppendLine($"input.shape = {input.shape}");
        }

        // Prepares the worker
        worker = new Worker(runtimeModel, BackendType.GPUCompute);

        // Prepares the input
        inputTensor = PrepareInputTensor();
        sb.AppendLine($"Input Tensor Shape: {inputTensor.shape}");
        if (inputTensor == null)
        {
            sb.AppendLine("Error: input tensor not loaded.");
            Debug.LogError("Error: input tensor not loaded.");
        }

        // Assigns the text constructed with StringBuilder
        outputText.text = sb.ToString();
    }

    /// <summary>
    /// Coroutine that runs the model in small steps to avoid blocking the frame rate
    /// </summary>
    public void StartPrediction()
    {
        if (m_Started)
        {
            outputText.text += "Prediction already in progress...\n";
            return;
        }
        StartCoroutine(RunModelCoroutine());
    }

    /// <summary>
    /// Coroutine that runs the model in small steps to avoid blocking the frame rate
    /// </summary>
    IEnumerator RunModelCoroutine()
    {
        m_Started = true;
        outputText.text += "scheduler initialized\n";
        scheduler = worker.ScheduleIterable(inputTensor);

        yield return new WaitForEndOfFrame();

        // Starts the scheduler to run the model
        int it = 0;
        while (scheduler.MoveNext())
        {
            it++;
            if (it % k_LayersPerFrame == 0)
            {
                outputText.text += "- ";
                // Waits for the next frame
                yield return new WaitForEndOfFrame();
            }

            if (!this.isActiveAndEnabled)
            {
                outputText.text += "This not enabled ";
                yield break;
            }
        }

        yield return new WaitForEndOfFrame();

        outputText.text += "finished\n";
        // The computation should be finished here
        var outputTensor = worker.PeekOutput() as Tensor<float>;
        if (outputTensor == null)
        {
            outputText.text += "Error: output tensor not loaded.\n";
            Debug.LogError("Error: output tensor not loaded.");
            m_Started = false;
            yield break;
        }

        // Reads the output from the GPU and creates a copy on the CPU
        var cpuTensor = outputTensor.ReadbackAndClone();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Output Tensor Shape: {cpuTensor.shape}");

        // Prints the values inside the tensor
        sb.Append(PrintResults(cpuTensor));
        outputText.text += sb.ToString();

        // Cleans up the resources used for this computation
        cpuTensor.Dispose();
        m_Started = false;
    }

    string PrintResults(Tensor<float> outputTensor)
    {
        float[,] actual = {
            { -19.31085362f, -5.01661123f, -1.63205759f },
            { -19.0080462f, -4.90257404f, -1.96607783f },
            { -18.71359161f, -4.56564631f, -2.06387754f },
            { -18.90975347f, -4.20430149f, -1.40140385f }
        };

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < 4; i++)
        {
            sb.AppendLine($"Planet {i + 1}:");
            sb.AppendLine($"Real: ({actual[i, 0]:F6}, {actual[i, 1]:F6}, {actual[i, 2]:F6})");
            sb.AppendLine($"Predicted: ({outputTensor[i, 0]:F6}, {outputTensor[i, 1]:F6}, {outputTensor[i, 2]:F6})");
        }

        return sb.ToString();
    }

    Tensor<float> PrepareInputTensor()
    {
        // Input data for each planet: [mass, pos_x, pos_y, pos_z, vel_x, vel_y, vel_z]
        float[] inputData = new float[]
        {
            // Planet 1 (mass, position xyz, velocity xyz)
            0.02146309f, -19.30458052f, -5.09142425f, -1.71743116f, -0.11754325f, 0.70240116f, 0.88894977f,
            
            // Planet 2
            0.01986073f, -18.95293991f, -4.954115f, -2.10290605f, -0.70086511f, 0.29593982f, 1.36838713f,
            
            // Planet 3
            1.115385f, -18.71318257f, -4.56528173f, -2.06411723f, 0f, 0f, 0f,

            // Planet 4
            0.02487979f, -18.83038067f, -4.12001627f, -1.43396349f, -0.81023741f, -0.79165406f, 0.40258876f
        };

        Tensor<float> inputTensor = new Tensor<float>(new TensorShape(1, 4, 7), inputData);

        return inputTensor;
    }

    void OnDisable()
    {
        // Ferma tutte le coroutine per evitare che task in background vengano eseguiti dopo la distruzione
        StopAllCoroutines();

        if (inputTensor != null)
        {
            inputTensor.Dispose();
            inputTensor = null;
        }
        if (worker != null)
        {
            worker.Dispose();
            worker = null;
        }
    }
}
