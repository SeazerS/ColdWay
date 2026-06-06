using UnityEngine;

public class SaveTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveSistemi.Instance?.Kaydet();
            Debug.Log("KAYDEDILDI");
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveSistemi.Instance?.Yukle();
            Debug.Log("YUKLENDI");
        }
    }
}