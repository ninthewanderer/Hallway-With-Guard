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
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button controlsBackButton; // NEW

    // ================= AUDIO =================
    [Header("Audio UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumeText;
    [SerializeField] private Button applyAudioButton;
    [SerializeField] private Button resetAudioButton;
    [SerializeField] private Button audioBackButton;

    private void Awake()
    {
        // ---------- MAIN MENU ----------
        if (startButton != null)
            startButton.onClick.AddListener(() => SceneManager.LoadScene(3));

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(Application.Quit);

        // ---------- SETTINGS ----------
        if (backButton != null)
            backButton.onClick.AddListener(CloseSettings);

        if (audioButton != null)
            audioButton.onClick.AddListener(OpenAudio);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(OpenControls);

        if (controlsBackButton != null) // NEW
            controlsBackButton.onClick.AddListener(CloseSubPanels);

        // ---------- AUDIO ----------
        if (audioBackButton != null)
            audioBackButton.onClick.AddListener(CloseSubPanels);

        if (applyAudioButton != null)
            applyAudioButton.onClick.AddListener(ApplyAudioSettings);

        if (resetAudioButton != null)
            resetAudioButton.onClick.AddListener(ResetAudioSettings);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(delegate { UpdateVolumeText(); });

        // ---------- INITIAL STATES ----------
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (audioPanel != null) audioPanel.SetActive(false);
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
}