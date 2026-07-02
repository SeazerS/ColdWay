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

    private float bolgeDisiEtkiGucu = 0f;
    private float bolgeDisiHedef = 0f;

    // Buz efekti
    private bool buzEfektiAktif = false;
    private float buzEfektiMevcut = 0f;
    private float buzEfektiHedef = 0f;
    public float buzEfektiGelisHizi = 0.3f;
    public float buzEfektiGidisHizi = 0.1f;

    // Fýrtýna efekti
    private float firtinYogunluk = 0f;

    [Header("Buzlanma UI Ayarlari")]
    public Image[] buzDokulari;
    [Range(0f, 1f)] private float maxBuzOpaklik = 0.4f;
    [Range(0f, 1f)] public float index8MaxOpaklik = 0.4f;

    [Header("UI Elemanlar")]
    public CanvasGroup[] uiElemanlar; // barlar, hotbar vs
    public float minUIOpaklik = 0.8f; // kritik noktada minimum opaklýk

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
            Debug.LogError("Global Volume veya Profil bulunamadi!");
        }
    }

    public void BuzEfektiBaslat()
    {
        buzEfektiAktif = true;
        buzEfektiHedef = 0.55f;
    }

    public void BuzEfektiKapat()
    {
        buzEfektiHedef = 0f;
    }

    public void FirtinaEfektiGuncelle(float yogunluk)
    {
        firtinYogunluk = yogunluk;
    }

    public void BolgeDisiEfektAc(float yogunluk)
    {
        bolgeDisiHedef = Mathf.Clamp01(yogunluk);
    }

    public void BolgeDisiEfektKapat()
    {
        bolgeDisiHedef = 0f;
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
        UIOpakliginiYonet();
    }

    void UIOpakliginiYonet()
    {
        if (uiElemanlar == null || uiElemanlar.Length == 0) return;

        float donmaIlerlemesi = Mathf.InverseLerp(
            0.30f, 0f, mevcutSicaklikOrani);

        float hedefOpaklik = Mathf.Lerp(1f, minUIOpaklik, donmaIlerlemesi);

        foreach (CanvasGroup cg in uiElemanlar)
        {
            if (cg != null)
                cg.alpha = Mathf.Lerp(cg.alpha, hedefOpaklik, Time.deltaTime * 2f);
        }
    }

    void EfektleriUygula()
    {
        float donmaEtkiGucu = 0f;
        float sabitMaviVignette = 0f;

        if (mevcutSicaklikOrani < 0.50f)
        {
            donmaEtkiGucu = 1f - (mevcutSicaklikOrani / 0.50f);
            sabitMaviVignette = Mathf.Lerp(0f, 0.25f, donmaEtkiGucu);
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.active = donmaEtkiGucu > 0f || firtinYogunluk > 0f;
            float saturation = Mathf.Min(
                Mathf.Lerp(0f, -40f, donmaEtkiGucu),
                Mathf.Lerp(0f, -25f, firtinYogunluk));
            colorAdjustments.saturation.Override(saturation);
        }

        float yorgunlukEtkiGucu = 0f;
        float yanipSonenSiyahVignette = 0f;

        if (mevcutEnerjiOrani < 0.20f)
        {
            yorgunlukEtkiGucu = 1f - (mevcutEnerjiOrani / 0.20f);
            float gozKirpma = Mathf.PingPong(Time.time * 1.2f, 0.2f)
                              * yorgunlukEtkiGucu;
            yanipSonenSiyahVignette = 0.15f + gozKirpma;
        }

        if (depthOfField != null)
        {
            bool depthAktif = yorgunlukEtkiGucu > 0f || firtinYogunluk > 0.3f;
            depthOfField.active = depthAktif;
            if (depthAktif)
            {
                float yorgunlukFocus = Mathf.Lerp(10f, 0.1f, yorgunlukEtkiGucu);
                float firtinFocus = Mathf.Lerp(10f, 2f, firtinYogunluk);
                depthOfField.focusDistance.Override(
                    Mathf.Min(yorgunlukFocus, firtinFocus));
            }
        }

        // Buz efekti
        float buzVignette = 0f;
        if (buzEfektiAktif)
        {
            float hiz = buzEfektiMevcut < buzEfektiHedef
                ? buzEfektiGelisHizi
                : buzEfektiGidisHizi;

            buzEfektiMevcut = Mathf.MoveTowards(
                buzEfektiMevcut, buzEfektiHedef, Time.deltaTime * hiz);

            buzVignette = buzEfektiMevcut;

            if (buzEfektiMevcut <= 0.01f && buzEfektiHedef <= 0f)
                buzEfektiAktif = false;
        }

        // Fýrtýna vignette — gri/koyu
        float firtinVignette = Mathf.Lerp(0f, 0.4f, firtinYogunluk);

        if (vignette != null)
        {
            float nihaiIntensity = Mathf.Max(
                sabitMaviVignette,
                yanipSonenSiyahVignette,
                buzVignette,
                firtinVignette);

            bool herhangiEfekt = donmaEtkiGucu > 0f
                || yorgunlukEtkiGucu > 0f
                || buzEfektiAktif
                || firtinYogunluk > 0.01f;

            if (herhangiEfekt)
            {
                vignette.active = true;
                vignette.smoothness.Override(0.8f);
                vignette.intensity.Override(
                    Mathf.Clamp(nihaiIntensity, 0f, 0.35f));

                if (buzEfektiAktif && buzVignette > 0f)
                    vignette.color.Override(new Color(0.2f, 0.5f, 0.9f));
                else if (firtinYogunluk > 0.01f)
                    vignette.color.Override(
                        Color.Lerp(Color.black,
                        new Color(0.15f, 0.15f, 0.2f), firtinYogunluk));
                else if (donmaEtkiGucu > 0f)
                {
                    Color donmaMavisi = new Color(0.0f, 0.2f, 0.4f);
                    vignette.color.Override(
                        Color.Lerp(Color.black, donmaMavisi, donmaEtkiGucu));
                }
                else
                    vignette.color.Override(Color.black);
            }
            else
            {
                vignette.intensity.Override(0f);
                vignette.active = false;
            }
        }

        // Bölge dýþý efekti
        bolgeDisiEtkiGucu = Mathf.Lerp(
            bolgeDisiEtkiGucu, bolgeDisiHedef, Time.deltaTime * 3f);

        if (bolgeDisiEtkiGucu > 0.01f)
        {
            float pulse = Mathf.Abs(Mathf.Sin(Time.time * 2.5f));
            float vignetteYogunluk = Mathf.Lerp(0.3f, 0.65f, pulse)
                                     * bolgeDisiEtkiGucu;

            if (vignette != null)
            {
                vignette.active = true;
                float mevcutYogunluk = vignette.intensity.value;
                vignette.intensity.Override(
                    Mathf.Max(mevcutYogunluk, vignetteYogunluk));
                vignette.color.Override(
                    Color.Lerp(vignette.color.value,
                    new Color(0.6f, 0f, 0f), bolgeDisiEtkiGucu));
            }

            if (colorAdjustments != null)
            {
                colorAdjustments.active = true;
                colorAdjustments.saturation.Override(
                    Mathf.Lerp(0f, -30f, bolgeDisiEtkiGucu));
            }

            if (depthOfField != null)
            {
                depthOfField.active = true;
                depthOfField.focusDistance.Override(
                    Mathf.Lerp(10f, 1.5f, bolgeDisiEtkiGucu * 0.5f));
            }
        }
    }

    void BuzGorselleriniYonet()
    {
        if (buzDokulari == null || buzDokulari.Length == 0) return;

        int gorselSayisi = buzDokulari.Length;
        float donmaIlerlemesi = Mathf.InverseLerp(
            0.30f, 0f, mevcutSicaklikOrani);

        for (int i = 0; i < gorselSayisi; i++)
        {
            float altSinir = (float)i / gorselSayisi;
            float ustSinir = (float)(i + 1) / gorselSayisi;
            float dokuOpaklik = Mathf.InverseLerp(
                altSinir, ustSinir, donmaIlerlemesi);

            if (i == 8)
                dokuOpaklik = Mathf.Clamp(dokuOpaklik, 0f, index8MaxOpaklik);
            else
                dokuOpaklik = Mathf.Clamp(dokuOpaklik, 0f, maxBuzOpaklik);

            Color c = buzDokulari[i].color;
            c.a = dokuOpaklik;
            buzDokulari[i].color = c;
        }
    }
}