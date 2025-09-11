using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{
    private int nivelActual = 0; // Índice del nivel actual
    public int puntos = 0; // Puntos del jugador
    private int totalNiveles; // Total de niveles disponibles
    private int dificultad = 1; // Dificultad del juego
    
    private bool isPaused = true; 
    private bool Guardado = false;
    private bool Cargado = false;
    private bool NivelTerminado = false;

    void Start()
    {
        // Asegúrate de que este script persista entre escenas
        DontDestroyOnLoad(gameObject);
        ContarNivelesDisponibles();
    }
    
    private void ContarNivelesDisponibles()
    {
        totalNiveles = 0;
        while (Resources.Load<NivelConfig>("Niveles/Nivel" + totalNiveles) != null)
        {
            Debug.Log("Nivel " + totalNiveles + " encontrado");
            totalNiveles++;
        }
        Debug.Log($"Total de niveles encontrados: {totalNiveles}");
    }

    // Método para cargar el siguiente nivel
    public void CargarNivel()
    {
        isPaused = true;
        Debug.Log("Cargando nivel " + nivelActual + "/" + totalNiveles);
        if (nivelActual < totalNiveles)
        {
            // Desuscribirse primero para evitar múltiples suscripciones
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            Debug.Log("Todos los niveles completados.");
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Verificar que estamos en la escena correcta
        if (scene.name == "SampleScene")
        {
            Generacion generacion = FindObjectOfType<Generacion>();
            GestorRecetas gestorRecetas = FindObjectOfType<GestorRecetas>();
            if (generacion != null && gestorRecetas != null)
            {
                // Obtener nivel del PlayerPrefs
                nivelActual = PlayerPrefs.GetInt("NivelActual", 0);
                Debug.Log($"Intentando cargar nivel: {nivelActual}");
                
                NivelConfig nivelConfig = Resources.Load<NivelConfig>("Niveles/Nivel" + nivelActual);
                if(nivelConfig != null)
                {
                    Debug.Log($"Configuración del nivel {nivelActual} cargada exitosamente");
                    generacion.Initialize(nivelConfig);
                }
                else
                {
                    Debug.LogError($"No se pudo cargar la configuración del nivel {nivelActual}");
                }

                gestorRecetas.difficultad = dificultad;
            }
            puntos = PlayerPrefs.GetInt("Puntos", puntos);
        }
    }

    // Método para terminar incrementar el nivel actual
    public void TerminarNivel()
    {
        if (nivelActual < totalNiveles - 1)
        {
            Debug.Log($"TerminarNivel: Avanzando de nivel {nivelActual} a {nivelActual + 1}");
            nivelActual++;
            PlayerPrefs.SetInt("NivelActual", nivelActual);
            PlayerPrefs.Save(); // Asegurarse de guardar inmediatamente
            
            puntos += PlayerPrefs.GetInt("Puntos", puntos);
            
            // Configurar difficultad
            // dificultad++;
            // int result = dificultad % 3;
            // if (result == 0)
            // {
            //     dificultad = 3;
            // } else {
            //     dificultad = result;
            // }

            if (nivelActual == 1 || nivelActual == 2)
            {
                dificultad = 2;
            }
            else if (nivelActual == 3 || nivelActual == 4)
            {
                dificultad = 3;
            }
            else
            {
                dificultad = 1;
            }

            PlayerPrefs.SetInt("Dificultad", dificultad);
        }
        else
        {
            Debug.Log("Último nivel completado");
        }
    }

    // Método para cargar el progreso del jugador
    private void CargarProgreso()
    {
        nivelActual = PlayerPrefs.GetInt("NivelActual", nivelActual);
        puntos = PlayerPrefs.GetInt("Puntos", puntos);
        dificultad = PlayerPrefs.GetInt("Dificultad", dificultad);
        Cargado = false;
        CargarNivel();
    }
    
    // Método para guardar el progreso del jugador
    public void GuardarProgreso()
    {
        PlayerPrefs.SetInt("NivelActual", nivelActual);
        PlayerPrefs.SetInt("Puntos", puntos);
        PlayerPrefs.SetInt("Dificultad", dificultad);
        PlayerPrefs.Save();
        Debug.Log("Progreso guardado. Nivel actual: " + nivelActual + ", Puntos: " + puntos);
        
    }

    
    
    public void btn_mainMenuWin()
    {
        // Terminar el nivel y guardar el progreso
        instance.TerminarNivel();
        SceneManager.LoadScene("HomeMenu");
    }
    
    public void btn_mainMenuLose()
    {
        SceneManager.LoadScene("HomeMenu");
    }
    
    public void btn_nextLevel()
    {
        instance.TerminarNivel();
        instance.CargarNivel();
    }
    
    public void btn_loadSavedGame()
    {
        instance.CargarProgreso();
        instance.CargarNivel();
    }
    
    public void btn_playtryAgain()
    {
        instance.CargarNivel();
    }
    
    public void btn_playGame()
    {
        PlayerPrefs.SetInt("NivelActual", 0);
        PlayerPrefs.SetInt("Puntos", 0);
        PlayerPrefs.SetInt("EstrellasTotales", 0);
        PlayerPrefs.Save();
        instance.CargarNivel();
    }
    
    public void btn_mainMenuEnd(){
        PlayerPrefs.SetInt("NivelActual", 0);
        PlayerPrefs.SetInt("Puntos", 0);
        PlayerPrefs.SetInt("EstrellasTotales", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("HomeMenu");
    }
    
}