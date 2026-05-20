using UnityEngine;

public class GoletSistemi : MonoBehaviour
{
    [Header("Ayarlar")]
    public int catlamaAdimi = 20;       // Kac adimda catlasýn
    public float adimMesafesi = 0.6f;   // Her adim kac metre

    [Header("Referanslar")]
    public BuzCatlama buzCatlama;       // Inspector'dan surukle
    public SicaklikSistemi sicaklik;    // Inspector'dan surukle

    private bool buzIcinde = false;
    private bool catladi = false;
    private int adimSayisi = 0;
    private Vector3 sonPozisyon;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        buzIcinde = true;
        catladi = false;
        adimSayisi = 0;
        sonPozisyon = other.transform.position;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        buzIcinde = false;
    }

    void Update()
    {
        if (!buzIcinde || catladi) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float mesafe = Vector3.Distance(player.transform.position, sonPozisyon);
        if (mesafe >= adimMesafesi)
        {
            adimSayisi++;
            sonPozisyon = player.transform.position;

            if (adimSayisi >= catlamaAdimi)
                BuzuCatlat(player.transform.position);
        }
    }

    void BuzuCatlat(Vector3 pozisyon)
    {
        catladi = true;

        // Buz catlama efekti
        if (buzCatlama != null)
            buzCatlama.CatlamaOynat(pozisyon);

        // Sicaklik sistemine ýslanma bildir
        if (sicaklik != null)
            sicaklik.GoleteGirdi();

        Debug.Log("Buz catladi! Oyuncu islandi.");
    }
}
