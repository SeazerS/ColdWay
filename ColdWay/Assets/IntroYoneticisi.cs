using UnityEngine;
using UnityEngine.Playables;

public class IntroYoneticisi : MonoBehaviour
{
    public PlayableDirector director;

    [Header("UI")]
    public GameObject[] introSuresinceSaklanacakUI;

    [Header("Fog Ayarlari")]
    public float introFogYogunluk = 0.003f; // intro sırasında az fog
    private float normalFogYogunluk;

    [Header("Referanslar")]
    public GecGunduzSistemi gecGunduz;

    void Start()
    {
        // GecGunduz fog güncellemesini durdur
        if (gecGunduz != null)
            gecGunduz.enabled = false;

        // Intro için fog'u azalt
        RenderSettings.fogDensity = introFogYogunluk;

        // Normal fog değerini kaydet
        normalFogYogunluk = RenderSettings.fogDensity;


        // UI'ları gizle
        foreach (GameObject ui in introSuresinceSaklanacakUI)
            if (ui != null) ui.SetActive(false);

        if (StarterAssets.FirstPersonController.Instance != null)
            StarterAssets.FirstPersonController.Instance.enabled = false;

        director.stopped += IntroBitti;
    }

    void IntroBitti(PlayableDirector pd)
    {
        // GecGunduz'u tekrar aktif et
        if (gecGunduz != null)
            gecGunduz.enabled = true;

        // Fog'u normale döndür
        RenderSettings.fogDensity = normalFogYogunluk;

        // UI'ları göster
        foreach (GameObject ui in introSuresinceSaklanacakUI)
            if (ui != null) ui.SetActive(true);

        if (StarterAssets.FirstPersonController.Instance != null)
            StarterAssets.FirstPersonController.Instance.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
