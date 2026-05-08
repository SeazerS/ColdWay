using UnityEngine;

public class RuzgarSistemi : MonoBehaviour
{
    private WindZone windZone;

    // Senin orijinal bolge 1 ayarlarin
    private float orijinalMain = 10f;
    private float orijinalTurbulence = 5f;
    private float orijinalPulseMag = 3f;
    private float orijinalPulseFreq = 0.3f;

    [Header("Bolge Carpanlari")]
    public float bolge2Carpani = 2f;
    public float bolge3Carpani = 4f;

    [Header("Zaman Carpanlari")]
    public float oglenCarpani = 1f;
    public float aksamCarpani = 1.5f;
    public float geceCarpani = 2f;

    private float aktifBolgeCarpani = 1f;
    private float aktifZamanCarpani = 1f;

    void Start()
    {
        windZone = GetComponent<WindZone>();
        if (windZone == null)
        {
            Debug.LogError("WindZone bulunamadi!");
            return;
        }
        BolgeGuncelle(1);
    }

    public void BolgeGuncelle(int bolgeNo)
    {
        switch (bolgeNo)
        {
            case 1: aktifBolgeCarpani = 1f; break;
            case 2: aktifBolgeCarpani = bolge2Carpani; break;
            case 3: aktifBolgeCarpani = bolge3Carpani; break;
        }
        RuzgarUygula();
        Debug.Log("Ruzgar sistemi Bolge " + bolgeNo + " icin guncellendi.");
    }

    public void ZamanCarpaniGuncelle(float carpan)
    {
        aktifZamanCarpani = carpan;
        RuzgarUygula();
    }

    void RuzgarUygula()
    {
        if (windZone == null) return;
        float carpan = aktifBolgeCarpani * aktifZamanCarpani;
        windZone.windMain = orijinalMain * carpan;
        windZone.windTurbulence = orijinalTurbulence * carpan;
        windZone.windPulseMagnitude = orijinalPulseMag * carpan;
        windZone.windPulseFrequency = orijinalPulseFreq;
    }
}
