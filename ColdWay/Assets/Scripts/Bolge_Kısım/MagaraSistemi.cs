using UnityEngine;

public class MagaraSistemi : MonoBehaviour
{
    [Header("Referanslar")]
    public SicaklikSistemi sicaklikSistemi;
    public EnerjiKontrol enerjiKontrol;

    [Header("Sicaklik Ayarlari")]
    public float sicaklikDususCarpani = 0.5f;

    [Header("Enerji Ayarlari")]
    public float enerjiDususCarpani = 1.2f;

    private bool oyuncuIcerde = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuIcerde = true;

        if (sicaklikSistemi != null)
            sicaklikSistemi.magaraCarpani = sicaklikDususCarpani;

        if (enerjiKontrol != null)
            enerjiKontrol.magaraCarpani = enerjiDususCarpani;

        IpucuYoneticisi.Instance?.MesajGoster(
            "magara", "Maðara içindesin — daha az ýsý kaybediyorsun");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuIcerde = false;

        if (sicaklikSistemi != null)
            sicaklikSistemi.magaraCarpani = 1f;

        if (enerjiKontrol != null)
            enerjiKontrol.magaraCarpani = 1f;

        IpucuYoneticisi.Instance?.MesajGizle("magara");
    }
}