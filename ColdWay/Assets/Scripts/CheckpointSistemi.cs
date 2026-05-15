using UnityEngine;

public class CheckpointSistemi : MonoBehaviour
{
    public static CheckpointSistemi Instance;

    [Header("Referanslar")]
    public Transform oyuncu;
    public SicaklikSistemi sicaklikSistemi;
    public EnerjiKontrol enerjiKontrol;

    [Header("Son Checkpoint")]
    public Vector3 sonCheckpointPoz;
    public bool checkpointVar = false;
    private bool olumIsleniyor = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (oyuncu != null)
            sonCheckpointPoz = oyuncu.position;
    }

    // Cadýr kurulunca cagir
    public void CheckpointKaydet(Vector3 poz)
    {
        sonCheckpointPoz = poz;
        checkpointVar = true;
        Debug.Log($"Checkpoint kaydedildi: {poz}");
    }

    public void OlumGerceklesti()
    {
        if (olumIsleniyor) return;
        olumIsleniyor = true;
        StartCoroutine(OlumSonrasiDon());
    }
    // Uyku sisteminden cagir - coroutine'i iptal et
    public void OlumIptal()
    {
        olumIsleniyor = false;
        StopAllCoroutines();
    }

    System.Collections.IEnumerator OlumSonrasiDon()
    {
        yield return new WaitForSeconds(1.5f);
        if (!olumIsleniyor) yield break; // Iptal edildiyse dur

        if (oyuncu != null)
            oyuncu.position = sonCheckpointPoz;

        sicaklikSistemi?.Oldu_Sifirla();
        enerjiKontrol?.Oldu_Sifirla();
        olumIsleniyor = false;
    }
}
