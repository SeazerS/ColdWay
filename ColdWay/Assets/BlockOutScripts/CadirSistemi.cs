using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CadirSistemi : MonoBehaviour
{
    [Header("Çadýr")]
    public GameObject çadýr;

    [Header("Çadýr Ayarlarý")]
    public int cadirHakki = 2;
    public float enerjiArtis = 40f;
    public float sicaklikArtis = 30f;
    public float cadirMesafesi = 5f;

    [Header("Baðlantýlar")]
    public SicaklikSistemi sicaklik;
    public EnerjiKontrol enerji;
    public GecGunduzSistemi gecGunduz;

    [Header("UI")]
    public TextMeshProUGUI mesajText;
    public TextMeshProUGUI cadirHakkiText;

    private bool cadirNoktasindaMi = false;
    private bool cadirKurulduMu = false;

    void Update()
    {
        // Çadýr kurma
        if (Input.GetKeyDown(KeyCode.T))
        {
            
            CadirKur();
            Instantiate(çadýr, transform.position, Quaternion.identity);
            
        }

        // UI güncelle
        if (cadirHakkiText != null)
            cadirHakkiText.text = "Çadýr: " + cadirHakki;
    }

    void CadirKur()
    {
        if (cadirHakki <= 0)
        {
            Mesaj("Çadýr hakkýn kalmadý!");
            return;
        }

        if (!cadirNoktasindaMi)
        {
            Mesaj("Buraya çadýr kurulamaaz!");
            return;
        }

        if (cadirKurulduMu)
        {
            Mesaj("Çadýr zaten kurulu!");
            return;
        }

        // Çadýr kuruldu
        cadirHakki--;
        cadirKurulduMu = true;

        // Enerji ve sýcaklýk artýþý
        if (sicaklik != null)
            sicaklik.mevcutSicaklik = Mathf.Min(
                sicaklik.mevcutSicaklik + sicaklikArtis,
                sicaklik.maxSicaklik);

        if (enerji != null)
            enerji.mevcutEnerji = Mathf.Min(
                enerji.mevcutEnerji + enerjiArtis,
                enerji.maxEnerji);

        // Sabah sýfýrla
        if (gecGunduz != null)
            gecGunduz.SabahOldu();

        Mesaj("Çadýr kuruldu. Sabah oldu.");
        Debug.Log("ÇADIR KURULDU — Hak kaldý: " + cadirHakki);
    }

    void Mesaj(string metin)
    {
        if (mesajText != null)
        {
            mesajText.text = metin;
            Invoke("MesajiTemizle", 3f);
        }
        Debug.Log(metin);
    }

    void MesajiTemizle()
    {
        if (mesajText != null)
            mesajText.text = "";
    }

    // Trigger ile çadýr noktasý
    void OnTriggerEnter(Collider diger)
    {
        if (diger.CompareTag("Player"))
        {
            cadirNoktasindaMi = true;

            
            Mesaj("Çadýr kurabilirsin [T]");
        }
    }

    void OnTriggerExit(Collider diger)
    {
        if (diger.CompareTag("Player"))
        {
            cadirNoktasindaMi = false;
        }
    }
}
