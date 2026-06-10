using StarterAssets;
using System.Collections;
using UnityEngine;

public class AtesSistemi : MonoBehaviour
{
    [Header("Referanslar")]
    public SicaklikSistemi sicaklikSistemi;
    public EnerjiKontrol enerjiKontrol;

    [Header("Odun Modelleri")]
    public GameObject yanmamisOdunModeli;
    public GameObject kulOdunModeli;

    [Header("Particle ve Isik")]
    public GameObject atesParticle;
    public Light atesIsigi;

    [Header("Kul")]
    public GameObject kulPrefab;

    [Header("Omur Ayarlari")]
    public float odunBasinaYanmaSuresi = 90f;
    public int maxOdun = 5;

    [Header("Besleme")]
    public float beslemeMesafesi = 3f;
    public ItemSO odunItemSO;
    public Inventory inventory;

    [Header("Gecis ve Ates Ayarlari")]
    public float gecisHizi = 1.5f;
    public float minAtesBoyu = 0.4f;
    public float maxAtesBoyu = 1f;
    public float kuculmeBaslangicYuzdesi = 0.3f;

    [Header("Ates Noktasi")]
    public AteþNoktasi atesNoktasi;

    [Header("Odun Gorselleri")]
    public GameObject[] odunObjeleri;

    private float kalanSure = 0f;
    private bool yaniyor = false;
    private int mevcutOdun = 0;

    private int[] baslangicIndexleri = { 3, 4, 6 };
    private int[] ekstraIndexler = { 0, 1, 2, 5, 7 };

    // Alpha ve Add cached referanslar
    private Transform fireAlpha;
    private Transform fireAdd;
    private Vector3 alphaBaseScale = Vector3.one * 0.5f;
    private Vector3 addBaseScale = Vector3.one * 0.5f;

    void Start()
    {
        if (odunObjeleri != null)
            foreach (GameObject odun in odunObjeleri)
                if (odun != null) odun.SetActive(false);

        if (yanmamisOdunModeli != null) yanmamisOdunModeli.SetActive(true);
        if (kulOdunModeli != null) kulOdunModeli.SetActive(false);
    }

    void Update()
    {
        if (!yaniyor) return;

        kalanSure -= Time.deltaTime;

        float toplamSure = mevcutOdun * odunBasinaYanmaSuresi;
        float oran = Mathf.Clamp01(kalanSure / toplamSure);

        float kuculmeFaktoru = oran > kuculmeBaslangicYuzdesi
            ? 1f
            : Mathf.Lerp(minAtesBoyu, 1f, oran / kuculmeBaslangicYuzdesi);

        // Iþýk
        if (atesIsigi != null)
        {
            float titreme = Mathf.Sin(Time.time * 8f) * 0.3f;
            float hedefYogunluk = Mathf.Lerp(0.5f, 1.8f, kuculmeFaktoru);
            atesIsigi.intensity = hedefYogunluk + titreme * kuculmeFaktoru;
        }

        // Alpha ve Add üzerinden küçült
        if (fireAlpha != null)
            fireAlpha.localScale = alphaBaseScale * kuculmeFaktoru;
        if (fireAdd != null)
            fireAdd.localScale = addBaseScale * kuculmeFaktoru;

        if (kalanSure <= 0f)
        {
            // Scale tamamen 0'a insin
            if (fireAlpha != null) fireAlpha.localScale = Vector3.zero;
            if (fireAdd != null) fireAdd.localScale = Vector3.zero;
            AtesSon();
        }
    }

    public void AtesBas(int odunMiktari)
    {
        mevcutOdun = Mathf.Min(odunMiktari, maxOdun);
        kalanSure = mevcutOdun * odunBasinaYanmaSuresi;
        yaniyor = true;

        if (yanmamisOdunModeli != null)
            StartCoroutine(FadeOut(yanmamisOdunModeli));
        if (kulOdunModeli != null) kulOdunModeli.SetActive(false);

        for (int i = 0; i < mevcutOdun && i < baslangicIndexleri.Length; i++)
        {
            int idx = baslangicIndexleri[i];
            if (idx < odunObjeleri.Length && odunObjeleri[idx] != null)
                StartCoroutine(FadeIn(odunObjeleri[idx]));
        }

        if (atesParticle != null)
        {
            // Cache et ve sýfýrla
            fireAlpha = atesParticle.transform.Find("PS_Fire_Alpha");
            fireAdd = atesParticle.transform.Find("PS_Fire_Add");
            alphaBaseScale = Vector3.one * 0.5f;
            addBaseScale = Vector3.one * 0.5f;
            if (fireAlpha != null) fireAlpha.localScale = alphaBaseScale;
            if (fireAdd != null) fireAdd.localScale = addBaseScale;

            atesParticle.SetActive(true);
            ParticleSystem[] particles = atesParticle
                .GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in particles)
            {
                ps.gameObject.SetActive(true);
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Clear();
                ps.Play();
            }
        }

