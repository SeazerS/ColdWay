using System.Collections;
using UnityEngine;
using TMPro;

public class BolgeSinirSistemi : MonoBehaviour
{
    [Header("Sayaç")]
    public float geriBosalmaSuresi = 10f;

    [Header("UI")]
    public GameObject uyariPanel;
    public TextMeshProUGUI sayacText;
    public TextMeshProUGUI uyariText;

    [Header("Referanslar")]
    public CheckpointSistemi checkpointSistemi;

    private bool bolgeDisinda = false;
    private bool olumSonrasiKoruma = false;
    private float kalanSure;
    private Coroutine sayacCoroutine;
    private Coroutine efektCoroutine;
    private Vector3 sonGuvenliPozisyon;
    private Transform oyuncu;
    private Collider bolgeCollider;

    void Start()
    {
        if (checkpointSistemi == null)
            checkpointSistemi = CheckpointSistemi.Instance;

        if (uyariPanel != null) uyariPanel.SetActive(false);

        bolgeCollider = GetComponent<Collider>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            oyuncu = p.transform;
            sonGuvenliPozisyon = oyuncu.position;
        }
    }

    void Update()
    {
        if (oyuncu == null) return;

        // Önce içeride mi dýþarýda mý kontrol et
        bool icerde = bolgeCollider != null
            ? bolgeCollider.bounds.Contains(oyuncu.position)
            : !bolgeDisinda;

        // Sadece içerideyken pozisyon kaydet
        if (icerde)
            sonGuvenliPozisyon = oyuncu.position;

        // Durum deðiþikliklerini iþle
        if (!icerde && !bolgeDisinda && !olumSonrasiKoruma)
            BolgedenCikti();
        else if (icerde && bolgeDisinda)
            BolgeyeDondu();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        BolgedenCikti();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        BolgeyeDondu();
    }

    void BolgedenCikti()
    {
        if (bolgeDisinda) return;
        bolgeDisinda = true;
        kalanSure = geriBosalmaSuresi;

        if (uyariPanel != null) uyariPanel.SetActive(true);
        if (uyariText != null) uyariText.text = "BÖLGEYE DÖN!";

        if (sayacCoroutine != null) StopCoroutine(sayacCoroutine);
        if (efektCoroutine != null) StopCoroutine(efektCoroutine);

        sayacCoroutine = StartCoroutine(SayacBaslat());
        efektCoroutine = StartCoroutine(EfektGuncelle());
    }

    void BolgeyeDondu()
    {
        if (!bolgeDisinda) return;
        bolgeDisinda = false;

        if (sayacCoroutine != null) StopCoroutine(sayacCoroutine);
        if (efektCoroutine != null) StopCoroutine(efektCoroutine);

        if (uyariPanel != null) uyariPanel.SetActive(false);
        PostProsses.Instance?.BolgeDisiEfektKapat();
    }

    IEnumerator SayacBaslat()
    {
        kalanSure = geriBosalmaSuresi;

        while (kalanSure > 0f)
        {
            kalanSure -= Time.deltaTime;
            yield return null;
        }

        OlumGerceklesti();
    }

    IEnumerator EfektGuncelle()
    {
        while (bolgeDisinda)
        {
            float yogunluk = 1f - (kalanSure / geriBosalmaSuresi);
            PostProsses.Instance?.BolgeDisiEfektAc(yogunluk);
            yield return null;
        }
    }

    void OlumGerceklesti()
    {
        bolgeDisinda = false;
        if (uyariPanel != null) uyariPanel.SetActive(false);
        PostProsses.Instance?.BolgeDisiEfektKapat();

        if (checkpointSistemi != null)
        {
            checkpointSistemi.CheckpointKaydet(sonGuvenliPozisyon);
            checkpointSistemi.OlumGerceklestiBolgeDisi();
        }

        // Ekran kararýrken teleport et — oyuncu görmez
        StartCoroutine(EkranKararinceIsin());
        StartCoroutine(OlumKorumasi());
    }

    IEnumerator EkranKararinceIsin()
    {
        // Ekranýn kararmasýný bekle
        yield return new WaitForSeconds(0.3f);

        if (oyuncu != null)
        {
            CharacterController cc = oyuncu.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            oyuncu.position = sonGuvenliPozisyon;
            if (cc != null) cc.enabled = true;
        }
    }

    IEnumerator OlumKorumasi()
    {
        olumSonrasiKoruma = true;
        yield return new WaitForSeconds(2f);
        olumSonrasiKoruma = false;
    }

    void OnEnable()
    {
        bolgeDisinda = false;
        olumSonrasiKoruma = false;
        if (sayacCoroutine != null) StopCoroutine(sayacCoroutine);
        if (efektCoroutine != null) StopCoroutine(efektCoroutine);
        if (uyariPanel != null) uyariPanel.SetActive(false);

        if (oyuncu != null)
            sonGuvenliPozisyon = oyuncu.position;
    }
}