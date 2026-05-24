using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProsses : MonoBehaviour
{
    public static PostProsses Instance;

    private Volume globalVolume;

    // Efektlerin referanslarý
    private Vignette vignette;
    private DepthOfField depthOfField;
    private ColorAdjustments colorAdjustments;

    // Durum takipleri (Çakýþmayý önlemek için)
    private float mevcutSicaklikOrani = 1f;
    private float mevcutEnerjiOrani = 1f;

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
        // Efektleri tek bir merkezden (LateUpdate) yöneterek çakýþmalarý engelliyoruz
        EfektleriUygula();
    }

    void EfektleriUygula()
    {
        // ------------------ 1. SICAKLIK (DONMA) EFEKTÝ ------------------
        float donmaEtkiGucu = 0f;
        float sabitMaviVignette = 0f;

        if (mevcutSicaklikOrani < 0.25f)
        {
            donmaEtkiGucu = 1f - (mevcutSicaklikOrani / 0.25f);
            // Sýcaklýk düþtükçe SABÝT olarak artan mavi çerçeve (Maksimum 0.45f)
            sabitMaviVignette = Mathf.Lerp(0f, 0.45f, donmaEtkiGucu);
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.active = donmaEtkiGucu > 0f;
            colorAdjustments.saturation.Override(Mathf.Lerp(0f, -50f, donmaEtkiGucu));
        }

        // ------------------ 2. ENERJÝ (YORGUNLUK) EFEKTÝ ------------------
        float yorgunlukEtkiGucu = 0f;
        float yanipSonenSiyahVignette = 0f;

        if (mevcutEnerjiOrani < 0.20f)
        {
            yorgunlukEtkiGucu = 1f - (mevcutEnerjiOrani / 0.20f);
            // Sadece enerjiye baðlý YANIP SÖNEN çerçeve deðeri
            float gozKirpma = Mathf.PingPong(Time.time * 1.2f, 0.2f) * yorgunlukEtkiGucu;
            yanipSonenSiyahVignette = 0.15f + gozKirpma;
        }

        if (depthOfField != null)
        {
            depthOfField.active = yorgunlukEtkiGucu > 0f;
            depthOfField.focusDistance.Override(Mathf.Lerp(10f, 0.1f, yorgunlukEtkiGucu));
        }

        // ------------------ 3. VIGNETTE BÝRLEÞTÝRME (ORTAK ALAN) ------------------
        if (vignette != null)
        {
            if (donmaEtkiGucu > 0f || yorgunlukEtkiGucu > 0f)
            {
                vignette.active = true;

                // Kritik Çözüm: Ýki etkiden hangisi o an daha güçlü (büyükse) ekrana onu basýyoruz
                // Böylece sýcaklýk düþtüðünde sabit mavi durur, enerji daha çok düþerse siyah yanýp sönme baskýn gelir
                float nihaiIntensity = Mathf.Max(sabitMaviVignette, yanipSonenSiyahVignette);
                vignette.intensity.Override(Mathf.Clamp(nihaiIntensity, 0f, 0.55f));

                // Renk Ayarý: Donma varsa net maviye boya, donma yoksa sadece yorgunluk varsa siyah yap
                if (donmaEtkiGucu > 0f)
                {
                    Color donmaMavisi = new Color(0f, 0.4f, 0.8f); // Canlý donma mavisi
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
}