using UnityEngine;
using UnityEngine.UI; // Necesario para trabajar con la UI

public class BloquearBoton : MonoBehaviour
{
    public Button boton; // Asigna el botón en el inspector

    void Start()
    {
        // Obtener el nivel actual de PlayerPrefs
        int nivelActual = PlayerPrefs.GetInt("NivelActual"); // Valor predeterminado es 0
        Debug.Log(nivelActual);
        // Verificar si el nivel es 0 y desactivar el botón si es necesario
        if (nivelActual == 0)
        {
            boton.interactable = false; // Desactiva la interacción del botón
        }
        else
        {
            boton.interactable = true; // Asegura que esté activo si el nivel es mayor a 0
        }
    }
}