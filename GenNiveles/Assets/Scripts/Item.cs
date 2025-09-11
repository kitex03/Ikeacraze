using UnityEngine;

[System.Serializable]
public class Item
{
    public int id;
    public int cantidad;
    public GameObject prefab;
    public AudioClip sound;
    
    public Item(int id, GameObject prefab)
    {
        this.id = id;
        this.prefab = prefab;
        cantidad = 1;
    }
}