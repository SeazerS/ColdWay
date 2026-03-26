using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnerjiKontrol : MonoBehaviour
{
    [Header("Enerji")]
    public float maxEnerji = 100f;
    public float mevcutEnerji = 100f;

    [Header("Düþüþ Hýzlarý")]
    public float yuruyusDususu = 0.025f;
    public float kosmaDususu = 0.08f;
    public float baltaDususu = 5f;

    [Header("Artýþ Hýzlarý")]
    public float atesBasiArtisi = 0.333f;
    public float etArtisi = 20f;
    public float konserveArtisi = 35f;

    [Header("UI")]
    public Slider enerjiSlider;
    public Image sliderDolgu;

    private Color normalRenk = new Color(1f, 0.8f, 0f);
    private Color tehlikeRenk = new Color(1f, 0.4f, 0f);
    private Color kritikRenk = new Color(1f, 0.1f, 0.1f);

    private Player_Controller hareket;
    private bool atesBasinda = false;

    void Start()
    {
        mevcutEnerji = maxEnerji;
        hareket = GetComponent<Player_Controller>();
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
            return; // Ateþ baþýndaysa düþme
        }

        // Karakter hareket ediyor mu kontrol et
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool hareketEdiyor = Mathf.Abs(horizontal) > 0.1f ||
                             Mathf.Abs(vertical) > 0.1f;

        if (!hareketEdiyor) return; // Duruyorsa düþme

        // Koþuyor mu?
        if (hareket != null && hareket.KosuyorMu())
            mevcutEnerji -= kosmaDususu * Time.deltaTime;
        else
            mevcutEnerji -= yuruyusDususu * Time.deltaTime;

        mevcutEnerji = Mathf.Clamp(mevcutEnerji, 0f, maxEnerji);
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

        // %20 altýnda koþma kapanýr
        if (oran < 0.2f)
            hareket.kosmakAktif = false;
        else
            hareket.kosmakAktif = true;
    }

    public void AtesAktif(bool durum) { atesBasinda = durum; }
    public void BaltaKullanildi() { mevcutEnerji -= baltaDususu; }
    public void EtYe() { mevcutEnerji += etArtisi; }
    public void KonserveYe() { mevcutEnerji += konserveArtisi; }
}
