using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour {
    public ToggleGroup selectedPlanet;
    private List<Planet> planets;

    public Planet initPlanet(){
        IEquatable<Toggle> selected = selectedPlanet.ActiveToggles;

        foreach (Toggle toggle in selected) {
            planets.Add(new Planet(toggle.gameObject.tag))
        }
    }
}
