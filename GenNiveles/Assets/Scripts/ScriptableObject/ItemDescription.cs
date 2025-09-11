using UnityEngine;

[CreateAssetMenu(menuName = "Recetas/ItemDescription")]
public class ItemDescription : ScriptableObject
{
    public int id;
    public int cantidad;
    public GameObject prefab;
    public AudioClip sound;
    
    public ItemDescription(int id, GameObject prefab)
    {
        this.id = id;
        this.prefab = prefab;
        cantidad = 1;
    }
}