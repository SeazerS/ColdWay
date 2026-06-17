using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Buton için gerekli

public class IpucuYoneticisi : MonoBehaviour
{
    public static IpucuYoneticisi Instance;

    [Header("Temel Ýpucu Paneli")]
    public GameObject ipucuPanel;
    public TextMeshProUGUI ipucuText;

    [Header("Detay Bilgi Paneli (Soru Ýþareti)")]
    public GameObject soruIsaretiButonu; // Sadece "?" ikonunun olduðu obje
    public GameObject detayYaziPaneli;   // Asýl uzun yazýnýn olduðu arka plan/panel
    public TextMeshProUGUI detayText;

    private Dictionary<string, string> aktifMesajlar = new Dictionary<string, string>();
    private Dictionary<string, string> aktifDetaylar = new Dictionary<string, string>();

    private bool detayAcik = false; // Týklanýnca açýlýp kapanmasýný takip eder

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        aktifMesajlar.Clear();
        aktifDetaylar.Clear();

        if (ipucuPanel != null) ipucuPanel.SetActive(false);
        if (soruIsaretiButonu != null) soruIsaretiButonu.SetActive(false);
        if (detayYaziPaneli != null) detayYaziPaneli.SetActive(false);
    }

    void Update()
    {
        // FPS oyunlarýnda mouse kilitli olduðu için týklamak yerine "H" tuþuyla da açýlmasýný saðlýyoruz.
        if (soruIsaretiButonu != null && soruIsaretiButonu.activeSelf && Input.GetKeyDown(KeyCode.H))
        {
            DetayTetikle();
        }
    }

    public void MesajGoster(string kaynak, string mesaj, string detayMesaji = "")
    {
        aktifMesajlar[kaynak] = mesaj;

        if (!string.IsNullOrEmpty(detayMesaji))
            aktifDetaylar[kaynak] = detayMesaji;
        else
            aktifDetaylar.Remove(kaynak);

        Guncelle();
    }

    public void MesajGizle(string kaynak)
    {
        if (aktifMesajlar.ContainsKey(kaynak))
            aktifMesajlar.Remove(kaynak);

        if (aktifDetaylar.ContainsKey(kaynak))
            aktifDetaylar.Remove(kaynak);

        // Oyuncu objeden uzaklaþtýðýnda açýk kalan detay panelini kapat
        if (aktifDetaylar.Count == 0)
        {
            detayAcik = false;
            if (detayYaziPaneli != null) detayYaziPaneli.SetActive(false);
        }

        Guncelle();
    }

    void Guncelle()
    {
        if (aktifMesajlar.Count == 0)
        {
            ipucuPanel.SetActive(false);
            soruIsaretiButonu.SetActive(false);
            detayYaziPaneli.SetActive(false);
            detayAcik = false;
            return;
        }

        ipucuPanel.SetActive(true);
        foreach (var mesaj in aktifMesajlar)
            ipucuText.text = mesaj.Value;

        bool detayVar = false;
        foreach (var detay in aktifDetaylar)
        {
            detayText.text = detay.Value;
            detayVar = true;
            break;
        }

        soruIsaretiButonu.SetActive(detayVar);

        // Eðer detay varsa ama oyuncu henüz týklamadýysa yazýyý gizli tut
        detayYaziPaneli.SetActive(detayVar && detayAcik);
    }

    // Bu fonksiyonu Unity'de "?" Butonunun "On Click()" olayýna baðlayacaðýz
    public void DetayTetikle()
    {
        detayAcik = !detayAcik; // Aç-Kapat mantýðý

        if (detayYaziPaneli != null)
            detayYaziPaneli.SetActive(detayAcik);

        // Týklama sesi
        if (StarterAssets.AudioManager.instance != null && detayAcik)
        {
            StarterAssets.AudioManager.instance.Play("Button_Týklama");
        }
    }
}