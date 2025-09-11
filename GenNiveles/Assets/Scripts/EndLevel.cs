using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class EndLevel : MonoBehaviour
{
    // Referencias al Timer del juego
    [Header("Timer")]
    public GameObject Timer;
    private float levelTime;
    private float timeLeft;
    
    // Referencias al CanvasGroup y RectTransform del panel de fin de juego
    public float fadeDuration = 1.0f;
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;
    
    [Header("Elementos de la UI a modificar")]
    public GameObject textoNivel;
    public GameObject textoSubtitulo;
    public GameObject contenidoPuntuacion;
    public GameObject botonesWin, botonesLose, botonesLastLevel;
    
    private int animationCount = 0;  // Contador de animaciones
    public int maxAnimations = 3;    // Máximo número de animaciones permitidas
    
    // Referencias al GameObject del personaje y su puntuación
    public GameObject personaje;
    public int puntuacion;
    
    private bool hasInitialized = false;
    private bool isInitializing = false;
    private bool puntuacionComprobada = false;
    
    public bool lastLevel = false;

    // Sistema de estrellas
    [System.Serializable]
    public class StarContainer
    {
        public RectTransform container;
        public RectTransform star;
        public RectTransform background;
    }

    public StarContainer[] starContainers = new StarContainer[3];
    
    private float starDelay = 0.3f;
    private float starAnimDuration = 0.5f;
    private float starRotations = 1f;
    private float starFinalScale = 1f;
    private float backgroundFinalScale = 1.2f;

    private void InitializeComponents()
    {
        // Buscar referencias necesarias
        Timer = GameObject.Find("Timer");
        personaje = GameObject.Find("Personaje");
        
        if (personaje != null)
        {
            puntuacion = personaje.GetComponent<Personaje>().puntuacion;
        }
        
        // Inicializar textos y botones
        textoNivel = GameObject.Find("win_lose_msg");
        textoSubtitulo = GameObject.Find("submsg");
        contenidoPuntuacion = GameObject.Find("score_value");
        botonesWin = GameObject.Find("win_buttons");
        botonesLose = GameObject.Find("lose_buttons");
        botonesLastLevel = GameObject.Find("lastlvl_buttons");

        // Inicializar contenedores de estrellas
        for (int i = 0; i < 3; i++)
        {
            if (starContainers[i] == null || starContainers[i].container == null)
            {
                GameObject container = GameObject.Find($"StarContainer{i + 1}");
                if (container != null)
                {
                    starContainers[i] = new StarContainer
                    {
                        container = container.GetComponent<RectTransform>(),
                        star = container.transform.Find("Star")?.GetComponent<RectTransform>(),
                        background = container.transform.Find("Background")?.GetComponent<RectTransform>()
                    };
                }
            }
        }
    }

    void Start()
    {
        InitializeComponents();
        InitializeStars();
        animationCount = 0;
        ResetAnimationState();
    }

    private void InitializeStars()
    {
        Debug.Log("Inicializando estrellas...");
        foreach (var starContainer in starContainers)
        {
            if (starContainer?.container != null)
            {
                if (starContainer.star != null)
                {
                    starContainer.star.localScale = Vector3.zero;
                    Debug.Log("Estrella inicializada");
                }
                if (starContainer.background != null)
                {
                    starContainer.background.localScale = Vector3.zero;
                    Debug.Log("Background inicializado");
                }
            }
        }
    }
    
    void Update()
    {
        levelTime = Timer.GetComponent<TimeController>().startTime;
        timeLeft = Timer.GetComponent<TimeController>().currentTime_animation;
        if (timeLeft <= 0 && !puntuacionComprobada)
        {
            Show();
        }
    }
    
    public void comprobarPuntuacion()
    {   
        puntuacionComprobada = true;
        puntuacion = personaje.GetComponent<Personaje>().puntuacion;
        Debug.Log($"Comprobando puntuación: {puntuacion}");
        
        // Primero desactivamos todos los paneles de botones
        botonesWin.SetActive(false);
        botonesLose.SetActive(false);
        botonesLastLevel.SetActive(false);
        
        // Ahora comprobamos las condiciones
        if (lastLevel && puntuacion >= 1000)
        {
            textoNivel.GetComponent<TMPro.TextMeshProUGUI>().text = "Congratulations!";
            textoSubtitulo.GetComponent<TMPro.TextMeshProUGUI>().text = "You've completed the last level!";
            contenidoPuntuacion.GetComponent<TMPro.TextMeshProUGUI>().text = puntuacion.ToString();
            botonesLastLevel.SetActive(true);
        }
        else if (puntuacion >= 1000)
        {
            textoNivel.GetComponent<TMPro.TextMeshProUGUI>().text = "Nice job!";
            textoSubtitulo.GetComponent<TMPro.TextMeshProUGUI>().text = "Level " + PlayerPrefs.GetInt("NivelActual", 0) + " completed";
            contenidoPuntuacion.GetComponent<TMPro.TextMeshProUGUI>().text = puntuacion.ToString();
            botonesWin.SetActive(true);
        }
        else
        {   
            textoNivel.GetComponent<TMPro.TextMeshProUGUI>().text = "Don't give up!";
            textoSubtitulo.GetComponent<TMPro.TextMeshProUGUI>().text = "You can do it";
            contenidoPuntuacion.GetComponent<TMPro.TextMeshProUGUI>().text = puntuacion.ToString();
            botonesLose.SetActive(true);
        }
        
        // Solo animamos si no hemos alcanzado el máximo de animaciones
        if (animationCount < maxAnimations)
        {
            int estrellas = CalculateStars();
            Debug.Log($"Mostrando {estrellas} estrellas - Animación {animationCount + 1} de {maxAnimations}");
            AnimateStars(estrellas);
            animationCount++;
        }
    }

    public void comprobarUltNivel()
    {
        int totalNiveles = 0;
        while (Resources.Load<NivelConfig>("Niveles/Nivel" + totalNiveles) != null)
        {
            Debug.Log("Nivel " + totalNiveles + " encontrado");
            totalNiveles++;
        }
        
        Debug.Log("Total de niveles: " + totalNiveles + " - Nivel actual: " + PlayerPrefs.GetInt("NivelActual"));

        if (totalNiveles - 1 == PlayerPrefs.GetInt("NivelActual"))
        {
            lastLevel = true;
        }
        else
        {
            lastLevel = false;
        }
    }

    private int CalculateStars()
    {
        int estrellas = PlayerPrefs.GetInt("EstrellasTotales", 0);
        int puntuacionTotal = PlayerPrefs.GetInt("Puntos", 0);
    
        // Cálculo de estrellas basado en la puntuación
        if (puntuacion >= 3000)
        {
            PlayerPrefs.SetInt("EstrellasTotales", estrellas + 3);
            PlayerPrefs.SetInt("Puntos", puntuacionTotal + personaje.GetComponent<Personaje>().puntuacion);
            return 3;
        }
        else if (puntuacion >= 2000)
        {
            PlayerPrefs.SetInt("EstrellasTotales", estrellas + 2);
            PlayerPrefs.SetInt("Puntos", puntuacionTotal + personaje.GetComponent<Personaje>().puntuacion);
            return 2;
        }
        else if (puntuacion >= 1000)
        {
            PlayerPrefs.SetInt("EstrellasTotales", estrellas + 1);
            PlayerPrefs.SetInt("Puntos", puntuacionTotal + personaje.GetComponent<Personaje>().puntuacion);
            return 1;
        }
        else
        {
            return 0;
        }
    }

    private void AnimateStars(int starCount)
    {
        Debug.Log($"Animando {starCount} estrellas");
        
        // Primero, animar todos los backgrounds
        for (int i = 0; i < starContainers.Length; i++)
        {
            if (starContainers[i]?.container != null)
            {
                Debug.Log($"Animando background {i + 1}");
                AnimateBackground(starContainers[i], i * starDelay);
            }
        }

        // Luego, animar solo las estrellas obtenidas
        for (int i = 0; i < starCount && i < starContainers.Length; i++)
        {
            if (starContainers[i]?.container != null)
            {
                Debug.Log($"Animando estrella {i + 1}");
                AnimateStar(starContainers[i], i * starDelay);
            }
        }
    }

    private void AnimateBackground(StarContainer starContainer, float delay)
    {
        if (starContainer.background != null)
        {
            Sequence bgSequence = DOTween.Sequence();
            starContainer.background.localScale = Vector3.zero;
            
            bgSequence
                .AppendInterval(delay)
                .Append(starContainer.background.DOScale(backgroundFinalScale * 1.3f, starAnimDuration * 0.3f))
                .Append(starContainer.background.DOScale(backgroundFinalScale, starAnimDuration * 0.2f));
        }
    }

    private void AnimateStar(StarContainer starContainer, float delay)
    {
        if (starContainer.star != null)
        {
            Sequence starSequence = DOTween.Sequence();
            starContainer.star.localScale = Vector3.zero;
            starContainer.star.localRotation = Quaternion.identity;
            
            starSequence
                .AppendInterval(delay)
                .Append(starContainer.star.DOScale(starFinalScale * 1.2f, starAnimDuration * 0.5f))
                .Join(starContainer.star.DORotate(new Vector3(0, 0, 360 * starRotations), starAnimDuration * 0.5f, RotateMode.FastBeyond360))
                .Append(starContainer.star.DOScale(starFinalScale, starAnimDuration * 0.2f))
                .Join(starContainer.star.DOShakeRotation(starAnimDuration * 0.3f, 30f));
        }
    }
    
    public void Show()
    {
        comprobarUltNivel();
        comprobarPuntuacion();
        canvasGroup.alpha = 0;
        rectTransform.transform.localPosition = new Vector3(0, -1000f, 0);
        Sequence showSequence = DOTween.Sequence();
        showSequence.Append(rectTransform.DOAnchorPos(new Vector2(0, 0), fadeDuration).SetEase(Ease.OutBounce));
        showSequence.Join(canvasGroup.DOFade(1.0f, fadeDuration));
        showSequence.Append(rectTransform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.2f).SetEase(Ease.OutQuad));
        showSequence.Append(rectTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad));
    }

    public void Hide()
    {
        canvasGroup.alpha = 1f;
        rectTransform.transform.localPosition = new Vector3(0, 0, 0);
        Sequence hideSequence = DOTween.Sequence();
        hideSequence.Append(rectTransform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.2f).SetEase(Ease.InQuad));
        hideSequence.Append(rectTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InQuad));
        hideSequence.Append(rectTransform.DOAnchorPos(new Vector2(0, -1000f), fadeDuration).SetEase(Ease.InBounce));
        hideSequence.Join(canvasGroup.DOFade(0.0f, fadeDuration));
    }

    private void ResetAnimationState()
    {
        // Resetear estado de animación
        animationCount = 0;
        
        // Resetear posición y alpha del panel
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }
        
        if (rectTransform != null)
        {
            rectTransform.transform.localPosition = new Vector3(0, -1000f, 0);
            rectTransform.localScale = Vector3.one;
        }

        // Matar todas las animaciones previas
        DOTween.Kill(rectTransform);
        DOTween.Kill(canvasGroup);
        
        foreach (var starContainer in starContainers)
        {
            if (starContainer?.star != null)
            {
                DOTween.Kill(starContainer.star);
            }
            if (starContainer?.background != null)
            {
                DOTween.Kill(starContainer.background);
            }
        }
    }
}