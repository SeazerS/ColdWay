using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    // Singleton yapýsý (Diðer scriptlerden kolayca eriþmek için)
    public static SettingsManager Instance { get; private set; }

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

    private Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();
    private string waitingForKey = "";

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        LoadSettings();
    }

    void Update()
    {
        if (waitingForKey != "")
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    // Fare týklamalarýný tuþ atamasý olarak algýlamasýn diye filtreliyoruz
                    if (Input.GetKeyDown(kcode) && kcode != KeyCode.Mouse0 && kcode != KeyCode.Mouse1)
                    {
                        keys[waitingForKey] = kcode;
                        PlayerPrefs.SetString(waitingForKey, kcode.ToString());

                        UpdateUI_Text(waitingForKey, kcode.ToString());

                        waitingForKey = "";
                        break;
                    }
                }
            }
        }
    }

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
    }

    public void ChangeKey(string keyName)
    {
        waitingForKey = keyName;
        UpdateUI_Text(keyName, "...");
    }

    public KeyCode GetKey(string keyName)
    {
        if (keys.ContainsKey(keyName))
            return keys[keyName];

        return KeyCode.None;
    }

    // --- DÝÐER AYAR FONKSÝYONLARI (SES, GRAFÝK VS.) ---
    public void SetMasterVolume(float volume) { audioMixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20); PlayerPrefs.SetFloat("MasterVolume", volume); }
    public void SetSFXVolume(float volume) { audioMixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20); PlayerPrefs.SetFloat("SFXVolume", volume); }
    public void SetGraphicsQuality(int index) { QualitySettings.SetQualityLevel(index); PlayerPrefs.SetInt("GraphicsQuality", index); }
    public void SetDisplayMode(int index) { Screen.fullScreenMode = (index == 0) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed; PlayerPrefs.SetInt("DisplayMode", index); }
    public void SetSensitivity(float value) { PlayerPrefs.SetFloat("MouseSensitivity", value); }

    private void LoadSettings()
    {
        // Ses, Grafik yüklemeleri...
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f); SetMasterVolume(masterSlider.value);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f); SetSFXVolume(sfxSlider.value);
        graphicsDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", 2); SetGraphicsQuality(graphicsDropdown.value);
        displayDropdown.value = PlayerPrefs.GetInt("DisplayMode", 0); SetDisplayMode(displayDropdown.value);
        sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2.0f);

        // Tuþlarý Hafýzadan Yükle (Eðer yoksa varsayýlan deðerleri ata)
        string[] keyNames = { "Ileri", "Geri", "Sol", "Sag", "Ziplama", "Kosma", "Canta_Acma", "Interaksiyon" };
        string[] defaultValues = { "W", "S", "A", "D", "Space", "LeftShift", "Tab", "E" };

        for (int i = 0; i < keyNames.Length; i++)
        {
            string savedKey = PlayerPrefs.GetString(keyNames[i], defaultValues[i]);
            keys[keyNames[i]] = (KeyCode)System.Enum.Parse(typeof(KeyCode), savedKey);
            UpdateUI_Text(keyNames[i], savedKey);
        }
    }
}
