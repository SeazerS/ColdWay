using UnityEngine;

public class BolgeYoneticisi : MonoBehaviour
{
    public static BolgeYoneticisi Instance;

    public int mevcutBolge = 1;

    [Header("Sistem Referanslari")]
    public KarTakip karSistemi;       // Inspector'dan surukle
    public RuzgarSistemi ruzgarSistemi; // Inspector'dan surukle

    private EnerjiKontrol enerjiKontrol;
    private SicaklikSistemi sicaklikSistemi;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            enerjiKontrol = player.GetComponent<EnerjiKontrol>();
            sicaklikSistemi = player.GetComponent<SicaklikSistemi>();
        }
    }

    public void BolgeGecisi(int yeniBolgeNo)
    {
        if (yeniBolgeNo == mevcutBolge) return;
        mevcutBolge = yeniBolgeNo;
        Debug.Log("Bolge " + mevcutBolge + "'e gecildi.");

        enerjiKontrol?.BolgeGuncelle(mevcutBolge);
        sicaklikSistemi?.BolgeGuncelle(mevcutBolge);
        karSistemi?.BolgeGuncelle(mevcutBolge);
        ruzgarSistemi?.BolgeGuncelle(mevcutBolge);
    }
}