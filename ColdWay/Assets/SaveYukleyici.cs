using UnityEngine;
using System.Collections;


public class SaveYukleyici : MonoBehaviour
{
    void Start()
    {
        if (PlayerPrefs.GetInt("SaveYukle", 0) == 1)
        {
            PlayerPrefs.SetInt("SaveYukle", 0);
            StartCoroutine(YuklemeGecikme());
        }
    }

    IEnumerator YuklemeGecikme()
    {
        // Tüm Start() metodlarý çalýþsýn
        yield return null;

        if (SaveSistemi.Instance != null)
            SaveSistemi.Instance.Yukle();
    }
}