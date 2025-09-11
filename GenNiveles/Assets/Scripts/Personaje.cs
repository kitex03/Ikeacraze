using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class Personaje : MonoBehaviour
{
    public SFXManager sfxManager;

    // Variables para el posicionamiento del personaje
    public GameObject spawnerTransform;                 // Spawner a partir del cual se genera el tablero de juego
    private Vector3 centroRectangulo;                   // Centro del rectangulo generado

    // Variables para el crafteo, coger y dejar objetos
    public GameObject spawnerEspalda;
    public Item objetoPersonaje;
    private bool estaCrafteando = false;
    private bool creafteoPausado = false;
    private bool objetoYaCrafteado = false;
    private bool estaCogiendo = false;
    private bool tiempoAgotado = false;

    private bool areaCrafteo = false;
    private bool areaCofre = false;
    private bool areaPapelera = false;
    private bool areaEntrega = false;

    private bool recetaValida = false;

    private Crafteo workbench = null;
    private Chest chest;
    private GestorRecetas _gestorRecetas;
    private GameObject entrega;

    // Variables para la caída
    public float intervaloActualizacion = 0.5f;         // Intervalo de actualización en segundos
    public float umbralCaida = -5f;
    private Rigidbody rb;
    private List<Vector3> posicionesRecientes = new List<Vector3>();
    private Vector3 posicionAnterior;

    // Variables para la GUI
    public int puntuacion = 0;                                 // Variable para almacenar la puntuación
    public TextMeshProUGUI puntuacionText;                      // Referencia al componente de texto en la UI que dice la puntuacion
    public TextMeshProUGUI collectedItemNameText;               // Referencia al componente de texto en la UI que dice el nombre del objeto recogido
    public TextMeshProUGUI collectedItemCountText;              // Referencia al componente de texto en la UI que dice la cantidad del objeto recogido
    public TextMeshProUGUI numeroNivelText;                     // Referencia al componente de texto en la UI que dice el número del nivel
    private string collectedItemName;                           // Nombre del objeto recogido
    private int collectedItemCount;                             // Cantidad del objeto recogido

    private void Start()
    {
        objetoPersonaje = null;                         // Inicialmente el personaje no tiene ningún objeto
        posicionAnterior = transform.position;          // Guardamos la posición inicial del personaje
        rb = GetComponent<Rigidbody>();                 // Obtener el componente Rigidbody
        StartCoroutine(ActualizarPosicion());     // Iniciar la corrutina de actualización de posición para tener siempre almacenada la última posición segura
        StartCoroutine(calcularCentro());         // Calcular el centro del rectángulo generado
        _gestorRecetas = GameObject.Find("RecetasController").GetComponent<GestorRecetas>();

        // Inicializar la puntuación en la UI
        ActualizarPuntuacionUI();
        collectedItemNameText.text = "Collected item: ";
        collectedItemCountText.text = "Quantity: 0";
        numeroNivelText.text = "Level " + PlayerPrefs.GetInt("NivelActual");

        // Get reference to SFXManager
        sfxManager = GameObject.FindObjectOfType<SFXManager>();
    }

    private void Update()
    {
        // Si el personaje cae por debajo del umbral de caída, reposicionarlo a la última posición segura tras 1 segundo
        if (transform.position.y < umbralCaida)
        {
            Debug.Log("Cayendo...");
            StartCoroutine(RepositionAfterDelay(gameObject, 1f));
        }
        // ActualizarPuntuacionUI();
        if (!tiempoAgotado)
        {
            keyMap();
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void keyMap()
    {
        // Tecla Q
        if (Input.GetKey(KeyCode.Q))
        {
            // DEJAR OBJETO EN EL WORKBENCH
            if (areaCrafteo && objetoPersonaje != null)
            {
                Debug.Log("TECLA Q. DEJAR OBJETO EN EL WORKBENCH");
                DejarObjeto();
                sfxManager.PlayDropItem();  // Play the drop item sound effect
                collectedItemCount = 0;
                collectedItemName = "";
                ActualizarCollectedItemsUI();
            }

            // ENTREGAR OBJETO
            if (areaEntrega && objetoYaCrafteado)
            {
                Debug.Log("TECLA Q. ENTREGAR OBJETO");
                // Incrementar la puntuación y actualizar la UI
                int puntos = _gestorRecetas.Entregar(objetoPersonaje);
                IncrementarPuntuacion(puntos);

                Vector3 posicionEntrega = entrega.transform.position;
                posicionEntrega.y +=  (entrega.transform.localScale.y * 0.5f);
                GameObject objeto = Instantiate(objetoPersonaje.prefab, entrega.transform);
                Destroy(objeto, 2);
                objeto.transform.position = posicionEntrega;

                objetoPersonaje = null;
                objetoYaCrafteado = false;
                sfxManager.PlaySoundDeliver();  // Play the sound deliver effect
            }

            // TIRAR OBJETO A LA PAPELERA
            if (areaPapelera)
            {
                Debug.Log("TECLA Q. TIRAR OBJETO A LA PAPELERA");

                while (spawnerEspalda.transform.childCount > 0)
                {
                    objetoPersonaje = null;
                    DestroyImmediate(spawnerEspalda.transform.GetChild(0).gameObject);
                }

                // Actualizar la UI al tirar el objeto
                collectedItemCount = 0;
                collectedItemName = "";
                ActualizarCollectedItemsUI();
                sfxManager.PlayTrashItem();  // Play trash item sound effect
            }
        }

        // Tecla E
        if (Input.GetKeyUp(KeyCode.E))
        {
            // COGER OBJETO DEL COFRE
            if (areaCofre)
            {
                Debug.Log("TECLA E. COGER OBJETO DEL COFRE");
                CogerObjeto();
                sfxManager.PlayChestOpen();
            }

            // COGER OBJETO CRAFTEADO DEL WORKBENCH
            if (areaCrafteo && objetoYaCrafteado)
            {
                Debug.Log("TECLA E. COGER OBJETO CRAFTEADO DEL WORKBENCH");
                objetoPersonaje = workbench.GetObjeto();
                workbench.yaCrafteado = false;
            }
        }

        if(Input.GetKey(KeyCode.C))
        {

            if (areaCrafteo && !workbench.yaCrafteado && recetaValida)
            {
                Debug.Log("en area de crafteo y con receta valida");
                if (!estaCrafteando)
                {
                    estaCrafteando = true;
                    workbench.tiempoTranscurrido = 0; // Reiniciar el tiempo si es un nuevo crafteo
                    workbench.crafteoBar.SetActive(true); // Mostrar la barra de progreso al iniciar
                    var emission = workbench.sistemaParticulas.emission;
                    emission.rateOverTime = 10; // Set emission rate to desired value
                    sfxManager.PlaySoundCrafting();
                }
                if(creafteoPausado)
                {
                    var emission = workbench.sistemaParticulas.emission;
                    emission.rateOverTime = 10; // Set emission rate to desired value
                    creafteoPausado = false;
                }

                workbench.tiempoTranscurrido += Time.deltaTime;

                if (workbench.sliderCrafteo != null)
                    workbench.sliderCrafteo.value = workbench.tiempoTranscurrido;

                // Si el tiempo de crafteo se completa
                if (workbench.tiempoTranscurrido >= workbench.tiempoCrafteo)
                {
                    workbench.Craftear();
                    estaCrafteando = false; // Marcar como terminado
                    workbench.yaCrafteado = true;
                    objetoYaCrafteado = true;
                    sfxManager.PlayCraftingComplete();  // Play the crafting complete sound effect if it's available
                    Debug.Log("Crafteo completado");

                    //pausar sist. particulas
                    var emission = workbench.sistemaParticulas.emission;
                    emission.rateOverTime = 0; // Set emission rate to 0

                    // Ocultar la barra al completar el crafteo
                    workbench.crafteoBar.SetActive(false);
                    if (workbench.sliderCrafteo != null)
                        workbench.sliderCrafteo.value = 0; // Reiniciar el valor del slider
                }

            }
        }
        else if(Input.GetKeyUp(KeyCode.C))
        {
            if (estaCrafteando)
            {
                var emission = workbench.sistemaParticulas.emission;
                emission.rateOverTime = 0; // Set emission rate to 0
                creafteoPausado = true;
                Debug.Log("Crafteo pausado, la barra permanece visible.");
            }
        }

        // Tecla X
        if (Input.GetKeyUp(KeyCode.X))
        {
            // ELIMINAR LOS OBJETOS DEL WORKBENCH
            if (areaCrafteo)
            {
                workbench.DestruirObjetos();
            }
        }
    }
    public void SetTiempoAgotado(bool agotado)
    {
        tiempoAgotado = agotado;
    }
    
    /**
    * Función que se encarga de coger un objeto del cofre
    */
    private void CogerObjeto()
    {
        // Llamamos a la función de coger objeto del cofre que devuelve el material
        ItemDescription material = chest.CogerMaterial();

        // Incrementar la puntuación y actualizar la UI
        //IncrementarPuntuacion(10);

        // Si el personaje no tiene ningún objeto, se le asigna el objeto cogido
        if (objetoPersonaje == null || objetoPersonaje.prefab == null)
        {
            objetoPersonaje = new Item(material.id, material.prefab);
            collectedItemName = objetoPersonaje.prefab.name;
            collectedItemCount = material.cantidad;
            Debug.Log("El personaje ha cogido " + collectedItemName + " cantidad: " + collectedItemCount);

            GameObject objeto = Instantiate(objetoPersonaje.prefab, spawnerEspalda.transform);
            objeto.transform.parent = spawnerEspalda.transform;
            objeto.transform.position = spawnerEspalda.transform.position;
            objeto.transform.localScale = new Vector3((float)0.05, (float)0.05, (float)0.05);
            Destroy(objeto.GetComponent<Collider>());

            ActualizarCollectedItemsUI();
        }
        else if (objetoPersonaje.id == material.id)
        {
            // Si el personaje ya tiene un objeto, se le suma la cantidad del objeto cogido
            collectedItemCount += material.cantidad;
            objetoPersonaje.cantidad++;
            Debug.Log("El personaje ya tiene " + collectedItemName + " cantidad: " + collectedItemCount);
            ActualizarCollectedItemsUI();
        }
        else
        {
            Debug.Log("El personaje tiene un objeto distinto");
        }
        Debug.Log(collectedItemName);
    }

private void ActualizarCollectedItemsUI()
{
    if (collectedItemNameText != null)
    {
        collectedItemNameText.text = "Collected item: " + collectedItemName;
    }
    if (collectedItemCountText != null)
    {
        collectedItemCountText.text = "Quantity: " + collectedItemCount;
    }
}
    private void IncrementarPuntuacion(int puntos)
    {
        puntuacion += puntos;
        ActualizarPuntuacionUI();
    }

    private void ActualizarPuntuacionUI()
    {
        if (puntuacionText != null)
        {
            puntuacionText.text = "SCORE: " + puntuacion;
        }
    }

    /**
    * Función que se encarga de detectar cuando el personaje entra en los triggers definidos
    */
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter Trigger");
        if (other.CompareTag("Workbench"))
        {
            areaCrafteo = true;
            workbench = other.GetComponent<Crafteo>();
        }
        else if (other.CompareTag("Chest"))
        {
            areaCofre = true;
            chest = other.GetComponent<Chest>();
            if (chest == null)
            {
                Debug.Log("Chest dentro de no encontrado");
            }
        }
        else if (other.CompareTag("TrashBin"))
        {
            Debug.Log("Papelera encontrada");
            areaPapelera = true;
        }
        else if (other.CompareTag("DeliverySpot"))
        {
            Debug.Log("Entrega encontrada");
            entrega = other.gameObject;
            areaEntrega = true;
        }
    }

    /**
    * Función que se encarga de detectar cuando el personaje sale de los triggers definidos
    */
    private void OnTriggerExit(Collider other)
    {
        areaCrafteo = false;
        areaCofre = false;
        areaPapelera = false;
        areaEntrega = false;
    }

    /**
    * Función que se encarga de dejar el objeto en el workbench
    * Se añade el objeto a la lista de objetos del workbench y se destruye el objeto del spawnerEspalda
    * La cantidad de objetos va dentro de objetoPersonaje (forma parte de la clase Item)
    */
    private void DejarObjeto()
    {
        if (objetoPersonaje == null)
        {
            Debug.Log("No hay objeto");
            return;
        }

        workbench.AddItemCrafteo(objetoPersonaje);

        if (spawnerEspalda.transform.childCount > 0)
        {
            Destroy(spawnerEspalda.transform.GetChild(0).gameObject);
        }
        else
        {
            Debug.LogWarning("No hay objetos en el spawnerEspalda para destruir.");
        }

        objetoPersonaje = null;
        estaCogiendo = false;
        recetaValida = workbench.comprobarRecetaPersonaje();
        Debug.Log("Receta valida: " + recetaValida);
    }

    /**
    * Corrutina para reposicionar al personaje tras un tiempo de espera
    */
    private IEnumerator RepositionAfterDelay(GameObject obj, float delay)
    {
        rb.isKinematic = true; // Desactivar la física
        yield return new WaitForSeconds(delay);

        Vector3 posicionSegura = ObtenerUltimaPosicionSegura();
        if (posicionSegura != Vector3.zero)
        {
            obj.transform.position = posicionSegura;
            obj.transform.rotation = Quaternion.identity; // Restaurar rotación
            Debug.Log($"Personaje reposicionado a {posicionSegura}");
        }
        else
        {
            Debug.LogWarning("No se encontró una posición segura para reposicionar.");
        }

        rb.isKinematic = false; // Reactivar la física
    }

    /**
    * Función que devuelve la última posición segura almacenada en la lista de posiciones recientes
    */
    private Vector3 ObtenerUltimaPosicionSegura()
    {
        for (int i = posicionesRecientes.Count - 1; i >= 0; i--)
        {
            Vector3 pos = posicionesRecientes[i];
            if (EstaSobreSuperficie(pos))
            {
                return pos; // Devuelve la primera posición válida encontrada
            }
        }
        return Vector3.zero; // No se encontró ninguna posición válida
    }

    /**
    * Corrutina para actualizar la posición del personaje cada cierto tiempo, va almacenando las posiciones seguras
    */
    private IEnumerator ActualizarPosicion()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloActualizacion);
            if (transform.position.y >= umbralCaida && EstaSobreSuperficie(transform.position))
            {
                posicionesRecientes.Add(transform.position);
                if (posicionesRecientes.Count > 10) // Limitar tamaño de la lista
                {
                    posicionesRecientes.RemoveAt(0);
                }
            }
            else
            {
                Debug.Log("Posición no válida para guardar: sobre un hueco o por debajo del umbral.");
            }
        }
    }

    /**
    * Función que devuelve si la posición dada está sobre una superficie sólida (para que solo almacenemos posiciones seguras (sobre la superficie))
    */
    private bool EstaSobreSuperficie(Vector3 posicion)
    {
        Ray ray = new Ray(posicion, Vector3.down); // Disparar ray hacia abajo
        return Physics.Raycast(ray, 10f); // Si hay colisión, está sobre una superficie sólida
    }

    /**
    * Corrutina para calcular el centro del mapa de juego
    */
    IEnumerator calcularCentro()
    {
        Generacion generacionScript = spawnerTransform.GetComponent<Generacion>();
        if (generacionScript != null)
        {
            while (generacionScript.tileSize == Vector3.zero)
            {
                yield return null;
            }

            // Calculate raw center position
            float centroX = generacionScript.rectX + generacionScript.rectWidth / 2f;
            float centroY = generacionScript.rectY + generacionScript.rectHeight / 2f;
            Debug.Log("Centro del rectángulo personaje: " + centroX + ", " + centroY);

            // Calculate grid-aligned position
            float gridX = Mathf.Round(centroX) * generacionScript.tileSize.x;
            float gridZ = Mathf.Round(centroY) * generacionScript.tileSize.z;

            // Set final position aligned to grid
            centroRectangulo = spawnerTransform.transform.position + new Vector3(gridX, 0, gridZ);
            transform.position = centroRectangulo;
        }
        else
        {
            Debug.LogError("No se ha encontrado el script de generacion en el spawner");
        }
    }
}