using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Nivel", menuName = "Configuración de Nivel")]
public class NivelConfig : ScriptableObject
{
    // Dimensiones del escenario
    // [Header("Dimensiones del escenario (100 x 100) / no tocar")]
    [HideInInspector] public int anchura = 100;
    [HideInInspector] public int profundidad = 100;

    // Coordenadas del rectángulo interior
    [Header("Coordenadas del rectángulo interior")]
    [HideInInspector] public int rectX = 4;
    [HideInInspector] public int rectY = 2;
    public int rectWidth;
    public int rectHeight;
    
    // Tiempo de juego
    [Header("Tiempo de juego (en segundos)")]
    public int tiempoDeJuego;
    
    // Número de huecos
    [Header("Número de huecos")]
    public int numeroDeHuecos;

    // Referencias a prefabs
    [Header("Prefabs para la generación")]
    public GameObject personajePrefab;
    public GameObject sueloPrefab;
    public GameObject bancoDeTrabajoPrefab;
    public GameObject papeleraPrefab;
    public GameObject cajaPrefab;
    public GameObject lugarDeEntregaPrefab;
    public GameObject encimeraPrefab;
    public GameObject paredPrefab;

    // Materiales disponibles para las cajas
    [Header("Materiales disponibles para las cajas")]
    public List<ItemDescription> materialesCaja = new List<ItemDescription>();

    // Definición de cantidades deseadas
    [Header("Definición de cantidades deseadas")]
    public int cantidadBancosDeTrabajo = 2;
    public int cantidadPapeleras = 3;
    public int cantidadPuntosEntrega = 1;

    private void OnEnable()
    {
        LoadPrefabs();
    }

    private void LoadPrefabs()
    {
        if (personajePrefab == null)
            personajePrefab = Resources.Load<GameObject>("Prefabs/Personaje");
        
        if (sueloPrefab == null)
            sueloPrefab = Resources.Load<GameObject>("Prefabs/Suelo");
        
        if (bancoDeTrabajoPrefab == null)
            bancoDeTrabajoPrefab = Resources.Load<GameObject>("Prefabs/Workbench");
        
        if (papeleraPrefab == null)
            papeleraPrefab = Resources.Load<GameObject>("Prefabs/Bin");
        
        if (cajaPrefab == null)
            cajaPrefab = Resources.Load<GameObject>("Prefabs/Chest");
        
        if (lugarDeEntregaPrefab == null)
            lugarDeEntregaPrefab = Resources.Load<GameObject>("Prefabs/DeliveryPoint");
        
        if (encimeraPrefab == null)
            encimeraPrefab = Resources.Load<GameObject>("Prefabs/Encimera");
        
        if (paredPrefab == null)
            paredPrefab = Resources.Load<GameObject>("Prefabs/Panel pared");

        if (materialesCaja.Count == 0)
        {
            ItemDescription[] items = Resources.LoadAll<ItemDescription>("Items");
            foreach (ItemDescription item in items)
            {
                materialesCaja.Add(item);
            }
        }
            
    }
}