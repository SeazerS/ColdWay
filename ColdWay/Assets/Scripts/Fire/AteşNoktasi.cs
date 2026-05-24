using UnityEngine;
using TMPro;

public class AteşNoktasi : MonoBehaviour
{
    [Header("Referanslar")]
    public AtesSistemi atesSistemi;
    public KibritMinigame kibritMinigame;
    public Inventory inventory;
    public ItemSO odunItemSO;
    public ItemSO kibritItemSO;

    [Header("Duman")]
    public GameObject dumanParticle;

    //[Header("UI")]
    //public GameObject ipucuPanel;
    //public TextMeshProUGUI ipucuText;

    [Header("Ayarlar")]
    public float etkilesimMesafesi = 3f;
    public int gerekliOdun = 2;

    private bool oyuncuYakinda = false;

    void Start()
    {
        //if (ipucuPanel != null) ipucuPanel.SetActive(false);
        if (dumanParticle != null) dumanParticle.SetActive(true);
    }

    void Update()
    {
        if (!oyuncuYakinda) return;
        if (atesSistemi != null && atesSistemi.YaniyorMu()) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!OdunVarMi())
            {
                IpucuYoneticisi.Instance.MesajGoster("ates", gerekliOdun + " odun gerekli!");
                return;
            }

            // Kibrit kontrolu
            if (!KibritVarMi())
            {
                IpucuYoneticisi.Instance.MesajGoster("ates", "Kibrit gerekli!");
                return;
            }

            // Minigame baslat
            if (kibritMinigame != null)
                kibritMinigame.Baslat(this);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuYakinda = true;
        GuncelleMesaj();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuYakinda = false;
        IpucuYoneticisi.Instance.MesajGizle("ates");
    }

    void GuncelleMesaj()
    {
        if (atesSistemi != null && atesSistemi.YaniyorMu())
        {
            IpucuYoneticisi.Instance.MesajGoster("ates", "E — Odun Ekle");
            return;
        }

        if (!OdunVarMi())
            IpucuYoneticisi.Instance.MesajGoster("ates", gerekliOdun + " odun gerekli");
        else if (!KibritVarMi())
            IpucuYoneticisi.Instance.MesajGoster("ates", "Kibrit gerekli");
        else
            IpucuYoneticisi.Instance.MesajGoster("ates", "E — Ateş Kur");
    }

    bool OdunVarMi()
    {
        if (inventory == null || odunItemSO == null) return false;
        int toplam = 0;
        foreach (Slot slot in inventory.allSlots)
            if (slot.HasItem() && slot.GetItem() == odunItemSO)
                toplam += slot.GetAmount();
        return toplam >= gerekliOdun;
    }

    bool KibritVarMi()
    {
        if (inventory == null || kibritItemSO == null) return false;
        foreach (Slot slot in inventory.allSlots)
            if (slot.HasItem() && slot.GetItem() == kibritItemSO)
                return true;
        return false;
    }

    // Minigame basarili oldugunda cagrilir
    public void AteşiYak()
    {
        // Odunu envanterden kaldir
        OdunuKaldir(gerekliOdun);

        // Kibrit azalt
        KibritiKullan();

        // Ateşi başlat
        if (atesSistemi != null)
            atesSistemi.AtesBas(gerekliOdun);

        // Duman kapat
        if (dumanParticle != null)
            dumanParticle.SetActive(false);

        GuncelleMesaj();
    }

    void OdunuKaldir(int miktar)
    {
        int kalanKaldir = miktar;
        foreach (Slot slot in inventory.allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == odunItemSO)
            {
                int amount = slot.GetAmount();
                if (amount >= kalanKaldir)
                {
                    slot.SetItem(odunItemSO, amount - kalanKaldir);
                    if (slot.GetAmount() <= 0) slot.ClearSlot();
                    break;
                }
                else
                {
                    kalanKaldir -= amount;
                    slot.ClearSlot();
                }
            }
        }
    }

    // 3 kibrit de bitince kutu envanterden kalkar
    public void KibritKutusunuAzalt()
    {
        KibritiKullan();
        Debug.Log("Kibrit kutusu bitti, envanterden kaldirildi.");
    }

    void KibritiKullan()
    {
        foreach (Slot slot in inventory.allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == kibritItemSO)
            {
                int amount = slot.GetAmount();
                if (amount <= 1) slot.ClearSlot();
                else slot.SetItem(kibritItemSO, amount - 1);
                return;
            }
        }
    }
}
