using UnityEngine;
using StarterAssets;

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
    private ParticleSystem dumanPS;

    [Header("Ayarlar")]
    public float etkilesimMesafesi = 3f;
    public int gerekliOdun = 3;

    private bool oyuncuYakinda = false;

    void Start()
    {
        dumanPS = dumanParticle != null
            ? dumanParticle.GetComponentInChildren<ParticleSystem>(true)
            : null;

        DumanBaslat();
    }

    public void DumanBaslat()
    {
        if (dumanParticle == null) return;

        dumanParticle.SetActive(true);

        ParticleSystem[] tumPS = dumanParticle.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem p in tumPS)
        {
            p.gameObject.SetActive(true);
            p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            p.Play();
        }
    }

    public void DumanDurdur()
    {
        if (dumanParticle == null) return;
        if (dumanPS != null)
            dumanPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        dumanParticle.SetActive(false);
    }

    void Update()
    {
        if (!oyuncuYakinda) return;

        KeyCode etkilesimTusu = SettingsManager.Instance.GetKey("Interaksiyon");

        if (Input.GetKeyDown(etkilesimTusu))
        {
            // Ateş yanıyorsa odun ekle
            if (atesSistemi != null && atesSistemi.YaniyorMu())
            {
                // Yeni yakıldıysa bu frame'i atla
                //if (atesSistemi.YeniYakilan()) return;
                atesSistemi.OdunEkle();
                return;
            }

            // Ateş yanmıyorsa yak
            if (!OdunVarMi())
            {
                IpucuYoneticisi.Instance.MesajGoster(
                    "ates", gerekliOdun + " odun gerekli!");
                return;
            }
            if (!KibritVarMi())
            {
                IpucuYoneticisi.Instance.MesajGoster(
                    "ates", "Kibrit gerekli!");
                return;
            }
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

    public void GuncelleMesaj()
    {
        string tusAdi = SettingsManager.Instance.GetKey("Interaksiyon").ToString();

        if (atesSistemi != null && atesSistemi.YaniyorMu())
        {
            IpucuYoneticisi.Instance.MesajGoster("ates", tusAdi + " — Odun Ekle");
            return;
        }

        if (!OdunVarMi())
            IpucuYoneticisi.Instance.MesajGoster("ates", gerekliOdun + " odun gerekli");
        else if (!KibritVarMi())
            IpucuYoneticisi.Instance.MesajGoster("ates", "Kibrit gerekli");
        else
            IpucuYoneticisi.Instance.MesajGoster("ates", tusAdi + " — Ateş Kur");
    }

    public void AteşiYak()
    {
        OdunuKaldir(gerekliOdun);
        KibritiKullan();

        // Dumanı durdur
        DumanDurdur();

        // Ateş sistemini ateşle (Ses artık AtesBas fonksiyonunun içinde çalacak)
        if (atesSistemi != null)
            atesSistemi.AtesBas(gerekliOdun);

        GuncelleMesaj();
    }

    // Ateş sönünce AtesSistemi burayı çağırır
    public void AtesSondu()
    {
        DumanYogunlukAyarla(1f);
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

    public void KibritKutusunuAzalt()
    {
        KibritiKullan();
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
        Slot secilenSlot = inventory.hotbarSlots[inventory.equippedHotbarIndex];
        return secilenSlot.HasItem() && secilenSlot.GetItem() == kibritItemSO;
    }

    public void DumanYogunlukAyarla(float oran)
    {
        if (dumanParticle == null) return;

        if (!dumanParticle.activeSelf)
            dumanParticle.SetActive(true);

        ParticleSystem[] tumPS = dumanParticle
            .GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in tumPS)
        {
            if (!ps.isPlaying) ps.Play();
            var emission = ps.emission;
            var rate = emission.rateOverTime;
            rate.constant = Mathf.Lerp(0f, 10f, oran);
            emission.rateOverTime = rate;
        }
    }
}