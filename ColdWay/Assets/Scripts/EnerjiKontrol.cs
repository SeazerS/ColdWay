using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnerjiKontrol : MonoBehaviour
{
    [Header("Enerji")]
    public float maxEnerji = 100f;
    public float mevcutEnerji = 100f;

    [Header("Düþüþ Hýzlarý - Bölge 1")]
    public float yuruyusDususu_B1 = 0.025f;
    public float kosmaDususu_B1 = 0.08f;

    [Header("Düþüþ Hýzlarý - Bölge 2")]
    public float yuruyusDususu_B2 = 0.05f;
    public float kosmaDususu_B2 = 0.14f;

    [Header("Düþüþ Hýzlarý - Bölge 3")]
    public float yuruyusDususu_B3 = 0.09f;
    public float kosmaDususu_B3 = 0.22f;

    // Aktif düþüþ hýzlarý (bölgeye göre güncellenir)
    private float aktifYuruyusDususu;
    private float aktifKosmaDususu;

    [Header("Sabit Deðerler")]
    public float baltaDususu = 5f;
    public float atesBasiArtisi = 0.333f;
    public float etArtisi = 20f;
    public float konserveArtisi = 35f;

    [Header("UI")]
    public Slider enerjiSlider;
    public Image sliderDolgu;
    private Color normalRenk = new Color(1f, 0.8f, 0f);
    private Color tehlikeRenk = new Color(1f, 0.4f, 0f);
    private Color kritikRenk = new Color(1f, 0.1f, 0.1f);

    // Aktif bölge
    private int mevcutBolge = 1;

    private Player_Controller hareket;
    private bool atesBasinda = false;

    void Start()
    {
        mevcutEnerji = maxEnerji;
        hareket = GetComponent<Player_Controller>();

        // Baþlangýçta bölge 1 deðerleri aktif
        BolgeGuncelle(1);
        UIGuncelle();
    }

    void Update()
    {
        EnerjiGuncelle();
        UIGuncelle();
        KisitlamaKontrol();
    }

    void EnerjiGuncelle()
    {
        if (atesBasinda)
        {
            mevcutEnerji += atesBasiArtisi * Time.deltaTime;
            mevcutEnerji = Mathf.Clamp(mevcutEnerji, 0f, maxEnerji);
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool hareketEdiyor = Mathf.Abs(horizontal) > 0.1f ||
                             Mathf.Abs(vertical) > 0.1f;

        if (!hareketEdiyor) return;

        // Her karede direkt Inspector deðerini oku
        float yuruDusus, kosDusus;
        switch (mevcutBolge)
        {
            case 2: yuruDusus = yuruyusDususu_B2; kosDusus = kosmaDususu_B2; break;
            case 3: yuruDusus = yuruyusDususu_B3; kosDusus = kosmaDususu_B3; break;
            default: yuruDusus = yuruyusDususu_B1; kosDusus = kosmaDususu_B1; break;
        }

        if (hareket != null && hareket.KosuyorMu())
            mevcutEnerji -= kosDusus * Time.deltaTime;
        else
            mevcutEnerji -= yuruDusus * Time.deltaTime;

        mevcutEnerji = Mathf.Clamp(mevcutEnerji, 0f, maxEnerji);
    }

    public void BolgeGuncelle(int bolgeNo)
    {
        mevcutBolge = bolgeNo;
        Debug.Log($"Enerji sistemi Bölge {bolgeNo} için güncellendi.");
    }

    void UIGuncelle()
    {
        if (enerjiSlider != null)
            enerjiSlider.value = mevcutEnerji / maxEnerji;

        if (sliderDolgu != null)
        {
            float oran = mevcutEnerji / maxEnerji;
            if (oran > 0.5f)
                sliderDolgu.color = normalRenk;
            else if (oran > 0.2f)
                sliderDolgu.color = tehlikeRenk;
            else
                sliderDolgu.color = kritikRenk;
        }
    }

    void KisitlamaKontrol()
    {
        if (hareket == null) return;
        float oran = mevcutEnerji / maxEnerji;
        hareket.kosmakAktif = oran >= 0.2f;
    }

    public void AtesAktif(bool durum) { atesBasinda = durum; }
    public void BaltaKullanildi() { mevcutEnerji -= baltaDususu; }
    public void EtYe() { mevcutEnerji = Mathf.Min(mevcutEnerji + etArtisi, maxEnerji); }
    public void KonserveYe() { mevcutEnerji = Mathf.Min(mevcutEnerji + konserveArtisi, maxEnerji); }
}