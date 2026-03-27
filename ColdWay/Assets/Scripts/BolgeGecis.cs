using UnityEngine;

public class BolgeGecis : MonoBehaviour
{
    [Header("Bölge Ayarları")]
    public int yeniBolge = 2;

    [Header("Bağlantılar")]
    public SicaklikSistemi sicaklik;
    public GecGunduzSistemi gecGunduz;
    public ParticleSystem karYagisi;

    [Header("Kar Yoğunluğu")]
    public float yeniKarYogunlugu = 1500f;

    void OnTriggerEnter(Collider diger)
    {
        if (!diger.CompareTag("Player")) return;

        // Sıcaklık güncelle
        if (sicaklik != null)
            sicaklik.BolgeGecis(yeniBolge);

        // Sahne geçişi
        if (SahneYoneticisi.Instance != null)
            SahneYoneticisi.Instance.SonrakiSahne();
    }
}
