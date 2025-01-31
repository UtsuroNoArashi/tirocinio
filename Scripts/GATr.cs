using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using TMPro;

public class Gatr : MonoBehaviour
{
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private TMP_Text outputText;
    private Model runtimeModel;
    private Worker worker;

    Tensor inputTensor;

    const int k_LayersPerFrame = 5;

    IEnumerator scheduler;
    bool m_Started = false;

    void OnEnable()
    {
        // Carica Assets con Debug
        if (modelAsset == null)
        {
            Debug.LogError("ModelAsset non assegnato!");
        }

        if (outputText == null)
        {
            Debug.LogError("TMP_Text non assegnato!");
        }

        outputText.text += "Caricamento in corso ...\n";

        // Try catch per gestire eccezioni
        runtimeModel = ModelLoader.Load(modelAsset);
        if (runtimeModel == null)
        {
            outputText.text = "Errore: modello non caricato.\n";
            Debug.LogError("Errore: modello non caricato.");
        }

        List<Model.Input> inputs = runtimeModel.inputs;

        // Loop through each input
        foreach (var input in inputs)
        {
            // Log the name of the input, for example Input3
            outputText.text += $"input.name = {input.name}\n";

            // Log the tensor shape of the input, for example (1, 1, 28, 28)
            outputText.text += $"input.shape = {input.shape}\n";
        }


        // Prepara il worker
        worker = new Worker(runtimeModel, BackendType.GPUCompute);

        // Prepara l'input
        inputTensor = PrepareInputTensor();
        outputText.text += $"Input Tensor Shape: {inputTensor.shape}\n";

        if (inputTensor == null)
        {
            outputText.text += "Errore: tensor di input non caricato.\n";
            Debug.LogError("Errore: tensor di input non caricato.");
        }
    }

    void Update()
    {
        if (!m_Started)
        {
            // ScheduleIterable starts the scheduling of the model
            // it returns a IEnumerator to iterate over the model layers, scheduling each layer sequentially
            outputText.text += "scheduler inizializzato\n";
            scheduler = worker.ScheduleIterable(inputTensor);
            m_Started = true;
        }

        int it = 0;
        while (scheduler.MoveNext())
        {
            if (++it % k_LayersPerFrame == 0)
            {
                outputText.text += $"fine ciclo, ";
                return;
            }
        }

        var outputTensor = worker.PeekOutput() as Tensor<float>;

        if (outputTensor == null)
        {
            outputText.text += "Errore: tensor di output non caricato.\n";
            Debug.LogError("Errore: tensor di output non caricato.");
        }

        var cpuTensor = outputTensor.ReadbackAndClone();

        outputText.text += $"Output Tensor Shape: {cpuTensor.shape}\n";
        m_Started = false;
        // Stampa i risultati
        string resultsString = cpuTensor.ToString();
        if (outputText != null)
        {
            outputText.text += $"risultato tensore di output: {resultsString}";
            Debug.Log(resultsString);
        }
        else
        {
            outputText.text += "Output Text is null";
            Debug.LogWarning("Il componente Text non è stato assegnato.");
        }

        cpuTensor.Dispose();

    }
    Tensor PrepareInputTensor()
    {
        // Dati di input per ogni pianeta: [massa, pos_x, pos_y, pos_z, vel_x, vel_y, vel_z]
        float[] inputData = new float[]
        {
            // Pianeta 1 (massa, posizione xyz, velocità xyz)
            0.02146309f, -19.30458052f, -5.09142425f, -1.71743116f, -0.11754325f, 0.70240116f, 0.88894977f,
            
            // Pianeta 2
            0.01986073f, -18.95293991f, -4.954115f, -2.10290605f, -0.70086511f, 0.29593982f, 1.36838713f,
            
            // Pianeta 3
            1.115385f, -18.71318257f, -4.56528173f, -2.06411723f, 0f, 0f, 0f,

            // Pianeta 4
            0.02487979f, -18.83038067f, -4.12001627f, -1.43396349f, -0.81023741f, -0.79165406f, 0.40258876f
        };

        Tensor<float> inputTensor = new Tensor<float>(new TensorShape(1, 4, 7), inputData);

        return inputTensor;
    }

    string PrintResults(Tensor<float> outputTensor)
    {
        float[,] actual = {
            { -19.31085362f, -5.01661123f, -1.63205759f },
            { -19.0080462f, -4.90257404f, -1.96607783f },
            { -18.71359161f, -4.56564631f, -2.06387754f },
            { -18.90975347f, -4.20430149f, -1.40140385f }
        };

        string resultsString = "";
        for (int i = 0; i < 4; i++)
        {
            resultsString += $"Pianeta {i + 1}:\n";
            resultsString += $"Reale: ({actual[i, 0]:F6}, {actual[i, 1]:F6}, {actual[i, 2]:F6})\n";
            resultsString += $"Previsto: ({outputTensor[i, 0]:F6}, {outputTensor[i, 1]:F6}, {outputTensor[i, 2]:F6})\n";
        }

        return resultsString;
    }

    void OnDisable()
    {
        if (inputTensor != null)
            inputTensor.Dispose();
        if (worker != null)
            worker.Dispose();
    }
}
