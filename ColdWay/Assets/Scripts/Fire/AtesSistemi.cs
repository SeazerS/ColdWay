using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtesSistemi : MonoBehaviour
{
    [Header("Referanslar")]
    public SicaklikSistemi sicaklikSistemi;
    public EnerjiKontrol enerjiKontrol;

    [Header("Odun Modelleri")]
    public GameObject yanmamisOdunModeli;

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

    [Header("Odun Erime Ayarlari")]
    public float minOdunBoyu = 0.7f;
    public float odunKuculmeBaslangic = 0.8f;
    public float odunKuculmeBitis = 0.5f;

    [Header("Odun Materyal Gecisi")]
    public Material kulMaterial;

    private Color[] odunOrijinalRenkler;
    private Color kulRengi;
    private Material[][] odunMateryalleri; // cached instances

    private float kalanSure = 0f;
    private bool yaniyor = false;
    private int mevcutOdun = 0;

    private int[] baslangicIndexleri = { 3, 4, 6 };
    private int[] ekstraIndexler = { 0, 1, 2, 5, 7 };

    private Transform fireAlpha;
    private Transform fireAdd;
    private Vector3 alphaBaseScale = Vector3.one * 0.5f;
    private Vector3 addBaseScale = Vector3.one * 0.5f;
    private float mevcutKuculmeFaktoru = 1f;

    private Vector3[] odunOrijinalScales;
    private Vector3[] odunOrijinalPozlar;

    void Start()
    {
        if (odunObjeleri != null)
            foreach (GameObject odun in odunObjeleri)
                if (odun != null) odun.SetActive(false);

        if (yanmamisOdunModeli != null) yanmamisOdunModeli.SetActive(true);
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

        mevcutKuculmeFaktoru = kuculmeFaktoru;

        if (atesIsigi != null)
        {
            float titreme = Mathf.Sin(Time.time * 8f) * 0.3f;
            float hedefYogunluk = Mathf.Lerp(0.5f, 1.8f, kuculmeFaktoru);
            atesIsigi.intensity = hedefYogunluk + titreme * kuculmeFaktoru;
        }

        if (fireAlpha != null)
            fireAlpha.localScale = alphaBaseScale * kuculmeFaktoru;
        if (fireAdd != null)
            fireAdd.localScale = addBaseScale * kuculmeFaktoru;

        if (odunObjeleri != null && odunOrijinalScales != null && odunOrijinalPozlar != null)
        {
            float odunFaktoru;
            if (oran > odunKuculmeBaslangic)
                odunFaktoru = 1f;
            else if (oran <= odunKuculmeBitis)
                odunFaktoru = minOdunBoyu;
            else
                odunFaktoru = Mathf.Lerp(
                    minOdunBoyu, 1f,
                    (oran - odunKuculmeBitis) /
                    (odunKuculmeBaslangic - odunKuculmeBitis));

            float hiz = Time.deltaTime * 0.3f;

            for (int i = 0; i < odunObjeleri.Length; i++)
            {
                if (odunObjeleri[i] == null || !odunObjeleri[i].activeSelf)
                    continue;

                // Scale
                odunObjeleri[i].transform.localScale = Vector3.Lerp(
                    odunObjeleri[i].transform.localScale,
                    odunOrijinalScales[i] * odunFaktoru,
                    hiz);

                // Pozisyon
                Vector3 hedefPoz = Vector3.Lerp(
                    odunOrijinalPozlar[i], Vector3.zero, 1f - odunFaktoru);
                odunObjeleri[i].transform.localPosition = Vector3.Lerp(
                    odunObjeleri[i].transform.localPosition, hedefPoz, hiz);

                // Renk — cached materyaller üzerinden
                if (odunFaktoru < 1f && odunOrijinalRenkler != null
                    && odunMateryalleri != null
                    && i < odunMateryalleri.Length
                    && odunMateryalleri[i] != null)
                {
                    Color hedefRenk = Color.Lerp(
                        kulRengi, odunOrijinalRenkler[i], odunFaktoru);

                    foreach (Material m in odunMateryalleri[i])
                        if (m != null && m.HasProperty("_BaseColor"))
                            m.SetColor("_BaseColor", Color.Lerp(
                                m.GetColor("_BaseColor"),
                                hedefRenk,
                                Time.deltaTime * 0.5f));
                }
            }
        }

        if (kalanSure <= 0f)
        {
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

        if (odunObjeleri != null)
        {
            odunOrijinalRenkler = new Color[odunObjeleri.Length];
            odunOrijinalScales = new Vector3[odunObjeleri.Length];
            odunOrijinalPozlar = new Vector3[odunObjeleri.Length];
            odunMateryalleri = new Material[odunObjeleri.Length][];

            for (int i = 0; i < odunObjeleri.Length; i++)
            {
                if (odunObjeleri[i] == null) continue;

                // Orijinal renk
                Renderer rend = odunObjeleri[i]
                    .GetComponentInChildren<Renderer>(true);
                if (rend != null && rend.sharedMaterial != null
                    && rend.sharedMaterial.HasProperty("_BaseColor"))
                {
                    odunOrijinalRenkler[i] = rend.sharedMaterial.GetColor("_BaseColor");
                    Debug.Log($"Odun {i}: {rend.sharedMaterial.name}" +
          $" | _BaseColor: {rend.sharedMaterial.HasProperty("_BaseColor")}" +
          $" | _Color: {rend.sharedMaterial.HasProperty("_Color")}");
                }
                else
                {
                    odunOrijinalRenkler[i] = Color.white;
                    Debug.Log($"Odun {i}: Renderer veya material bulunamadý!");

                }

                // Material instance'larýný cache'le
                var renderers = odunObjeleri[i]
                    .GetComponentsInChildren<Renderer>(true);
                var matList = new List<Material>();
                foreach (var r in renderers)
                    matList.AddRange(r.materials); // instance oluþturur
                odunMateryalleri[i] = matList.ToArray();

                odunOrijinalScales[i] = odunObjeleri[i].transform.localScale;
                odunOrijinalPozlar[i] = odunObjeleri[i].transform.localPosition;
                odunObjeleri[i].SetActive(false);
            }

            if (kulMaterial != null && kulMaterial.HasProperty("_BaseColor"))
                kulRengi = kulMaterial.GetColor("_BaseColor");
            else
                kulRengi = new Color(0.15f, 0.15f, 0.15f, 1f);
        }

        if (yanmamisOdunModeli != null)
            StartCoroutine(FadeOut(yanmamisOdunModeli));

        for (int i = 0; i < mevcutOdun && i < baslangicIndexleri.Length; i++)
        {
            int idx = baslangicIndexleri[i];
            if (idx < odunObjeleri.Length && odunObjeleri[idx] != null)
                StartCoroutine(FadeIn(odunObjeleri[idx], idx));
        }

        if (atesParticle != null)
        {
            fireAlpha = atesParticle.transform.Find("PS_Fire_Alpha");
            fireAdd = atesParticle.transform.Find("PS_Fire_Add");
            alphaBaseScale = new Vector3(0.4f, 0.3f, 0.4f);
            addBaseScale = new Vector3(0.4f, 0.3f, 0.4f);
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
                {
                    StartCoroutine(FadeIn(odunObjeleri[newIndex], newIndex));

                    if (odunOrijinalScales != null && newIndex < odunOrijinalScales.Length)
                        odunOrijinalScales[newIndex] =
                            odunObjeleri[newIndex].transform.localScale;
                    if (odunOrijinalPozlar != null && newIndex < odunOrijinalPozlar.Length)
                        odunOrijinalPozlar[newIndex] =
                            odunObjeleri[newIndex].transform.localPosition;

                    // Yeni odunun materyallerini cache'le
                    if (odunMateryalleri != null && newIndex < odunMateryalleri.Length)
                    {
                        var renderers = odunObjeleri[newIndex]
                            .GetComponentsInChildren<Renderer>(true);
                        var matList = new List<Material>();
                        foreach (var r in renderers)
                            matList.AddRange(r.materials);
                        odunMateryalleri[newIndex] = matList.ToArray();
                    }
                }

                alphaBaseScale += Vector3.one * 0.1f;
                addBaseScale += Vector3.one * 0.1f;
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

        alphaBaseScale = Vector3.zero;
        addBaseScale = Vector3.zero;
        if (fireAlpha != null) fireAlpha.localScale = Vector3.zero;
        if (fireAdd != null) fireAdd.localScale = Vector3.zero;

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

    IEnumerator FadeIn(GameObject obj, int index = -1)
    {
        if (obj == null) yield break;
        obj.SetActive(true);
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>();

        // Önce instance oluþtur
        var matList = new System.Collections.Generic.List<Material>();
        foreach (Renderer r in rends)
            matList.AddRange(r.materials); // instance oluþturur

        // Cache'e kaydet
        if (index >= 0 && odunMateryalleri != null
            && index < odunMateryalleri.Length)
            odunMateryalleri[index] = matList.ToArray();

        // Fade in
        foreach (Material m in matList)
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
            foreach (Material m in matList)
                if (m.HasProperty("_BaseColor"))
                {
                    Color c = m.GetColor("_BaseColor");
                    c.a = Mathf.Clamp01(t);
                    m.SetColor("_BaseColor", c);
                }
            yield return null;
        }
    }

    public float AtesYogunlugu()
    {
        if (!yaniyor) return 0f;
        float maxY = 0.3f + (maxOdun - 2) * 0.1f;
        float scaleOrani = Mathf.Clamp01(alphaBaseScale.y / maxY);
        return scaleOrani * mevcutKuculmeFaktoru;
    }

    public bool YaniyorMu() { return yaniyor; }
    public float KalanSure() { return kalanSure; }
}