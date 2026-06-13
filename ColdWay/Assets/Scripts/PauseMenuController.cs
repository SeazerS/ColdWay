using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject menuButtonsContainer; // Menu_Container
    public GameObject optionsPanel;         // Options_Panel

    public GameObject kayitIsimPanel;
    public TMPro.TMP_InputField isimInput;

    private bool isPaused = false;

    void Update()
    {
        // Oyuncu ESC (Escape) tuþuna bastýðýnda menü açýlýr/kapanýr
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        Debug.Log("Devam et butonuna týklandý!");
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // DÜZELTME: Oyuna dönüldüðünde fare gizlenmeli ve kilitlenmeli
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // StarterAssets kullanýyorsan oyuncunun etrafa bakabilmesini tekrar açýyoruz
        if (StarterAssets.FirstPersonController.Instance != null)
        {
            StarterAssets.FirstPersonController.Instance.CanLook = true;
        }
    }

    void PauseGame()
    {
        pausePanel.SetActive(true);
        menuButtonsContainer.SetActive(true);   // Menü her açýldýðýnda ilk baþta butonlar görünsün
        optionsPanel.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;

        // Menü açýldýðýnda fare serbest kalsýn ve görünsün
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Oyunu durdurunca oyuncu kamerayý çeviremesin
        if (StarterAssets.FirstPersonController.Instance != null)
        {
            StarterAssets.FirstPersonController.Instance.CanLook = false;
        }
    }

    // AYARLAR butonuna basýlýnca çalýþacak fonksiyon
    public void OpenOptions()
    {
        menuButtonsContainer.SetActive(false); // Ana butonlarý gizle
        optionsPanel.SetActive(true);          // Ayarlar panelini göster
    }

    public void OyunuKaydet()
    {
        menuButtonsContainer.SetActive(false);
        kayitIsimPanel.SetActive(true);

        // Mevcut kayýt adýný input'a doldur
        if (SaveSistemi.Instance != null && SaveSistemi.Instance.SaveVar())
        {
            // Mevcut kayýt adýný oku
            string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/save.json");
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

    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýlýyor...");
        Application.Quit();
    }
}