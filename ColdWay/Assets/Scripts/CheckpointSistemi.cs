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

    [Header("Olum Ekrani")]
    public Image karartmaEkrani; // Canvas'ta tam ekran siyah Image
    public float karartmaSuresi = 1.0f;
    public float beklemeSuresi = 1.5f;
    public float acilmaSuresi = 1.0f;

    private bool olumIsleniyor = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (oyuncu != null)
            sonCheckpointPoz = oyuncu.position;

        // Baslangicta ekran acik
        if (karartmaEkrani != null)
            karartmaEkrani.color = new Color(0, 0, 0, 0);
    }

    public void CheckpointKaydet(Vector3 poz)
    {
        sonCheckpointPoz = poz;
        checkpointVar = true;
        Debug.Log("Checkpoint kaydedildi.");
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

        // Ekrani ac
        if (karartmaEkrani != null)
            karartmaEkrani.color = new Color(0, 0, 0, 0);
    }

    IEnumerator OlumSonrasiDon()
    {
        // 1. Ekrani kaart
        yield return StartCoroutine(EkranKarar(karartmaSuresi));

        // 2. Bekle
        yield return new WaitForSeconds(beklemeSuresi);

        // 3. Oyuncuyu checkpoint'e taþý
        if (oyuncu != null && checkpointVar)
            oyuncu.position = sonCheckpointPoz;

        // 4. Sistemleri sifirla
        sicaklikSistemi?.Oldu_Sifirla();
        enerjiKontrol?.Oldu_Sifirla();

        // 5. Ekrani ac
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
            float alpha = Mathf.Clamp01(t / sure);
            karartmaEkrani.color = new Color(0, 0, 0, alpha);
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
            float alpha = 1f - Mathf.Clamp01(t / sure);
            karartmaEkrani.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        karartmaEkrani.color = new Color(0, 0, 0, 0);
    }
}