import numpy as np
import onnxruntime
import logging
from pathlib import Path

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def test_model(onnx_path, eval_data_path):
    """Test the ONNX model with the first experiment from the eval dataset."""
    try:
        # Crea una sessione di inferenza
        ort_session = onnxruntime.InferenceSession(str(onnx_path))
        
        # Ottieni informazioni sull'input
        input_name = ort_session.get_inputs()[0].name
        input_shape = ort_session.get_inputs()[0].shape
        logger.info(f"Input shape: {input_shape}")
        
        # Carica i dati dal file eval.npz
        eval_data = np.load(eval_data_path)
        
        # Recupera i dati necessari
        m = eval_data['m']  # Masse dei pianeti
        x_initial = eval_data['x_initial']  # Posizioni iniziali
        v_initial = eval_data['v_initial']  # Velocità iniziali
        x_final = eval_data['x_final']  # Posizioni finali (obiettivo per il confronto)
        x_trajectory = eval_data['trajectories']  # Traiettoria dei pianeti

        # Seleziona il primo esperimento (500 esperimenti disponibili)
        m_batch = m[0]  # Masse dei 4 pianeti per il primo esperimento
        x_initial_batch = x_initial[0]  # Posizioni iniziali dei 4 pianeti
        v_initial_batch = v_initial[0]  # Velocità iniziali dei 4 pianeti
        x_final_batch = x_final[0]  # Posizioni finali dei 4 pianeti
        x_trajectory_batch = x_trajectory[0]  # Traiettoria dei 4 pianeti

        # Stampa le informazioni sugli input
        logger.info("\n--- Dati di Input ---")
        logger.info(f"Masse dei pianeti: {m_batch}")
        logger.info(f"Posizioni iniziali (x, y, z) dei pianeti: \n{x_initial_batch}")
        logger.info(f"Velocità iniziali (vx, vy, vz) dei pianeti: \n{v_initial_batch}")

        # Prepara l'input per il modello (unendo massa, posizioni e velocità iniziali)
        # Forma finale: (4, 7) per 4 pianeti con 7 feature (massa + posizioni + velocità)
        inputs_batch = np.zeros((4, 7), dtype=np.float32)
        
        # Popola l'array con massa, posizioni iniziali e velocità iniziali
        for i in range(4):
            inputs_batch[i, 0] = m_batch[i]  # Massa
            inputs_batch[i, 1:4] = x_initial_batch[i]  # Posizioni (x, y, z)
            inputs_batch[i, 4:7] = v_initial_batch[i]  # Velocità (vx, vy, vz)
        
        # Aggiungi una dimensione per il batch (forma: (1, 4, 7))
        inputs_batch = inputs_batch[None, :, :]  # Aggiunge la dimensione batch

        # Esegui l'inferenza
        ort_inputs = {input_name: inputs_batch}  # Passa l'input con la dimensione batch
        ort_outputs = ort_session.run(None, ort_inputs)
        
        logger.info(f"Inferenza completata! Shape dell'output: {ort_outputs[0].shape}")
        
        # Stampa l'output del modello
        logger.info("\n--- Dati di Output ---")
        logger.info(f"Posizioni finali predette (x, y, z) per i pianeti: \n{ort_outputs[0]}")
        
        # Stampa la posizione finale rispetto ai valori reali per confronto
        logger.info("\n--- Confronto con le Posizioni Finali Reali ---")
        logger.info(f"Posizioni finali reali (x, y, z): \n{x_final_batch}")
        
        # Calcola l'accuratezza (distanza euclidea tra le posizioni finali reali e quelle predette)
        distances = np.linalg.norm(x_final_batch - ort_outputs[0], axis=1)
        accuracy = np.mean(distances)  # Accuratezza come distanza media

        # Mostra l'accuratezza
        logger.info(f"\n--- Accuratezza ---")
        logger.info(f"Distanze Euclidee medie per ciascun pianeta: {distances}")
        logger.info(f"Accuratezza media: {accuracy}")
    
        # Calcolare l'errore medio assoluto
        errors = np.linalg.norm(ort_outputs[0] - x_final_batch, axis=-1)  # Errore per ciascun pianeta

        # Calcolare l'errore medio
        mean_error = np.mean(errors)

        # Calcolare il valore massimo di verità, ad esempio la distanza massima tra le posizioni reali
        max_distance_real = np.max(np.linalg.norm(x_final_batch, axis=-1))

        # Calcolare l'accuratezza come 1 - errore relativo
        accuracy_percentage = 100 * (1 - mean_error / max_distance_real)

        # Stampa dell'accuratezza come percentuale
        logger.info(f"Accuratezza come percentuale: {accuracy_percentage:.2f}%")

        return accuracy_percentage


    except Exception as e:
        logger.error(f"Errore nel test del modello ONNX: {str(e)}")
        return None


def analyze_npz(file_path):
    """Analizza il contenuto del file .npz e stampa le informazioni sugli array."""
    try:
        # Carica il file .npz
        data = np.load(file_path)

        # Stampa i nomi degli array nel file
        print(f"Chiavi nel file {file_path}:")
        for key in data.keys():
            print(f" - {key}")

        # Mostra alcune informazioni sui dati contenuti in ciascun array
        for key in data.keys():
            array = data[key]
            print(f"\nArray '{key}':")
            print(f" Shape: {array.shape}")
            print(f" Tipo: {array.dtype}")
            print(f" Primo elemento: {array[0] if len(array) > 0 else 'N/A'}")
            if array.ndim <= 2:
                print(f" Primo array completo (seppur limitato): \n{array[:3]}")
            else:
                print(f" Primo array completo (limitato a 3 elementi): \n{array[:3]}")

    except Exception as e:
        print(f"Errore nell'analizzare il file: {e}")


def main():
    onnx_path = Path("GATrExperiments/experiments/nbody/gatr/models/model_final.onnx")
    eval_data_path = Path("GATrExperiments/data/nbody/eval.npz")
    logger.info(f"Testing model: {onnx_path}")
    
    # Test del modello
    accuracy = test_model(onnx_path, eval_data_path)
    logger.info(f"Accuracy: {accuracy}")
    
if __name__ == "__main__":
    main()
