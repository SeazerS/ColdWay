using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProsses : MonoBehaviour
{
    public static PostProsses Instance;

    private Volume globalVolume;

    private Vignette vignette;
    private DepthOfField depthOfField;
    private ColorAdjustments colorAdjustments;

    private float mevcutSicaklikOrani = 1f;
    private float mevcutEnerjiOrani = 1f;

    [Header("Buzlanma UI Ayarlarý")]

    public Image[] buzDokulari;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        globalVolume = GetComponent<Volume>();

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out depthOfField);
            globalVolume.profile.TryGet(out colorAdjustments);
        }
        else
        {
            Debug.LogError("Global Volume veya Profil script tarafýndan bulunamadý!");
        }
    }

    public void SicaklikEfektiGuncelle(float sicaklikOrani)
    {
        mevcutSicaklikOrani = sicaklikOrani;
    }

    public void EnerjiEfektiGuncelle(float enerjiOrani)
    {
        mevcutEnerjiOrani = enerjiOrani;
    }

    void LateUpdate()
    {
        EfektleriUygula();
        BuzGorselleriniYonet(); 
    }

    void EfektleriUygula()
    {
        float donmaEtkiGucu = 0f;
        float sabitMaviVignette = 0f;

        if (mevcutSicaklikOrani < 0.50f)
        {
            donmaEtkiGucu = 1f - (mevcutSicaklikOrani / 0.50f);
            sabitMaviVignette = Mathf.Lerp(0f, 0.55f, donmaEtkiGucu);
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.active = donmaEtkiGucu > 0f;
            colorAdjustments.saturation.Override(Mathf.Lerp(0f, -40f, donmaEtkiGucu));
        }

        float yorgunlukEtkiGucu = 0f;
        float yanipSonenSiyahVignette = 0f;

        if (mevcutEnerjiOrani < 0.20f)
        {
            yorgunlukEtkiGucu = 1f - (mevcutEnerjiOrani / 0.20f);
            float gozKirpma = Mathf.PingPong(Time.time * 1.2f, 0.2f) * yorgunlukEtkiGucu;
            yanipSonenSiyahVignette = 0.15f + gozKirpma;
        }

        if (depthOfField != null)
        {
            depthOfField.active = yorgunlukEtkiGucu > 0f;
            depthOfField.focusDistance.Override(Mathf.Lerp(10f, 0.1f, yorgunlukEtkiGucu));
        }

        if (vignette != null)
        {
            if (donmaEtkiGucu > 0f || yorgunlukEtkiGucu > 0f)
            {
                vignette.active = true;
                float nihaiIntensity = Mathf.Max(sabitMaviVignette, yanipSonenSiyahVignette);
                vignette.intensity.Override(Mathf.Clamp(nihaiIntensity, 0f, 0.60f));

                if (donmaEtkiGucu > 0f)
                {
                    Color donmaMavisi = new Color(0.0f, 0.2f, 0.4f); 
                    vignette.color.Override(Color.Lerp(Color.black, donmaMavisi, donmaEtkiGucu));
                }
                else
                {
                    vignette.color.Override(Color.black);
                }
            }
            else
            {
                vignette.intensity.Override(0f);
                vignette.active = false;
            }
        }
    }

    void BuzGorselleriniYonet()
    {
        if (buzDokulari == null || buzDokulari.Length == 0) return;

        int gorselSayisi = buzDokulari.Length;

        float donmaIlerlemesi = Mathf.InverseLerp(0.40f, 0f, mevcutSicaklikOrani);

        for (int i = 0; i < gorselSayisi; i++)
        {
            float altSinir = (float)i / gorselSayisi;
            float ustSinir = (float)(i + 1) / gorselSayisi;

            float döküOpaklik = Mathf.InverseLerp(altSinir, ustSinir, donmaIlerlemesi);

            Color c = buzDokulari[i].color;
            c.a = döküOpaklik;
            buzDokulari[i].color = c;
        }
    }
}