using UnityEngine;

public class UykuSistemi : MonoBehaviour
{
    public static UykuSistemi Instance;

    [Header("Envanter")]
    public Inventory inventory;
    public ItemSO cadirItem;

    [Header("Referanslar")]
    public GecGunduzSistemi gecGunduz;
    public SicaklikSistemi sicaklikSistemi;
    public EnerjiKontrol enerjiKontrol;
    public CheckpointSistemi checkpoint;
    public AtesSistemi atesSistemi;

    [Header("Saat Kisitlamasi")]
    public float uykunaBaslangicSaati = 20f;
    public float uykunaBitisSaati = 6f;

    [Header("Cadir")]
    public GameObject cadirPrefab;
    public float cadirMesafesi = 3f;
    private GameObject mevcutCadir;

    private bool oyuncuYakinda = false;
    private Vector3 cadirKurulacakPoz;

    private enum CadirDurum { Kurulmadi, KurulduUyunmadi, Uyundu }
    private CadirDurum cadirDurum = CadirDurum.Kurulmadi;

    void Awake()
    {
        //if (Instance == null) Instance = this;
        //else Destroy(gameObject);
    }

    void Update()
    {
        if (!oyuncuYakinda) return;

        KeyCode uyumaTusu = SettingsManager.Instance.GetKey("Uyuma");

        if (Input.GetKeyDown(uyumaTusu))
        {
            switch (cadirDurum)
            {
                case CadirDurum.Kurulmadi:
                    CadirKur();
                    break;
                case CadirDurum.KurulduUyunmadi:
                    Uyu();
                    break;
                case CadirDurum.Uyundu:
                    CadirKaldir();
                    break;
            }
        }
    }

    void GuncelleMesaj()
    {
        bool geceVakti = sicaklikSistemi != null && sicaklikSistemi.geceBonusu;
        string tusAdi = SettingsManager.Instance.GetKey("Uyuma").ToString();

        switch (cadirDurum)
        {
            case CadirDurum.Kurulmadi:
                if (!CadirVarMi())
                {
                    IpucuYoneticisi.Instance.MesajGoster("Çadýr",
                        "Çadýr yok",
                        "Uyuyup enerjini toplayabilmek için envanterinde bir çadýr olmalý.");
                }
                else
                {
                    IpucuYoneticisi.Instance.MesajGoster("Çadýr",
                        geceVakti ? tusAdi + " — Çadýr Kur" : "Hava henüz kararmadý",
                        "Sadece hava karardýðýnda çadýr kurabilirsin. Uyumak zamaný sabaha alýr, enerjini doldurur ve oyunu kaydeder.");
                }
                break;

            case CadirDurum.KurulduUyunmadi:
                IpucuYoneticisi.Instance.MesajGoster("Çadýr",
                    tusAdi + " — Uyu",
                    "Uyumak mevcut konumunu yeni doðma noktan (checkpoint) olarak belirler. Yakýnda yanan bir ateþ varsa uyandýðýnda vücut ýsýn daha yüksek olur.");
                break;

            case CadirDurum.Uyundu:
                IpucuYoneticisi.Instance.MesajGoster("Çadýr",
                    tusAdi + " — Çadýrý Topla",
                    "Çadýrý toplayýp çantana geri koymalýsýn. Eðer çadýrý burada býrakýrsan bir sonraki gece açýkta uyumak zorunda kalýrsýn.");
                break;
        }
    }

    bool CadirVarMi()
    {
        if (inventory == null || cadirItem == null) return false;
        foreach (Slot slot in inventory.allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == cadirItem)
                return true;
        }
        return false;
    }

    void CadirKur()
    {
        bool geceVakti = sicaklikSistemi != null && sicaklikSistemi.geceBonusu;

        if (!geceVakti || !CadirVarMi()) return;

        if (StarterAssets.AudioManager.instance != null)
            StarterAssets.AudioManager.instance.Play("Item_Surukleme");

        inventory.RemoveItem(cadirItem, 1);

        Vector3 cadirPoz = cadirKurulacakPoz + transform.forward * cadirMesafesi;
        cadirPoz.y = cadirKurulacakPoz.y;
        mevcutCadir = Instantiate(cadirPrefab, cadirPoz, transform.rotation);

        cadirDurum = CadirDurum.KurulduUyunmadi;

        // Eylem yapýldýktan sonra arayüzü sadece 1 kere güncelle
        GuncelleMesaj();
    }

    void Uyu()
    {
        checkpoint?.OlumIptal();

        if (sicaklikSistemi != null)
        {
            sicaklikSistemi.geceBonusu = false;
            sicaklikSistemi.alacakaranlýkBonusu = false;
        }

        checkpoint?.CheckpointKaydet(cadirKurulacakPoz);

        bool atesYaniyor = atesSistemi != null && atesSistemi.YaniyorMu();
        sicaklikSistemi?.UykuSonrasiIsi(atesYaniyor);

        if (enerjiKontrol != null)
            enerjiKontrol.UykuSonrasiEnerji(enerjiKontrol.mevcutEnerji);

        gecGunduz?.SabahOldu();
        GunSayaci.Instance?.YeniGun();

        cadirDurum = CadirDurum.Uyundu;
        Debug.Log("Uyku tamamlandi!");

        // Eylem yapýldýktan sonra arayüzü sadece 1 kere güncelle
        GuncelleMesaj();
    }

    void CadirKaldir()
    {
        if (StarterAssets.AudioManager.instance != null)
            StarterAssets.AudioManager.instance.Play("Item_Alma");

        if (mevcutCadir != null)
            Destroy(mevcutCadir);

        inventory.AddItem(cadirItem, 1);
        cadirDurum = CadirDurum.Kurulmadi;

        // Eylem yapýldýktan sonra arayüzü sadece 1 kere güncelle
        GuncelleMesaj();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuYakinda = true;
        cadirKurulacakPoz = other.transform.position;

        // Oyuncu alana ilk girdiðinde mesajý 1 kere göster
        GuncelleMesaj();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuYakinda = false;
        IpucuYoneticisi.Instance.MesajGizle("Çadýr");
    }
}