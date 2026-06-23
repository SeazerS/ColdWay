using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KibritMinigame : MonoBehaviour
{
    [Header("UI Elemanlari")]
    public GameObject minigameCanvas;
    public GameObject minigamePanel;
    public RectTransform oyunAlani;
    public RectTransform surtmeAlani;
    public RectTransform hedefNokta;
    public RectTransform kibritObj;
    public Image dolumBar;
    public TextMeshProUGUI minigameIpucuText;
    public TextMeshProUGUI kibritSayacText;

    [Header("Zorluk Ayarlari")]
    [Range(0.1f, 2f)] public float artisHizi = 1.2f;
    [Range(0.05f, 0.5f)] public float dususHizi = 0.08f;
    [Range(20f, 60f)] public float hedefYaricap = 40f;
    [Range(1f, 5f)] public float hedefHareketSuresi = 3f;

    [Header("Kibrit Kutusu")]
    public int kibritBasinaKullanim = 3;
    private int kalanKibrit;

    [Header("Hava Sartlari")]
    public SicaklikSistemi sicaklikSistemi;

    [Header("Gece Ayarlari")]
    public float geceZorluCarpani = 2.5f;

    [Header("Envanter")]
    public Inventory inventory;
    public ItemSO kibritItemSO;

    public KivilcimSistemi kivilcimSistemi;

    private AteþNoktasi aktifAtesNoktasi;
    private float bar = 0f;
    private bool aktif = false;
    private bool basiliMi = false;
    private float hedefZamanlayici = 0f;
    private Vector2 hedefPoz;
    private Vector2 sonFare, oncekiFare;
    private bool kibritKirildi = false;
    private float kirilmaZamanlayici = 0f;
    private float kirilmaBekleme = 1.5f;

    // Ses için zamanlayýcý
    private float surtmeSesZamanlayici = 0f;

    void Start()
    {
        if (minigamePanel != null) minigamePanel.SetActive(false);
    }

    void Update()
    {
        if (!aktif) return;


        // Fýrtýnada minigame kapat
        if (FirtinaSistemi.Instance != null &&
            FirtinaSistemi.Instance.FirtinaAktifMi())
        {
            IpucuYoneticisi.Instance?.MesajGoster(
                "ates", "Fýrtýnada ateþ yakýlamaz!");
            Kapat();
            return;
        }

        if (kibritKirildi)
        {
            kirilmaZamanlayici += Time.deltaTime;
            if (kirilmaZamanlayici >= kirilmaBekleme)
                SonrakiKibrit();
            return;
        }

        HedefGuncelle();
        FareGuncelle();
        if (bar >= 1f) Basarili();
    }

    public void Baslat(AteþNoktasi atestNoktasi)
    {
        aktifAtesNoktasi = atestNoktasi;
        Slot secilenSlot = inventory.hotbarSlots[inventory.equippedHotbarIndex];
        kalanKibrit = (secilenSlot.HasItem() && secilenSlot.GetItem() == kibritItemSO)
            ? secilenSlot.GetAmount()
            : 0;
        aktif = true;
        bar = 0f;
        basiliMi = false;
        kibritKirildi = false;
        hedefZamanlayici = 0f;
        surtmeSesZamanlayici = 0f;

        if (minigameCanvas != null) minigameCanvas.SetActive(true);
        if (minigamePanel != null) minigamePanel.SetActive(true);
        if (dolumBar != null) dolumBar.fillAmount = 0f;

        SayacGuncelle();
        HedefYeniPoz();
        IpucuGuncelle("sol týk basýlý tut + kýrmýzý hedefe sürt");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (StarterAssets.FirstPersonController.Instance != null)
            StarterAssets.FirstPersonController.Instance.CanLook = false;
    }

    void HedefGuncelle()
    {
        hedefZamanlayici += Time.deltaTime;
        if (hedefZamanlayici >= hedefHareketSuresi)
        {
            hedefZamanlayici = 0f;
            HedefYeniPoz();
        }
    }

    void HedefYeniPoz()
    {
        if (surtmeAlani != null)
        {
            float w = surtmeAlani.rect.width * 0.3f;
            float h = surtmeAlani.rect.height * 0.25f;
            hedefPoz = new Vector2(Random.Range(-w, w), Random.Range(-h, h));
        }
        else
        {
            hedefPoz = new Vector2(
                Random.Range(-80f, 80f),
                Random.Range(-20f, 20f));
        }
        hedefNokta.anchoredPosition = hedefPoz;
    }

    void FareGuncelle()
    {
        Vector2 fareLocalPoz;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            oyunAlani, Input.mousePosition, null, out fareLocalPoz);

        float yarimW = oyunAlani.rect.width * 0.48f;
        float yarimH = oyunAlani.rect.height * 0.48f;
        fareLocalPoz.x = Mathf.Clamp(fareLocalPoz.x, -yarimW, yarimW);
        fareLocalPoz.y = Mathf.Clamp(fareLocalPoz.y, -yarimH, yarimH);

        oncekiFare = sonFare;
        sonFare = fareLocalPoz;
        kibritObj.anchoredPosition = fareLocalPoz;

        basiliMi = Input.GetMouseButton(0);
        float hareketMiktari = (sonFare - oncekiFare).magnitude;
        float uzaklik = Vector2.Distance(fareLocalPoz, hedefPoz);
        bool hedefdeMi = uzaklik < hedefYaricap;

        // --- YENÝ SES SÝSTEMÝ (Hedeften Baðýmsýz) ---
        // 1. Oyuncu sol týka ÝLK bastýðý an sesi kesin çýkar
        if (Input.GetMouseButtonDown(0))
        {
            if (StarterAssets.AudioManager.instance != null)
                StarterAssets.AudioManager.instance.Play("Kibrit_Yakma");
            surtmeSesZamanlayici = 0.25f;
        }
        // 2. Basýlý tutmaya ve hafifçe hareket etmeye (sürtmeye) devam ettiði sürece çal
        else if (basiliMi && hareketMiktari > 0.1f)
        {
            surtmeSesZamanlayici -= Time.deltaTime;
            if (surtmeSesZamanlayici <= 0f)
            {
                if (StarterAssets.AudioManager.instance != null)
                    StarterAssets.AudioManager.instance.Play("Kibrit_Yakma");
                surtmeSesZamanlayici = 0.25f;
            }
        }
        else if (!basiliMi)
        {
            surtmeSesZamanlayici = 0f;
        }

        // --- KÝBRÝT KIRILMA KONTROLÜ ---
        if (surtmeAlani != null && basiliMi)
        {
            Vector2 surtmeFarePoz;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                surtmeAlani, Input.mousePosition, null, out surtmeFarePoz);

            float sinirW = surtmeAlani.rect.width * 0.5f + 5f;
            float sinirH = surtmeAlani.rect.height * 0.5f + 5f;

            bool disinda = Mathf.Abs(surtmeFarePoz.x) > sinirW ||
                           Mathf.Abs(surtmeFarePoz.y) > sinirH;

            if (disinda && hareketMiktari > 3f)
            {
                KibritiKir();
                return;
            }
        }

        // --- ZORLUK VE BAR DOLUMU HESAPLAMALARI ---
        bool ruzgar = sicaklikSistemi != null && sicaklikSistemi.ruzgarda;
        bool islak = sicaklikSistemi != null && sicaklikSistemi.ayakIslak;
        bool geceVakti = sicaklikSistemi != null && sicaklikSistemi.geceBonusu;
        bool alacakaranlik = sicaklikSistemi != null && sicaklikSistemi.alacakaranlýkBonusu;

        float artis = islak ? artisHizi * 0.6f : artisHizi;
        float dusus = ruzgar ? dususHizi * 1.6f : dususHizi;

        if (geceVakti)
        {
            dusus *= geceZorluCarpani;
            artis *= 0.5f;
            hedefYaricap = 25f;
            hedefHareketSuresi = 1.5f;
        }
        else if (alacakaranlik)
        {
            dusus *= geceZorluCarpani * 0.5f;
            artis *= 0.75f;
            hedefYaricap = 32f;
            hedefHareketSuresi = 2f;
        }
        else
        {
            hedefYaricap = 40f;
            hedefHareketSuresi = 3f;
        }

        if (basiliMi && hareketMiktari > 0.3f)
        {
            if (hedefdeMi)
            {
                bar += artis * Time.deltaTime;
                IpucuGuncelle("iyi gidiyorsun — devam et!");
                kivilcimSistemi?.Baslat();
            }
            else
            {
                bar -= dusus * Time.deltaTime;
                IpucuGuncelle("kýrmýzý hedefe daha yakýn ol");
                kivilcimSistemi?.Durdur();
            }
        }
        else if (basiliMi)
        {
            bar -= dusus * 0.5f * Time.deltaTime;
            IpucuGuncelle("ileri geri hareket et");
        }
        else
        {
            bar -= dusus * Time.deltaTime;
            IpucuGuncelle("sol týk basýlý tutarak sürt");
            kivilcimSistemi?.Durdur();
        }

        bar = Mathf.Clamp01(bar);

        if (dolumBar != null)
        {
            dolumBar.fillAmount = bar;
            dolumBar.color = Color.white;
        }
    }

    void KibritiKir()
    {
        kalanKibrit--;
        Slot secilenSlot = inventory.hotbarSlots[inventory.equippedHotbarIndex];
        if (secilenSlot.HasItem() && secilenSlot.GetItem() == kibritItemSO)
            secilenSlot.RemoveAmount(1);

        kibritKirildi = true;
        kirilmaZamanlayici = 0f;
        bar = 0f;
        if (dolumBar != null) dolumBar.fillAmount = 0f;
        SayacGuncelle();

        if (kalanKibrit <= 0)
            IpucuGuncelle("kibrit bitti!");
        else
            IpucuGuncelle("kibrit söndü! " + kalanKibrit + " kibrit kaldý...");
    }

    void SonrakiKibrit()
    {
        kibritKirildi = false;
        kirilmaZamanlayici = 0f;

        if (kalanKibrit <= 0)
        {
            if (aktifAtesNoktasi != null)
                aktifAtesNoktasi.KibritKutusunuAzalt();
            Kapat();
            return;
        }

        bar = 0f;
        if (dolumBar != null) dolumBar.fillAmount = 0f;
        HedefYeniPoz();
        IpucuGuncelle("sol týk basýlý tut + kýrmýzý hedefe sürt");
    }

    void SayacGuncelle()
    {
        if (kibritSayacText != null)
            kibritSayacText.text = "Kibrit: " + kalanKibrit;
    }

    void IpucuGuncelle(string mesaj)
    {
        if (minigameIpucuText != null)
            minigameIpucuText.text = mesaj;
    }

    [Header("Ates Simgesi")]
    public Image atesSimgesi;
    public float atesSimgesiGosterSuresi = 1.8f;

    void Basarili()
    {
        aktif = false;
        bar = 1f;

        if (atesSimgesi != null)
            atesSimgesi.gameObject.SetActive(true);

        StartCoroutine(BasariliGecikme());
        Debug.Log("Ates yandi!");
    }

    System.Collections.IEnumerator BasariliGecikme()
    {
        yield return new UnityEngine.WaitForSeconds(atesSimgesiGosterSuresi);

        if (atesSimgesi != null)
            atesSimgesi.gameObject.SetActive(false);

        Kapat();

        if (aktifAtesNoktasi != null)
            aktifAtesNoktasi.AteþiYak();
    }

    public void Kapat()
    {
        aktif = false;
        bar = 0f;
        kibritKirildi = false;
        if (minigamePanel != null) minigamePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (StarterAssets.FirstPersonController.Instance != null)
            StarterAssets.FirstPersonController.Instance.CanLook = true;
        kivilcimSistemi?.Temizle();
    }
}