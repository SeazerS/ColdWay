using StarterAssets;
using System.Collections;
using UnityEngine;

public class FirtinaSistemi : MonoBehaviour
{
    public static FirtinaSistemi Instance;

    [Header("Zamanlama")]
    public float minBekleme = 180f;
    public float maxBekleme = 480f;
    public float minSure = 120f;
    public float maxSure = 300f;

    [Header("Gecis Suresi")]
    public float gecisHizi = 0.5f;

    [Header("Referanslar")]
    public SicaklikSistemi sicaklikSistemi;
    public RuzgarSistemi ruzgarSistemi;
    public KarTakip karSistemi;
    public GecGunduzSistemi gecGunduz;

    [Header("Fog")]
    public float normalFogYog = 0.008f;
    public float firtinaliFogYog = 0.04f;

    [Header("Ruzgar Carpani")]
    public float firtinRuzgarCarpani = 4f;

    [Header("Kar Carpani")]
    public float firtinKarCarpani = 5f;

    [Header("Ses")]
    public string ruzgarSesiAdi = "Ruzgar_Sesi";

    [Header("Kopek")]
    public KopekAI kopek;

    private bool firtinAktif = false;
    private float firtinYogunluk = 0f;
    private float hedefYogunluk = 0f;
    private bool korunuyor = false;

    private float gercekYogunlukHedef = 0f;
    private float gercekYogunlukMevcut = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(FirtinaZamanlayici());
    }

    void Update()
    {
        firtinYogunluk = Mathf.MoveTowards(
            firtinYogunluk, hedefYogunluk,
            Time.deltaTime * gecisHizi);

        EfektleriUygula();
    }

    IEnumerator FirtinaZamanlayici()
    {
        while (true)
        {
            float bekleme = Random.Range(minBekleme, maxBekleme);
            yield return new WaitForSeconds(bekleme);

            FirtinaBaslat();

            float sure = Random.Range(minSure, maxSure);
            yield return new WaitForSeconds(sure);

            FirtinaBit();
        }
    }

    void FirtinaBaslat()
    {
        firtinAktif = true;
        hedefYogunluk = 1f;

        if (kopek != null)
            kopek.FirtinaYaklasiyorSigina();

        IpucuYoneticisi.Instance?.MesajGoster(
            "firtina", "Fýrtýna yaklaþýyor!");
    }

    void FirtinaBit()
    {
        firtinAktif = false;
        hedefYogunluk = 0f;

        if (kopek != null)
            kopek.FirtinaBittiSigina();

        IpucuYoneticisi.Instance?.MesajGizle("firtina");
    }

    void EfektleriUygula()
    {
        // Hedefi belirle
        gercekYogunlukHedef = korunuyor ? 0f : firtinYogunluk;

        // Yavaþ geçiþ
        gercekYogunlukMevcut = Mathf.MoveTowards(
            gercekYogunlukMevcut, gercekYogunlukHedef,
            Time.deltaTime * 0.3f);

        float gercekYogunluk = gercekYogunlukMevcut;

        // Fog — korunuyorsa etkilenme
        RenderSettings.fogDensity = Mathf.Lerp(
            normalFogYog, firtinaliFogYog, gercekYogunluk);

        // Gökyüzü — korunuyor olsa da görsel devam etsin
        if (gecGunduz != null)
            gecGunduz.firtinExposureCarpani =
                Mathf.Lerp(1f, 0.2f, firtinYogunluk); // gercekYogunluk deðil

        // Sýcaklýk — sadece korunmuyorsa etkilensin
        if (sicaklikSistemi != null)
        {
            sicaklikSistemi.ruzgarda = gercekYogunluk > 0.3f;
            sicaklikSistemi.firtinAktif = gercekYogunluk > 0.3f;
        }

        // Rüzgar
        if (ruzgarSistemi != null)
        {
            ruzgarSistemi.firtinaCarpani = Mathf.Lerp(
                1f, firtinRuzgarCarpani, gercekYogunluk);
            ruzgarSistemi.ZamanCarpaniGuncelle(
                ruzgarSistemi.ZamanCarpaniniAl(),
                RuzgarSistemi.GununVakti.Gece);
        }

        // Kar
        if (karSistemi != null)
        {
            karSistemi.firtinaCarpani = Mathf.Lerp(
                1f, firtinKarCarpani, gercekYogunluk);
            karSistemi.ZamanCarpaniGuncelle(
                karSistemi.ZamanCarpaniniAl());
        }

        // Post process
        if (PostProsses.Instance != null)
            PostProsses.Instance.FirtinaEfektiGuncelle(gercekYogunluk);

        // Rüzgar ses seviyesi fýrtýnayla artar
        if (AudioManager.instance != null)
        {
            Sounds ruzgar = System.Array.Find(
                AudioManager.instance.sounds,
                s => s.audioName == ruzgarSesiAdi);
            if (ruzgar != null && ruzgar.source != null)
            {
                ruzgar.source.volume = Mathf.Lerp(
                    ruzgar.originalVolume,
                    Mathf.Min(ruzgar.originalVolume * firtinRuzgarCarpani, 1f),
                    gercekYogunluk);
            }
        }
    }

    public void KorunmaBaslat() { korunuyor = true; }

    public void KorunmaBit() { korunuyor = false; }

    public bool FirtinaAktifMi() { return firtinAktif; }
    public float FirtinaYogunlugu() { return firtinYogunluk; }
}