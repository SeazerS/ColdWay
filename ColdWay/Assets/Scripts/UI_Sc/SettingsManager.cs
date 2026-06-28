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
                        // EÐER OYUNCU ESC'YE BASARSA ATAMAYI ÝPTAL ET
                        if (kcode == KeyCode.Escape)
                        {
                            UpdateUI_Text(waitingForKey, keys[waitingForKey].ToString());
                            waitingForKey = "";
                            break;
                        }

                        // ÇAKIÞMA KONTROLÜ: Ayný tuþ baþka eylemde var mý?
                        string cakisanIslem = "";
                        foreach (var kvp in keys)
                        {
                            if (kvp.Value == kcode && kvp.Key != waitingForKey)
                            {
                                cakisanIslem = kvp.Key;
                                break;
                            }
                        }

                        // Eðer çakýþan varsa onu "..." yap
                        if (cakisanIslem != "")
                        {
                            keys[cakisanIslem] = KeyCode.None;
                            UpdateUI_Text(cakisanIslem, "...");
                        }

                        keys[waitingForKey] = kcode;
                        UpdateUI_Text(waitingForKey, kcode.ToString());
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
        currentTabIndex = tabIndex;
        graphicsPanel.SetActive(tabIndex == 0);
        audioPanel.SetActive(tabIndex == 1);
        keysPanel.SetActive(tabIndex == 2);
    }
    #endregion

    #region SAVE, RESET & CLOSE
    public void SaveSettings()
    {
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
            // Hassasiyeti anýnda uygula
            StarterAssets.FirstPersonController.Instance.RotationSpeed = sensitivitySlider.value;
        }
    }

    public void CloseSettings()
    {
        LoadSettings();
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (menuArea != null) menuArea.SetActive(true);
        SwitchTab(0);
    }

    public void ResetToDefaults()
    {
        if (currentTabIndex == 0)
        {
            graphicsDropdown.value = 2; SetGraphicsQuality(2);
            displayDropdown.value = 0; SetDisplayMode(0);
        }
        else if (currentTabIndex == 1)
        {
            masterSlider.value = 0.75f; SetMasterVolume(0.75f);
            sfxSlider.value = 0.75f; SetSFXVolume(0.75f);
        }
        else if (currentTabIndex == 2)
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

    public void SetMasterVolume(float volume) { audioMixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20); }
    public void SetSFXVolume(float volume) { audioMixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20); }

    public void SetGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true); // true parametresi deðiþimi anýnda render motoruna zorlar
    }

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