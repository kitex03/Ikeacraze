using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public ItemDescription materialAsignado; // Material que contiene el cofre
    public GameObject burbujaPrefab; // Prefab de la burbuja
    
    private GameObject burbujaInstance;
    private Animator anim;
    private bool jugadorCerca = false;
    private GameObject materialInstance;
    
    void Start()
    {
        // Instanciar la burbuja sobre el cofre
        CrearBurbuja();

        anim = GetComponent<Animator>();
        if (!anim)
        {
            Debug.Log("Animator no encontrado en el cofre.");
        }
        
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("El jugador está cerca del cofre y este se abre.");
            anim.SetBool("open", true);
            anim.SetBool("close", false);
            jugadorCerca = true;
            MostrarMaterial();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("El jugador se ha alejado del cofre.");
            jugadorCerca = false;
            anim.SetBool("close", true);
            anim.SetBool("open", false);
            EliminarMaterial();
        }
    }

    public ItemDescription CogerMaterial()
    {
        return materialAsignado;
    }

    public void AsignarMaterial(ItemDescription item)
    {
        // Asignar el material al cofre
        materialAsignado = item;
        // ActualizarBurbuja();
    }
    
public void MostrarMaterial()
{
    if (materialAsignado != null && materialAsignado.prefab != null)
    {
        // Instanciar el prefab del material
        materialInstance = Instantiate(materialAsignado.prefab, transform.position, Quaternion.identity);

        // Posicionar el material encima del cofre
        materialInstance.transform.position = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);

        // Verificar si el material no es madera y ajustar el tamaño
        if (materialAsignado.prefab.name == "Crystal")
        {
            materialInstance.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
        if (materialAsignado.prefab.name == "Screw")
        {
            materialInstance.transform.localScale = new Vector3(50f, 50f, 50f);
        }
        if (materialAsignado.prefab.name == "Wood")
        {
            materialInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }

        // Opcional: hacerlo hijo del cofre para mantener la posición relativa
        materialInstance.transform.SetParent(this.transform);

        Debug.Log("Material mostrado sobre el cofre: " + materialAsignado.prefab.name);
    }
    else
    {
        Debug.LogWarning("El material o su prefab no están asignados.");
    }
}
    
    public void EliminarMaterial()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance); // Destruye la instancia del material
            materialInstance = null;   // Limpia la referencia
            Debug.Log("Material eliminado del cofre.");
        }
    }

    void CrearBurbuja()
    {
        if (burbujaPrefab != null)
        {
            // Instanciar la burbuja sobre el cofre
            burbujaInstance = Instantiate(burbujaPrefab, transform);

            // Posicionar la burbuja encima del cofre
            burbujaInstance.transform.localPosition = new Vector3(0, 2f, 0);

            // Actualizar el texto o imagen del material
            ActualizarBurbuja();
        }
        else
        {
            Debug.LogWarning("No se ha asignado un prefab para la burbuja.");
        }
    }

    void ActualizarBurbuja()
    {
        // Busca un componente de texto dentro de la burbuja
        var texto = burbujaInstance.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (texto != null)
        {
            texto.text = materialAsignado.prefab.name;
        }
    }
}