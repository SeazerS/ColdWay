using UnityEngine;

public class GecGunduzSistemi : MonoBehaviour
{
    [Header("Skybox")]
    public Material gunduzSkybox;
    public Material geceSkybox;

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

    [Header("Ay Iþýðý")]
    public Color ayIsigiRengi = new Color(0.5f, 0.6f, 1.0f);
    public float ayIsigiYogunlugu = 0.08f;

    [Header("Kar Zaman Çarpanlarý")]
    public float gunduzKarCarpani = 1f;    // Bölge 1 ayarý korunur
    public float aksamKarCarpani = 1.5f;   // Akþam biraz artar
    public float geceKarCarpani = 2.5f;    // Gece çok yoðun

    [Header("Sistem Referanslarý")]
    public KarTakip karSistemi;         // Inspector'dan sürükle
    public RuzgarSistemi ruzgarSistemi; // Inspector'dan sürükle

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
        RenderSettings.skybox = gunduzSkybox;
    }

    void Update()
    {
        ZamaniGuncelle();
        GunesGuncelle();
        SkyboxGuncelle();
        FogGuncelle();
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

        bool geceVakti = mevcutSaat < 6f || mevcutSaat >= 20f;

        if (geceVakti)
        {
            gunes.color = ayIsigiRengi;
            gunes.intensity = ayIsigiYogunlugu;
            return;
        }

        if (mevcutSaat >= 6f && mevcutSaat < 8f)
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
        bool geceVakti = mevcutSaat < 6f || mevcutSaat >= 20f;

        if (geceVakti)
        {
            RenderSettings.skybox = geceSkybox;
            DynamicGI.UpdateEnvironment();
            return;
        }

        if (mevcutSaat >= 17f && mevcutSaat < 20f)
        {
            float oran = (mevcutSaat - 17f) / 3f;
            RenderSettings.skybox = oran > 0.5f ? geceSkybox : gunduzSkybox;
        }
        else
        {
            RenderSettings.skybox = gunduzSkybox;
        }
        DynamicGI.UpdateEnvironment();
    }

    // Kar zaman çarpanýný KarTakip'e ilet
    // KarTakip bölge çarpanýyla birleþtirir
    void KarZamanGuncelle()
    {
        bool geceVakti = mevcutSaat < 6f || mevcutSaat >= 20f;

        float karCarpan;
        float ruzgarCarpan;

        if (geceVakti)
        {
            karCarpan = geceKarCarpani;
            ruzgarCarpan = 2f;
        }
        else if (mevcutSaat >= 6f && mevcutSaat < 16f)
        {
            karCarpan = gunduzKarCarpani;
            ruzgarCarpan = 1f;
        }
        else // 16:00 - 20:00 arasý geçiþ
        {
            float oran = (mevcutSaat - 16f) / 4f;
            karCarpan = Mathf.Lerp(gunduzKarCarpani, geceKarCarpani, oran);
            ruzgarCarpan = Mathf.Lerp(1f, 2f, oran);
        }

        karSistemi?.ZamanCarpaniGuncelle(karCarpan);
        ruzgarSistemi?.ZamanCarpaniGuncelle(ruzgarCarpan);
    }

    void FogGuncelle()
    {
        RenderSettings.fog = true;
        bool geceVakti = mevcutSaat < 6f || mevcutSaat >= 20f;

        if (geceVakti)
        {
            RenderSettings.fogColor = geceFogRengi;
            RenderSettings.fogDensity = geceFogYogunluk;
            return;
        }

        if (mevcutSaat >= 6f && mevcutSaat < 16f)
        {
            RenderSettings.fogColor = gunduzFogRengi;
            RenderSettings.fogDensity = gunduzFogYogunluk;
        }
        else if (mevcutSaat >= 16f && mevcutSaat < 20f)
        {
            float oran = (mevcutSaat - 16f) / 4f;
            RenderSettings.fogColor = Color.Lerp(gunduzFogRengi, geceFogRengi, oran);
            RenderSettings.fogDensity = Mathf.Lerp(gunduzFogYogunluk, geceFogYogunluk, oran);
        }
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
            if (sicaklik != null)
            {
                sicaklik.alacakaranlýkBonusu = true;
                Debug.Log("Alacakaranlýk aktif.");
            }
        }

        if (mevcutSaat >= 20f && !olumBasladi)
        {
            olumBasladi = true;
            if (sicaklik != null)
            {
                sicaklik.geceBonusu = true;
                Debug.Log("Gece aktif.");
            }
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
        DynamicGI.UpdateEnvironment();
    }
}
