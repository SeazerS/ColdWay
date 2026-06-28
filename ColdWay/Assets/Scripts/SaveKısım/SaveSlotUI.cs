using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI Referanslar")]
    public GameObject slotObje;
    public TextMeshProUGUI slotAdiText;
    public TextMeshProUGUI tarihText;
    public Button slotButon;

    [Header("Sahne")]
    public string gameplaySceneName = "GameScene";

    private string savePath =>
        Application.persistentDataPath + "/save.json";

    void Start()
    {
        SlotuGuncelle();
    }

    void OnEnable()
    {
        SlotuGuncelle();
    }

    void SlotuGuncelle()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (slotObje != null) slotObje.SetActive(true);
            if (slotAdiText != null) slotAdiText.text = data.kayitAdi;
            if (tarihText != null) tarihText.text = "Kayit Tarihi - " + data.kayitZamani;
            if (slotButon != null) slotButon.interactable = true;
        }
        else
        {
            if (slotAdiText != null) slotAdiText.text = "Kayit Yok";
            if (tarihText != null) tarihText.text = "";
            if (slotButon != null) slotButon.interactable = false;
        }
    }

    public void SlotaBasildi()
    {
        if (!File.Exists(savePath)) return;
        PlayerPrefs.SetInt("SaveYukle", 1); // yükle flag
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }
}
