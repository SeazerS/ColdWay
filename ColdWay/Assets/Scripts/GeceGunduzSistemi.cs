using UnityEngine;

public class GecGunduzSistemi : MonoBehaviour
{
    [Header("Skybox")]
    public Material gunduzSkybox;
    public Material aksamSkybox;

    [Header("Güneþ")]
    public Light gunes;

    [Header("Zaman")]
    public float gununSuresi = 15f;
    public float baslangicSaati = 8f;

    [Header("Renkler")]
    public Color gunduzIsigiRengi = new Color(1f, 0.95f, 0.8f);
    public Color aksamIsigiRengi = new Color(1f, 0.4f, 0.1f);
    public Color geceIsigiRengi = new Color(0.1f, 0.1f, 0.3f);
    public float geceIsigiYogunlugu = 0.05f;

    [Header("Ambient")]
    public Color gunduzAmbient = new Color(0.5f, 0.5f, 0.6f);
    public Color aksamAmbient = new Color(0.25f, 0.18f, 0.12f);
    public Color geceAmbient = new Color(0.03f, 0.03f, 0.08f);

    [Header("Fog")]
    public Color gunduzFog = new Color(0.7f, 0.8f, 0.9f);
    public Color aksamFog = new Color(0.3f, 0.2f, 0.15f);
    public Color geceFog = new Color(0.02f, 0.02f, 0.05f);
    public float gunduzFogYogunluk = 0.008f;
    public float geceFogYogunluk = 0.025f;

    [Header("Kar / Rüzgar")]
    public float gunduzKarCarpani = 1f;
    public float geceKarCarpani = 2.5f;
    public KarTakip karSistemi;
    public RuzgarSistemi ruzgarSistemi;

    [Header("UI")]
    public TMPro.TextMeshProUGUI saatText;

    private float mevcutSaat;
    private float gunSuresiSaniye;
    private bool uyariVerildi = false;
    private bool olumBasladi = false;
    private SicaklikSistemi sicaklik;

    void Start()
    {
        mevcutSaat = baslangicSaati;
        gunSuresiSaniye = gununSuresi * 60f;
        sicaklik = FindObjectOfType<SicaklikSistemi>();

        RenderSettings.ambientMode =
            UnityEngine.Rendering.AmbientMode.Flat;

        // Gunduz ile baslat
        RenderSettings.skybox = gunduzSkybox;
        RenderSettings.ambientLight = gunduzAmbient;
        RenderSettings.fog = true;
        RenderSettings.fogColor = gunduzFog;
        RenderSettings.fogDensity = gunduzFogYogunluk;

        if (gunes != null)
        {
            gunes.color = gunduzIsigiRengi;
            gunes.intensity = 1f;
        }

        DynamicGI.UpdateEnvironment();
    }

    void Update()
    {
        mevcutSaat += (24f / gunSuresiSaniye) * Time.deltaTime;
        if (mevcutSaat >= 24f) mevcutSaat = 0f;

        GunesGuncelle();
        SkyboxGuncelle();
        AmbientFogGuncelle();
        KarZamanGuncelle();
        UIGuncelle();
        TehlikeKontrol();
    }

    void GunesGuncelle()
    {
        if (gunes == null) return;

        Color hedefRenk;
        float hedefYogunluk;

        if (mevcutSaat >= 6f && mevcutSaat < 16f)
        {
            // Gunduz — parlak
            float oran = Mathf.Clamp01((mevcutSaat - 6f) / 2f);
            hedefRenk = Color.Lerp(aksamIsigiRengi, gunduzIsigiRengi, oran);
            hedefYogunluk = Mathf.Lerp(0.3f, 1f, oran);
        }
        else if (mevcutSaat >= 16f && mevcutSaat < 20f)
        {
            // Aksam — yavaþ karariyor
            float oran = (mevcutSaat - 16f) / 4f;
            hedefRenk = Color.Lerp(gunduzIsigiRengi, aksamIsigiRengi, oran);
            hedefYogunluk = Mathf.Lerp(1f, 0.15f, oran);
        }
        else
        {
            // Gece
            hedefRenk = geceIsigiRengi;
            hedefYogunluk = geceIsigiYogunlugu;
        }

        gunes.color = Color.Lerp(gunes.color, hedefRenk, Time.deltaTime * 0.8f);
        gunes.intensity = Mathf.Lerp(gunes.intensity, hedefYogunluk,
            Time.deltaTime * 0.8f);

        float gunOrani = Mathf.Clamp01((mevcutSaat - 6f) / 14f);
        gunes.transform.rotation =
            Quaternion.Euler(gunOrani * 180f - 90f, 170f, 0f);
    }

