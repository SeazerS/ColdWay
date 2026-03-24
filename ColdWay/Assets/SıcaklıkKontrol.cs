using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SicaklikSistemi : MonoBehaviour
{
    [Header("Sıcaklık")]
    public float maxSicaklik = 100f;
    public float mevcutSicaklik = 100f;

    [Header("Düşüş Hızları")]
    public float bolge1Hiz = 0.033f;
    public float bolge2Hiz = 0.043f;
    public float bolge3Hiz = 0.080f;
    public float bolge4Hiz = 0.150f;

    [Header("Durum")]
    public int mevcutBolge = 1;
    public bool atesBasinda = false;
    public bool ayakIslak = false;
    public bool ruzgarda = false;

    [Header("UI")]
    public Slider sicaklikSlider;
    public Image sliderDolgu;

    // Renkler
    private Color normalRenk = new Color(0.2f, 0.6f, 1f);
    private Color tehlikeRenk = new Color(1f, 0.5f, 0f);
    private Color kritikRenk = new Color(1f, 0.1f, 0.1f);

    private float dususHizi;
    private bool oldu = false;

    void Start()
    {
        mevcutSicaklik = maxSicaklik;
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
            case 4: dususHizi = bolge4Hiz; break;
        }

        if (ayakIslak) dususHizi *= 2f;
        if (ruzgarda) dususHizi *= 1.5f;
    }

    void SicaklikGuncelle()
    {
        if (atesBasinda)
            mevcutSicaklik += 0.5f * Time.deltaTime;
        else
            mevcutSicaklik -= dususHizi * Time.deltaTime;

        mevcutSicaklik = Mathf.Clamp(mevcutSicaklik, 0f, maxSicaklik);
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

        // Hareket yavaşlaması
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

        // Vignette artışı
        // Volume profile üzerinden ayarlanacak
        // Şimdilik Debug ile test edelim
        if (oran < 0.25f)
            Debug.Log("KRİTİK — Ekran efekti başlasın");
    }

    void OlumKontrol()
    {
        if (mevcutSicaklik <= 0f)
        {
            oldu = true;
            Debug.Log("ÖLDÜ — Checkpoint'e dön");
            // CheckpointManager.Instance.Don();
        }
    }

    public void AyakIslandi() { ayakIslak = true; }
    public void AtesAktif(bool durum) { atesBasinda = durum; }
    public void BolgeGecis(int bolge) { mevcutBolge = bolge; }
}
