using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour {
    public ToggleGroup toggledPlanets;
    public Button confirm;

    private List<Planet> planets = Button.planetes;

    void print(){
        foreach (Planet planet in planets) {
            Console.WriteLine($"Planet: {planet.tag}");
        }
    }
}
