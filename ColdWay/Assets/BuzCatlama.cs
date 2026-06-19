using UnityEngine;

public class BuzCatlama : MonoBehaviour
{
    public void CatlamaOynat(Vector3 pozisyon)
    {
        if (PostProsses.Instance != null)
            PostProsses.Instance.BuzEfektiBaslat();
    }
}