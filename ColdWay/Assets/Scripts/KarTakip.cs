using UnityEngine;

public class KarTakip : MonoBehaviour
{
    public Transform karakter;
    private ParticleSystem karPartikul;

    void Start()
    {
        karPartikul = GetComponent<ParticleSystem>();

        if (karakter == null) return;

        // Karakterin üstünde baþlat
        transform.position = new Vector3(
            karakter.position.x,
            karakter.position.y + 15f,
            karakter.position.z);

        // Anýnda doldur
        karPartikul.Simulate(5f, true, true);
        karPartikul.Play();
    }

    void LateUpdate()
    {
        if (karakter == null) return;

        // Her karede takip et
        transform.position = new Vector3(
            karakter.position.x,
            karakter.position.y + 15f,
            karakter.position.z);

        // Rotasyon sabit
        transform.rotation = Quaternion.identity;
    }

    public void SahneGecisYenile()
    {
        if (karakter == null) return;

        karPartikul.Stop();
        karPartikul.Clear();

        transform.position = new Vector3(
            karakter.position.x,
            karakter.position.y + 15f,
            karakter.position.z);

        karPartikul.Simulate(5f, true, true);
        karPartikul.Play();

        Debug.Log("Kar sistemi sýfýrlandý.");
    }
}
