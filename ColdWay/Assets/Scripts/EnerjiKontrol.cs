using UnityEngine;
using UnityEngine.UI;

public class EnerjiKontrol : MonoBehaviour
{
    [Header("Enerji")]
    public float maxEnerji = 100f;
    public float mevcutEnerji = 100f;

    [Header("Gece Ayarlari")]
    public float geceEnerjCarpani = 2f;
    public float alacakaranlýkEnerjCarpani = 1.5f;

    [Header("Dusus Hizlari - Bolge 1")]
    public float yuruyusDususu_B1 = 0.025f;
    public float kosmaDususu_B1 = 0.08f;

    [Header("Dusus Hizlari - Bolge 2")]
    public float yuruyusDususu_B2 = 0.05f;
    public float kosmaDususu_B2 = 0.14f;

    [Header("Dusus Hizlari - Bolge 3")]
    public float yuruyusDususu_B3 = 0.09f;
    public float kosmaDususu_B3 = 0.22f;

    [Header("Sabit Degerler")]
    public float baltaDususu = 5f;
    public float atesBasiArtisi = 0.333f;
    public float etArtisi = 20f;
    public float konserveArtisi = 35f;

    [Header("UI")]
    public Slider enerjiSlider;
    public Image sliderDolgu;

    public SicaklikSistemi SicaklikSistemi;

    private Color normalRenk = new Color(1f, 0.8f, 0f);
    private Color tehlikeRenk = new Color(1f, 0.4f, 0f);
    private Color kritikRenk = new Color(1f, 0.1f, 0.1f);

    private int mevcutBolge = 1;
    private Player_Controller hareket;
    private bool atesBasinda = false;

    void Start()
    {
        mevcutEnerji = maxEnerji;
        hareket = GetComponent<Player_Controller>();
        BolgeGuncelle(1);
        UIGuncelle();
    }

    void Update()
    {
        EnerjiGuncelle();
        UIGuncelle();
        KisitlamaKontrol();
        OlumKontrol();
    }

    void EnerjiGuncelle()
    {
        if (atesBasinda)
        {
            mevcutEnerji += atesBasiArtisi * Time.deltaTime;
            mevcutEnerji = Mathf.Clamp(mevcutEnerji, 0f, maxEnerji);
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f) return;
        
        // Gun sayaci zorluk carpani
        float gunCarpani = GunSayaci.Instance != null ?
                           GunSayaci.Instance.ZorlukCarpani : 1f;

        float yuruDusus, kosDusus;
        switch (mevcutBolge)
        {
            case 2: yuruDusus = yuruyusDususu_B2; kosDusus = kosmaDususu_B2; break;
            case 3: yuruDusus = yuruyusDususu_B3; kosDusus = kosmaDususu_B3; break;
            default: yuruDusus = yuruyusDususu_B1; kosDusus = kosmaDususu_B1; break;
        }

        float geceCarpani = 1f;
        if (SicaklikSistemi != null && SicaklikSistemi.geceBonusu)
            geceCarpani = geceEnerjCarpani;
        else if (SicaklikSistemi != null && SicaklikSistemi.alacakaranlýkBonusu)
            geceCarpani = alacakaranlýkEnerjCarpani;

        if (hareket != null && hareket.KosuyorMu())
            mevcutEnerji -= kosDusus * geceCarpani * Time.deltaTime;
        else
            mevcutEnerji -= yuruDusus * geceCarpani * Time.deltaTime;

        yuruDusus *= gunCarpani;
        kosDusus *= gunCarpani;

        if (hareket != null && hareket.KosuyorMu())
            mevcutEnerji -= kosDusus * Time.deltaTime;
        else
            mevcutEnerji -= yuruDusus * Time.deltaTime;

        mevcutEnerji = Mathf.Clamp(mevcutEnerji, 0f, maxEnerji);
    }

    void OlumKontrol()
    {
        if (mevcutEnerji <= 0f)
            CheckpointSistemi.Instance?.OlumGerceklesti();
    }

    // Uyku sonrasi enerji
    public void UykuSonrasiEnerji(float mevcut)
    {
        float dusus = 20f; // Aç uyuduysa -20
        if (mevcut < 30f) dusus = 10f; // Cok actiysa daha az dus
        mevcutEnerji = Mathf.Max(5f, mevcutEnerji - dusus);
        Debug.Log($"Uyku sonrasi enerji -{dusus}. Mevcut: {mevcutEnerji}");
    }

    public void BolgeGuncelle(int bolgeNo) { mevcutBolge = bolgeNo; }

    void UIGuncelle()
    {
        if (enerjiSlider != null)
            enerjiSlider.value = mevcutEnerji / maxEnerji;
        if (sliderDolgu != null)
        {
            float oran = mevcutEnerji / maxEnerji;
            sliderDolgu.color = oran > 0.5f ? normalRenk :
                                oran > 0.2f ? tehlikeRenk : kritikRenk;
        }
    }

    void KisitlamaKontrol()
    {
        if (hareket == null) return;
        hareket.kosmakAktif = (mevcutEnerji / maxEnerji) >= 0.2f;
    }

    public void Oldu_Sifirla() { mevcutEnerji = maxEnerji * 0.75f; }
    public void AtesAktif(bool d) { atesBasinda = d; }
    public void BaltaKullanildi() { mevcutEnerji -= baltaDususu; }
    public void EtYe() { mevcutEnerji = Mathf.Min(mevcutEnerji + etArtisi, maxEnerji); }
    public void KonserveYe() { mevcutEnerji = Mathf.Min(mevcutEnerji + konserveArtisi, maxEnerji); }
}