using UnityEngine;
using System.Collections.Generic;

public class PlanetSpawner : MonoBehaviour
{
    // 1. Struttura dati per i prefab dei pianeti
    [System.Serializable]
    public class PlanetPrefab
    {
        public string tag;       // Deve corrispondere al tag dei Toggle
        public GameObject prefab; // Prefab del pianeta da spawnare
    }

    // 2. Configurazione nell'Inspector
    [SerializeField] private List<PlanetPrefab> planetPrefabs = new List<PlanetPrefab>();
    [SerializeField] private Transform spawnPoint; // Punto di spawn base

    // 3. Registrazione agli eventi
    private void OnEnable()
    {
        GetComponent<InputManager>().OnDataValidated.AddListener(SpawnPlanet);
    }

    private void OnDisable()
    {
        GetComponent<InputManager>().OnDataValidated.RemoveListener(SpawnPlanet);
    }

    // 4. Metodo principale di spawn
    private void SpawnPlanet()
    {
        // Recupera i dati dall'InputManager
        var data = InputManager.CurrentPlanetData;
        
        // Trova il prefab corretto
        GameObject planetPrefab = planetPrefabs.Find(p => p.tag == data.planetTag).prefab;
        
        // Calcola la posizione finale
        Vector3 spawnPosition = spawnPoint.position + data.position;
        
        // Crea l'istanza del pianeta
        GameObject newPlanet = Instantiate(planetPrefab, spawnPosition, Quaternion.identity);
        
        // Applica le proprietà fisiche
        ApplyPhysics(newPlanet, data);
    }

    // 5. Applicazione delle proprietà fisiche
    private void ApplyPhysics(GameObject planet, InputManager.PlanetData data)
    {
        Rigidbody rb = planet.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.mass = data.mass;          // Imposta la massa
            rb.velocity = data.velocity;  // Imposta la velocità iniziale
        }
        else
        {
            Debug.LogWarning("Manca il componente Rigidbody sul prefab del pianeta!");
        }
    }
}