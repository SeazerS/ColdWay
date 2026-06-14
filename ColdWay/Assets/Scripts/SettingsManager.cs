using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Menu Navigation")]
    public GameObject optionsPanel;
    public GameObject menuArea;

    [Header("Tab Panels")]
    public GameObject graphicsPanel;
    public GameObject audioPanel;
    public GameObject keysPanel;

    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider sfxSlider;

    [Header("Graphics & Display")]
    public TMP_Dropdown graphicsDropdown;
    public TMP_Dropdown displayDropdown;

    [Header("Controls")]
    public Slider sensitivitySlider;

    [Header("Keybinding Text Elements")]
    public TMP_Text forwardText;
    public TMP_Text backwardText;
    public TMP_Text leftText;
    public TMP_Text rightText;
    public TMP_Text jumpText;
    public TMP_Text sprintText;
    public TMP_Text inventoryText;
    public TMP_Text interactText;
    public TMP_Text sleepText;


    private Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();
    private string waitingForKey = "";

    private int currentTabIndex = 0;

    void Awake()
    {
        Instance = this;
        LoadSettings();

    }

    void Start()
    {
        SwitchTab(0);
    }

    void Update()
    {
        if (waitingForKey != "")
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(kcode) && kcode != KeyCode.Mouse0 && kcode != KeyCode.Mouse1)
                    {
                        keys[waitingForKey] = kcode;
                        UpdateUI_Text(waitingForKey, kcode.ToString()); // Sadece arayüzde gösterir, henüz KAYDETMEZ
                        waitingForKey = "";
                        break;
                    }
                }
            }
        }
    }

    #region TAB MANAGEMENT
    public void SwitchTab(int tabIndex)
    {
        currentTabIndex = tabIndex; // Açýk olan sekmeyi hafýzaya al

        graphicsPanel.SetActive(tabIndex == 0);
        audioPanel.SetActive(tabIndex == 1);
        keysPanel.SetActive(tabIndex == 2);
    }
    #endregion

    #region SAVE, RESET & CLOSE
    public void SaveSettings()
    {
        // "KAYDET" BUTONUNA BASILINCA TÜM DEÐERLERÝ UI'DAN ALIP HAFIZAYA YAZIYORUZ
        PlayerPrefs.SetFloat("MasterVolume", masterSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);

        PlayerPrefs.SetInt("GraphicsQuality", graphicsDropdown.value);
        PlayerPrefs.SetInt("DisplayMode", displayDropdown.value);

        PlayerPrefs.SetFloat("MouseSensitivity", sensitivitySlider.value);

        string[] keyNames = { "Ileri", "Geri", "Sol", "Sag", "Ziplama", "Kosma", "Canta_Acma", "Interaksiyon", "Uyuma" };

        foreach (string k in keyNames)
        {
            if (keys.ContainsKey(k))
                PlayerPrefs.SetString(k, keys[k].ToString());
        }

        PlayerPrefs.Save();
        Debug.Log("Ayarlar baþarýyla kalýcý olarak kaydedildi!");
        if (StarterAssets.FirstPersonController.Instance != null)
        {
            StarterAssets.FirstPersonController.Instance.LoadKeys();
        }
    }

    public void CloseSettings()
    {
        // EÐER KAYDETMEDEN ÇIKILDIYSA, ESKÝ AYARLARI GERÝ YÜKLE (Ýptal etme mantýðý)
        LoadSettings();

        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (menuArea != null) menuArea.SetActive(true);

        SwitchTab(0);
    }

    public void ResetToDefaults()
    {
        // SADECE AKTÝF OLAN SEKMEYÝ SIFIRLA
        if (currentTabIndex == 0) // Grafik
        {
            graphicsDropdown.value = 2; SetGraphicsQuality(2); // Yüksek
            displayDropdown.value = 0; SetDisplayMode(0);     // Tam Ekran
        }
        else if (currentTabIndex == 1) // Ses
        {
            masterSlider.value = 0.75f; SetMasterVolume(0.75f);
            sfxSlider.value = 0.75f; SetSFXVolume(0.75f);
        }
        else if (currentTabIndex == 2) // Kontroller
        {
            sensitivitySlider.value = 2.0f; SetSensitivity(2.0f);

            string[] keyNames = { "Ileri", "Geri", "Sol", "Sag", "Ziplama", "Kosma", "Canta_Acma", "Interaksiyon", "Uyuma" };
            string[] defaultValues = { "W", "S", "A", "D", "Space", "LeftShift", "Tab", "E", "T" };

            for (int i = 0; i < keyNames.Length; i++)
            {
                keys[keyNames[i]] = (KeyCode)System.Enum.Parse(typeof(KeyCode), defaultValues[i]);
                UpdateUI_Text(keyNames[i], defaultValues[i]);
            }
        }

        // Not: Sýfýrladýðýnda hemen kaydetmez. Oyuncunun önizlemesi içindir, isterse "Kaydet" butonuna basar.
    }
    #endregion

    #region SETTING ACTIONS (Oyun Ýçi Etkiler)
    private void UpdateUI_Text(string keyName, string value)
    {
        if (keyName == "Ileri") forwardText.text = value;
        if (keyName == "Geri") backwardText.text = value;
        if (keyName == "Sol") leftText.text = value;
        if (keyName == "Sag") rightText.text = value;
        if (keyName == "Ziplama") jumpText.text = value;
        if (keyName == "Kosma") sprintText.text = value;
        if (keyName == "Canta_Acma") inventoryText.text = value;
        if (keyName == "Interaksiyon") interactText.text = value;
        if (keyName == "Uyuma") sleepText.text = value;

    }

    public void ChangeKey(string keyName) { waitingForKey = keyName; UpdateUI_Text(keyName, "..."); }
    public KeyCode GetKey(string keyName) { return keys.ContainsKey(keyName) ? keys[keyName] : KeyCode.None; }

    // Bu fonksiyonlar artýk PlayerPrefs'e anýnda KAYDETMÝYOR, sadece oyunu anlýk güncelliyor (Önizleme için)
    public void SetMasterVolume(float volume) { audioMixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20); }
    public void SetSFXVolume(float volume) { audioMixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20); }
    public void SetGraphicsQuality(int index) { QualitySettings.SetQualityLevel(index); }
    public void SetDisplayMode(int index) { Screen.fullScreenMode = (index == 0) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed; }

    public void SetSensitivity(float value)
    {
        if (StarterAssets.FirstPersonController.Instance != null)
        {
            StarterAssets.FirstPersonController.Instance.RotationSpeed = value;
        }
    }
    #endregion

    #region LOAD SETTINGS
    private void LoadSettings()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f); SetMasterVolume(masterSlider.value);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f); SetSFXVolume(sfxSlider.value);
        graphicsDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", 2); SetGraphicsQuality(graphicsDropdown.value);
        displayDropdown.value = PlayerPrefs.GetInt("DisplayMode", 0); SetDisplayMode(displayDropdown.value);
        sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2.0f); SetSensitivity(sensitivitySlider.value);

        string[] keyNames = { "Ileri", "Geri", "Sol", "Sag", "Ziplama", "Kosma", "Canta_Acma", "Interaksiyon", "Uyuma" };
        string[] defaultValues = { "W", "S", "A", "D", "Space", "LeftShift", "Tab", "E", "T" };

        for (int i = 0; i < keyNames.Length; i++)
        {
            string savedKey = PlayerPrefs.GetString(keyNames[i], defaultValues[i]);
            keys[keyNames[i]] = (KeyCode)System.Enum.Parse(typeof(KeyCode), savedKey);
            UpdateUI_Text(keyNames[i], savedKey);
        }
    }
    #endregion
}