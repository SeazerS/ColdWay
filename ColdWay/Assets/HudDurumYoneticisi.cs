using UnityEngine;
using UnityEngine.UI;

public class HudDurumYoneticisi : MonoBehaviour
{
    [Header("Diamond Icon Image")]
    public Image durumIkonu;

    [Header("Ýkonlar")]
    public Sprite atesIcon;      // ateþ yanýnda
    public Sprite karIcon;       // düþük ýsý
    public Sprite aclikIcon;     // düþük açlýk
    public Sprite ruzgarIcon;    // rüzgar
    public Sprite islaklýkIcon;  // ýslak
    public Sprite enerjIcon;     // düþük enerji
    public Sprite olumIcon;      // ölü

    [Header("Sistem Referanslarý")]
    public SicaklikSistemi sicaklikSistemi;
    public EnerjiKontrol enerjiyoneticisi;
    // EnerjiSistemi, AclikSistemi vs. referanslarý buraya

    void Update()
    {
        DurumuGuncelle();
    }

    void DurumuGuncelle()
    {
        Sprite secilenIkon = null;

        if (sicaklikSistemi.mevcutSicaklik <= 0f)
            secilenIkon = olumIcon;
        else if (sicaklikSistemi.atesBasinda)
            secilenIkon = atesIcon;
        else if (sicaklikSistemi.mevcutSicaklik < 30f)
            secilenIkon = karIcon;
        else if (sicaklikSistemi.ayakIslak)
            secilenIkon = islaklýkIcon;
        else if (sicaklikSistemi.ruzgarda)
            secilenIkon = ruzgarIcon;
        else if (enerjiyoneticisi.mevcutEnerji < 30f)
            secilenIkon = enerjIcon;

        if (secilenIkon != null)
        {
            durumIkonu.sprite = secilenIkon;
            durumIkonu.color = Color.white; // görünür yap
        }
        else
        {
            durumIkonu.sprite = null;
            durumIkonu.color = new Color(0, 0, 0, 0); // tamamen þeffaf
        }
    }
}