    void SkyboxGuncelle()
    {
        // Gunduz skybox: 06:00 - 17:00
        // Aksam skybox : 17:00 - 06:00
        Material gereken = (mevcutSaat >= 6f && mevcutSaat < 17f)
            ? gunduzSkybox : aksamSkybox;

        if (RenderSettings.skybox != gereken)
        {
            RenderSettings.skybox = gereken;
            DynamicGI.UpdateEnvironment();
        }
    }

    void AmbientFogGuncelle()
    {
        Color hedefAmbient;
        Color hedefFog;
        float hedefFogYogunluk;

        if (mevcutSaat >= 6f && mevcutSaat < 16f)
        {
            // Tam gunduz
            hedefAmbient = gunduzAmbient;
            hedefFog = gunduzFog;
            hedefFogYogunluk = gunduzFogYogunluk;
        }
        else if (mevcutSaat >= 16f && mevcutSaat < 20f)
        {
            // Aksam gecisi
            float oran = (mevcutSaat - 16f) / 4f;
            hedefAmbient = Color.Lerp(gunduzAmbient, geceAmbient, oran);
            hedefFog = Color.Lerp(aksamFog, geceFog, oran);
            hedefFogYogunluk = Mathf.Lerp(gunduzFogYogunluk,
                geceFogYogunluk, oran);
        }
        else if (mevcutSaat >= 20f || mevcutSaat < 5f)
        {
            // Tam gece
            hedefAmbient = geceAmbient;
            hedefFog = geceFog;
            hedefFogYogunluk = geceFogYogunluk;
        }
        else
        {
            // Sabah gecisi (05:00 - 06:00)
            float oran = (mevcutSaat - 5f);
            hedefAmbient = Color.Lerp(geceAmbient, gunduzAmbient, oran);
            hedefFog = Color.Lerp(geceFog, gunduzFog, oran);
            hedefFogYogunluk = Mathf.Lerp(geceFogYogunluk,
                gunduzFogYogunluk, oran);
        }

        float hiz = Time.deltaTime * 0.4f;
        RenderSettings.ambientLight = Color.Lerp(
            RenderSettings.ambientLight, hedefAmbient, hiz);
        RenderSettings.fogColor = Color.Lerp(
            RenderSettings.fogColor, hedefFog, hiz);
        RenderSettings.fogDensity = Mathf.Lerp(
            RenderSettings.fogDensity, hedefFogYogunluk, hiz);
    }

    void KarZamanGuncelle()
    {
        float karCarpan, ruzgarCarpan;
        if (mevcutSaat >= 6f && mevcutSaat < 16f)
        { karCarpan = gunduzKarCarpani; ruzgarCarpan = 1f; }
        else if (mevcutSaat >= 16f && mevcutSaat < 20f)
        {
            float oran = (mevcutSaat - 16f) / 4f;
            karCarpan = Mathf.Lerp(gunduzKarCarpani, geceKarCarpani, oran);
            ruzgarCarpan = Mathf.Lerp(1f, 2f, oran);
        }
        else
        { karCarpan = geceKarCarpani; ruzgarCarpan = 2f; }

        karSistemi?.ZamanCarpaniGuncelle(karCarpan);
        ruzgarSistemi?.ZamanCarpaniGuncelle(ruzgarCarpan);
    }

    void UIGuncelle()
    {
        if (saatText == null) return;
        int saat = Mathf.FloorToInt(mevcutSaat);
        int dakika = Mathf.FloorToInt((mevcutSaat - saat) * 60f);
        saatText.text = string.Format("{0:00}:{1:00}", saat, dakika);
    }

    void TehlikeKontrol()
    {
        if (mevcutSaat >= 18f && mevcutSaat < 20f && !uyariVerildi)
        {
            uyariVerildi = true;
            if (sicaklik != null) sicaklik.alacakaranlýkBonusu = true;
        }
        if (mevcutSaat >= 20f && !olumBasladi)
        {
            olumBasladi = true;
            if (sicaklik != null) sicaklik.geceBonusu = true;
        }
    }

    public string SaatiAl()
    {
        int saat = Mathf.FloorToInt(mevcutSaat);
        int dakika = Mathf.FloorToInt((mevcutSaat - saat) * 60f);
        return string.Format("{0:00}:{1:00}", saat, dakika);
    }

    public void SabahOldu()
    {
        mevcutSaat = 6f;
        uyariVerildi = false;
        olumBasladi = false;

        if (sicaklik != null)
        {
            sicaklik.geceBonusu = false;
            sicaklik.alacakaranlýkBonusu = false;
        }

        RenderSettings.skybox = gunduzSkybox;
        RenderSettings.ambientLight = gunduzAmbient;
        RenderSettings.fogColor = gunduzFog;
        RenderSettings.fogDensity = gunduzFogYogunluk;

        if (gunes != null)
        {
            gunes.color = gunduzIsigiRengi;
            gunes.intensity = 1f;
        }

        DynamicGI.UpdateEnvironment();
    }
}