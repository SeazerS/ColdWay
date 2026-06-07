using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Sahneleri yüklemek için bu kütüphane þarttýr

public class MainMenuController : MonoBehaviour
{
    // Hayatta kalma oyununun olduðu sahnenin tam adýný buraya yazacaðýz
    [SerializeField] private string gameplaySceneName = "GameScene";

    public void NewGame()
    {
        Debug.Log("Oyun yükleniyor...");
        // Zaman donuk kaldýysa (Pause menüsünden ötürü) normal akýþýna döndür
        Time.timeScale = 1f;

        // Ýsmini verdiðimiz oyun sahnesini yükle
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýlýyor...");
        Application.Quit();
    }
}