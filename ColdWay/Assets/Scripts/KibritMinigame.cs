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

    void Start()
    {
        if (minigamePanel != null) minigamePanel.SetActive(false);
    }

    void Update()
    {
        if (!aktif) return;

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
        kalanKibrit = kibritBasinaKullanim;
        aktif = true;
        bar = 0f;
        basiliMi = false;
        kibritKirildi = false;
        hedefZamanlayici = 0f;

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

        // Surtme alani disina cikinca kibrit kirilir
        // SurtmeAlani koordinatinda fareyi kontrol et
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

        bool ruzgar = sicaklikSistemi != null && sicaklikSistemi.ruzgarda;
        bool islak = sicaklikSistemi != null && sicaklikSistemi.ayakIslak;
        float artis = islak ? artisHizi * 0.6f : artisHizi;
        float dusus = ruzgar ? dususHizi * 1.6f : dususHizi;

        if (basiliMi && hareketMiktari > 0.3f)
        {
            if (hedefdeMi)
            {
                bar += artis * Time.deltaTime;
                IpucuGuncelle("iyi gidiyorsun — devam et!");
            }
            else
            {
                bar -= dusus * Time.deltaTime;
                IpucuGuncelle("kýrmýzý hedefe daha yakýn ol");
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
        }

        bar = Mathf.Clamp01(bar);

        if (dolumBar != null)
        {
            dolumBar.fillAmount = bar;
            if (bar < 0.35f) dolumBar.color = new Color(0.53f, 0.53f, 0.5f);
            else if (bar < 0.7f) dolumBar.color = new Color(0.73f, 0.46f, 0.09f);
            else dolumBar.color = new Color(0.89f, 0.29f, 0.29f);
        }
    }

    void KibritiKir()
    {
        kalanKibrit--;
        kibritKirildi = true;
        kirilmaZamanlayici = 0f;
        bar = 0f;
        if (dolumBar != null) dolumBar.fillAmount = 0f;
        SayacGuncelle();

        if (kalanKibrit <= 0)
            IpucuGuncelle("kibrit kutusu bitti!");
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
            kibritSayacText.text = "Kibrit: " + kalanKibrit
                                 + " / " + kibritBasinaKullanim;
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
        // Oyunu durdur ama paneli kapat
        aktif = false;
        bar = 1f;

        // Ates simgesini goster
        if (atesSimgesi != null)
            atesSimgesi.gameObject.SetActive(true);

        // Gecikme ile kapat
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
    }
}