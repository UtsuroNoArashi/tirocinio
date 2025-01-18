using UnityEngine;
using Unity.Sentis;

public class ONNXTest : MonoBehaviour 
{
    private Model runtimeModel;

    void Start()
    {
        // Carica il modello ONNX
        runtimeModel = ModelLoader.Load("NomeDelModelloSenzaEstensione");
        if (runtimeModel == null)
        {
            Debug.LogError("Errore: modello non caricato.");
            return;
        }

        // Esegui la predizione
        Tensor inputTensor = PrepareInputTensor();
        Tensor outputTensor = runtimeModel.Execute(inputTensor);
        PrintResults(outputTensor);

        // Libera la memoria
        inputTensor.Dispose();
        outputTensor.Dispose();
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

        return Tensor.FromArray(inputData, new TensorShape(1, 4, 7));
    }

    void PrintResults(Tensor outputTensor)
    {
        float[] predicted = outputTensor.ToReadOnlyArray();
        float[,] actual = {
            { -19.31085362f, -5.01661123f, -1.63205759f },
            { -19.0080462f, -4.90257404f, -1.96607783f },
            { -18.71359161f, -4.56564631f, -2.06387754f },
            { -18.90975347f, -4.20430149f, -1.40140385f }
        };

        for (int i = 0; i < 4; i++)
        {
            Debug.Log($"Pianeta {i + 1}:");
            Debug.Log($"Predetto: ({predicted[i * 3]:F6}, {predicted[i * 3 + 1]:F6}, {predicted[i * 3 + 2]:F6})");
            Debug.Log($"Reale: ({actual[i,0]:F6}, {actual[i,1]:F6}, {actual[i,2]:F6})");
        }
    }
}