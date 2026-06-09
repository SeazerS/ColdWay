using UnityEngine;

public class GecGunduzSistemi : MonoBehaviour
{
    [Header("Skybox (2 materyal yeter)")]
    public Material gunduzSkybox;
    public Material aksamSkybox;

    [Header("Güneþ")]
    public Light gunes;

    [Header("Zaman")]
    public float gununSuresi = 15f;
    public float baslangicSaati = 8f;

    [Header("Gecis Suresi (saniye)")]
    public float gecisAdimiSuresi = 25f; // karartma + aydýnlatma her biri bu kadar sürer

    [Header("Gunes Renkleri")]
    public Color gunduzIsigiRengi = new Color(1f, 0.95f, 0.8f);
    public Color aksamIsigiRengi = new Color(1f, 0.45f, 0.1f);
    public Color geceIsigiRengi = new Color(0.15f, 0.15f, 0.35f);
    public float gunduzYogunluk = 1f;
    public float aksamYogunluk = 0.35f;
    public float geceYogunluk = 0.04f;

    [Header("Ambient")]
    public Color gunduzAmbient = new Color(0.45f, 0.47f, 0.55f);
    public Color aksamAmbient = new Color(0.20f, 0.14f, 0.10f);
    public Color geceAmbient = new Color(0.03f, 0.03f, 0.07f);

    [Header("Fog")]
    public Color gunduzFog = new Color(0.68f, 0.78f, 0.88f);
    public Color aksamFog = new Color(0.28f, 0.18f, 0.12f);
    public Color geceFog = new Color(0.02f, 0.02f, 0.05f);
    public float gunduzFogYog = 0.008f;
    public float geceFogYog = 0.022f;

    [Header("Kar / Rüzgar")]
    public float gunduzKar = 1f;
    public float geceKar = 2.5f;
    public KarTakip karSistemi;
    public RuzgarSistemi ruzgarSistemi;

    [Header("UI")]
    public TMPro.TextMeshProUGUI saatText;

    // --- private ---
    private float mevcutSaat;
    private float gunSuresiSaniye;
    private bool uyariVerildi, olumBasladi;
    private SicaklikSistemi sicaklik;

    // Instance'lar
    private Material gunduzInst;
    private Material aksamInst;

    // Orijinal parlaklýk deðerleri
    private float gunduzOrijBrightness;
    private float aksamOrijBrightness;

    // Geçiþ durumu
    private enum GecisAsamasi { Stabil, KaynakKarariyor, HedefAciliyor }
    private GecisAsamasi asama = GecisAsamasi.Stabil;
    private Material kaynakInst;
    private Material hedefInst;
    private float gecisIlerleme = 0f;

    // Iþýk / ambient için sürekli oran (0=gündüz, 1=gece)
    private float gecisOrani = 0f;

    void Start()
    {
        mevcutSaat = baslangicSaati;
        gunSuresiSaniye = gununSuresi * 60f;
        sicaklik = FindObjectOfType<SicaklikSistemi>();

        // Instance oluþtur
        gunduzInst = new Material(gunduzSkybox);
        aksamInst = new Material(aksamSkybox);

        // Orijinal parlaklýk deðerlerini oku
        gunduzOrijBrightness = OrijinalBrightness(gunduzSkybox);
        aksamOrijBrightness = OrijinalBrightness(aksamSkybox);

        // Hepsini tam parlak baþlat
        BrightnessAyarla(gunduzInst, gunduzOrijBrightness);
        BrightnessAyarla(aksamInst, aksamOrijBrightness);

        // Ambient düz mod
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        // Anýnda gündüz baþlat
        RenderSettings.skybox = gunduzInst;
        kaynakInst = gunduzInst;
        hedefInst = gunduzInst;
        RenderSettings.ambientLight = gunduzAmbient;
        RenderSettings.fog = true;
        RenderSettings.fogColor = gunduzFog;
        RenderSettings.fogDensity = gunduzFogYog;

        if (gunes != null)
        {
            gunes.color = gunduzIsigiRengi;
            gunes.intensity = gunduzYogunluk;
            float o = Mathf.Clamp01((mevcutSaat - 6f) / 14f);
            gunes.transform.rotation = Quaternion.Euler(o * 180f - 90f, 170f, 0f);
        }

        DynamicGI.UpdateEnvironment();
    }

    void Update()
    {
        mevcutSaat += (24f / gunSuresiSaniye) * Time.deltaTime;
        if (mevcutSaat >= 24f) mevcutSaat -= 24f;

        HedefSkyboxKontrol();
        GecisGuncelle();
        GunesGuncelle();
        AmbientFogGuncelle();
        KarZamanGuncelle();
        UIGuncelle();
        TehlikeKontrol();
    }

