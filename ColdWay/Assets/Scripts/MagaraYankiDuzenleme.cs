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
            yankiBolgesi.room = -1000; 
            yankiBolgesi.roomHF = -237;

            Debug.Log("[MAÐARA] Yanký Gücü Açýldý: Sesi bozmadan geçiþ yapýldý.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && yankiBolgesi != null)
        {
            YankiyiKapat();

            Debug.Log("[MAÐARA] Yanký Gücü Kapatýldý: Normal yürüme sesi korunuyor.");
        }
    }

    void YankiyiKapat()
    {
        if (yankiBolgesi == null) return;
        yankiBolgesi.room = -10000;
        yankiBolgesi.roomHF = -10000;
    }
}