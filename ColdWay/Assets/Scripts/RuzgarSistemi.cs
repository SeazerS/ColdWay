using UnityEngine;
using System.Collections;
using StarterAssets;

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

    [Range(0f, 1f)] public float MaksimumSesSiniri = 0.7f;

    void Start()
    {
        windZone = GetComponent<WindZone>();
        if (windZone == null)
        {
            Debug.LogError("WindZone bulunamadi!");
            return;
        }
        BolgeGuncelle(1);

        StartCoroutine(RuzgarSesiniBaslat());
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

        SesSeviyesiniAyarla();  
    }

    private IEnumerator RuzgarSesiniBaslat()
    {
        yield return new WaitForSeconds(0.2f);

        if (AudioManager.instance != null)
        {
            BolgeGuncelle(1);

            Sounds ruzgarSesi = System.Array.Find(AudioManager.instance.sounds, sound => sound.audioName == "Ruzgar_Sesi");
            if (ruzgarSesi != null && ruzgarSesi.source != null)
            {
                ruzgarSesi.source.volume = ruzgarSesi.originalVolume;
            }

            AudioManager.instance.Play("Ruzgar_Sesi");

            SesSeviyesiniAyarla();
        }
    }

    void SesSeviyesiniAyarla()
    {
        if (AudioManager.instance == null) return;

        Sounds ruzgarSesi = System.Array.Find(AudioManager.instance.sounds, sound => sound.audioName == "Ruzgar_Sesi");

        if (ruzgarSesi != null && ruzgarSesi.source != null)
        {
            float toplamCarpan = aktifBolgeCarpani * aktifZamanCarpani;

            float yeniVolume = ruzgarSesi.originalVolume * toplamCarpan;

            yeniVolume = Mathf.Clamp(yeniVolume, 0f, MaksimumSesSiniri);
            ruzgarSesi.source.volume = yeniVolume;
        }
    }
}
