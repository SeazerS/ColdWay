using System.Collections;
using UnityEngine;

public class BozukLamba : MonoBehaviour
{
    // Artýk tek bir ýþýk deðil, ýþýklarýn bir listesini (array) tutuyoruz
    private Light[] tumLambalar;
    private AudioSource sesKaynagi;

    [Header("Zamanlama Ayarlari")]
    public float minNormalYanma = 5f;
    public float maxNormalYanma = 10f;

    [Header("Gorsel & Ses")]
    public AudioClip cizirtiSesi;
    public MeshRenderer ampulRenderer;
    public int materialIndex = 1;

    [ColorUsage(true, true)]
    public Color acikRenk = new Color(1f, 0.7f, 0.2f, 1f) * 2f;

    [Header("Ses Duyulma Alani")]
    public float minMesafe = 5f;
    public float maxMesafe = 35f;

    void Start()
    {
        // --- YENÝ: Objede ve içindeki tüm alt objelerde ne kadar ýþýk varsa hepsini bulur ---
        tumLambalar = GetComponentsInChildren<Light>();

        sesKaynagi = gameObject.AddComponent<AudioSource>();
        sesKaynagi.clip = cizirtiSesi;
        sesKaynagi.spatialBlend = 1f;
        sesKaynagi.minDistance = minMesafe;
        sesKaynagi.maxDistance = maxMesafe;
        sesKaynagi.rolloffMode = AudioRolloffMode.Linear;
        sesKaynagi.loop = true;
        sesKaynagi.Play();

        StartCoroutine(TitremeSistemi());
    }

    IEnumerator TitremeSistemi()
    {
        while (true)
        {
            DurumuAyarla(true);
            yield return new WaitForSeconds(Random.Range(minNormalYanma, maxNormalYanma));

            int titremeSayisi = Random.Range(2, 6);
            for (int i = 0; i < titremeSayisi; i++)
            {
                DurumuAyarla(false);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

                DurumuAyarla(true);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }
        }
    }

    void DurumuAyarla(bool acikMi)
    {
        foreach (Light l in tumLambalar)
        {
            if (l != null)
                l.enabled = acikMi;
        }

        sesKaynagi.volume = acikMi ? 0.7f : 0f;

        if (ampulRenderer != null)
        {
            Material mat = ampulRenderer.materials[materialIndex];
            if (acikMi)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", acikRenk);
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            }
        }
    }
}