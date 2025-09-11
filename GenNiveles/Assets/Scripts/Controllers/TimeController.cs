using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Events;

public class TimeController : MonoBehaviour
{
    public float startTime = 181f;
    [HideInInspector] public float currentTime;
    public float currentTime_animation;
    public TextMeshProUGUI countdownText;
    public UnityEvent OnTimeOver;
    private bool timeOverCalled = false; // Variable para verificar si la función ya ha sido llamada
    private Personaje personaje; // Referencia a la clase Personaje

    void Start()
    {
        currentTime = startTime;
        currentTime_animation = startTime + 10f;
        UpdateTimerDisplay();
        personaje = FindObjectOfType<Personaje>(); // Obtener la referencia al personaje
    }

    void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else if (!timeOverCalled)
        {
            timeOverCalled = true;
            currentTime = 0;
            UpdateTimerDisplay();
            Debug.Log("¡Tiempo terminado!");
            OnTimeOver.Invoke();
            personaje.SetTiempoAgotado(true); // Llamar a SetTiempoAgotado cuando el tiempo se agote
            GameObject truck = GameObject.FindWithTag("TruckAnimation");
            truck.GetComponent<AnimaciónCamino>().PlayAnimation();

            GameObject camara = GameObject.FindWithTag("MainCamera");
            camara.GetComponent<ControlCamara>().distanciaInicial = 16f;
            camara.GetComponent<ControlCamara>().alturaInicial = 20f;
        }

        if (currentTime_animation > 0)
        {
            currentTime_animation -= Time.deltaTime;
        }
        else
        {
            currentTime_animation = 0;
            //Debug.Log("¡Tiempo de animación terminado!");
        }
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}