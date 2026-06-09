    using UnityEngine;

public class AgacKesme : MonoBehaviour
{
    [Header("Agac Tipi")]
    public bool kuruAagac = true; // Sadece kuru agaclar kesilebilir

    [Header("Kesme Ayarlari")]
    public int maxVurus = 3;
    private int kalanVurus;

    [Header("Odun Ayarlari")]
    public int odunMiktari = 2;
    public float odunFirlatmaKuvveti = 3f;

    void Start()
    {
        kalanVurus = maxVurus;
    }

    public void Vur(GameObject odunPrefab)
    {
        if (!kuruAagac) return;

        kalanVurus--;
        Debug.Log("Agaca vuruldu. Kalan: " + kalanVurus);

        if (kalanVurus <= 0)
            Devir(odunPrefab);
    }

    void Devir(GameObject odunPrefab)
    {
        // Odun spawna
        if (odunPrefab != null)
        {
            for (int i = 0; i < odunMiktari; i++)
            {
                Vector3 spawnPoz = transform.position +
                    Vector3.up * 1f +
                    Random.insideUnitSphere * 0.5f;

                GameObject odun = Instantiate(
                    odunPrefab, spawnPoz, Random.rotation);

                // Fizik varsa fýrlat
                Rigidbody rb = odun.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 yon = (Random.insideUnitSphere +
                                   Vector3.up).normalized;
                    rb.AddForce(yon * odunFirlatmaKuvveti,
                                ForceMode.Impulse);
                }
            }
        }

        Debug.Log("Agac devrildi! " + odunMiktari + " odun dustu.");
        gameObject.SetActive(false); // Agaci gizle
    }
}
