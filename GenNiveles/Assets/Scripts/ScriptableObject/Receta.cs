using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(menuName = "Recetas/Receta")]
public class Receta : ScriptableObject
{
    public string id;
    public string nombre;
    public bool activo;
    public int dificultad;
    public List<ItemDescription> materiales;
    public Item resultado;
    public int timeCreated;


    
    
    public int GetDifficultad()
    {
        return dificultad;
    }
}