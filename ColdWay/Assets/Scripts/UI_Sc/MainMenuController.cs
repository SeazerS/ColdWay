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
        PlayerPrefs.SetInt("YeniOyun", 1);
        PlayerPrefs.SetInt("SaveYukle", 0);
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSaveSlots()
    {
        menuArea.SetActive(false);
        saveSlotsPanel.SetActive(true);
    }

    public void CloseSaveSlots()
    {
        saveSlotsPanel.SetActive(false);
        menuArea.SetActive(true);
    }

    public void OpenOptions()
    {
        menuArea.SetActive(false);    // Ana butonlarý gizle
        optionsPanel.SetActive(true); // Ayarlar panelini göster
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýlýyor...");
        Application.Quit();
    }
}