using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button : MonoBehaviour
{

    List<Planet> planets = new List<Planet> { };

    void GetPlanets(ToggleGroup group)
    {
        IEquatable<Toggle> toggledPlanetes = group.ActiveToggles();

        foreach (Toggle toggle in toggledPlanetes)
        {
            planets.Add(new Planet(toggle.gameObject.tag));
        }
    }
}
