using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GestorRecetas : MonoBehaviour
{

    // Diccionario de recetas (ID -> materiales)
    private Dictionary<string, Receta> Recetasnivel = new Dictionary<string, Receta>();

    // Recetas activas en pantalla
    private List<Receta> recetasActivas = new List<Receta>();
    private List<GameObject> recetaVisualLista = new List<GameObject>();
    public GameObject recetaVisual;
    public Transform canvasTransform;
    public int maxRecetasMostradas = 3; // Máximo de recetas que se pueden mostrar al mismo tiempo
    public float intervaloCargaRecetas = 20f; // Intervalo de tiempo para cargar nuevas recetas
    private Vector2 initialPosition = new Vector2(-420, 220);
    private float offsetX = 190f;
    public int difficultad = 1;

    // Variables del nivel
    public int nivelActual;

    void Start()
    {
        // Cargar las recetas desde la carpeta "Recursos"
        CargarRecetas();

        // Iniciar la carga periódica de recetas
        InvokeRepeating("MostrarReceta", 1f, intervaloCargaRecetas);
    }

    void CargarRecetas()
    {
        Debug.Log("Cargando recetas de dificultad: " + difficultad);
        for (int i = 1; i <= 100; i++)
        {
            Receta receta = Resources.Load<Receta>($"Recetas/{i:D3}");
            if (receta != null)
            {
                string id = CalcularID(receta.materiales);
                receta.id = id;
                receta.resultado.id = int.Parse(id);
                
                
                //Debug.Log("Receta cargada: " + id);
                if (receta.GetDifficultad() == difficultad)
                {
                    //Debug.Log("Receta activa: " + id);
                    Recetasnivel.Add(id, receta);
                }
            }
        }
    }

    
    void MostrarReceta()
    {
        if (recetasActivas.Count < maxRecetasMostradas)
        {
            // Seleccionar una receta y agregarla a las activas
            Receta nuevaRecetaBase = Recetasnivel.Values.ToList()[Random.Range(0, Recetasnivel.Count)];
            Receta nuevaReceta = Instantiate(nuevaRecetaBase);
            nuevaReceta.timeCreated = (int)Time.time;
            recetasActivas.Add(nuevaReceta);
            
            GameObject prefabVisual = Resources.Load<GameObject>($"Visual/{nuevaReceta.id}");
            if (prefabVisual == null)
            {
                Debug.LogError($"Prefab visual no encontrado para la receta con ID: {nuevaReceta.id}");
                return;
            }

            GameObject newReceta = Instantiate(prefabVisual, canvasTransform);
            recetaVisualLista.Add(newReceta);
            
            for (int i = 0; i < recetasActivas.Count - 1; i++)
            {
                RectTransform rectTransform = recetaVisualLista[i].GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x + offsetX, rectTransform.anchoredPosition.y);
            }

            RectTransform newRectTransform  = newReceta.GetComponent<RectTransform>();
            newRectTransform .anchoredPosition = initialPosition;
        }
    }
    
    private string CalcularID(List<ItemDescription> materiales)
    {
        List<int> ids = new List<int>();
        string id = "";
        foreach (var i in materiales)
        {
            ids.Add(i.id);
        }
        
        ids.Sort();

        foreach (var t in ids)
        {
            id += t.ToString("D2");
        }
        
        return id;
    }

    private string CalcularID(List<Item> materialesEnMesa)
    {
        List<int> ids = new List<int>();
        string id = "";
        foreach (var i in materialesEnMesa)
        {
            ids.Add(i.id);
        }

        ids.Sort();

        foreach (var t in ids)
        {
            id += t.ToString("D2");
        }

        return id;
    }

    public Item ComprobarReceta(List<Item> materialesEnMesa)
    {
        // Crear el ID de los materiales ordenados de mayor a menor
        string idComprobado = CalcularID(materialesEnMesa);
        
        Item itemCreado = null;
        Receta recetaComprobada = null;

        if (Recetasnivel.TryGetValue(idComprobado, out recetaComprobada) )
        {
            itemCreado = recetaComprobada.resultado;
        }

        return itemCreado;
    }

    public int Entregar(Item item)
    {
        int puntos = 0;
        
        
        if(recetasActivas.Count > 0)
        {
            string id = "";
            if(recetasActivas[0].GetDifficultad() == 1)
            {
                id = item.id.ToString("D6");
            } else if (recetasActivas[0].GetDifficultad() == 2)
            {
               id = item.id.ToString("D8");
            } else if (recetasActivas[0].GetDifficultad() == 3)
            {
                id = item.id.ToString("D10");
            }
            Receta receta = recetasActivas.Find(r => r.id == id);

            if (receta != null)
            {
                
                if (((int)Time.time - receta.timeCreated) > 0)
                {
                    puntos = ( 10000 / ((int)Time.time - receta.timeCreated));
                } else
                {
                    puntos = ( 10000 / 1);
                }
                
                if(puntos < 0)
                {
                    puntos = 0;
                }

                recetasActivas.Remove(receta);
                
                GameObject recetaEliminar = recetaVisualLista.Find(r => r.name.Contains(id));
                int index = recetaVisualLista.FindIndex(r => r.name.Contains(id));
                
                recetaVisualLista.Remove(recetaEliminar);
                
                Destroy(recetaEliminar);
                
                for (int i = 0; i < index; i++)
                {
                    RectTransform rectTransform = recetaVisualLista[i].GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x - offsetX, rectTransform.anchoredPosition.y);
                }
            }
        }

        return puntos;
    }
}