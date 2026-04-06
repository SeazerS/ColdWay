using UnityEngine;

public class AtesBlockout : MonoBehaviour
{
    [Header("Küp Prefablarý")]
    public GameObject kiraPrefab;
    public GameObject odunPrefab;
    public GameObject kibritPrefab;

    [Header("Ayarlar")]
    public float onMesafe = 2f;  // Karakterin önü

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            KupOlustur(kiraPrefab, "Çýra kondu.");

        if (Input.GetKeyDown(KeyCode.Alpha2))
            KupOlustur(odunPrefab, "Odun kondu.");

        if (Input.GetKeyDown(KeyCode.Alpha3))
            KupOlustur(kibritPrefab, "Kibrit kondu.");
    }

    void KupOlustur(GameObject prefab, string mesaj)
    {
        if (prefab == null) return;

        // Karakterin baktýðý yönün önünde oluþtur
        Vector3 konum = transform.position +
                        transform.forward * onMesafe;
        konum.y = transform.position.y;

        Instantiate(prefab, konum,
                    Quaternion.identity);

        Debug.Log(mesaj);
    }
}
