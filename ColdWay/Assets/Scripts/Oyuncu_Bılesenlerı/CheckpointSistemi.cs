using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CheckpointSistemi : MonoBehaviour
{
    public static CheckpointSistemi Instance;

    [Header("Referanslar")]
    public Transform oyuncu;
    public SicaklikSistemi sicaklikSistemi;
    public EnerjiKontrol enerjiKontrol;

    [Header("Son Checkpoint")]
    public Vector3 sonCheckpointPoz;
    public bool checkpointVar = false;

    [Header("Bolge Baslangic Noktalari")]
    public Transform[] bolgeBaslangicNoktalari; // Inspector'dan ata
    public BolgeYoneticisi bolgeYoneticisi;

    [Header("Olum Ekrani")]
    public Image karartmaEkrani;
    public float karartmaSuresi = 1.0f;
    public float beklemeSuresi = 1.5f;
    public float acilmaSuresi = 1.0f;

    public enum OlumNedeni { Sicaklik, Enerji }
    private OlumNedeni sonOlumNedeni;

    private bool olumIsleniyor = false;
    private float olumAnindakiSicaklik;
    private float olumAnindakiEnerji;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (oyuncu != null)
            sonCheckpointPoz = oyuncu.position;

        if (karartmaEkrani != null)
            karartmaEkrani.color = new Color(0, 0, 0, 0);
    }

    public void CheckpointKaydet(Vector3 poz)
    {
        sonCheckpointPoz = poz;
        checkpointVar = true;
    }

    // Checkpoint yoksa bölge baþlangýç noktasýný döndür
    Vector3 SpawnNoktasiniGetir()
    {
        if (checkpointVar)
            return sonCheckpointPoz;

        // Mevcut bölgenin baþlangýç noktasý
        if (bolgeYoneticisi != null && bolgeBaslangicNoktalari != null)
        {
            int bolge = bolgeYoneticisi.mevcutBolge - 1;
            if (bolge >= 0 && bolge < bolgeBaslangicNoktalari.Length
                && bolgeBaslangicNoktalari[bolge] != null)
                return bolgeBaslangicNoktalari[bolge].position;
        }

        // Hiçbiri yoksa oyuncunun mevcut pozisyonu
        return oyuncu != null ? oyuncu.position : Vector3.zero;
    }

    public void OlumGerceklesti(OlumNedeni neden)
    {
        if (olumIsleniyor) return;
        sonOlumNedeni = neden;

        olumAnindakiSicaklik = sicaklikSistemi != null ?
            sicaklikSistemi.mevcutSicaklik : 50f;
        olumAnindakiEnerji = enerjiKontrol != null ?
            enerjiKontrol.mevcutEnerji : 50f;

        olumIsleniyor = true;
        StartCoroutine(OlumSonrasiDon());
    }

    public void OlumGerceklesti()
    {
        if (olumIsleniyor) return;
        olumIsleniyor = true;
        StartCoroutine(OlumSonrasiDon());
    }

    public void OlumIptal()
    {
        olumIsleniyor = false;
        StopAllCoroutines();
        if (karartmaEkrani != null)
            karartmaEkrani.color = new Color(0, 0, 0, 0);
    }

    IEnumerator OlumSonrasiDon()
    {
        yield return StartCoroutine(EkranKarar(karartmaSuresi));
        yield return new WaitForSeconds(beklemeSuresi);

        // Spawn noktasýný belirle
        Vector3 spawnPoz = SpawnNoktasiniGetir();

        if (oyuncu != null)
        {
            CharacterController cc = oyuncu.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            oyuncu.position = spawnPoz;
            if (cc != null) cc.enabled = true;
        }

        // Sistemleri sýfýrla
        if (sonOlumNedeni == OlumNedeni.Sicaklik)
        {
            sicaklikSistemi?.Oldu_Sifirla();
            if (enerjiKontrol != null)
            {
                enerjiKontrol.OlduFlagSifirla();
                enerjiKontrol.mevcutEnerji = olumAnindakiEnerji;
            }
        }
        else
        {
            enerjiKontrol?.Oldu_Sifirla();
            if (sicaklikSistemi != null)
            {
                sicaklikSistemi.OlduFlagSifirla();
                sicaklikSistemi.mevcutSicaklik = olumAnindakiSicaklik;
            }
        }

        yield return StartCoroutine(EkranAc(acilmaSuresi));
        olumIsleniyor = false;
    }

    public void OlumGerceklestiBolgeDisi()
    {
        if (olumIsleniyor) return;
        olumIsleniyor = true;
        StartCoroutine(BolgeDisiOlumCoroutine());
    }

    IEnumerator BolgeDisiOlumCoroutine()
    {
        yield return StartCoroutine(EkranKarar(karartmaSuresi));
        yield return new WaitForSeconds(beklemeSuresi);

        Vector3 spawnPoz = SpawnNoktasiniGetir();

        if (oyuncu != null)
        {
            CharacterController cc = oyuncu.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            oyuncu.position = spawnPoz;
            if (cc != null) cc.enabled = true;
        }

        sicaklikSistemi?.OlduFlagSifirla();
        enerjiKontrol?.OlduFlagSifirla();

        yield return StartCoroutine(EkranAc(acilmaSuresi));
        olumIsleniyor = false;
    }

    IEnumerator EkranKarar(float sure)
    {
        if (karartmaEkrani == null) yield break;
        float t = 0f;
        while (t < sure)
        {
            t += Time.deltaTime;
            karartmaEkrani.color = new Color(0, 0, 0, Mathf.Clamp01(t / sure));
            yield return null;
        }
        karartmaEkrani.color = new Color(0, 0, 0, 1);
    }

    IEnumerator EkranAc(float sure)
    {
        if (karartmaEkrani == null) yield break;
        float t = 0f;
        while (t < sure)
        {
            t += Time.deltaTime;
            karartmaEkrani.color = new Color(0, 0, 0, 1f - Mathf.Clamp01(t / sure));
            yield return null;
        }
        karartmaEkrani.color = new Color(0, 0, 0, 0);
    }
}