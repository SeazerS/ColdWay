using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SicaklikSistemi : MonoBehaviour
{
    [Header("Gece Ayarları")]
    public bool geceBonusu = false;
    public float geceDususCarpani = 5f;
    public bool alacakaranlıkBonusu = false;
    public float alacakaranlıkCarpani = 2f;

    [Header("Sıcaklık")]
    public float maxSicaklik = 100f;
    public float mevcutSicaklik = 100f;

    [Header("Düşüş Hızları - Bölgeye Göre")]
    public float bolge1Hiz = 0.033f;  // Normal
    public float bolge2Hiz = 0.055f;  // Orta
    public float bolge3Hiz = 0.120f;  // Çok sert

    [Header("Hava Durumu Çarpanları")]
    // Bölge 1 — hafif rüzgar
    public float ruzgarCarpani_B1 = 1.5f;

    // Bölge 2 — orta rüzgar + gölet ıslaklığı riski
    public float ruzgarCarpani_B2 = 2.5f;
    public float islaklıkCarpani_B2 = 3.0f; // Sadece bölge 2'de

    // Bölge 3 — çok sert rüzgar, ıslaklık yok
    public float ruzgarCarpani_B3 = 4.5f;

    // Aktif çarpanlar
    private float aktifRuzgarCarpani = 1.5f;
    private float aktifIslaklıkCarpani = 1.0f;
    private bool islaklıkAktif = false; // Sadece bölge 2'de true olabilir

    [Header("Durum")]
    public int mevcutBolge = 1;
    public bool atesBasinda = false;
    public bool ayakIslak = false;
    public bool ruzgarda = false;

    [Header("UI")]
    public Slider sicaklikSlider;
    public Image sliderDolgu;

    private Color normalRenk = new Color(0.2f, 0.6f, 1f);
    private Color tehlikeRenk = new Color(1f, 0.5f, 0f);
    private Color kritikRenk = new Color(1f, 0.1f, 0.1f);

    private float dususHizi;
    private bool oldu = false;

    void Start()
    {
        mevcutSicaklik = maxSicaklik;
        BolgeGuncelle(1);
        UIGuncelle();
    }

    void Update()
    {
        if (oldu) return;
        HizBelirle();
        SicaklikGuncelle();
        UIGuncelle();
        PostProcessGuncelle();
        OlumKontrol();
    }

    void HizBelirle()
    {
        switch (mevcutBolge)
        {
            case 1: dususHizi = bolge1Hiz; break;
            case 2: dususHizi = bolge2Hiz; break;
            case 3: dususHizi = bolge3Hiz; break;
        }

        // Rüzgar her bölgede var ama şiddeti farklı
        if (ruzgarda)
            dususHizi *= aktifRuzgarCarpani;

        // Islaklık SADECE bölge 2'de ve gölete girince
        if (ayakIslak && islaklıkAktif)
            dususHizi *= aktifIslaklıkCarpani;

        if (alacakaranlıkBonusu && !geceBonusu)
            dususHizi *= alacakaranlıkCarpani;

        if (geceBonusu)
            dususHizi *= geceDususCarpani;
    }

    void SicaklikGuncelle()
    {
        if (atesBasinda)
            mevcutSicaklik += 0.5f * Time.deltaTime;
        else
            mevcutSicaklik -= dususHizi * Time.deltaTime;

        mevcutSicaklik = Mathf.Clamp(mevcutSicaklik, 0f, maxSicaklik);
    }

    public void BolgeGuncelle(int bolgeNo)
    {
        mevcutBolge = bolgeNo;

        switch (bolgeNo)
        {
            case 1:
                aktifRuzgarCarpani = ruzgarCarpani_B1;
                islaklıkAktif = false; // Bölge 1'de ıslaklık yok
                ayakIslak = false;
                break;
            case 2:
                aktifRuzgarCarpani = ruzgarCarpani_B2;
                aktifIslaklıkCarpani = islaklıkCarpani_B2;
                islaklıkAktif = true; // Gölet var, ıslanabilir
                break;
            case 3:
                aktifRuzgarCarpani = ruzgarCarpani_B3;
                islaklıkAktif = false; // Bölge 3'te ıslaklık yok
                ayakIslak = false;     // Varsa sıfırla
                break;
        }

        Debug.Log($"Sıcaklık sistemi Bölge {bolgeNo} için güncellendi. " +
                  $"Islaklık aktif: {islaklıkAktif}");
    }

    void UIGuncelle()
    {
        if (sicaklikSlider != null)
            sicaklikSlider.value = mevcutSicaklik / maxSicaklik;

        if (sliderDolgu != null)
        {
            float oran = mevcutSicaklik / maxSicaklik;
            if (oran > 0.5f)
                sliderDolgu.color = normalRenk;
            else if (oran > 0.25f)
                sliderDolgu.color = tehlikeRenk;
            else
                sliderDolgu.color = kritikRenk;
        }

        Player_Controller hareket = GetComponent<Player_Controller>();
        if (hareket != null)
        {
            float oran = mevcutSicaklik / maxSicaklik;
            hareket.yuruyusHizi = oran < 0.5f ?
                Mathf.Lerp(0.5f, 2f, oran * 2f) : 2f;
        }
    }

    void PostProcessGuncelle()
    {
        float oran = mevcutSicaklik / maxSicaklik;
        if (oran < 0.25f)
            Debug.Log("KRİTİK — Ekran efekti başlasın");
    }

    void OlumKontrol()
    {
        if (mevcutSicaklik <= 0f)
        {
            oldu = true;
            Debug.Log("ÖLDÜ — Checkpoint'e dön");
        }
    }

    // Gölet trigger'ından çağrılır (sadece bölge 2'de)
    public void GoleteGirdi()
    {
        if (mevcutBolge == 2)
        {
            ayakIslak = true;
            Debug.Log("Ayak ıslandı!");
        }
    }

    // Ateş başında veya kuruduktan sonra çağrılır
    public void Kurudu() { ayakIslak = false; }

    public void AyakIslandi() { if (islaklıkAktif) ayakIslak = true; }
    public void AtesAktif(bool durum) { atesBasinda = durum; }
}
