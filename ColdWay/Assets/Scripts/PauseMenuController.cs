using System.Collections;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject menuButtonsContainer;
    public GameObject optionsPanel;
    public GameObject kayitIsimPanel;
    public TMPro.TMP_InputField isimInput;

    [Header("Debug")]
    public Light gunesIsigi;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (StarterAssets.FirstPersonController.Instance != null)
            StarterAssets.FirstPersonController.Instance.CanLook = true;

        float exp = RenderSettings.skybox != null &&
                    RenderSettings.skybox.HasProperty("_Exposure")
                    ? RenderSettings.skybox.GetFloat("_Exposure") : -1f;
    }

    void PauseGame()
    {
        float exp = RenderSettings.skybox != null &&
                    RenderSettings.skybox.HasProperty("_Exposure")
                    ? RenderSettings.skybox.GetFloat("_Exposure") : -1f;
        pausePanel.SetActive(true);
        menuButtonsContainer.SetActive(true);
        optionsPanel.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (StarterAssets.FirstPersonController.Instance != null)
            StarterAssets.FirstPersonController.Instance.CanLook = false;
    }

    public void OpenOptions()
    {
        menuButtonsContainer.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OyunuKaydet()
    {
        menuButtonsContainer.SetActive(false);
        kayitIsimPanel.SetActive(true);

        if (SaveSistemi.Instance != null && SaveSistemi.Instance.SaveVar())
        {
            string json = System.IO.File.ReadAllText(
                Application.persistentDataPath + "/save.json");
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (isimInput != null) isimInput.text = data.kayitAdi;
        }
        else
        {
            if (isimInput != null) isimInput.text = "";
        }
    }

    public void KayitOnayla()
    {
        string isim = isimInput.text.Trim();
        if (string.IsNullOrEmpty(isim))
            isim = "Kayit Dosyasi 1";

        if (SaveSistemi.Instance != null)
            SaveSistemi.Instance.KaydetIsimle(isim);

        kayitIsimPanel.SetActive(false);
        menuButtonsContainer.SetActive(true);
    }

    public void KayitIptal()
    {
        kayitIsimPanel.SetActive(false);
        menuButtonsContainer.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        menuButtonsContainer.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}