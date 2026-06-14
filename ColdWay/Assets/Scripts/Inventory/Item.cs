using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemSO item;
    public int amount = 1;

    [Header("Kibrit Kutusu")]
    public bool kibritKutusumu = false;
    public int minKibrit = 1;
    public int maxKibrit = 3;

    void Start()
    {
        if (kibritKutusumu)
            amount = Random.Range(minKibrit, maxKibrit + 1);
    }
}