using UnityEngine;

public class BuzCatlama : MonoBehaviour
{
    [Header("Catlama Particle")]
    public ParticleSystem catlamaParticle;  // Inspector'dan ata
    public ParticleSystem suSicramaParticle; // Inspector'dan ata

    [Header("Kamera Sarsintisi")]
    public float sarsintiSiddeti = 0.3f;
    public float sarsintiSuresi = 0.4f;

    private Camera anaKamera;
    private bool sarsintiAktif = false;
    private float sarsintiZamani = 0f;
    private Vector3 kameraBaslangicPoz;

    void Start()
    {
        anaKamera = Camera.main;
    }

    void Update()
    {
        if (!sarsintiAktif) return;

        sarsintiZamani += Time.deltaTime;
        float kalan = 1f - (sarsintiZamani / sarsintiSuresi);

        if (kalan <= 0f)
        {
            sarsintiAktif = false;
            anaKamera.transform.localPosition = kameraBaslangicPoz;
            return;
        }

        // Kamera sarsintisi
        float x = Random.Range(-1f, 1f) * sarsintiSiddeti * kalan;
        float y = Random.Range(-1f, 1f) * sarsintiSiddeti * kalan;
        anaKamera.transform.localPosition = kameraBaslangicPoz + new Vector3(x, y, 0);
    }

    public void CatlamaOynat(Vector3 pozisyon)
    {
        // Particle efektleri
        if (catlamaParticle != null)
        {
            catlamaParticle.transform.position = pozisyon;
            catlamaParticle.Play();
        }

        if (suSicramaParticle != null)
        {
            suSicramaParticle.transform.position = pozisyon;
            suSicramaParticle.Play();
        }

        // Kamera sarsintisi
        if (anaKamera != null)
        {
            kameraBaslangicPoz = anaKamera.transform.localPosition;
            sarsintiAktif = true;
            sarsintiZamani = 0f;
        }
    }
}