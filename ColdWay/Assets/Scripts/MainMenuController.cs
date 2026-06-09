using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // sahnenin tam adýný yaz
    [SerializeField] private string gameplaySceneName = "GameScene";

    public GameObject menuArea;
    public GameObject optionsPanel;
    public GameObject saveSlotsPanel; // Kayit Slotlari paneli

    public void NewGame()
    {
        Debug.Log("Oyun yükleniyor...");
        // normal akýþýna döndür
        Time.timeScale = 1f;


        SceneManager.LoadScene(gameplaySceneName);
    }

    // YENI Yükle butonuna basýnca slot panelini açar
    public void OpenSaveSlots()
    {
        menuArea.SetActive(false);
        saveSlotsPanel.SetActive(true);
    }

    // YENI Slot panelindeki Geri butonuna basýnca menüye döner
    public void CloseSaveSlots()
    {
        saveSlotsPanel.SetActive(false);
        menuArea.SetActive(true);
    }



    // AYARLAR butonuna basýnca çalýþacak
    public void OpenOptions()
    {
        menuArea.SetActive(false);    // Ana butonlarý gizle
        optionsPanel.SetActive(true); // Ayarlar panelini göster
    }

    // GERI butonuna basýnca çalýþacak
    public void CloseOptions()
    {
        optionsPanel.SetActive(false); // Ayarlar panelini gizle
        menuArea.SetActive(true);      // Ana butonlarý geri getir
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýlýyor...");
        Application.Quit();
    }
}