using System.Collections.Generic;
using TMPro.Examples;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Generacion : MonoBehaviour
{
    // Dimensiones del escenario
    [HideInInspector] public int anchura = 50;
    [HideInInspector] public int profundidad = 50;
    
    [Header("Spawner y personaje")]
    public GameObject spawner; // Objeto dónde se aplica el script, desde el cual se genera el escenario
    public GameObject personaje; 
    
    // Coordenadas del rectángulo interior
    [HideInInspector] public int rectX = 4;
    [HideInInspector] public int rectY = 2;
    
    [Header("Configuración del tamaño del area de juego")]
    public int rectWidth = 6;
    public int rectHeight = 6;
    public int numeroDeHuecos = 7;
    
    // Centro del rectángulo
    private Vector3 _centro;
    
    // Prefabs para la generación de suelo y otros elementos
    [Header("Prefabs")]
    public GameObject sueloPrefab;
    public GameObject bancoDeTrabajoPrefab;
    public GameObject papeleraPrefab;
    public GameObject cajaPrefab;
    public GameObject lugarDeEntregaPrefab;
    public GameObject encimeraPrefab;
    public GameObject paredPrefab;
    public GameObject deliveryTruckPrefab;
    
    [Header("Prefabs de exterior")]
    public GameObject exteriorDerechaPrefab;
    public GameObject exteriorIzquierdaPrefab;
    public GameObject exteriorArribaPrefab;
    public GameObject exteriorAbajoPrefab;
    
    private GameObject _paredesParent;
    private GameObject _exteriorParent;
    private GameObject _sueloParent;
    private GameObject _elementosParent;
    
    // Contadores para los elementos
    private int _bancosCount = 0;
    private int _papelerasCount = 0;
    private int _cajasCount = 0;
    private int _puntosDeEntregaCount = 0;
    
    // Huecos en el suelo
    private int[,] _huecos; // matriz bidimensional, 0 = suelo, 1 = hueco, 2 = posición especial
    
    // Tamaño del tile
    [HideInInspector] public Vector3 tileSize;
    private Vector3 _personajeSize;
    
    // Listado de Items para los materiales en las cajas
    [SerializeField] private List<ItemDescription> itemDescriptions = new List<ItemDescription>();
    private List<ItemDescription> _items = new List<ItemDescription>();
    
    // Almacenar los caminos para el pathfinding (nivel resoluble)
    private List<List<Vector2Int>> _caminos = new List<List<Vector2Int>>();
    
    // Configuración del nivel que pasamos desde SceneController
    private NivelConfig _config;
    
    // Definición de cantidades deseadas
    [Header("Cantidades de elementos")]
    public int cantidadBancosDeTrabajo = 2;
    public int cantidadPapeleras = 2;
    [HideInInspector] public int cantidadCajas = 0;
    public int cantidadPuntosEntrega = 1;
    
    [Header("Timer")]
    public GameObject Timer;
    
    
    // Comprobación de si los datos han sido inicializados
    private bool _isInitialized = false;
    
    // Método para inicializar los datos
    public void Initialize(NivelConfig nivelConfig)
    {
        if (_isInitialized)
        {
            Debug.LogWarning("Los datos ya han sido inicializados.");
            return;
        }
        
        this._config = nivelConfig;
    
        // Configurar las variables según el nivel
        anchura = _config.anchura;
        profundidad = _config.profundidad;
        rectX = _config.rectX;
        rectY = _config.rectY;
        rectWidth = _config.rectWidth;
        rectHeight = _config.rectHeight;
        
        // Configurar el número de huecos
        numeroDeHuecos = _config.numeroDeHuecos;
        
        // Configurar el tiempo de juego
        Timer.GetComponent<TimeController>().startTime = _config.tiempoDeJuego;
    
        // Configurar los prefabs
        sueloPrefab = _config.sueloPrefab;
        bancoDeTrabajoPrefab = _config.bancoDeTrabajoPrefab;
        papeleraPrefab = _config.papeleraPrefab;
        cajaPrefab = _config.cajaPrefab;
        lugarDeEntregaPrefab = _config.lugarDeEntregaPrefab;
        encimeraPrefab = _config.encimeraPrefab;
        paredPrefab = _config.paredPrefab;
        
        // Items para las cajas (materiales)
        itemDescriptions = _config.materialesCaja;
    
        // Configurar cantidades
        cantidadBancosDeTrabajo = _config.cantidadBancosDeTrabajo;
        cantidadPapeleras = _config.cantidadPapeleras;
        cantidadCajas = itemDescriptions.Count;
        cantidadPuntosEntrega = _config.cantidadPuntosEntrega;
    
        Debug.Log($"Configuración de nivel aplicada: {_config.name}");
        _isInitialized = true;
    }
        
    void Start()
    {   
        // Definición de los padres de los objetos, mantener una estructura más organizada
        _paredesParent = new GameObject("Paredes");
        _paredesParent.transform.parent = spawner.transform;
        
        _sueloParent = new GameObject("Suelo");
        _sueloParent.transform.parent = spawner.transform;
        
        _elementosParent = new GameObject("Elementos");
        _elementosParent.transform.parent = spawner.transform;
        
        _exteriorParent = new GameObject("Exterior");
        _exteriorParent.transform.parent = spawner.transform;

        // Limpiar la lista de items y añadir los items del nivel
        _items.Clear();
        foreach (var desc in itemDescriptions)
        {
            _items.Add(new ItemDescription(desc.id, desc.prefab));
        }
        cantidadCajas = _items.Count;
        
        // Define el tamaño del tile basado en el prefab del suelo
        tileSize = sueloPrefab.GetComponent<Renderer>().bounds.size;
        
        // Se establece la matriz en la que podrán aparecer huecos
        _huecos = new int[anchura, profundidad];
        
        itemDescriptions = new List<ItemDescription>(_items);

        // Lista de posiciones alrededor del rectángulo
        List<Vector2Int> posiciones = ObtenerPosicionesAleatorias();

        // Generar los elementos
        GenerarObjetosEnPosiciones(posiciones);
        
        // Inicializar la posición del jugador
        CalcularCentro();

        // Generar el suelo aleatorio
        GenerarSueloAleatorio();
        
        // Generar las paredes
        GenerarParedes();
        
        // Generar el exterior
        GenerarExterior();
    }
    
    void Update()
    {
        // Verificar si se ha presionado la tecla R
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    // Reiniciar la escena actual
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //}
    }
    
    void GenerarExterior()
    {
        int repeticiones = 4;
        Renderer renderer = exteriorDerechaPrefab.transform.Find("Plane").GetComponent<Renderer>();
        
        float prefabSizeZ = renderer.bounds.size.z;
        float prefabSizeX = renderer.bounds.size.x;
        Debug.Log($"Tamaño del prefab: {prefabSizeZ}");
        
        // Initial position at the spawner point
        Vector3 posicionInicial = spawner.transform.position;

        // Generate exterior on the right side of the rectangle
        for (int i = -1; i < repeticiones; i++)
        {
            for (int j = -1; j < 1; j++)
            {
                // Calculate position on the right side aligned with rectX
                Vector3 posicion = new Vector3(prefabSizeX*j, -0.01f, prefabSizeZ*i);
                
                // Instantiate the prefab at the calculated position with a 90-degree rotation around the y-axis
                GameObject exterior = Instantiate(exteriorDerechaPrefab, posicion, Quaternion.Euler(0, 0, 0));

                // Ensure the prefab is in the correct hierarchy and active
                exterior.transform.parent = _exteriorParent.transform;
                exterior.SetActive(true);
            }
        }
        
        for (int i = -1; i < repeticiones; i++)
        {
            for (int j = 1; j < 3; j++)
            {
                // Calculate position on the right side aligned with rectX
                Vector3 posicion = new Vector3(rectWidth*tileSize.z + prefabSizeX*j, -0.01f, prefabSizeZ*i);
                
                // Instantiate the prefab at the calculated position with a 90-degree rotation around the y-axis
                GameObject exterior = Instantiate(exteriorIzquierdaPrefab, posicion, Quaternion.Euler(0, 0, 0));

                // Ensure the prefab is in the correct hierarchy and active
                exterior.transform.parent = _exteriorParent.transform;
                exterior.SetActive(true);
            }
        }
        
        for (int i = 0; i < repeticiones; i++)
        {
            for (int j = 1; j < repeticiones; j++)
            {
                Vector3 posicion = new Vector3(prefabSizeX * i, -0.01f, rectHeight * tileSize.z + 4 * prefabSizeZ / 4 * j);
        
                // Instantiate the prefab at the calculated position with a 90-degree rotation around the y-axis
                GameObject exterior = Instantiate(exteriorArribaPrefab, posicion, Quaternion.Euler(0, 0, 0));

                // Ensure the prefab is in the correct hierarchy and active
                exterior.transform.parent = _exteriorParent.transform;
                exterior.SetActive(true);
            }
        }
        
        for (int i = 0; i < repeticiones; i++)
        {
            Vector3 posicion = new Vector3();
            // Calculate position on the right side aligned with rectX
            posicion = new Vector3(prefabSizeX * i, -0.01f, prefabSizeZ/4);
                
            // Instantiate the prefab at the calculated position with a 90-degree rotation around the y-axis
            GameObject exterior = Instantiate(exteriorAbajoPrefab, posicion, Quaternion.Euler(0, 0, 0));

            // Ensure the prefab is in the correct hierarchy and active
            exterior.transform.parent = _exteriorParent.transform;
            exterior.SetActive(true);
        }
    }
    
    void GenerarParedes()
    {
       // Generar paredes en el contorno del rectángulo
       for (int i = rectX; i < rectX + rectWidth; i++)
       {
           GeneraPanelPared(i, rectY + rectHeight - 1, Quaternion.Euler(0, 180, 0)); // Lado superior
       }
       for (int j = rectY; j < rectY + rectHeight; j++)
       {
           GeneraPanelPared(rectX, j, Quaternion.Euler(0, 90, 0)); // Lado izquierdo
           GeneraPanelPared(rectX + rectWidth - 1, j, Quaternion.Euler(0, -90, 0)); // Lado derecho
       }
    }
    
    void GeneraPanelPared(int i, int j, Quaternion rotation)
    {
        Vector3 posicionObjeto;
        
        // Calcular posición del objeto en el borde de la celda
        if (Mathf.Approximately(rotation.eulerAngles.y, 90))
        {
            posicionObjeto = spawner.transform.position + new Vector3(i * tileSize.x, paredPrefab.transform.position.y/2, j * tileSize.z) + new Vector3(-tileSize.x / 2, 0, 0);
        }
        else if (Mathf.Approximately(rotation.eulerAngles.y, 270)) // 270 degrees is equivalent to -90 degrees
        {
            posicionObjeto = spawner.transform.position + new Vector3(i * tileSize.x, paredPrefab.transform.position.y/2, j * tileSize.z) + new Vector3(tileSize.x / 2, 0, 0);
        }
        else
        {
            posicionObjeto = spawner.transform.position + new Vector3(i * tileSize.x, paredPrefab.transform.position.y/2, j * tileSize.z) + new Vector3(0, 0, tileSize.z / 2);
        }
    
        // Instanciar un panel de pared en la posición
        GameObject wall = Instantiate(paredPrefab, posicionObjeto, rotation);
        wall.transform.parent = _paredesParent.transform;
        wall.SetActive(true);
    }
    
    /**
     * Genera un suelo aleatorio con huecos en posiciones aleatorias
     */
    void GenerarSueloAleatorio()
{
    bool resoluble = false;
    
    // Convertir la posición del jugador a coordenadas de la cuadrícula
    int playerGridX = Mathf.RoundToInt(_centro.x) + 1;
    int playerGridZ = Mathf.RoundToInt(_centro.z) + 1;
    
    Debug.Log($"Posición del jugador en grid: ({playerGridX}, {playerGridZ})");

    while (!resoluble)
    {
        // Guardar posiciones especiales
        List<Vector2Int> posicionesEspeciales = new List<Vector2Int>();
        List<Vector2Int> posicionesEncimera = new List<Vector2Int>();
        for (int i = 0; i < anchura; i++)
        {
            for (int j = 0; j < profundidad; j++)
            {
                if (_huecos[i, j] == 2)
                {
                    posicionesEspeciales.Add(new Vector2Int(i, j));
                }
                if (_huecos[i, j] == 3)
                {
                    posicionesEncimera.Add(new Vector2Int(i, j));
                }
            }
        }

        // Reiniciar el suelo
        for (int i = 0; i < anchura; i++)
        {
            for (int j = 0; j < profundidad; j++)
            {
                _huecos[i, j] = 0;
            }
        }

        // Marcar la posición del jugador con un valor especial (4)
        _huecos[playerGridX, playerGridZ] = 4;

        // Restaurar posiciones especiales
        foreach (Vector2Int pos in posicionesEspeciales)
        {
            _huecos[pos.x, pos.y] = 2;
        }
        foreach (Vector2Int pos in posicionesEncimera)
        {
            _huecos[pos.x, pos.y] = 3;
        }

        // Lista para almacenar posiciones válidas para huecos
        List<Vector2Int> posicionesValidas = new List<Vector2Int>();
        
        // Recolectar posiciones válidas para huecos, excluyendo explícitamente la posición del jugador
        for (int i = rectX + 1; i < rectX + rectWidth - 1; i++)
        {
            for (int j = rectY + 1; j < rectY + rectHeight - 1; j++)
            {
                if (_huecos[i, j] == 0 && 
                    _huecos[i, j] != 4 && // Asegurarse de que no es la posición del jugador
                    Vector2.Distance(new Vector2(i, j), new Vector2(playerGridX, playerGridZ)) > 1) // Excluir posiciones adyacentes
                {
                    posicionesValidas.Add(new Vector2Int(i, j));
                }
            }
        }

        // Barajar las posiciones válidas
        for (int i = posicionesValidas.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2Int temp = posicionesValidas[i];
            posicionesValidas[i] = posicionesValidas[j];
            posicionesValidas[j] = temp;
        }

        // Crear huecos, asegurándonos de que no estén en la posición del jugador
        int huecosCreados = 0;
        foreach (Vector2Int pos in posicionesValidas)
        {
            if (huecosCreados >= numeroDeHuecos) break;
            
            // Solo crear hueco si no es la posición del jugador
            if (_huecos[pos.x, pos.y] != 4)
            {
                _huecos[pos.x, pos.y] = 1;
                huecosCreados++;
            }
        }

        // Generar el suelo
        for (int i = rectX; i < rectX + rectWidth; i++)
        {
            for (int j = rectY; j < rectY + rectHeight; j++)
            {
                // Generar suelo si no es un hueco (1) o si es la posición del jugador (4)
                if (_huecos[i, j] != 1 || _huecos[i, j] == 4)
                {
                    GameObject tile = Instantiate(sueloPrefab, 
                        spawner.transform.position + new Vector3(i * tileSize.x, 0, j * tileSize.z), 
                        Quaternion.identity);
                    tile.AddComponent<BoxCollider>();
                    tile.transform.parent = _sueloParent.transform;
                    tile.SetActive(true);
                }
            }
        }

        resoluble = SePuedeResolver();
        
        if (!resoluble)
        {
            Debug.Log("Nivel no resoluble, regenerando...");
            foreach (Transform child in _sueloParent.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
    
    /**
     * Verifica si el nivel es resoluble
     * @return true si el nivel es resoluble, false en caso contrario
     */
    bool SePuedeResolver()
    {
        // Lista para almacenar las posiciones marcadas como 2
        List<Vector2Int> puntosClave = new List<Vector2Int>();
    
        // Buscar todas las posiciones con huecos[i, j] = 2
        for (int i = 0; i < anchura; i++)
        {
            for (int j = 0; j < profundidad; j++)
            {
                if (_huecos[i, j] == 2)
                {
                    Debug.Log("Se almacena en la funcion sePuedeResolver");
                    puntosClave.Add(new Vector2Int(i, j));
                }
            }
        }
        
        // Si no hay puntos clave o solo hay uno, el puzzle es trivialmente resolvible
        if (puntosClave.Count <= 1)
            return true;
    
        // Verificar que todos los puntos están conectados
        for (int i = 0; i < puntosClave.Count; i++)
        {
            for (int j = i + 1; j < puntosClave.Count; j++)
            {
                if (!AStarPathfinding(puntosClave[i], puntosClave[j]))
                    return false; // Si algún par no está conectado, el puzzle no es resolvible
            }
        }
        return true; // Todos los puntos están conectados
    }
    
    /**
     * Obtiene una lista de posiciones aleatorias en el contorno del rectángulo
     * Las obtiene ordenadas y despues las baraja
     * @return Lista de posiciones en el contorno del rectángulo
     */
    List<Vector2Int> ObtenerPosicionesAleatorias()
    {
        List<Vector2Int> posiciones = new List<Vector2Int>();

        // Recorrer el borde del rectángulo
        for (int i = rectX; i < rectX + rectWidth; i++)
        {
            posiciones.Add(new Vector2Int(i, rectY)); // Lado inferior
            posiciones.Add(new Vector2Int(i, rectY + rectHeight - 1)); // Lado superior
        }
        for (int j = rectY + 1; j < rectY + rectHeight - 1; j++)
        {
            posiciones.Add(new Vector2Int(rectX, j)); // Lado izquierdo
            posiciones.Add(new Vector2Int(rectX + rectWidth - 1, j)); // Lado derecho
        }

        // Barajar las posiciones
        for (int k = 0; k < posiciones.Count; k++)
        {
            Vector2Int temp = posiciones[k];
            int randomIndex = Random.Range(0, posiciones.Count);
            posiciones[k] = posiciones[randomIndex];
            posiciones[randomIndex] = temp;
        }

        return posiciones;
    }
    
    /**
     * Genera los objetos en el contorno del rectángulo
     * @param posiciones Lista de posiciones en el contorno del rectángulo
     */
    void GenerarObjetosEnPosiciones(List<Vector2Int> posiciones)
    {
        foreach (Vector2Int posicion in posiciones)
        {
            // Generar un elemento en cada posición
            GenerateElement(posicion.x, posicion.y);
        }
    }
    
    /**
     * Genera un elemento en la posición (i, j) del suelo
     * @param i Coordenada x
     * @param j Coordenada z
     */
    void GenerateElement(int i, int j)
    {
        GameObject element = null;
    
        // Verificar si el índice es válido y el hueco está disponible
        if (i < 0 || i >= _huecos.GetLength(0) || j < 0 || j >= _huecos.GetLength(1))
        {
            Debug.LogWarning($"Índices fuera de rango: i={i}, j={j}");
            return;
        }
    
        if (_huecos[i, j] != 0)
        {
            Debug.LogWarning($"Hueco ya ocupado en la posición: i={i}, j={j}");
            return;
        }
    
        // Calcular posición del objeto
        Vector3 posicionObjeto = spawner.transform.position + new Vector3(i * tileSize.x, 0, j * tileSize.z);
    
        // Verificar si es esquina
        bool isCorner = (i == rectX && j == rectY) ||
                        (i == rectX && j == rectY + rectHeight - 1) ||
                        (i == rectX + rectWidth - 1 && j == rectY) ||
                        (i == rectX + rectWidth - 1 && j == rectY + rectHeight - 1);
    
        // Determinar qué tipo de elemento generar
        if (isCorner)
        {
            // Generar encimera en las esquinas
            element = Instantiate(encimeraPrefab, posicionObjeto, Quaternion.identity);
        }
        else if (TieneObjetosEspecialesAdyacentes(i, j))
        {
            // Generar encimera si hay objetos especiales adyacentes
            element = Instantiate(encimeraPrefab, posicionObjeto, Quaternion.identity);
        }
        else if (_bancosCount < cantidadBancosDeTrabajo || 
                 _papelerasCount < cantidadPapeleras || 
                 _cajasCount < cantidadCajas || 
                 _puntosDeEntregaCount < cantidadPuntosEntrega)
        {
            // Lista de prefabs disponibles
            List<GameObject> prefabsDisponibles = new List<GameObject>();
    
            if (_bancosCount < cantidadBancosDeTrabajo)
                prefabsDisponibles.Add(bancoDeTrabajoPrefab);
            if (_papelerasCount < cantidadPapeleras)
                prefabsDisponibles.Add(papeleraPrefab);
            if (_cajasCount < cantidadCajas)
                prefabsDisponibles.Add(cajaPrefab);
            if (_puntosDeEntregaCount < cantidadPuntosEntrega && j == rectY)
                prefabsDisponibles.Add(lugarDeEntregaPrefab);
    
            if (prefabsDisponibles.Count > 0)
            {
                // Seleccionar y generar un prefab aleatorio
                int randomIndex = Random.Range(0, prefabsDisponibles.Count);
                element = Instantiate(prefabsDisponibles[randomIndex], posicionObjeto, Quaternion.identity);
    
                // Actualizar contadores
                if (element.CompareTag("Workbench"))
                    _bancosCount++;
                else if (element.CompareTag("TrashBin"))
                    _papelerasCount++;
                else if (element.CompareTag("DeliverySpot"))
                {
                    _puntosDeEntregaCount++;
                    CreateDeliveryTruck(element);
                }
                else if (element.CompareTag("Chest"))
                {
                    _cajasCount++;
                    AsignarMaterialACaja(element);
                }
            }
        }
        
        // Si no se ha generado ningún elemento hasta ahora, generar una encimera
        if (element == null)
        {
            element = Instantiate(encimeraPrefab, posicionObjeto, Quaternion.identity);
        }
    
        // Configuración final del elemento generado
        if (element != null)
        {
            // Asignar tipo en la matriz de huecos
            if (element.name.StartsWith("Bin") || 
                element.name.StartsWith("Chest") || 
                element.name.StartsWith("Workbench") || 
                element.name.StartsWith("Delivery"))
            {
                _huecos[i, j] = 2;
            }
            else if (element.name.StartsWith("Encimera"))
            {
                _huecos[i, j] = 3;
            }
            else
            {
                _huecos[i, j] = 1;
            }
    
            // Aplicar rotación y configurar transform
            AplicarRotacion(element, i, j);
            element.transform.parent = _elementosParent.transform;
            element.SetActive(true);
        }
    }
    
    /**
     * Verifica si hay objetos especiales adyacentes a la posición (i, j)
     * @param i Coordenada x
     * @param j Coordenada z
     * @return true si hay objetos especiales adyacentes, false en caso contrario
     */
    bool TieneObjetosEspecialesAdyacentes(int i, int j)
    {
        // Definir las direcciones para verificar (8 direcciones: cardinales y diagonales)
        int[,] direcciones = new int[,]
        {
            { 0, 1 },  // Norte
            { 1, 0 },  // Este
            { 0, -1 }, // Sur
            { -1, 0 }, // Oeste
            { 1, 1 },  // Noreste
            { 1, -1 }, // Sureste
            { -1, 1 }, // Noroeste
            { -1, -1 } // Suroeste
        };
    
        for (int k = 0; k < direcciones.GetLength(0); k++)
        {
            int ni = i + direcciones[k, 0];
            int nj = j + direcciones[k, 1];
    
            // Verificar que la posición está dentro de los límites
            if (ni >= 0 && ni < _huecos.GetLength(0) && nj >= 0 && nj < _huecos.GetLength(1))
            {
                // Verificar si hay un objeto especial (valor 2) o si es una posición que está siendo verificada
                // para un objeto especial
                if (_huecos[ni, nj] == 2 || 
                    (ni == i && nj == j && 
                     (_bancosCount < cantidadBancosDeTrabajo || 
                      _papelerasCount < cantidadPapeleras ||
                      _cajasCount < cantidadCajas ||
                      _puntosDeEntregaCount < cantidadPuntosEntrega)))
                {
                    return true;
                }
            }
        }
    
        return false;
    }
    
    /**
     * Asigna un material aleatorio a la caja
     * @param caja GameObject de la caja a la que se le asignará el material
     */
    void AsignarMaterialACaja(GameObject caja)
    {
        //Debug.Log("AsignarMaterialACaja");
        // Validar que la lista de materiales no esté vacía
        if (itemDescriptions.Count > 0)
        {
            int randomIndex = Random.Range(0, itemDescriptions.Count);
            ItemDescription materialAsignado = itemDescriptions[randomIndex];
            Debug.Log($"Asignando material a la caja: {materialAsignado.prefab.name}");
            
            // Obtener el script del cofre y asignar el material
            Chest chestScript = caja.GetComponent<Chest>();
            if (chestScript == null)
            {
                chestScript = caja.AddComponent<Chest>();
            }

            chestScript.AsignarMaterial(materialAsignado);
            itemDescriptions.RemoveAt(randomIndex); // Remover el material usado
        }
        else
        {
            Debug.LogWarning("No hay materiales disponibles para asignar.");
        }
    }
    
    /**
     * Aplica la rotación al objeto basada en la posición en el rectángulo, todos los objetos miran hacia el centro
     * @param objeto GameObject al que se le aplicará la rotación
     * @param i Coordenada x
     * @param j Coordenada z
     */
    void AplicarRotacion(GameObject objeto, int i, int j)
    {
        if(objeto.name.StartsWith("Delivery")){
            // El prefab del punto de entrega esta orientado al revés
            if (j == rectY) // Lado inferior
            {
                objeto.transform.rotation = Quaternion.Euler(0, 180, 0); // Mirando hacia arriba
                objeto.transform.position += new Vector3(0, 0, -0.15f);
            }
            else if (j == rectY + rectHeight - 1) // Lado superior
            {
                objeto.transform.rotation = Quaternion.Euler(0, 0, 0); // Mirando hacia abajo
                objeto.transform.position += new Vector3(0, 0, -0.15f);
            }
            else if (i == rectX) // Lado izquierdo
            {
                objeto.transform.rotation = Quaternion.Euler(0, -90, 0); // Mirando hacia la derecha
                objeto.transform.position += new Vector3(0.15f, 0, -0.15f);
            }
            else if (i == rectX + rectWidth - 1) // Lado derecho
            {
                objeto.transform.rotation = Quaternion.Euler(0, 90, 0); // Mirando hacia la izquierda
                objeto.transform.position += new Vector3(-0.15f, 0, -0.15f);
            }
        }
        else{
            // Verificar en qué lado del rectángulo está el objeto
            if (j == rectY) // Lado inferior
            {
                objeto.transform.rotation = Quaternion.Euler(0, 0, 0); // Mirando hacia arriba
                objeto.transform.position += new Vector3(0, 0, -0.15f);
            }
            else if (j == rectY + rectHeight - 1) // Lado superior
            {
                objeto.transform.rotation = Quaternion.Euler(0, 180, 0); // Mirando hacia abajo
                objeto.transform.position += new Vector3(0, 0, -0.15f);
            }
            else if (i == rectX) // Lado izquierdo
            {
                objeto.transform.rotation = Quaternion.Euler(0, 90, 0); // Mirando hacia la derecha
                objeto.transform.position += new Vector3(0.15f, 0, -0.15f);
            }
            else if (i == rectX + rectWidth - 1) // Lado derecho
            {
                objeto.transform.rotation = Quaternion.Euler(0, -90, 0); // Mirando hacia la izquierda
                objeto.transform.position += new Vector3(-0.15f, 0, -0.15f);
            }
        }
    }
    
    /**
     * Implementación del algoritmo A* para encontrar el camino más corto entre dos puntos
     * @param start Posición de inicio
     * @param end Posición de destino
     * @return true si se puede encontrar un camino, false en caso contrario
     */
    
    bool AStarPathfinding(Vector2Int start, Vector2Int end)
    {
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        PriorityQueue<Vector2Int> openSet = new PriorityQueue<Vector2Int>();
        openSet.Enqueue(start, 0);

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>
        {
            [start] = 0
        };

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet.Dequeue();

            if (current == end)
            {
                ReconstruirCamino(cameFrom, current);
                return true;
            }

            closedSet.Add(current);

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor) || _huecos[neighbor.x, neighbor.y] == 1 || _huecos[neighbor.x, neighbor.y] == 3)
                {
                    continue;
                }

                int tentativeGScore = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    int fScore = tentativeGScore + Heuristic(neighbor, end);
                    openSet.Enqueue(neighbor, fScore);
                }
            }
        }

        return false;
    }

    void ReconstruirCamino(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        _caminos.Add(path);
    }
    
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     foreach (var path in _caminos)
    //     {
    //         for (int i = 0; i < path.Count - 1; i++)
    //         {
    //             Vector3 start = spawner.transform.position + new Vector3(path[i].x * tileSize.x, 0, path[i].y * tileSize.z);
    //             Vector3 end = spawner.transform.position + new Vector3(path[i + 1].x * tileSize.x, 0, path[i + 1].y * tileSize.z);
    //             Gizmos.DrawLine(start, end);
    //         }
    //     }
    // }
    
    List<Vector2Int> GetNeighbors(Vector2Int node)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
    
        if (node.x > 0 && node.x < anchura - 1 && node.y > 0 && node.y < profundidad - 1)
        {
            if (node.x > rectX) neighbors.Add(new Vector2Int(node.x - 1, node.y));
            if (node.x < rectX + rectWidth - 1) neighbors.Add(new Vector2Int(node.x + 1, node.y));
            if (node.y > rectY) neighbors.Add(new Vector2Int(node.x, node.y - 1));
            if (node.y < rectY + rectHeight - 1) neighbors.Add(new Vector2Int(node.x, node.y + 1));
        }
    
        return neighbors;
    }
    
    int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
    
    /**
     * NOTA: No se está utilizando, pero se deja por si se requiere en el futuro
     * Ajusta la escala de un objeto para que se ajuste al tamaño del personaje
     * @param obj GameObject al que se le ajustará la escala
     */
    void AdjustScale(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("El objeto no tiene un Renderer, no se puede ajustar la escala.");
            return;
        }

        Vector3 prefabSize = renderer.bounds.size;
        if (prefabSize.x <= 0 || prefabSize.y <= 0 || prefabSize.z <= 0)
        {
            Debug.LogError("El tamaño del prefab tiene una dimensión no válida. Usando escala por defecto.");
            obj.transform.localScale = Vector3.one;
            return;
        }

        Vector3 scale = new Vector3(_personajeSize.x / prefabSize.x, _personajeSize.y * 3, _personajeSize.z / prefabSize.z);
        obj.transform.localScale = scale;
    }
    
    public void CreateDeliveryTruck(GameObject deliverySpot)
    {
        Vector3 position = spawner.transform.position +(-3 * Vector3.forward);
        GameObject truck = Instantiate(deliveryTruckPrefab, position, deliveryTruckPrefab.transform.rotation);
        truck.transform.parent = _elementosParent.transform;
        truck.GetComponent<AnimaciónCamino>().destino = deliverySpot.transform;
        truck.SetActive(true);
    }

    public void CalcularCentro()
    {
        // Calculate raw center position
        _centro.x = rectX + rectWidth / 2;
        _centro.z = rectY + rectHeight / 2;
        Debug.Log("Centro del rectángulo personaje: " + _centro.x + ", " + _centro.z);

        // Calculate grid-aligned position
        float gridX = Mathf.Round(_centro.x) * tileSize.x;
        float gridZ = Mathf.Round(_centro.z) * tileSize.z;

        // Set final position aligned to grid
        Vector3 centroRectangulo = spawner.transform.position + new Vector3(gridX, 0, gridZ);
        personaje.transform.position = centroRectangulo;
    }
}
    

//********************************************************************************************************************
/**
 * Clase PriorityQueue para almacenar elementos con prioridad para el algoritmo A*
 */
public class PriorityQueue<T>
{
    private List<KeyValuePair<T, int>> elements = new List<KeyValuePair<T, int>>();

    public int Count => elements.Count;

    public void Enqueue(T item, int priority)
    {
        elements.Add(new KeyValuePair<T, int>(item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;

        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].Value < elements[bestIndex].Value)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].Key;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}
