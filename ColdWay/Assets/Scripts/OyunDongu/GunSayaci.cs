using UnityEngine;

public class GunSayaci : MonoBehaviour
{
    public static GunSayaci Instance;

    [Header("Gun Durumu")]
    public int mevcutGun = 1;
    public float gunlukZorlukArtisi = 0.05f; // Her gun %5 artar

    public float ZorlukCarpani => 1f + (mevcutGun - 1) * gunlukZorlukArtisi;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void YeniGun()
    {
        mevcutGun++;
        Debug.Log($"Gun {mevcutGun} basladi. Zorluk: {ZorlukCarpani:F2}x");

        // Tum sistemleri guncelle
        var sicaklik = FindObjectOfType<SicaklikSistemi>();
        var enerji = FindObjectOfType<EnerjiKontrol>();
        int bolge = BolgeYoneticisi.Instance != null ?
                       BolgeYoneticisi.Instance.mevcutBolge : 1;

        sicaklik?.BolgeGuncelle(bolge);
        enerji?.BolgeGuncelle(bolge);
    }
}
