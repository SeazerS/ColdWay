using UnityEngine;

public class GecGunduzSistemi : MonoBehaviour
{
    [Header("Skybox")]
    public Material gunduzSkybox;
    public Material aksamSkybox;
    public Material geceSkybox;

    [Header("Gecis Ayarlari")]
    public float gecisSuresi = 30f;

    [Header("Güneþ")]
    public Light gunes;

    [Header("Zaman")]
    public float gununSuresi = 40f;
    public float baslangicSaati = 6f;

    [Header("Renkler")]
    public Color gunduzIsigiRengi = new Color(1f, 0.95f, 0.8f);
    public Color aksamIsigiRengi = new Color(1f, 0.5f, 0.2f);
    public Color geceIsigiRengi = new Color(0.1f, 0.1f, 0.3f);

    [Header("UI")]
    public TMPro.TextMeshProUGUI saatText;

    [Header("Fog Ayarlarý")]
    public Color gunduzFogRengi = new Color(0.7f, 0.8f, 0.9f);
    public Color aksamFogRengi = new Color(0.4f, 0.3f, 0.3f);
    public Color geceFogRengi = new Color(0.05f, 0.05f, 0.1f);
    public float gunduzFogYogunluk = 0.01f;
    public float geceFogYogunluk = 0.03f;

    [Header("Ambient Iþýk")]
    public Color gunduzAmbient = new Color(0.4f, 0.4f, 0.5f);
    public Color aksamAmbient = new Color(0.3f, 0.2f, 0.15f);
    public Color geceAmbient = new Color(0.05f, 0.05f, 0.1f);

    [Header("Ay Iþýðý")]
    public Color ayIsigiRengi = new Color(0.5f, 0.6f, 1.0f);
    public float ayIsigiYogunlugu = 0.08f;

    [Header("Kar Zaman Çarpanlarý")]
    public float gunduzKarCarpani = 1f;
    public float aksamKarCarpani = 1.5f;
    public float geceKarCarpani = 2.5f;

    [Header("Sistem Referanslarý")]
    public KarTakip karSistemi;
    public RuzgarSistemi ruzgarSistemi;

    private float mevcutSaat;
    private float gunSuresiSaniye;
    private bool uyariVerildi = false;
    private bool olumBasladi = false;
    private SicaklikSistemi sicaklik;

    // Skybox gecis degiskenleri
    private float tintDeger = 1f;
    private bool karartiyor = false;
    private bool aciyor = false;
    private Material bekleyenSkybox;
    private Material aktifSkybox;

    private Color gunduzOrijinalTint;
    private Color aksamOrijinalTint;
    private Color geceOrijinalTint;

    private Material gunduzInstance;
    private Material aksamInstance;
    private Material geceInstance;


    void Start()
    {
        mevcutSaat = baslangicSaati;
        gunSuresiSaniye = gununSuresi * 60f;
        sicaklik = FindObjectOfType<SicaklikSistemi>();

        gunduzInstance = new Material(gunduzSkybox);
        aksamInstance = new Material(aksamSkybox);
        geceInstance = new Material(geceSkybox);

        gunduzOrijinalTint = SkyboxTintAl(gunduzSkybox);
        aksamOrijinalTint = SkyboxTintAl(aksamSkybox);
        geceOrijinalTint = SkyboxTintAl(geceSkybox);

        Color SkyboxTintAl(Material mat)
        {
            if (mat == null) return Color.white;
            string[] properties = { "_Tint", "_SkyTint", "_Color" };
            foreach (string prop in properties)
                if (mat.HasProperty(prop))
                    return mat.GetColor(prop);
            return Color.white;
        }

        aktifSkybox = gunduzSkybox;
        bekleyenSkybox = gunduzSkybox;
        RenderSettings.skybox = gunduzSkybox;
        DynamicGI.UpdateEnvironment();
    }

    void Update()
    {
        ZamaniGuncelle();
        GunesGuncelle();
        SkyboxGuncelle();
        FogGuncelle();
        AmbientGuncelle();
        KarZamanGuncelle();
        UIGuncelle();
        TehlikeKontrol();
    }

    void ZamaniGuncelle()
    {
        float saatlikArtis = 24f / gunSuresiSaniye;
        mevcutSaat += saatlikArtis * Time.deltaTime;
        if (mevcutSaat >= 24f) mevcutSaat = 0f;
    }

    void GunesGuncelle()
    {
        if (gunes == null) return;

        if (mevcutSaat < 6f || mevcutSaat >= 20f)
        {
            gunes.color = ayIsigiRengi;
            gunes.intensity = ayIsigiYogunlugu;
        }
        else if (mevcutSaat >= 6f && mevcutSaat < 8f)
        {
            float oran = (mevcutSaat - 6f) / 2f;
            gunes.color = Color.Lerp(aksamIsigiRengi, gunduzIsigiRengi, oran);
            gunes.intensity = Mathf.Lerp(0.2f, 1f, oran);
        }
        else if (mevcutSaat >= 8f && mevcutSaat < 16f)
        {
            gunes.color = gunduzIsigiRengi;
            gunes.intensity = 1f;
        }
        else if (mevcutSaat >= 16f && mevcutSaat < 20f)
        {
            float oran = (mevcutSaat - 16f) / 4f;
            gunes.color = Color.Lerp(gunduzIsigiRengi, ayIsigiRengi, oran);
            gunes.intensity = Mathf.Lerp(1f, ayIsigiYogunlugu, oran);
        }

        float gunOrani = Mathf.Clamp01((mevcutSaat - 6f) / 14f);
        gunes.transform.rotation = Quaternion.Euler(gunOrani * 180f - 90f, 170f, 0f);
    }

