using UnityEngine;
using StarterAssets;

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
        if (other.CompareTag("Player"))
        {
            if (yankiBolgesi != null)
            {
                yankiBolgesi.room = -1000;
                yankiBolgesi.roomHF = -237;
            }

            if (FirstPersonController.Instance != null)
            {
                if (AudioManager.instance != null) AudioManager.instance.Stop(FirstPersonController.Instance.currentFootstepSound);
                FirstPersonController.Instance.currentFootstepSound = "Magara_Yurume_Sesi";
            }

            Debug.Log("[MAÐARA] Maðara yürüme sesine geçildi ve Yanký açýldý.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            YankiyiKapat();

            if (FirstPersonController.Instance != null)
            {
                if (AudioManager.instance != null) AudioManager.instance.Stop(FirstPersonController.Instance.currentFootstepSound);
                FirstPersonController.Instance.currentFootstepSound = "Yurume_Sesi";
            }

            Debug.Log("[MAÐARA] Normal yürüme sesine geçildi ve Yanký kapatýldý.");
        }
    }

    void YankiyiKapat()
    {
        if (yankiBolgesi == null) return;
        yankiBolgesi.room = -10000;
        yankiBolgesi.roomHF = -10000;
    }
}