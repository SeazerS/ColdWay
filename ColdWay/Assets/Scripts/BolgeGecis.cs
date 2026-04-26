using UnityEngine;

public class BolgeGecis : MonoBehaviour
{
    [Header("Bölge Ayarları")]
    public int yeniBolge = 2;

    [Header("Bağlantılar")]
    public SicaklikSistemi sicaklik;
    public GecGunduzSistemi gecGunduz;

    void OnTriggerEnter(Collider diger)
    {
        if (!diger.CompareTag("Player")) return;

        // Sıcaklık güncelle
        if (sicaklik != null)
            sicaklik.BolgeGecis(yeniBolge);

        // Kar sistemini sıfırla
        KarTakip kar =
            FindObjectOfType<KarTakip>();
        if (kar != null)
            kar.SahneGecisYenile();

        // Sahne geçişi
        if (SahneYoneticisi.Instance != null)
            SahneYoneticisi.Instance.SonrakiSahne();
    }
}