    void SkyboxGuncelle()
    {
        Material hedef = null;

        if (mevcutSaat >= 6f && mevcutSaat < 16f)
            hedef = gunduzInstance;
        else if (mevcutSaat >= 16f && mevcutSaat < 20f)
            hedef = aksamInstance;
        else
            hedef = geceInstance;

        if (hedef != aktifSkybox && hedef != bekleyenSkybox
            && !karartiyor && !aciyor)
        {
            bekleyenSkybox = hedef;
            karartiyor = true;
        }

        // Karar: skybox'i kaart
        if (karartiyor)
        {
            tintDeger -= Time.deltaTime / gecisSuresi;

            if (tintDeger <= 0f)
            {
                tintDeger = 0f;
                karartiyor = false;
                aciyor = true;

                // Skybox'i degistir
                aktifSkybox = bekleyenSkybox;
                RenderSettings.skybox = aktifSkybox;
                DynamicGI.UpdateEnvironment();
            }

            SkyboxTintAyarla(tintDeger);
        }

        // Ac: skybox'i ac
        if (aciyor)
        {
            tintDeger += Time.deltaTime / gecisSuresi;

            if (tintDeger >= 1f)
            {
                tintDeger = 1f;
                aciyor = false;
            }

            SkyboxTintAyarla(tintDeger);
        }
    }

    void SkyboxTintAyarla(float deger)
    {
        if (RenderSettings.skybox == null) return;

        Color orijinal = Color.white;
        if (aktifSkybox == gunduzInstance) orijinal = gunduzOrijinalTint;
        else if (aktifSkybox == aksamInstance) orijinal = aksamOrijinalTint;
        else if (aktifSkybox == geceInstance) orijinal = geceOrijinalTint;

        string[] properties = { "_Tint", "_SkyTint", "_Color" };
        foreach (string prop in properties)
        {
            if (RenderSettings.skybox.HasProperty(prop))
            {
                RenderSettings.skybox.SetColor(prop,
                    new Color(
                        orijinal.r * deger,
                        orijinal.g * deger,
                        orijinal.b * deger,
                        orijinal.a));
                break;
            }
        }

        if (RenderSettings.skybox.HasProperty("_Exposure"))
            RenderSettings.skybox.SetFloat("_Exposure", deger);
    }

    void AmbientGuncelle()
    {
        Color hedefAmbient;

        if (mevcutSaat >= 6f && mevcutSaat < 16f)
            hedefAmbient = gunduzAmbient;
        else if (mevcutSaat >= 16f && mevcutSaat < 20f)
        {
            float oran = (mevcutSaat - 16f) / 4f;
            hedefAmbient = Color.Lerp(gunduzAmbient, geceAmbient, oran);
        }
        else
            hedefAmbient = geceAmbient;

        RenderSettings.ambientLight = Color.Lerp(
            RenderSettings.ambientLight, hedefAmbient, Time.deltaTime * 0.3f);
    }

    void FogGuncelle()
    {
        RenderSettings.fog = true;

        Color hedefFog;
        float hedefYogunluk;

        if (mevcutSaat >= 6f && mevcutSaat < 16f)
        {
            hedefFog = gunduzFogRengi;
            hedefYogunluk = gunduzFogYogunluk;
        }
        else if (mevcutSaat >= 16f && mevcutSaat < 20f)
        {
            float oran = (mevcutSaat - 16f) / 4f;
            hedefFog = Color.Lerp(gunduzFogRengi, geceFogRengi, oran);
            hedefYogunluk = Mathf.Lerp(gunduzFogYogunluk, geceFogYogunluk, oran);
        }
        else
        {
            hedefFog = geceFogRengi;
            hedefYogunluk = geceFogYogunluk;
        }

        RenderSettings.fogColor = Color.Lerp(
            RenderSettings.fogColor, hedefFog, Time.deltaTime * 0.5f);
        RenderSettings.fogDensity = Mathf.Lerp(
            RenderSettings.fogDensity, hedefYogunluk, Time.deltaTime * 0.5f);
    }

    void KarZamanGuncelle()
    {
        float karCarpan;
        float ruzgarCarpan;

        if (mevcutSaat < 6f || mevcutSaat >= 20f)
        {
            karCarpan = geceKarCarpani;
            ruzgarCarpan = 2f;
        }
        else if (mevcutSaat >= 6f && mevcutSaat < 16f)
        {
            karCarpan = gunduzKarCarpani;
            ruzgarCarpan = 1f;
        }
        else
        {
            float oran = (mevcutSaat - 16f) / 4f;
            karCarpan = Mathf.Lerp(gunduzKarCarpani, geceKarCarpani, oran);
            ruzgarCarpan = Mathf.Lerp(1f, 2f, oran);
        }

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
        karartiyor = false;
        aciyor = false;
        tintDeger = 1f;

        if (sicaklik != null)
        {
            sicaklik.geceBonusu = false;
            sicaklik.alacakaranlýkBonusu = false;
        }

        aktifSkybox = gunduzInstance;
        bekleyenSkybox = gunduzInstance;
        RenderSettings.skybox = gunduzInstance;
        SkyboxTintAyarla(1f);
        DynamicGI.UpdateEnvironment();
    }
}