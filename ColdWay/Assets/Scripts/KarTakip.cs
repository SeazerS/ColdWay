using UnityEngine;

public class KarTakip : MonoBehaviour
{
    public Transform karakter;
    private ParticleSystem karPartikul;

    // Bölge 1'deki senin orijinal ayarlarýn
    private float orijinalEmission;
    private float orijinalHiz;

    // Aktif çarpanlar
    private float bolgeEmissionCarpani = 1f;
    private float bolgeHizCarpani = 1f;
    private float zamanEmissionCarpani = 1f;

    [Header("Bölge Emission Çarpanlarý")]
    public float bolge2EmissionCarpani = 2.5f;
    public float bolge3EmissionCarpani = 6f;

    [Header("Bölge Hýz Çarpanlarý")]
    public float bolge2HizCarpani = 2f;
    public float bolge3HizCarpani = 4f;

    [Header("Firtina")]
    public float firtinaCarpani = 1f;

    void Start()
    {
        karPartikul = GetComponent<ParticleSystem>();
        if (karakter == null) return;

        // Senin bölge 1 ayarlarýný kaydet
        orijinalEmission = karPartikul.emission.rateOverTime.constant;
        orijinalHiz = karPartikul.main.startSpeed.constant;

        KonumGuncelle();
        karPartikul.Simulate(5f, true, true);
        karPartikul.Play();
    }

    void LateUpdate()
    {
        if (karakter == null) return;
        KonumGuncelle();
        transform.rotation = Quaternion.identity;
    }

    void KonumGuncelle()
    {
        transform.position = new Vector3(
            karakter.position.x,
            karakter.position.y + 15f,
            karakter.position.z);
    }

    // BolgeYoneticisi tarafýndan çaðrýlýr
    public void BolgeGuncelle(int bolgeNo)
    {
        switch (bolgeNo)
        {
            case 1:
                bolgeEmissionCarpani = 1f;
                bolgeHizCarpani = 1f;
                break;
            case 2:
                bolgeEmissionCarpani = bolge2EmissionCarpani;
                bolgeHizCarpani = bolge2HizCarpani;
                break;
            case 3:
                bolgeEmissionCarpani = bolge3EmissionCarpani;
                bolgeHizCarpani = bolge3HizCarpani;
                break;
        }

        EmissionUygula();
        Debug.Log($"Kar sistemi Bölge {bolgeNo} için güncellendi.");
    }

    // GecGunduzSistemi tarafýndan çaðrýlýr
    public void ZamanCarpaniGuncelle(float zamanCarpani)
    {
        zamanEmissionCarpani = zamanCarpani;
        EmissionUygula();
    }

    // Bölge × Zaman çarpanýný uygula
    void EmissionUygula()
    {
        if (karPartikul == null) return;
        var emission = karPartikul.emission;
        var mainModule = karPartikul.main;
        emission.rateOverTime = orijinalEmission * bolgeEmissionCarpani
                              * zamanEmissionCarpani * firtinaCarpani;
        mainModule.startSpeed = orijinalHiz * bolgeHizCarpani * firtinaCarpani;
    }

    public float ZamanCarpaniniAl() { return zamanEmissionCarpani; }
}

