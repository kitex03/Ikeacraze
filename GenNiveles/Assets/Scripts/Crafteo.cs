using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Crafteo : MonoBehaviour
{
    public GameObject spawnerElementos;
    public GameObject particulasCrafteo;
    
    private List<Item> objetos;
    private List<GameObject> objetosCreados;
    private GestorRecetas _gestorRecetas;
    public bool yaCrafteado = false;

    // Diccionario con los objetos y su escala
    private Dictionary<string, Vector3> objetosEscala = new Dictionary<string, Vector3>()
    {
        {"Screw", new Vector3(10f,10f, 10f)},
        {"Wood", new Vector3(0.2f, 0.2f, 0.2f)},
        {"Crystal", new Vector3(0.5f, 0.5f, 0.5f)},
        {"Metal", new Vector3(0.5f, 0.5f, 0.5f)},
        {"Pillow", new Vector3(0.5f, 0.5f, 0.5f)}
    };

    // Dividir el workbench en 9 partes
    private float gridTAM = 0.25f;
    private int gridWidth = 3;
    private List<Vector3> posicionesOcupadas = new List<Vector3>(); // Almacena las posiciones ocupadas por los objetos
    
    // ProgressBar para el progreso de crafteo 
    private GameObject canvasCrafteo;
    public GameObject progressBarPrefab;
    public float tiempoCrafteo = 5f;
    public float tiempoTranscurrido = 0f;
    public Slider sliderCrafteo = null;
    public GameObject crafteoBar = null;
    public ParticleSystem sistemaParticulas = null;
    
    // Start is called before the first frame update
    void Start()
    {
        _gestorRecetas = GameObject.Find("RecetasController").GetComponent<GestorRecetas>();
        objetos = new List<Item> { };
        objetosCreados = new List<GameObject> { };
        canvasCrafteo = GameObject.Find("CanvasCrafteo");
        
        if (canvasCrafteo != null && progressBarPrefab != null)
        {
            // Crear la barra de progreso
            crafteoBar = Instantiate(progressBarPrefab, canvasCrafteo.transform);
            sliderCrafteo = crafteoBar.GetComponent<Slider>();
            sliderCrafteo.minValue = 0;
            sliderCrafteo.maxValue = tiempoCrafteo;
            sliderCrafteo.value = 0;
   
            RectTransform sliderRect = crafteoBar.GetComponent<RectTransform>();
            
            // Obtener las dimensiones de la mesa
            Renderer mesaRenderer = GetComponent<Renderer>();
            Vector3 mesaSize = mesaRenderer.bounds.size;
            Vector3 posicionMesa = mesaRenderer.bounds.center;
        
            // Configurar tamaño y posición del Slider
            posicionMesa.y += 1f; // Ajustar altura sobre la mesa
            sliderRect.position = posicionMesa; // Establecer posición en el mundo
            sliderRect.localScale = new Vector3(0.0125f ,0.0125f ,0.0125f); // Asegurar escala consistente
               
            // Barra de progreso desactivada por defecto
            crafteoBar.SetActive(false);
        }
        
        // Instanciar el sistema de partículas sobre el workbench
        if (particulasCrafteo != null)
        {
            Vector3 posicion = GetComponent<Renderer>().bounds.center;
            posicion.y += .5f; // Ajustar la altura del sistema de partículas sobre el workbench
            particulasCrafteo = Instantiate(particulasCrafteo, posicion, Quaternion.identity);
            sistemaParticulas = particulasCrafteo.GetComponent<ParticleSystem>();
            var emission = sistemaParticulas.emission;
            emission.rateOverTime = 0; // Set emission rate to desired value
            sistemaParticulas.Play();
        }
    }
    
    /**
     * Método que se encarga de actualizar el progreso de crafteo (de momento no se esta utilizando?)
     */
    public Item GetObjeto()
    {
        Item objeto = objetos[^1];
        objetos.RemoveAt(objetos.Count - 1);
        objetosCreados.RemoveAt(objetosCreados.Count - 1);
        Destroy(spawnerElementos.transform.GetChild(0).gameObject);
        Debug.Log("Objeto obtenido: " + objeto.prefab.name);
        return objeto;
    }
    
    /**
     * Método que se encarga de añadir un item de crafteo al workbench
     */
    public void AddItemCrafteo(Item item)
    {
        // Si ya se ha crafteado, no se pueden añadir más objetos
        if (yaCrafteado)
        {
            return;
        }
        
        // Añadir el/los objetos al workbench (los objetos serán del mismo tipo)
        for (int i = 0; i < item.cantidad; i++)
        {
            objetos.Add(new Item(item.id, item.prefab));
            GameObject objeto = Instantiate(item.prefab, spawnerElementos.transform);
            Vector3 posicion = GetPosition();
            if(objeto.GetComponentInChildren<MeshRenderer>() != null && item.prefab.name == "Cristal")
            {
                posicion.y += objeto.GetComponentInChildren<MeshRenderer>().bounds.size.y/2;
            }
            else if(item.prefab.name == "Madera")
            {
                posicion.y += 0.15f;
            }
            else if(item.prefab.name == "Metal" )
            {
                posicion.y += objeto.GetComponentInChildren<MeshRenderer>().bounds.size.x/4;
            }
            else if(item.prefab.name == "tornillo")
            {
                posicion.y += 0.03f;
            }
            
            
            objeto.transform.position = posicion;
            
            string nombre = item.prefab.name;
            // Escalar el objeto
            if (objetosEscala.ContainsKey(nombre))
                objeto.transform.localScale = objetosEscala[nombre];
            else
                objeto.transform.localScale = new Vector3((float)0.05, (float)0.05, (float)0.05);
            
            objetosCreados.Add(objeto);
        }
    }
    
    /**
     * Método que se encarga de obtener la posición sobre el workbench en la que se colocará el objeto
     */
    private Vector3 GetPosition()
    {
        Vector3 posicion = spawnerElementos.transform.position;
        int objetosCreados = posicionesOcupadas.Count;

        int row = objetosCreados / gridWidth;
        int col = objetosCreados % gridWidth;

        Vector3 pos = posicion + ( col * gridTAM) * spawnerElementos.transform.right+ spawnerElementos.transform.forward * ( row * gridTAM);
        posicionesOcupadas.Add(pos);
        return pos;
    }

    
    /**
     * Método que se encarga de craftear los objetos
     */
    public void Craftear()
    {
        // Si ya se ha crafteado, no se puede volver a craftear
        if(yaCrafteado)
            return;
        
        // Comprobar si con los objetos actuales se puede craftear algo, si se puede craftear será distinto de null
        Item item = _gestorRecetas.ComprobarReceta(objetos);
        if(item != null)
        {
            DestruirObjetos();
            ColocarCrafteo(item);
        }
    }
    
    public bool comprobarRecetaPersonaje()
    {
        // Comprobar si con los objetos actuales se puede craftear algo, si se puede craftear será distinto de null
        Item item = _gestorRecetas.ComprobarReceta(objetos);
        if(item == null)
        {
            return false;
        }
        return true;
    }
    /**
    * Función que se encarga de colocar el objeto crafteado sobre el workbench
    */
    private void ColocarCrafteo(Item item)
    {
        Vector3 posicion = GetComponent<Renderer>().bounds.center;

        posicion.y += GetComponent<Renderer>().bounds.size.y / 2;
        GameObject objeto = Instantiate(item.prefab, spawnerElementos.transform);
        objeto.transform.position = posicion;
        objeto.transform.localScale = item.prefab.transform.localScale;
        objetosCreados.Add(objeto);
        objetos.Add(item);
        if(objetos.Count > 0)
        {
            Debug.Log("Crafteado: " + objetos[0].prefab.name);
        }
        
        yaCrafteado = true;
    }
    
    ///
    // MÉTODOS PARA LA DESTRUCCIÓN DE OBJETOS
    ///

    /**
    * Método que se encarga de destruir los objetos que se encuentran en el workbench
    */
    public void DestruirObjetos()
    {
        int numObjetos = objetos.Count;
        for (int i = 0; i < numObjetos; i++)
        {
            Destroy(spawnerElementos.transform.GetChild(i).gameObject);
            DestruirObjeto(objetos[0]);
        }
        
        posicionesOcupadas.Clear();
    }
    
    /**
    * Método que se encarga de destruir un objeto en concreto
    */
    private void DestruirObjeto(Item item)
    {
        
        for (int i = 0; i < objetos.Count; i++)
        {
            if(objetos[i].id == item.id)
            {
                Destroy(objetosCreados[i]);
                objetosCreados.RemoveAt(i);
                objetos.RemoveAt(i);
                break;
            }
        }
    }
}