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
        GuncelleMesaj();

        if (Input.GetKeyDown(KeyCode.T))
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

        switch (cadirDurum)
        {
            case CadirDurum.Kurulmadi:
                IpucuYoneticisi.Instance.MesajGoster("cadir",
                    geceVakti ? "T — Çadýr Kur" : "Hava henüz kararmadý");
                break;
            case CadirDurum.KurulduUyunmadi:
                IpucuYoneticisi.Instance.MesajGoster("cadir", "T — Uyu");
                break;
            case CadirDurum.Uyundu:
                IpucuYoneticisi.Instance.MesajGoster("cadir", "T — Çadýrý Topla");
                break;
        }
    }

    void CadirKur()
    {
        // Gece kontrolü
        bool geceVakti = sicaklikSistemi != null && sicaklikSistemi.geceBonusu;

        if (!geceVakti)
        {
            IpucuYoneticisi.Instance.MesajGoster("cadir", "Hava henüz kararmadý!");
            return;
        }

        // Envanterde çadýr var mý
        bool cadirVar = false;
        foreach (Slot slot in inventory.allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == cadirItem)
            { cadirVar = true; break; }
        }

        if (!cadirVar)
        {
            IpucuYoneticisi.Instance.MesajGoster("cadir", "Çadýr yok!");
            return;
        }

        inventory.RemoveItem(cadirItem, 1);

        Vector3 cadirPoz = cadirKurulacakPoz +
                           transform.forward * cadirMesafesi;
        cadirPoz.y = cadirKurulacakPoz.y;
        mevcutCadir = Instantiate(cadirPrefab, cadirPoz, transform.rotation);

        cadirDurum = CadirDurum.KurulduUyunmadi;
    }

    void Uyu()
    {
        checkpoint?.OlumIptal();

        if (sicaklikSistemi != null)
        {
            sicaklikSistemi.geceBonusu = false;
            sicaklikSistemi.alacakaranlýkBonusu = false;
        }

        sicaklikSistemi?.Oldu_Sifirla();
        enerjiKontrol?.Oldu_Sifirla();

        checkpoint?.CheckpointKaydet(cadirKurulacakPoz);

        bool atesYaniyor = atesSistemi != null && atesSistemi.YaniyorMu();
        sicaklikSistemi?.UykuSonrasiIsi(atesYaniyor);

        if (enerjiKontrol != null)
            enerjiKontrol.UykuSonrasiEnerji(enerjiKontrol.mevcutEnerji);

        gecGunduz?.SabahOldu();
        GunSayaci.Instance?.YeniGun();

        cadirDurum = CadirDurum.Uyundu;
        Debug.Log("Uyku tamamlandi!");
    }

    void CadirKaldir()
    {
        if (mevcutCadir != null)
            Destroy(mevcutCadir);

        inventory.AddItem(cadirItem, 1);
        cadirDurum = CadirDurum.Kurulmadi;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuYakinda = true;
        cadirKurulacakPoz = other.transform.position;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuYakinda = false;
        IpucuYoneticisi.Instance.MesajGizle("cadir");
    }
}