    // ??? Hangi skybox aktif olmalý ?????????????????????????????????????????
    void HedefSkyboxKontrol()
    {
        // Gündüz: 06:00 – 17:00   |   Akþam/Gece: 17:00 – 06:00
        Material gereken = (mevcutSaat >= 6f && mevcutSaat < 17f)
            ? gunduzInst : aksamInst;

        if (gereken != hedefInst && asama == GecisAsamasi.Stabil)
        {
            kaynakInst = hedefInst;   // þu anki aktif
            hedefInst = gereken;
            gecisIlerleme = 0f;
            asama = GecisAsamasi.KaynakKarariyor;

            // Hedefi sýfýrdan baþlat
            BrightnessAyarla(hedefInst, 0f);
        }
    }

    // ??? Geçiþ animasyonu ??????????????????????????????????????????????????
    void GecisGuncelle()
    {
        float adim = Time.deltaTime / gecisAdimiSuresi;

        if (asama == GecisAsamasi.KaynakKarariyor)
        {
            gecisIlerleme = Mathf.Clamp01(gecisIlerleme + adim);
            float kaynak = hedefInst == gunduzInst ? gunduzOrijBrightness : aksamOrijBrightness;
            // kaynaðýn orijinal parlaklýðýndan 0'a
            float kaynakOrig = kaynakInst == gunduzInst ? gunduzOrijBrightness : aksamOrijBrightness;
            BrightnessAyarla(kaynakInst, Mathf.Lerp(kaynakOrig, 0f, gecisIlerleme));

            if (gecisIlerleme >= 1f)
            {
                // Skybox geç
                RenderSettings.skybox = hedefInst;
                DynamicGI.UpdateEnvironment();
                gecisIlerleme = 0f;
                asama = GecisAsamasi.HedefAciliyor;
            }
        }
        else if (asama == GecisAsamasi.HedefAciliyor)
        {
            gecisIlerleme = Mathf.Clamp01(gecisIlerleme + adim);
            float hedefOrig = hedefInst == gunduzInst ? gunduzOrijBrightness : aksamOrijBrightness;
            BrightnessAyarla(hedefInst, Mathf.Lerp(0f, hedefOrig, gecisIlerleme));

            if (gecisIlerleme >= 1f)
            {
                BrightnessAyarla(hedefInst, hedefOrig);
                asama = GecisAsamasi.Stabil;
            }
        }
    }

    // ??? Güneþ ????????????????????????????????????????????????????????????
    void GunesGuncelle()
    {
        if (gunes == null) return;

        Color hedefRenk;
        float hedefYog;

        if (mevcutSaat >= 6f && mevcutSaat < 16f)
        {
            // Sabahtan öðlene: tam gündüz
            float t = Mathf.Clamp01((mevcutSaat - 6f) / 2f);
            hedefRenk = Color.Lerp(aksamIsigiRengi, gunduzIsigiRengi, t);
            hedefYog = Mathf.Lerp(aksamYogunluk, gunduzYogunluk, t);
        }
        else if (mevcutSaat >= 16f && mevcutSaat < 20f)
        {
            // Akþam kararmasý
            float t = (mevcutSaat - 16f) / 4f;
            hedefRenk = Color.Lerp(aksamIsigiRengi, geceIsigiRengi, t);
            hedefYog = Mathf.Lerp(aksamYogunluk, geceYogunluk, t);
        }
        else
        {
            hedefRenk = geceIsigiRengi;
            hedefYog = geceYogunluk;
        }

        float hiz = Time.deltaTime * 0.7f;
        gunes.color = Color.Lerp(gunes.color, hedefRenk, hiz);
        gunes.intensity = Mathf.Lerp(gunes.intensity, hedefYog, hiz);

        float gunOrani = Mathf.Clamp01((mevcutSaat - 6f) / 14f);
        gunes.transform.rotation =
            Quaternion.Euler(gunOrani * 180f - 90f, 170f, 0f);
    }

