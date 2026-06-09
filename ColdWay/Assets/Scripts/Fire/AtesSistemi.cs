using StarterAssets;
using UnityEngine;

public class AtesSistemi : MonoBehaviour
{
    [Header("Referanslar")]
    public SicaklikSistemi sicaklikSistemi;
    public EnerjiKontrol enerjiKontrol;

    [Header("Odun Modelleri")]
    public GameObject yanmamisOdunModeli;  // kýrmýzýlýklý
    public GameObject kulOdunModeli;       // gri kül

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

    private float kalanSure = 0f;
    private bool yaniyor = false;
    private int mevcutOdun = 0;

  

    [Header("Ates Noktasi")]
    public AteþNoktasi atesNoktasi;

    [Header("Odun Gorselleri")]
    public GameObject[] odunObjeleri; // Inspector'dan odun child objelerini sur

    // Baþlangýçta aktif olacak indexler
    private int[] baslangicIndexleri = { 3, 4, 6 };
    // Sonradan ekleneceklerin sýrasý
    private int[] ekstraIndexler = { 0, 1, 2, 5, 7 };


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

        // Isik titremesi
        if (atesIsigi != null)
            atesIsigi.intensity = 1.5f + Mathf.Sin(Time.time * 8f) * 0.3f;

        // Ates sonurme
        if (kalanSure <= 0f)
            AtesSon();

        // E ile odun besleme
        if (Input.GetKeyDown(KeyCode.E))
        {
            float mesafe = Vector3.Distance(
                transform.position,
                Camera.main.transform.position);

            if (mesafe <= beslemeMesafesi)
                OdunEkle();
        }
    }

    public void AtesBas(int odunMiktari)
    {
        Debug.Log("ATES BAS ÇAÐRILDI");

        mevcutOdun = Mathf.Min(odunMiktari, maxOdun);
        kalanSure = mevcutOdun * odunBasinaYanmaSuresi;
        yaniyor = true;

        if (yanmamisOdunModeli != null) yanmamisOdunModeli.SetActive(false);
        if (kulOdunModeli != null) kulOdunModeli.SetActive(false);

        OdunGorselGuncelle();

        if (atesParticle != null)
        {
            atesParticle.SetActive(true);
            ParticleSystem[] particles = atesParticle.GetComponentsInChildren<ParticleSystem>(true);
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

        // SES BURADA BAÞLAR: Ateþ gerçekten yandýðý an ses devreye girer
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play("Ates_Sesi");
        }
    }

    void OdunEkle()
    {
        if (inventory == null || odunItemSO == null) return;

        foreach (Slot slot in inventory.allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == odunItemSO)
            {
                int miktar = slot.GetAmount();
                int eklenecek = Mathf.Min(miktar, maxOdun - mevcutOdun);

                if (eklenecek <= 0)
                {
                    Debug.Log("Ates zaten dolu!");
                    return;
                }

                slot.SetItem(odunItemSO, miktar - eklenecek);
                if (slot.GetAmount() <= 0) slot.ClearSlot();

                mevcutOdun += eklenecek;
                kalanSure += eklenecek * odunBasinaYanmaSuresi;

                OdunGorselGuncelle();

                Debug.Log(eklenecek + " odun eklendi. Kalan sure: " + Mathf.Round(kalanSure) + " sn");
                return;
            }
        }
        Debug.Log("Envanterde odun yok!");
    }

    void AtesSon()
    {
        yaniyor = false;
        mevcutOdun = 0;
        OdunGorselGuncelle();

        // Campfire mesh gizle
        if (odunObjeleri != null)
            foreach (GameObject odun in odunObjeleri)
                if (odun != null) odun.SetActive(false);

        if (yanmamisOdunModeli != null) yanmamisOdunModeli.SetActive(false);
        if (kulOdunModeli != null) kulOdunModeli.SetActive(true);

        if (atesParticle != null) atesParticle.SetActive(false);
        if (atesIsigi != null) atesIsigi.enabled = false;

        if (sicaklikSistemi != null) sicaklikSistemi.AtesAktif(false);
        if (enerjiKontrol != null) enerjiKontrol.AtesAktif(false);

        // Kul spawn
        if (kulPrefab != null)
            Instantiate(kulPrefab, transform.position, Quaternion.identity);

        Debug.Log("Ates sondu. Kul biraktirildi.");

        if (AudioManager.instance != null)
        {
            AudioManager.instance.Stop("Ates_Sesi");
        }

        if (atesNoktasi != null)
        {
            atesNoktasi.AtesSondu();
        }
    }

    void OdunGorselGuncelle()
    {
        if (odunObjeleri == null) return;

        // Önce hepsini kapat
        foreach (GameObject odun in odunObjeleri)
            if (odun != null) odun.SetActive(false);

        if (!yaniyor) return;

        // Baþlangýç 3 odunu aç (index 3, 4, 6)
        foreach (int index in baslangicIndexleri)
            if (index < odunObjeleri.Length && odunObjeleri[index] != null)
                odunObjeleri[index].SetActive(true);

        // 3'ten fazla odun varsa ekstralarý aç
        int ekstra = mevcutOdun - 3;
        for (int i = 0; i < ekstra && i < ekstraIndexler.Length; i++)
        {
            int index = ekstraIndexler[i];
            if (index < odunObjeleri.Length && odunObjeleri[index] != null)
                odunObjeleri[index].SetActive(true);
        }
    }

    public bool YaniyorMu() { return yaniyor; }
    public float KalanSure() { return kalanSure; }
}