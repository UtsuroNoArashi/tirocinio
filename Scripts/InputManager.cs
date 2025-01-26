using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class InputManager : MonoBehaviour
{
    [Header("Planet Selection")]
    [SerializeField] public ToggleGroup planetToggleGroup;

    [Header("Input Fields")]
    [SerializeField] public TMP_InputField massInput;  

    // Add more fields for velocity and position
    [SerializeField] public TMP_InputField velocityInputX;
    [SerializeField] public TMP_InputField velocityInputY;
    [SerializeField] public TMP_InputField velocityInputZ;

    [SerializeField] public TMP_InputField positionInputX;
    [SerializeField] public TMP_InputField positionInputY;
    [SerializeField] public TMP_InputField positionInputZ;

    [Header("UI Interactions")]
    [SerializeField] public XRSimpleInteractable confirmButton;
    [SerializeField] private XRVirtualKeyboard xrKeyboard;


    // Class that contains planet Data
    public class PlanetData
    {
        public string name;
        public float mass;
        public Vector3 velocity;
        public Vector3 position;
    }
    
    // Private and only read
    public static PlanetData CurrentPlanetData { get; private set; } = new PlanetData();

    // Event
    public UnityEvent OnDataValidated = new UnityEvent();

    private void Start()
    {
        confirmButton.selectEntered.AddListener(ProcessInput);
        SetupKeyboard();
    }

    private void SetupKeyboard()
    {
        var inputFields = GetComponentsInChildren<TMP_InputField>();
        foreach(var field in inputFields)
        {
            var keyboardInput = field.gameObject.AddComponent<XRKeyboardInputField>();
            keyboardInput.keyboard = xrKeyboard;
            
            // Add selection handler
            var trigger = field.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
            entry.eventID = UnityEngine.EventSystems.EventTriggerType.Select;
            entry.callback.AddListener((data) => OnInputFieldSelected(field));
            trigger.triggers.Add(entry);
        }
    }

    private void OnInputFieldSelected(TMP_InputField field)
    {
        xrKeyboard.SetActiveInputField(field);
    }

   
    public void ProcessInput(SelectEnterEventArgs args)
    {
        if(TryParseInputs(out PlanetData data))
        {
            SavePlanetData(data);
            OnDataValidated.Invoke();
            ClearInputFields();
        }
    }

    private bool TryParseInputs(out PlanetData data)
    {
        data = new PlanetData();
        bool parseSuccess = true;

        try
        {
            // Get selected planet
            Toggle selectedToggle = planetToggleGroup.GetFirstActiveToggle();
            if(selectedToggle == null)
            {
                Debug.LogError("Nessun pianeta selezionato!");
                parseSuccess = false;
            }
            data.planetTag = selectedToggle.tag;

            // Parse input values
            data.mass = float.Parse(massInput.text);
            data.velocity = new Vector3(
                float.Parse(velocityInputX.text),
                float.Parse(velocityInputY.text),
                float.Parse(velocityInputZ.text)
            );
            
            data.position = new Vector3(
                float.Parse(positionInputX.text),
                float.Parse(positionInputY.text),
                float.Parse(positionInputZ.text)
            );
        }
        catch (System.FormatException)
        {
            Debug.LogError("Formato input non valido!");
            parseSuccess = false;
        }

        return parseSuccess;
    }

    private void SavePlanetData(PlanetData data)
    {
        CurrentPlanetData = data;
        Debug.Log($"Dati salvati:\n" +
                 $"Pianeta: {data.planetTag}\n" +
                 $"Massa: {data.mass}\n" +
                 $"Velocità: {data.velocity}\n" +
                 $"Posizione: {data.position}");
    }

    private void ClearInputFields()
    {
        massInput.text = "";
        velocityInputX.text = "";
        velocityInputY.text = "";
        velocityInputZ.text = "";
        positionInputX.text = "";
        positionInputY.text = "";
        positionInputZ.text = "";
    }
}
