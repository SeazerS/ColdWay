using UnityEngine;

public class BuzCatlama : MonoBehaviour
{
    public void CatlamaOynat(Vector3 pozisyon)
    {
        if (StarterAssets.AudioManager.instance != null)
        {
            StarterAssets.AudioManager.instance.Play("Buz_Kirilma");
        }

        if (PostProsses.Instance != null)
            PostProsses.Instance.BuzEfektiBaslat();
    }
}