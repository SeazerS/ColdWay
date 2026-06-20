using UnityEngine;

public class SiginakTrigger : MonoBehaviour
{
    [Header("Ayarlar")]
    public bool firtinayiKoruyor = true;
    public bool sicakligiKoruyor = true;

    [Header("Sicaklik Kazanimi")]
    public float sicaklikKazanimCarpani = 0.5f; // içeride ýsý daha yavaþ düþer

    private bool oyuncuIcerde = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuIcerde = true;

        // Fýrtýnadan koru
        if (firtinayiKoruyor && FirtinaSistemi.Instance != null)
            FirtinaSistemi.Instance.KorunmaBaslat();

        // Ipucu
        IpucuYoneticisi.Instance?.MesajGoster(
            "siginak", "Sýðýnaktasýn — korunuyorsun");

        Debug.Log("Sýðýnaða girildi.");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuIcerde = false;

        // Fýrtýna korumasýný kaldýr
        if (firtinayiKoruyor && FirtinaSistemi.Instance != null)
            FirtinaSistemi.Instance.KorunmaBit();

        IpucuYoneticisi.Instance?.MesajGizle("siginak");

        Debug.Log("Sýðýnaktan çýkýldý.");
    }
}