        if (atesIsigi != null) atesIsigi.enabled = true;
        if (sicaklikSistemi != null) sicaklikSistemi.AtesAktif(true);
        if (enerjiKontrol != null) enerjiKontrol.AtesAktif(true);

        if (AudioManager.instance != null)
            AudioManager.instance.Play("Ates_Sesi");
    }

    public void OdunEkle()
    {
        if (inventory == null || odunItemSO == null) return;

        if (mevcutOdun >= maxOdun)
        {
            IpucuYoneticisi.Instance?.MesajGoster("ates", "Ateþ dolu!");
            return;
        }

        foreach (Slot slot in inventory.allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == odunItemSO)
            {
                int miktar = slot.GetAmount();
                slot.SetItem(odunItemSO, miktar - 1);
                if (slot.GetAmount() <= 0) slot.ClearSlot();

                mevcutOdun++;
                kalanSure += odunBasinaYanmaSuresi;

                // Yeni odun modelini göster
                int newIndex = -1;
                if (mevcutOdun <= baslangicIndexleri.Length)
                    newIndex = baslangicIndexleri[mevcutOdun - 1];
                else
                {
                    int ekstraIdx = mevcutOdun - baslangicIndexleri.Length - 1;
                    if (ekstraIdx >= 0 && ekstraIdx < ekstraIndexler.Length)
                        newIndex = ekstraIndexler[ekstraIdx];
                }

                if (newIndex >= 0 && newIndex < odunObjeleri.Length
                    && odunObjeleri[newIndex] != null)
                    StartCoroutine(FadeIn(odunObjeleri[newIndex]));

                // Base scale'i artýr ve uygula
                alphaBaseScale += Vector3.one * 0.2f;
                addBaseScale += Vector3.one * 0.2f;
                if (fireAlpha != null) fireAlpha.localScale = alphaBaseScale;
                if (fireAdd != null) fireAdd.localScale = addBaseScale;

                if (atesIsigi != null) atesIsigi.intensity = 1.8f;
                return;
            }
        }
        IpucuYoneticisi.Instance?.MesajGoster("ates", "Odun yok!");
    }

    void AtesSon()
    {
        yaniyor = false;
        mevcutOdun = 0;

        // Scale sýfýrla
        alphaBaseScale = Vector3.zero;
        addBaseScale = Vector3.zero;
        if (fireAlpha != null) fireAlpha.localScale = Vector3.zero;
        if (fireAdd != null) fireAdd.localScale = Vector3.zero;

        if (odunObjeleri != null)
            foreach (GameObject odun in odunObjeleri)
                if (odun != null && odun.activeSelf)
                    StartCoroutine(FadeOut(odun));

        if (kulOdunModeli != null) StartCoroutine(FadeIn(kulOdunModeli));

        if (atesParticle != null) atesParticle.SetActive(false);
        if (atesIsigi != null) atesIsigi.enabled = false;
        if (sicaklikSistemi != null) sicaklikSistemi.AtesAktif(false);
        if (enerjiKontrol != null) enerjiKontrol.AtesAktif(false);

        if (kulPrefab != null)
            Instantiate(kulPrefab, transform.position, Quaternion.identity);

        if (AudioManager.instance != null)
            AudioManager.instance.Stop("Ates_Sesi");

        if (atesNoktasi != null) atesNoktasi.AtesSondu();
    }

    IEnumerator FadeOut(GameObject obj)
    {
        if (obj == null) yield break;
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>();
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * gecisHizi;
            foreach (Renderer r in rends)
                foreach (Material m in r.materials)
                    if (m.HasProperty("_BaseColor"))
                    {
                        Color c = m.GetColor("_BaseColor");
                        c.a = Mathf.Clamp01(t);
                        m.SetColor("_BaseColor", c);
                    }
            yield return null;
        }
        obj.SetActive(false);
        foreach (Renderer r in rends)
            foreach (Material m in r.materials)
                if (m.HasProperty("_BaseColor"))
                {
                    Color c = m.GetColor("_BaseColor");
                    c.a = 1f;
                    m.SetColor("_BaseColor", c);
                }
    }

    IEnumerator FadeIn(GameObject obj)
    {
        if (obj == null) yield break;
        obj.SetActive(true);
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
            foreach (Material m in r.materials)
                if (m.HasProperty("_BaseColor"))
                {
                    Color c = m.GetColor("_BaseColor");
                    c.a = 0f;
                    m.SetColor("_BaseColor", c);
                }
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * gecisHizi;
            foreach (Renderer r in rends)
                foreach (Material m in r.materials)
                    if (m.HasProperty("_BaseColor"))
                    {
                        Color c = m.GetColor("_BaseColor");
                        c.a = Mathf.Clamp01(t);
                        m.SetColor("_BaseColor", c);
                    }
            yield return null;
        }
    }

    public bool YaniyorMu() { return yaniyor; }
    public float KalanSure() { return kalanSure; }
}