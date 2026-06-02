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
    private float kalanSure;
    private Coroutine sayacCoroutine;
    private Coroutine efektCoroutine;

    private Vector3 sonGuvenliPozisyon;
    private Transform oyuncu;

    void Start()
    {
        if (checkpointSistemi == null)
            checkpointSistemi = CheckpointSistemi.Instance;

        if (uyariPanel != null) uyariPanel.SetActive(false);

        // Oyuncuyu bul
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) oyuncu = p.transform;
    }

    void Update()
    {
        // Bölge içindeyken pozisyonu kaydet
        if (!bolgeDisinda && oyuncu != null)
            sonGuvenliPozisyon = oyuncu.position;
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
            if (sayacText != null)
                sayacText.text = Mathf.CeilToInt(kalanSure).ToString();
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

        // Son güvenli pozisyonu checkpoint'e kaydet
        if (checkpointSistemi != null)
        {
            checkpointSistemi.CheckpointKaydet(sonGuvenliPozisyon);
            checkpointSistemi.OlumGerceklestiBolgeDisi();
        }
    }
}