using System;
using UnityEngine;
using TMPro;

public class EndGame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI starsCountText;

    private void Start()
    {
        if (finalScoreText == null)
        {
            finalScoreText = GameObject.Find("finalscore_value").GetComponent<TextMeshProUGUI>();
            Debug.Log("finalscore_value: " + finalScoreText);
        }
        if (starsCountText == null)
        {
            starsCountText = GameObject.Find("finalstars_count").GetComponent<TextMeshProUGUI>();
            Debug.Log("finalstars_count: " + starsCountText);
        }
    }

    public void UpdateScore()
    {
        // Actualizar texto de puntuación
        if (finalScoreText != null)
        {
            Debug.Log("Puntos: " + PlayerPrefs.GetInt("Puntos", 0));
            finalScoreText.text = PlayerPrefs.GetInt("Puntos", 0).ToString();
            
        }

        // Calcular y actualizar estrellas
        if (starsCountText != null)
        {
            Debug.Log("Estrellas: " + PlayerPrefs.GetInt("EstrellasTotales", 0));
            starsCountText.text = PlayerPrefs.GetInt("EstrellasTotales", 0).ToString();
        }
    }
}