    // ??? Ambient + Fog ?????????????????????????????????????????????????????
    void AmbientFogGuncelle()
    {
        Color hedefAmb;
        Color hedefFog;
        float hedefFogYog;

        if (mevcutSaat >= 6f && mevcutSaat < 16f)
        {
            hedefAmb = gunduzAmbient;
            hedefFog = gunduzFog;
            hedefFogYog = gunduzFogYog;
        }
        else if (mevcutSaat >= 16f && mevcutSaat < 20f)
        {
            float t = (mevcutSaat - 16f) / 4f;
            hedefAmb = Color.Lerp(aksamAmbient, geceAmbient, t);
            hedefFog = Color.Lerp(aksamFog, geceFog, t);
            hedefFogYog = Mathf.Lerp(gunduzFogYog, geceFogYog, t);
        }
        else if (mevcutSaat >= 20f || mevcutSaat < 5f)
        {
            hedefAmb = geceAmbient;
            hedefFog = geceFog;
            hedefFogYog = geceFogYog;
        }
        else // 05:00 – 06:00 sabah
        {
            float t = (mevcutSaat - 5f);
            hedefAmb = Color.Lerp(geceAmbient, gunduzAmbient, t);
            hedefFog = Color.Lerp(geceFog, gunduzFog, t);
            hedefFogYog = Mathf.Lerp(geceFogYog, gunduzFogYog, t);
        }

        float hiz = Time.deltaTime * 0.35f;
        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, hedefAmb, hiz);
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, hedefFog, hiz);
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, hedefFogYog, hiz);
    }

    // ??? Yardýmcýlar ???????????????????????????????????????????????????????
    float OrijinalBrightness(Material mat)
    {
        if (mat == null) return 1f;
        if (mat.HasProperty("_Exposure")) return mat.GetFloat("_Exposure");
        if (mat.HasProperty("_Tint")) return mat.GetColor("_Tint").r;
        if (mat.HasProperty("_SkyTint")) return mat.GetColor("_SkyTint").r;
        return 1f;
    }

    void BrightnessAyarla(Material mat, float deger)
    {
        if (mat == null) return;
        if (mat.HasProperty("_Exposure"))
        {
            mat.SetFloat("_Exposure", deger);
            return;
        }
        if (mat.HasProperty("_Tint"))
        {
            Color c = mat.GetColor("_Tint");
            mat.SetColor("_Tint", new Color(deger, deger, deger, c.a));
            return;
        }
        if (mat.HasProperty("_SkyTint"))
        {
            Color c = mat.GetColor("_SkyTint");
            mat.SetColor("_SkyTint", new Color(deger, deger, deger, c.a));
        }
    }

    // ??? Diðerleri ?????????????????????????????????????????????????????????
    void KarZamanGuncelle()
    {
        float karC, ruzgarC;
        RuzgarSistemi.GununVakti secilenVakit;

        if (mevcutSaat >= 6f && mevcutSaat < 16f)
        {
            karC = gunduzKar;
            ruzgarC = 1f;
            secilenVakit = RuzgarSistemi.GununVakti.SabahOglen;
        }
        else if (mevcutSaat >= 16f && mevcutSaat < 20f)
        {
            float t = (mevcutSaat - 16f) / 4f;
            karC = Mathf.Lerp(gunduzKar, geceKar, t);
            ruzgarC = Mathf.Lerp(1f, 2f, t);
            secilenVakit = RuzgarSistemi.GununVakti.Aksam;
        }
        else
        {
            karC = geceKar;
            ruzgarC = 2f;
            secilenVakit = RuzgarSistemi.GununVakti.Gece;
        }

        karSistemi?.ZamanCarpaniGuncelle(karC);

        // Rüzgar sistemine hem yeni rüzgar gücünü hem de günün vaktini gönderiyoruz
        ruzgarSistemi?.ZamanCarpaniGuncelle(ruzgarC, secilenVakit);
    }

    void UIGuncelle()
    {
        if (saatText == null) return;
        int s = Mathf.FloorToInt(mevcutSaat);
        int d = Mathf.FloorToInt((mevcutSaat - s) * 60f);
        saatText.text = string.Format("{0:00}:{1:00}", s, d);
    }

    void TehlikeKontrol()
    {
        if (mevcutSaat >= 18f && mevcutSaat < 20f && !uyariVerildi)
        { uyariVerildi = true; if (sicaklik) sicaklik.alacakaranlýkBonusu = true; }

        if (mevcutSaat >= 20f && !olumBasladi)
        { olumBasladi = true; if (sicaklik) sicaklik.geceBonusu = true; }
    }

    public string SaatiAl()
    {
        int s = Mathf.FloorToInt(mevcutSaat);
        int d = Mathf.FloorToInt((mevcutSaat - s) * 60f);
        return string.Format("{0:00}:{1:00}", s, d);
    }

    public void SabahOldu()
    {
        mevcutSaat = 6f;
        uyariVerildi = false;
        olumBasladi = false;
        asama = GecisAsamasi.Stabil;
        gecisIlerleme = 0f;

        if (sicaklik) { sicaklik.geceBonusu = false; sicaklik.alacakaranlýkBonusu = false; }

        BrightnessAyarla(gunduzInst, gunduzOrijBrightness);
        BrightnessAyarla(aksamInst, aksamOrijBrightness);
        hedefInst = kaynakInst = gunduzInst;

        RenderSettings.skybox = gunduzInst;
        RenderSettings.ambientLight = gunduzAmbient;
        RenderSettings.fogColor = gunduzFog;
        RenderSettings.fogDensity = gunduzFogYog;
        if (gunes) { gunes.color = gunduzIsigiRengi; gunes.intensity = gunduzYogunluk; }

        ruzgarSistemi?.ZamanCarpaniGuncelle(1f, RuzgarSistemi.GununVakti.SabahOglen);

        DynamicGI.UpdateEnvironment();
    }
}