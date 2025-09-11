using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject settingsMenu;    // Asigna "menuSettings" en el panel Inspector
    public GameObject mainMenu;        // Asigna "MainMenu" en el panel Inspector
    public GameObject Tutorial;        // Asigna "Tutorial" en el panel Inspector
    public GameObject pauseMenu;       // Asigna "menuPause" en el panel Inspector
    private MusicManager musicManager; // Gestionar audio, script

    void Start()
    {
        musicManager = FindObjectOfType<MusicManager>(); // MusicManager para gestionar audio
    }

    public void OpenTutorial()
    {
        mainMenu.SetActive(false);
        Tutorial.SetActive(true);  // muestra menu tutorial
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // nombre de la escena del juego
        if (musicManager != null)
        {
            musicManager.PlayGameMusic(); // Riproduci la musica del gameplay
        }
    }

    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
        if (SceneManager.GetActiveScene().name == "HomeMenu")
        {
            mainMenu.SetActive(false);
        }
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            pauseMenu.SetActive(false);
        }

    }

    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;             // Reanudar el juego
        pauseMenu.SetActive(false);
        if (musicManager != null)
        {
            musicManager.musicSource.volume = 1.0f; // Ripristina il volume
        }
    }

    public void ReplayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recargar la escena actual
    }

    public void Home()
    {
        Time.timeScale = 1;            // Asegura que el tiempo vuelva a la normalidad
        SceneManager.LoadScene("HomeMenu"); // Carga el menú principal
        if (musicManager != null)
        {
            musicManager.PlayMenuMusic(); // Riproduci la musica del menu
        }
    }
    
    public void Back()
    {
        
        if (SceneManager.GetActiveScene().name == "HomeMenu")
        {
            settingsMenu.SetActive(false);
            mainMenu.SetActive(true);
        }
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            settingsMenu.SetActive(false);
            pauseMenu.SetActive(true);
        }
    }

    public void OpenPauseMenu()
    {
        pauseMenu.SetActive(true); // Mostrar el menú de pausa
        Time.timeScale = 0;       // Pausar el juego
        if (musicManager != null)
        {
            musicManager.musicSource.volume = 0.3f; // Abbassa il volume (opzionale)
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Debug.Log("¡Tecla Esc presionada!");
            if (Time.timeScale > 0)
            {
                OpenPauseMenu(); // Si el juego está activo, abre el menú de pausa
            }
            else
            {
                ResumeGame(); // Si el juego ya está en pausa, reanúdalo
            }
        }
    }

}
