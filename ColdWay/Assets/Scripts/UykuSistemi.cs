using UnityEngine;
using TMPro;

public class UykuSistemi : MonoBehaviour
{
    public static UykuSistemi Instance;

    [Header("Referanslar")]
    public GecGunduzSistemi gecGunduz;
    public SicaklikSistemi sicaklikSistemi;
    public EnerjiKontrol enerjiKontrol;
    public CheckpointSistemi checkpoint;
    public AtesSistemi atesSistemi;

    [Header("Saat Kisitlamasi")]
    public float uykunaBaslangicSaati = 20f; // 20:00
    public float uykunaBitisSaati = 6f;  // 06:00

    [Header("UI")]
    public GameObject ipucuPanel;
    public TextMeshProUGUI ipucuText;
    public GameObject uyuOnayPanel;

    private bool cadirBolgesi = false;
    private bool oyuncuYakinda = false;
    private Vector3 cadirKurulacakPoz;


    void Awake()
    {
     
    }

    void Update()
    {
        if (!oyuncuYakinda) return;

        GuncelleMesaj();

        if (Input.GetKeyDown(KeyCode.E))
            EtusunaBasildi();
    }

    void GuncelleMesaj()
    {
        if (ipucuText == null) return;

        float saat = gecGunduz != null ?
            float.Parse(gecGunduz.SaatiAl().Split(':')[0]) : 12f;

        bool geceVakti = saat >= uykunaBaslangicSaati || saat < uykunaBitisSaati;

        if (geceVakti)
            ipucuText.text = "E — Cadýr Kur ve Uyu";
        else
            ipucuText.text = "Sadece gece uyuyabilirsin (20:00 - 06:00)";
    }

    void EtusunaBasildi()
    {
        float saat = gecGunduz != null ?
            float.Parse(gecGunduz.SaatiAl().Split(':')[0]) : 12f;

        bool geceVakti = sicaklikSistemi != null &&
                 sicaklikSistemi.geceBonusu;

        if (geceVakti)
            ipucuText.text = "E — Çadýr Kur ve Uyu";
        else
            ipucuText.text = "Hava henüz kararmadý";

        // Enerji kontrolu
        if (enerjiKontrol != null && enerjiKontrol.mevcutEnerji < 10f)
        {
            if (ipucuText != null)
                ipucuText.text = "Enerji cok dusuk! Once yemek ye.";
            return;
        }

        Uyu();
    }

    void Uyu()
    {
        // Olum coroutine'ini iptal et
        checkpoint?.OlumIptal();
        // 1. ONCE gece bonuslarini kapat
        if (sicaklikSistemi != null)
        {
            sicaklikSistemi.geceBonusu = false;
            sicaklikSistemi.alacakaranlýkBonusu = false;
        }

        // 2. oldu flaglerini sifirla
        sicaklikSistemi?.Oldu_Sifirla();
        enerjiKontrol?.Oldu_Sifirla();

        // 3. Checkpoint kaydet
        checkpoint?.CheckpointKaydet(cadirKurulacakPoz);

        // 4. Isi toparlama
        bool atesYaniyor = atesSistemi != null && atesSistemi.YaniyorMu();
        sicaklikSistemi?.UykuSonrasiIsi(atesYaniyor);

        // 5. Enerji dususu
        if (enerjiKontrol != null)
            enerjiKontrol.UykuSonrasiEnerji(enerjiKontrol.mevcutEnerji);

        // 6. Sabaha atla
        gecGunduz?.SabahOldu();

        // 7. Yeni gun
        GunSayaci.Instance?.YeniGun();

        Debug.Log("Uyku tamamlandi!");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuYakinda = true;
        cadirKurulacakPoz = other.transform.position;
        if (ipucuPanel != null) ipucuPanel.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuYakinda = false;
        if (ipucuPanel != null) ipucuPanel.SetActive(false);
    }
}
