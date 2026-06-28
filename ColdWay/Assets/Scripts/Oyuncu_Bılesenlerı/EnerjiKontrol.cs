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
    public float durmaDususu_B1 = 0.01f;
    public float yuruyusDususu_B1 = 0.08f;
    public float kosmaDususu_B1 = 0.20f;
    public float ziplamaDususu = 2f; // anlýk

    [Header("Dusus Hizlari - Bolge 2")]
    public float durmaDususu_B2 = 0.02f;
    public float yuruyusDususu_B2 = 0.14f;
    public float kosmaDususu_B2 = 0.30f;

    [Header("Dusus Hizlari - Bolge 3")]
    public float durmaDususu_B3 = 0.03f;
    public float yuruyusDususu_B3 = 0.22f;
    public float kosmaDususu_B3 = 0.45f;

    [Header("Magara")]
    public float magaraCarpani = 1f;

    [Header("Sabit Degerler")]
    public float baltaDususu = 2f;
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
    private bool oldu = false;
    private bool oncekiZiplama = false;

    [Header("Ates Isinma")]
    public float atesEtkiMesafesi = 5f;
    private AtesSistemi[] tumAtesler;

    void Start()
    {
        mevcutEnerji = maxEnerji;
        hareket = GetComponent<Player_Controller>();
        BolgeGuncelle(1);
        UIGuncelle();
        tumAtesler = FindObjectsOfType<AtesSistemi>();
    }

    void Update()
    {
        AtesYakinlikKontrol();
        EnerjiGuncelle();
        ZiplamaKontrol();
        UIGuncelle();
        KisitlamaKontrol();
        OlumKontrol();

        if (PostProsses.Instance != null)
            PostProsses.Instance.EnerjiEfektiGuncelle(mevcutEnerji / maxEnerji);
    }

    void ZiplamaKontrol()
    {
        bool simdikiZiplama = Input.GetButtonDown("Jump");
        if (simdikiZiplama && !oncekiZiplama)
        {
            mevcutEnerji -= ziplamaDususu;
            mevcutEnerji = Mathf.Clamp(mevcutEnerji, 0f, maxEnerji);
        }
        oncekiZiplama = simdikiZiplama;
    }

    void EnerjiGuncelle()
    {
        if (atesBasinda)
        {
            mevcutEnerji += atesBasiArtisi * Time.deltaTime;
            mevcutEnerji = Mathf.Clamp(mevcutEnerji, 0f, maxEnerji);
            return;
        }

        float gunCarpani = GunSayaci.Instance != null ?
                           GunSayaci.Instance.ZorlukCarpani : 1f;

        float yuruDusus, kosDusus, durDusus;
        switch (mevcutBolge)
        {
            case 2:
                yuruDusus = yuruyusDususu_B2;
                kosDusus = kosmaDususu_B2;
                durDusus = durmaDususu_B2;
                break;
            case 3:
                yuruDusus = yuruyusDususu_B3;
                kosDusus = kosmaDususu_B3;
                durDusus = durmaDususu_B3;
                break;
            default:
                yuruDusus = yuruyusDususu_B1;
                kosDusus = kosmaDususu_B1;
                durDusus = durmaDususu_B1;
                break;
        }

        // Maðara çarpaný
        yuruDusus *= magaraCarpani;
        kosDusus *= magaraCarpani;
        durDusus *= magaraCarpani;

        float geceCarpani = 1f;
        if (SicaklikSistemi != null && SicaklikSistemi.geceBonusu)
            geceCarpani = geceEnerjCarpani;
        else if (SicaklikSistemi != null && SicaklikSistemi.alacakaranlýkBonusu)
            geceCarpani = alacakaranlýkEnerjCarpani;

        float toplamCarpan = geceCarpani * gunCarpani;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool harketEdiyor = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

        if (harketEdiyor)
        {
            if (hareket != null && hareket.KosuyorMu())
                mevcutEnerji -= kosDusus * toplamCarpan * Time.deltaTime;
            else
                mevcutEnerji -= yuruDusus * toplamCarpan * Time.deltaTime;
        }
        else
        {
            // Duruyorken de yavaþ düþüþ
            mevcutEnerji -= durDusus * toplamCarpan * Time.deltaTime;
        }

        mevcutEnerji = Mathf.Clamp(mevcutEnerji, 0f, maxEnerji);
    }

    void OlumKontrol()
    {
        if (oldu) return;
        if (mevcutEnerji <= 0f)
        {
            oldu = true;
            CheckpointSistemi.Instance?.OlumGerceklesti(
                CheckpointSistemi.OlumNedeni.Enerji);
        }
    }

    public void OlduFlagSifirla() { oldu = false; }

    public void Oldu_Sifirla()
    {
        oldu = false;
        mevcutEnerji = maxEnerji * 0.15f;
    }

    public void UykuSonrasiEnerji(float mevcut)
    {
        float artisOrani = 0.15f;
        float artis = maxEnerji * artisOrani;
        mevcutEnerji = Mathf.Min(maxEnerji, mevcutEnerji + artis);
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

    void AtesYakinlikKontrol()
    {
        if (tumAtesler == null || tumAtesler.Length == 0)
        {
            tumAtesler = FindObjectsOfType<AtesSistemi>();
            return;
        }

        bool yakinAtes = false;
        foreach (AtesSistemi ates in tumAtesler)
        {
            if (ates == null || !ates.YaniyorMu()) continue;
            float mesafe = Vector3.Distance(
                transform.position, ates.transform.position);
            if (mesafe <= atesEtkiMesafesi)
            {
                yakinAtes = true;
                break;
            }
        }
        atesBasinda = yakinAtes;
    }

    public void BaltaKullanildi(float oran = 1f)
    {
        mevcutEnerji -= baltaDususu * oran;
        mevcutEnerji = Mathf.Clamp(mevcutEnerji, 0f, maxEnerji);
    }

    public void AtesAktif(bool d) { }
    public void EtYe() { mevcutEnerji = Mathf.Min(mevcutEnerji + etArtisi, maxEnerji); }
    public void KonserveYe() { mevcutEnerji = Mathf.Min(mevcutEnerji + konserveArtisi, maxEnerji); }
}