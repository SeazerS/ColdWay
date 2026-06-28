using UnityEngine;

public class MagaraYankiDuzenleme : MonoBehaviour
{
    private AudioReverbZone yankiBolgesi;

    void Start()
    {
        yankiBolgesi = GetComponent<AudioReverbZone>();

        if (yankiBolgesi != null)
        {
            yankiBolgesi.enabled = true;
            yankiBolgesi.reverbPreset = AudioReverbPreset.StoneCorridor;
            YankiyiKapat();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && yankiBolgesi != null)
        {
            // Sadece yankýyý açýyoruz, ayak sesine dokunmuyoruz
            yankiBolgesi.room = -1000;
            yankiBolgesi.roomHF = -237;
            Debug.Log("[MAÐARA] Maðaraya girildi, Yanký açýldý.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Yankýyý kapatýyoruz
            YankiyiKapat();
            Debug.Log("[MAÐARA] Maðaradan çýkýldý, Yanký kapatýldý.");
        }
    }

    void YankiyiKapat()
    {
        if (yankiBolgesi == null) return;
        yankiBolgesi.room = -10000;
        yankiBolgesi.roomHF = -10000;
    }
}