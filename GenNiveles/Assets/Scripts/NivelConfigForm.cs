using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class NivelFormManager : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider rectWidthSlider;
    [SerializeField] private Slider rectHeightSlider;
    [SerializeField] private Slider holesNumberSlider;
    [SerializeField] private Slider difficultySlider;

    [Header("InputField")]
    [SerializeField] private TMP_InputField playTimeInput;

    [Header("Value Texts")]
    [SerializeField] private TMP_Text rectWidthText;
    [SerializeField] private TMP_Text rectHeightText;
    [SerializeField] private TMP_Text holesNumberText;
    [SerializeField] private TMP_Text difficultyText;

    [Header("Controls")]
    [SerializeField] private Button saveButton;

    private void Start()
    {
        // Configurar los listeners de los sliders
        rectWidthSlider.onValueChanged.AddListener(OnRectWidthChanged);
        rectHeightSlider.onValueChanged.AddListener(OnRectHeightChanged);
        holesNumberSlider.onValueChanged.AddListener(OnHolesNumberChanged);

        // Configurar el botón de guardado
        saveButton.onClick.AddListener(CreateAndSaveLevel);

        // Inicializar los textos con los valores actuales
        UpdateAllTexts();
    }

    private void UpdateAllTexts()
    {
        OnRectWidthChanged(rectWidthSlider.value);
        OnRectHeightChanged(rectHeightSlider.value);
        OnHolesNumberChanged(holesNumberSlider.value);
    }

    private void OnRectWidthChanged(float value)
    {
        rectWidthText.text = value.ToString();
    }

    private void OnRectHeightChanged(float value)
    {
        rectHeightText.text = value.ToString();
    }

    private void OnHolesNumberChanged(float value)
    {
        holesNumberText.text = value.ToString();
    }

    public void CreateAndSaveLevel()
    {
        // Validar el tiempo de juego
        if (!int.TryParse(playTimeInput.text, out int playTime))
        {
            Debug.LogError("Tiempo de juego inválido");
            return;
        }

        // Crear nuevo NivelConfig
        NivelConfig nuevoNivel = ScriptableObject.CreateInstance<NivelConfig>();

        // Asignar valores del formulario
        nuevoNivel.rectWidth = (int)rectWidthSlider.value;
        nuevoNivel.rectHeight = (int)rectHeightSlider.value;
        nuevoNivel.numeroDeHuecos = Mathf.RoundToInt(holesNumberSlider.value);
        nuevoNivel.tiempoDeJuego = playTime;

        // Obtener el siguiente número de nivel
        int nextLevelNumber = GetNextLevelNumber();

        // Guardar el ScriptableObject
        #if UNITY_EDITOR
        string directoryPath = "Assets/Resources/Niveles";
        
        // Crear el directorio si no existe
        if (!System.IO.Directory.Exists(directoryPath))
        {
            System.IO.Directory.CreateDirectory(directoryPath);
        }

        string path = $"{directoryPath}/Nivel{nextLevelNumber}.asset";
        AssetDatabase.CreateAsset(nuevoNivel, path);
        AssetDatabase.SaveAssets();
        Debug.Log($"Nivel guardado en: {path}");
        #endif
        
        // SceneController
        SceneController.instance.btn_nextLevel();
    }

    private int GetNextLevelNumber()
    {
        // Buscar el último nivel en Resources/Niveles
        Object[] niveles = Resources.LoadAll("Niveles", typeof(NivelConfig));
        return niveles.Length;
    }

    private void OnDestroy()
    {
        // Limpiar los listeners
        if (rectWidthSlider != null) rectWidthSlider.onValueChanged.RemoveListener(OnRectWidthChanged);
        if (rectHeightSlider != null) rectHeightSlider.onValueChanged.RemoveListener(OnRectHeightChanged);
        if (holesNumberSlider != null) holesNumberSlider.onValueChanged.RemoveListener(OnHolesNumberChanged);
        if (saveButton != null) saveButton.onClick.RemoveListener(CreateAndSaveLevel);
    }
}