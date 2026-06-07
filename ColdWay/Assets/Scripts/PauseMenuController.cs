using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    // Unity içinden Pause_Panel'i buraya sürükle
    public GameObject pausePanel;
    public GameObject menuButtonsContainer; // Menu_Container
    public GameObject optionsPanel;         // Options_Panel

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
        Cursor.lockState = CursorLockMode.None; // Fareyi serbest býrak
        Cursor.visible = true;                  // Fareyi görünür yap
    }

    void PauseGame()
    {
        pausePanel.SetActive(true);
        menuButtonsContainer.SetActive(true);   // Menü her açýldýðýnda ilk baþta butonlar görünsün, ayarlar paneli gizli olsun
        optionsPanel.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None; // Menü açýldýðýnda fare kilitliyse çözülsün
        Cursor.visible = true;                  // Fare ekranda görünsün
    }

    // AYARLAR butonuna basýlýnca çalýþacak fonksiyon
    public void OpenOptions()
    {
        menuButtonsContainer.SetActive(false); // Ana butonlarý gizle
        optionsPanel.SetActive(true);          // Ayarlar panelini göster
    }

    // GERÝ butonuna basýlýnca çalýþacak fonksiyon
    public void CloseOptions()
    {
        optionsPanel.SetActive(false);          // Ayarlar panelini gizle
        menuButtonsContainer.SetActive(true);  // Ana butonlarý geri getir
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýlýyor...");
        Application.Quit();
    }
}