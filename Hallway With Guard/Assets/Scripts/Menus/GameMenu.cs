using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Main Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Sub Panels")]
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject gameplayPanel; // <-- Assign your "GamePlay Popout" here
    [SerializeField] private GameObject controlsPanel;

    // ================= MAIN MENU BUTTONS =================
    [Header("Main Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    // ================= SETTINGS BUTTONS =================
    [Header("Settings Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button gameplayButton;
    [SerializeField] private Button controlsButton;

    // ================= AUDIO =================
    [Header("Audio UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumeText;
    [SerializeField] private Button applyAudioButton;
    [SerializeField] private Button resetAudioButton;
    [SerializeField] private Button audioBackButton;

    // ================= GAMEPLAY =================
    [Header("Gameplay UI")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text sensitivityText;
    [SerializeField] private Toggle invertYToggle;
    [SerializeField] private Button applyGameplayButton;
    [SerializeField] private Button resetGameplayButton;
    [SerializeField] private Button gameplayBackButton;

    private void Awake()
    {
        // ---------- MAIN MENU ----------
        if (startButton != null)
            startButton.onClick.AddListener(() => SceneManager.LoadScene(mainMenuSceneName));

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(Application.Quit);

        // ---------- SETTINGS ----------
        if (backButton != null)
            backButton.onClick.AddListener(CloseSettings);

        if (audioButton != null)
            audioButton.onClick.AddListener(OpenAudio);

        if (gameplayButton != null)
            gameplayButton.onClick.AddListener(OpenGameplay);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(OpenControls);

        // ---------- AUDIO ----------
        if (audioBackButton != null)
            audioBackButton.onClick.AddListener(CloseSubPanels);

        if (applyAudioButton != null)
            applyAudioButton.onClick.AddListener(ApplyAudioSettings);

        if (resetAudioButton != null)
            resetAudioButton.onClick.AddListener(ResetAudioSettings);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(delegate { UpdateVolumeText(); });

        // ---------- GAMEPLAY ----------
        if (gameplayBackButton != null)
            gameplayBackButton.onClick.AddListener(CloseSubPanels);

        if (applyGameplayButton != null)
            applyGameplayButton.onClick.AddListener(ApplyGameplaySettings);

        if (resetGameplayButton != null)
            resetGameplayButton.onClick.AddListener(ResetGameplaySettings);

        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(delegate { UpdateSensitivityText(); });

        // ---------- INITIAL STATES ----------
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (audioPanel != null) audioPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    // ================= NAVIGATION =================

    void OpenSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    void CloseSubPanels()
    {
        if (audioPanel != null) audioPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);

        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    void OpenAudio()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (audioPanel != null) audioPanel.SetActive(true);

        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        if (volumeSlider != null)
            volumeSlider.value = savedVolume;

        UpdateVolumeText();
    }

    void OpenGameplay()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(true);

        float savedSensitivity = PlayerPrefs.GetFloat("Sensitivity", 5f);
        int savedInvert = PlayerPrefs.GetInt("InvertY", 0);

        if (sensitivitySlider != null)
            sensitivitySlider.value = savedSensitivity;

        if (invertYToggle != null)
            invertYToggle.isOn = savedInvert == 1;

        UpdateSensitivityText();
    }

    void OpenControls()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    // ================= AUDIO =================

    void UpdateVolumeText()
    {
        if (volumeText != null && volumeSlider != null)
            volumeText.text = Mathf.RoundToInt(volumeSlider.value * 100).ToString();
    }

    void ApplyAudioSettings()
    {
        if (volumeSlider == null) return;

        float volume = volumeSlider.value;
        AudioListener.volume = volume;

        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();

        Debug.Log("Audio applied: " + volume);
    }

    void ResetAudioSettings()
    {
        if (volumeSlider == null) return;

        volumeSlider.value = 1f;
        AudioListener.volume = 1f;
        UpdateVolumeText();
    }

    // ================= GAMEPLAY =================

    void UpdateSensitivityText()
    {
        if (sensitivityText != null && sensitivitySlider != null)
            sensitivityText.text = sensitivitySlider.value.ToString("F1");
    }

    void ApplyGameplaySettings()
    {
        if (sensitivitySlider == null || invertYToggle == null) return;

        float sensitivity = sensitivitySlider.value;
        bool invertY = invertYToggle.isOn;

        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
        PlayerPrefs.SetInt("InvertY", invertY ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Gameplay settings applied");
    }

    void ResetGameplaySettings()
    {
        if (sensitivitySlider != null)
            sensitivitySlider.value = 5f;

        if (invertYToggle != null)
            invertYToggle.isOn = false;

        UpdateSensitivityText();
    }
}