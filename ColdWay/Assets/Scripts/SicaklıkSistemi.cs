using UnityEngine;
using UnityEngine.UI;

public class SicaklikSistemi : MonoBehaviour
{
    [Header("Gece Ayarlari")]
    public bool geceBonusu = false;
    public float geceDususCarpani = 5f;
    public bool alacakaranlıkBonusu = false;
    public float alacakaranlıkCarpani = 2f;

    [Header("Sicaklik")]
    public float maxSicaklik = 100f;
    public float mevcutSicaklik = 100f;

    [Header("Dusus Hizlari")]
    public float bolge1Hiz = 0.033f;
    public float bolge2Hiz = 0.055f;
    public float bolge3Hiz = 0.120f;

    [Header("Hava Durumu Carpanlari")]
    public float ruzgarCarpani_B1 = 1.5f;
    public float ruzgarCarpani_B2 = 2.5f;
    public float islaklıkCarpani_B2 = 3.0f;
    public float ruzgarCarpani_B3 = 4.5f;

    private float aktifRuzgarCarpani = 1.5f;
    private float aktifIslaklıkCarpani = 1.0f;
    private bool islaklıkAktif = false;

    [Header("Durum")]
    public int mevcutBolge = 1;
    public bool atesBasinda = false;
    public bool ayakIslak = false;
    public bool ruzgarda = false;

    [Header("Islak Durum")]
    public float islaklıkDususCarpani = 4f;
    public float atesIyilesmeHizi = 0.8f;
    private float islaklıkZamani = 0f;
    public float maxIslaklıkSuresi = 120f;

    [Header("UI")]
    public Slider sicaklikSlider;
    public Image sliderDolgu;

    private Color normalRenk = new Color(0.2f, 0.6f, 1f);
    private Color tehlikeRenk = new Color(1f, 0.5f, 0f);
    private Color kritikRenk = new Color(1f, 0.1f, 0.1f);

    private float dususHizi;
    private bool oldu = false;

    void Start()
    {
        mevcutSicaklik = maxSicaklik;
        BolgeGuncelle(1);
        UIGuncelle();
    }

    void Update()
    {
        if (oldu) return;
        HizBelirle();
        SicaklikGuncelle();
        UIGuncelle();
        PostProcessGuncelle();
        OlumKontrol();
    }

    void HizBelirle()
    {
        // Her karede BASE degerden baslat, uzerine ekle
        float baseDusus;
        switch (mevcutBolge)
        {
            case 1: baseDusus = bolge1Hiz; break;
            case 2: baseDusus = bolge2Hiz; break;
            case 3: baseDusus = bolge3Hiz; break;
            default: baseDusus = bolge1Hiz; break;
        }

        // Gun carpanini sadece bir kez uygula
        float gunCarpani = GunSayaci.Instance != null ?
                           GunSayaci.Instance.ZorlukCarpani : 1f;

        dususHizi = baseDusus * gunCarpani;

        if (ruzgarda) dususHizi *= aktifRuzgarCarpani;
        if (ayakIslak && islaklıkAktif) dususHizi *= aktifIslaklıkCarpani;
        if (alacakaranlıkBonusu && !geceBonusu) dususHizi *= alacakaranlıkCarpani;
        if (geceBonusu) dususHizi *= geceDususCarpani;
    }

    void SicaklikGuncelle()
    {
        if (mevcutSicaklik < 10f)
            Debug.Log($"Dusuk isi! {mevcutSicaklik:F1} | dususHizi: {dususHizi:F4} | gece: {geceBonusu}");
        
        if (atesBasinda)
        {
            if (ayakIslak)
            {
                islaklıkZamani += Time.deltaTime * 3f;
                if (islaklıkZamani >= maxIslaklıkSuresi)
                { ayakIslak = false; islaklıkZamani = 0f; }
            }
            mevcutSicaklik += atesIyilesmeHizi * Time.deltaTime;
        }
        else
        {
            float gercekDusus = ayakIslak ?
                dususHizi * islaklıkDususCarpani : dususHizi;
            mevcutSicaklik -= gercekDusus * Time.deltaTime;

            if (ayakIslak)
            {
                islaklıkZamani += Time.deltaTime;
                if (islaklıkZamani >= maxIslaklıkSuresi)
                { ayakIslak = false; islaklıkZamani = 0f; }
            }
        }
        mevcutSicaklik = Mathf.Clamp(mevcutSicaklik, 0f, maxSicaklik);
    }

    // Uyku sonrasi isiy toparlama
    public void UykuSonrasiIsi(bool atesVarMiydi)
    {
        float artis = atesVarMiydi ? 30f : 15f;
        mevcutSicaklik = Mathf.Min(mevcutSicaklik + artis, maxSicaklik);
        Debug.Log($"Uyku sonrasi isi +{artis}. Mevcut: {mevcutSicaklik}");
    }

    public void BolgeGuncelle(int bolgeNo)
    {
        mevcutBolge = bolgeNo;
        switch (bolgeNo)
        {
            case 1:
                aktifRuzgarCarpani = ruzgarCarpani_B1;
                islaklıkAktif = false; ayakIslak = false;
                break;
            case 2:
                aktifRuzgarCarpani = ruzgarCarpani_B2;
                aktifIslaklıkCarpani = islaklıkCarpani_B2;
                islaklıkAktif = true;
                break;
            case 3:
                aktifRuzgarCarpani = ruzgarCarpani_B3;
                islaklıkAktif = false; ayakIslak = false;
                break;
        }
    }

    void UIGuncelle()
    {
        if (sicaklikSlider != null)
            sicaklikSlider.value = mevcutSicaklik / maxSicaklik;

        if (sliderDolgu != null)
        {
            float oran = mevcutSicaklik / maxSicaklik;
            sliderDolgu.color = oran > 0.5f ? normalRenk :
                                oran > 0.25f ? tehlikeRenk : kritikRenk;
        }

        var hareket = GetComponent<Player_Controller>();
        if (hareket != null)
        {
            float oran = mevcutSicaklik / maxSicaklik;
            hareket.yuruyusHizi = oran < 0.5f ?
                Mathf.Lerp(0.5f, 2f, oran * 2f) : 2f;
        }
    }

    void PostProcessGuncelle()
    {
        if (mevcutSicaklik / maxSicaklik < 0.25f)
            Debug.Log("KRITIK — Ekran efekti baslasin");
    }

    void OlumKontrol()
    {
        if (oldu) return;
        if (mevcutSicaklik <= 0f)
        {
            oldu = true;
            Debug.LogWarning("OLUM! Sicaklik: " + mevcutSicaklik +
                             " | geceBonusu: " + geceBonusu +
                             " | alacakaranlık: " + alacakaranlıkBonusu +
                             " | ruzgarda: " + ruzgarda +
                             " | dususHizi: " + dususHizi);
            CheckpointSistemi.Instance?.OlumGerceklesti();
        }
    }

    public void Oldu_Sifirla()
    {
        oldu = false;
        mevcutSicaklik = 50f;
    }
    public void GoleteGirdi() { if (mevcutBolge == 2) ayakIslak = true; }
    public void Kurudu() { ayakIslak = false; }
    public void AyakIslandi() { if (islaklıkAktif) ayakIslak = true; }
    public void AtesAktif(bool d) { atesBasinda = d; }
    public void BolgeGecis(int b) { BolgeGuncelle(b); }
}