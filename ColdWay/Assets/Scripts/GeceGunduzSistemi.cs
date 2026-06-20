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
    public float gecisAdimiSuresi = 25f;

    [Header("Gunes Renkleri")]
    public Color gunduzIsigiRengi = new Color(1f, 0.95f, 0.8f);
    public Color geceIsigiRengi = new Color(0.15f, 0.15f, 0.35f);
    public float gunduzYogunluk = 0.5f;
    public float geceYogunluk = 0.04f;

    [Header("Ambient")]
    public Color gunduzAmbient = new Color(0.45f, 0.47f, 0.55f);
    public Color geceAmbient = new Color(0.03f, 0.03f, 0.07f);

    [Header("Fog")]
    public Color gunduzFog = new Color(0.68f, 0.78f, 0.88f);
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

    [Header("Skybox Parlaklik")]
    public float gunduzParlaklik = 1f;
    public float aksamParlaklik = 1f;

    [Header("Bulut Hareketi")]
    public float bulutHizi = 0.5f;

    [Header("Firtina")]
    public float firtinExposureCarpani = 1f;

    // --- private ---
    private float mevcutSaat;
    private float gunSuresiSaniye;
    private bool uyariVerildi, olumBasladi;
    private SicaklikSistemi sicaklik;

    private Material gunduzInst;
    private Material aksamInst;

    private float gunduzOrijBrightness;
    private float aksamOrijBrightness;

    private enum GecisAsamasi { Stabil, KaynakKarariyor, HedefAciliyor }
    private GecisAsamasi asama = GecisAsamasi.Stabil;
    private Material kaynakInst;
    private Material hedefInst;
    private float gecisIlerleme = 0f;

    void Start()
    {
        mevcutSaat = baslangicSaati;
        gunSuresiSaniye = gununSuresi * 60f;
        sicaklik = FindObjectOfType<SicaklikSistemi>();

        gunduzInst = new Material(gunduzSkybox);
        aksamInst = new Material(aksamSkybox);

        gunduzOrijBrightness = gunduzParlaklik;
        aksamOrijBrightness = aksamParlaklik;

        BrightnessAyarla(gunduzInst, gunduzOrijBrightness);
        BrightnessAyarla(aksamInst, aksamOrijBrightness);

        if (gunduzSkybox != null && gunduzSkybox.HasProperty("_Exposure"))
            gunduzSkybox.SetFloat("_Exposure", gunduzParlaklik);
        if (aksamSkybox != null && aksamSkybox.HasProperty("_Exposure"))
            aksamSkybox.SetFloat("_Exposure", aksamParlaklik);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
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
            gunes.transform.rotation =
                Quaternion.Euler(o * 180f - 90f, 170f, 0f);
        }

        DynamicGI.UpdateEnvironment();
    }

    void Update()
    {
        mevcutSaat += (24f / gunSuresiSaniye) * Time.deltaTime;
        if (mevcutSaat >= 24f) mevcutSaat -= 24f;

        if (gunduzInst != null && gunduzInst.HasProperty("_Exposure"))
        {
            float hedef = firtinExposureCarpani < 1f
                ? Mathf.Lerp(0.52f, 0.1f, 1f - firtinExposureCarpani)
                : 0.52f;
            gunduzInst.SetFloat("_Exposure", hedef);
        }

        HedefSkyboxKontrol();
        GecisGuncelle();
        GunesGuncelle();
        AmbientFogGuncelle();
        KarZamanGuncelle();
        UIGuncelle();
        TehlikeKontrol();
        BulutGuncelle();
    }

    void BulutGuncelle()
    {
        if (mevcutSaat >= 6f && mevcutSaat < 19f
            && RenderSettings.skybox != null
            && RenderSettings.skybox.HasProperty("_Rotation"))
        {
            float rot = RenderSettings.skybox.GetFloat("_Rotation");
            RenderSettings.skybox.SetFloat(
                "_Rotation", rot + bulutHizi * Time.deltaTime);
        }
    }

    void HedefSkyboxKontrol()
    {
        Material gereken = (mevcutSaat >= 6f && mevcutSaat < 19f)
            ? gunduzInst : aksamInst;

        if (gereken != hedefInst && asama == GecisAsamasi.Stabil)
        {
            kaynakInst = hedefInst;
            hedefInst = gereken;
            gecisIlerleme = 0f;
            asama = GecisAsamasi.KaynakKarariyor;
            BrightnessAyarla(hedefInst, 0f);
        }
    }

    void GecisGuncelle()
    {
        float adim = Time.deltaTime / gecisAdimiSuresi;

        if (asama == GecisAsamasi.KaynakKarariyor)
        {
            gecisIlerleme = Mathf.Clamp01(gecisIlerleme + adim);
            float kaynakOrig = kaynakInst == gunduzInst
                ? gunduzOrijBrightness : aksamOrijBrightness;
            BrightnessAyarla(kaynakInst,
                Mathf.Lerp(kaynakOrig, 0f, gecisIlerleme));

            if (gecisIlerleme >= 1f)
            {
                RenderSettings.skybox = hedefInst;
                DynamicGI.UpdateEnvironment();
                gecisIlerleme = 0f;
                asama = GecisAsamasi.HedefAciliyor;
            }
        }
        else if (asama == GecisAsamasi.HedefAciliyor)
        {
            gecisIlerleme = Mathf.Clamp01(gecisIlerleme + adim);
            float hedefOrig = hedefInst == gunduzInst
                ? gunduzOrijBrightness * firtinExposureCarpani
                : aksamOrijBrightness;
            BrightnessAyarla(hedefInst,
                Mathf.Lerp(0f, hedefOrig, gecisIlerleme));

            if (gecisIlerleme >= 1f)
            {
                BrightnessAyarla(hedefInst, hedefOrig);
                asama = GecisAsamasi.Stabil;
            }
        }
        else if (asama == GecisAsamasi.Stabil
                 && RenderSettings.skybox == gunduzInst)
        {
            // Fýrtýnada gündüz skybox'ý karart
            BrightnessAyarla(gunduzInst,
                gunduzOrijBrightness * firtinExposureCarpani);
        }
    }

    void GunesGuncelle()
    {
        if (gunes == null) return;

        Color hedefRenk;
        float hedefYog;

        if (mevcutSaat >= 6f && mevcutSaat < 17f)
        {
            hedefRenk = gunduzIsigiRengi;
            hedefYog = gunduzYogunluk;
        }
        else if (mevcutSaat >= 17f && mevcutSaat < 21f)
        {
            float t = (mevcutSaat - 17f) / 4f;
            hedefRenk = Color.Lerp(gunduzIsigiRengi, geceIsigiRengi, t);
            hedefYog = Mathf.Lerp(gunduzYogunluk, geceYogunluk, t);
        }
        else
        {
            hedefRenk = geceIsigiRengi;
            hedefYog = geceYogunluk;
        }

        // Fýrtýnada güneþi de karart
        hedefYog *= firtinExposureCarpani;

        float hiz = Time.deltaTime * 3f;
        gunes.color = Color.Lerp(gunes.color, hedefRenk, hiz);
        gunes.intensity = Mathf.Lerp(gunes.intensity, hedefYog, hiz);

        float gunOrani = Mathf.Clamp01((mevcutSaat - 6f) / 14f);
        gunes.transform.rotation =
            Quaternion.Euler(gunOrani * 180f - 90f, 170f, 0f);
    }

    void AmbientFogGuncelle()
    {
        Color hedefAmb;
        Color hedefFog;
        float hedefFogYog;

        if (mevcutSaat >= 6f && mevcutSaat < 19f)
        {
            hedefAmb = gunduzAmbient;
            hedefFog = gunduzFog;
            hedefFogYog = gunduzFogYog;
        }
        else if (mevcutSaat >= 19f && mevcutSaat < 22f)
        {
            float t = (mevcutSaat - 19f) / 3f;
            hedefAmb = Color.Lerp(gunduzAmbient, geceAmbient, t);
            hedefFog = Color.Lerp(gunduzFog, geceFog, t);
            hedefFogYog = Mathf.Lerp(gunduzFogYog, geceFogYog, t);
        }
        else if (mevcutSaat >= 22f || mevcutSaat < 5f)
        {
            hedefAmb = geceAmbient;
            hedefFog = geceFog;
            hedefFogYog = geceFogYog;
        }
        else
        {
            float t = (mevcutSaat - 5f);
            hedefAmb = Color.Lerp(geceAmbient, gunduzAmbient, t);
            hedefFog = Color.Lerp(geceFog, gunduzFog, t);
            hedefFogYog = Mathf.Lerp(geceFogYog, gunduzFogYog, t);
        }

        // Fýrtýnada ambient de kararsýn
        hedefAmb = Color.Lerp(hedefAmb, geceAmbient,
            1f - firtinExposureCarpani);

        float hiz = Time.deltaTime * 0.35f;
        RenderSettings.ambientLight =
            Color.Lerp(RenderSettings.ambientLight, hedefAmb, hiz);
        RenderSettings.fogColor =
            Color.Lerp(RenderSettings.fogColor, hedefFog, hiz);

        // Fog yoðunluðunu FirtinaSistemi yönetir, burada sadece normal ak
        if (FirtinaSistemi.Instance == null ||
            !FirtinaSistemi.Instance.FirtinaAktifMi())
        {
            RenderSettings.fogDensity =
                Mathf.Lerp(RenderSettings.fogDensity, hedefFogYog, hiz);
        }
    }

    public void BrightnessAyarla(Material mat, float deger)
    {
        if (mat == null) return;
        if (mat.HasProperty("_Exposure"))
        { mat.SetFloat("_Exposure", deger); return; }
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

    void KarZamanGuncelle()
    {
        // Fýrtýna aktifse KarTakip ve RuzgarSistemi FirtinaSistemi yönetir
        if (FirtinaSistemi.Instance != null &&
            FirtinaSistemi.Instance.FirtinaAktifMi()) return;

        float karC, ruzgarC;
        RuzgarSistemi.GununVakti secilenVakit;

        if (mevcutSaat >= 6f && mevcutSaat < 19f)
        {
            karC = gunduzKar; ruzgarC = 1f;
            secilenVakit = RuzgarSistemi.GununVakti.SabahOglen;
        }
        else if (mevcutSaat >= 19f && mevcutSaat < 22f)
        {
            float t = (mevcutSaat - 19f) / 3f;
            karC = Mathf.Lerp(gunduzKar, geceKar, t);
            ruzgarC = Mathf.Lerp(1f, 2f, t);
            secilenVakit = RuzgarSistemi.GununVakti.Aksam;
        }
        else
        {
            karC = geceKar; ruzgarC = 2f;
            secilenVakit = RuzgarSistemi.GununVakti.Gece;
        }

        karSistemi?.ZamanCarpaniGuncelle(karC);
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
        if (mevcutSaat >= 19f && mevcutSaat < 20f && !uyariVerildi)
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

        if (sicaklik)
        { sicaklik.geceBonusu = false; sicaklik.alacakaranlýkBonusu = false; }

        BrightnessAyarla(gunduzInst, gunduzOrijBrightness);
        BrightnessAyarla(aksamInst, aksamOrijBrightness);
        hedefInst = kaynakInst = gunduzInst;

        RenderSettings.skybox = gunduzInst;
        RenderSettings.ambientLight = gunduzAmbient;
        RenderSettings.fogColor = gunduzFog;
        RenderSettings.fogDensity = gunduzFogYog;

        if (gunes)
        { gunes.color = gunduzIsigiRengi; gunes.intensity = gunduzYogunluk; }

        ruzgarSistemi?.ZamanCarpaniGuncelle(
            1f, RuzgarSistemi.GununVakti.SabahOglen);
        DynamicGI.UpdateEnvironment();
    }
}