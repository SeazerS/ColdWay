using UnityEngine;

public class GoletSistemi : MonoBehaviour
{
    [Header("Ayarlar")]
    public float kirilmaSuresi = 3f; // kaç saniyede kýrýlsýn

    [Header("Referanslar")]
    public BuzCatlama buzCatlama;
    public SicaklikSistemi sicaklik;

    [Header("Gorsel")]
    public GameObject suYuzeyi;
    public float suBelirsuresi = 2f;

    private bool buzIcinde = false;
    private bool catladi = false;
    private float goletteGecenSure = 0f;

    private bool suFadeBasladi = false;
    private float suFadeZamani = 0f;
    private Vector3 suHedefScale;

    void Start()
    {
        if (suYuzeyi != null)
        {
            suHedefScale = suYuzeyi.transform.localScale;
            suYuzeyi.transform.localScale = Vector3.zero;
            suYuzeyi.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        buzIcinde = true;
        goletteGecenSure = 0f; // gölete girince sayaç sýfýrla
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        buzIcinde = false;
        goletteGecenSure = 0f; // çýkýnca sýfýrla
    }

    void Update()
    {
        // Su fade
        if (suFadeBasladi && suYuzeyi != null)
        {
            suFadeZamani += Time.deltaTime;
            float t = Mathf.Clamp01(suFadeZamani / suBelirsuresi);
            suYuzeyi.transform.localScale = Vector3.Lerp(
                Vector3.zero, suHedefScale, t);
            if (t >= 1f)
            {
                suYuzeyi.transform.localScale = suHedefScale;
                suFadeBasladi = false;
            }
        }

        if (!buzIcinde || catladi) return;

        // Gölet içinde geçen süreyi say
        goletteGecenSure += Time.deltaTime;

        if (goletteGecenSure >= kirilmaSuresi)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                BuzuCatlat(player.transform.position);
        }
    }

    void BuzuCatlat(Vector3 pozisyon)
    {
        catladi = true;

        if (suYuzeyi != null)
        {
            suYuzeyi.SetActive(true);
            suYuzeyi.transform.localScale = Vector3.zero;
            suFadeBasladi = true;
            suFadeZamani = 0f;
        }

        if (buzCatlama != null)
            buzCatlama.CatlamaOynat(pozisyon);

        if (sicaklik != null)
            sicaklik.GoleteGirdi();
    }
}