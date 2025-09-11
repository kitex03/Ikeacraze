using UnityEngine;
using System.Collections;

public class ControlCamara : MonoBehaviour
{
    public Transform spawnerTransform; // Spawner a partir del cual se genera el tablero de juego
    public float distanciaInicial = 10f; // Distancia de la camara al centro del rectangulo
    public float alturaInicial = 10f; // Altura de la camara
    
    [Header("Configuración del Zoom")]
    public float distanciaCercana = 5f;  // Distancia cuando la cámara está cerca
    public float alturaCercana = 5f;     // Altura cuando la cámara está cerca
    public float velocidadInterpolacion = 5f;  // Velocidad de la transición
    public float velocidadRotacion = 5f;  // Velocidad de la rotación
    public KeyCode teclaZoom = KeyCode.Z;  // Tecla para activar/desactivar el zoom
    public Transform personajeTransform; // Personaje al que sigue la cámara
    
    private Vector3 centroRectangulo; // Centro del rectangulo generado
    
    private bool estaZoomActivo = false;
    private float distanciaActual;
    private float alturaActual;


    void Start()
    {
        distanciaActual = distanciaInicial;
        alturaActual = alturaInicial;
        StartCoroutine(calcularCentro());
    }

    void Update()
    {
        if (centroRectangulo != Vector3.zero)
        {
            // Zoom activo/desactivado
            if (Input.GetKeyDown(teclaZoom))
            {
                estaZoomActivo = !estaZoomActivo;
            }
            
            if(estaZoomActivo)
            {
                // Interpolación de la distancia y la altura para una transición suave
                distanciaActual = Mathf.Lerp(distanciaActual, distanciaCercana, Time.deltaTime * velocidadInterpolacion);
                alturaActual = Mathf.Lerp(alturaActual, alturaCercana, Time.deltaTime * velocidadInterpolacion);
                
                // Nueva posición en base al personaje
                Vector3 posicionPersonaje = personajeTransform.position;
                Vector3 posicionZoom = posicionPersonaje - spawnerTransform.forward * distanciaActual + Vector3.up * alturaActual;
    
                // Aplicar la posición interpolada
                transform.position = Vector3.Lerp(transform.position, posicionZoom, Time.deltaTime * velocidadInterpolacion);
                
                // Suavizado de rotación: interpolar hacia la rotación deseada
                Quaternion rotacionDeseada = Quaternion.LookRotation(personajeTransform.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, Time.deltaTime * velocidadRotacion);
            }
            else
            {
                distanciaActual = Mathf.Lerp(distanciaActual, distanciaInicial, Time.deltaTime * velocidadInterpolacion);
                alturaActual = Mathf.Lerp(alturaActual, alturaInicial, Time.deltaTime * velocidadInterpolacion);
                
                // Calcular la posicion deseada de la camara
                Vector3 posicionInicial = centroRectangulo - spawnerTransform.forward * distanciaActual + Vector3.up * alturaActual;
    
                // Establecer la posicion de la camara
                transform.position = Vector3.Lerp(transform.position, posicionInicial, Time.deltaTime * velocidadInterpolacion);;
                            
                // Look-at al centro del rectangulo
                transform.LookAt(centroRectangulo);
            }
            
            
        }
    }
    
    IEnumerator calcularCentro()
    {
        Generacion generacionScript = spawnerTransform.GetComponent<Generacion>();
        if (generacionScript != null)
        {
            // Esperar a que se haya calculado el tilesize durante la ejecución de la generación
            while (generacionScript.tileSize == Vector3.zero)
            {
                yield return null;
            }
    
            float centroX = generacionScript.rectX + generacionScript.rectWidth / 2f;
            float centroY = generacionScript.rectY + generacionScript.rectHeight / 2f;
            Debug.Log("Centro del rectángulo: " + centroX + ", " + centroY);
        
            // Calculate grid-aligned position
            float gridX = Mathf.Round(centroX) * generacionScript.tileSize.x;
            float gridZ = Mathf.Round(centroY) * generacionScript.tileSize.z;
        
            // Set final position aligned to grid
            centroRectangulo = spawnerTransform.position + new Vector3(gridX, 0, gridZ);
            if (generacionScript.rectWidth % 2 == 0)
            {
                centroRectangulo -= new Vector3(generacionScript.tileSize.x / 2f, 0, 0);
            }
            else{
                centroRectangulo.x -= 1;
            }

            transform.position = centroRectangulo;
        }
        else
        {
            Debug.LogError("No se ha encontrado el script de generacion en el spawner");
        }
    }
}