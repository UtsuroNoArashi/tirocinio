import numpy as np
import onnxruntime as ort
import onnx

# 1. Carica il modello ONNX
model_path = "model_final.onnx"  # Sostituisci con il path corretto al tuo modello
#session = ort.InferenceSession(model_path)

# 2. Genera dati di input dummy (modifica la shape in base al tuo modello)
def generate_dummy_data(batch_size=1):
    """
    Genera dati di input di test con la dimensione corretta.
    Adatta le dimensioni a quelle richieste dal tuo modello.
    """
    input_data = np.random.rand(batch_size, 4, 128).astype(np.float32)  # Adatta questa shape
    return input_data

# 3. Esegui l'inferenza
def run_inference(session, input_data):
    """
    Esegue l'inferenza con il modello ONNX.
    """
    input_name = session.get_inputs()[0].name  # Nome del primo input
    output_name = session.get_outputs()[0].name  # Nome del primo output

    # Esegui l'inferenza
    output = session.run([output_name], {input_name: input_data})
    return output


model = onnx.load("model_final.onnx")
for input_tensor in model.graph.input:
    print(f"Input: {input_tensor.name}, Tipo: {input_tensor.type}")
for output_tensor in model.graph.output:
    print(f"Output: {output_tensor.name}, Tipo: {output_tensor.type}")


# 4. Testa il modello
dummy_input = generate_dummy_data(batch_size=2)  # Cambia batch_size secondo necessità
output = run_inference(session, dummy_input)

# 5. Mostra i risultati
print("Input shape:", dummy_input.shape)
print("Output shape:", np.array(output[0]).shape)  # Output del modello
print("Output (prima riga):", output[0][0])  # Mostra il primo risultato
