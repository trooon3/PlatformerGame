using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenu : MonoBehaviour
{
    private const string FirstLevelName = "LevelDungeon";
    private const string GameSavedKey = "GameSaved";
    private const int GameSavedValue = 1;

    [Header("UI Panels")]
    [SerializeField] private GameObject _controlsPanel;
    [SerializeField] private GameObject _settingsPanel; 

    [Header("Menu Buttons")]
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _controlsButton;
    [SerializeField] private Button _closeControlsButton;
    [SerializeField] private Button _settingsButton; 
    [SerializeField] private Button _closeSettingsButton; 
    [SerializeField] private Button _exitButton;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip _buttonClickSound;
    [SerializeField] private AudioSource _audioSource;

    private void Start()
    {
        InitializeGameState();
        InitializeButtonListeners();
        UpdateContinueButtonVisibility();

        if (_controlsPanel != null)
        {
            _controlsPanel.SetActive(false);
        }

        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(false);
        }
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    private void OnDestroy()
    {
        _newGameButton?.onClick.RemoveListener(StartNewGame);
        _continueButton?.onClick.RemoveListener(ContinueGame);

        _controlsButton?.onClick.RemoveListener(OpenControls);
        _closeControlsButton?.onClick.RemoveListener(CloseControls);

        _settingsButton?.onClick.RemoveListener(OpenSettings);
        _closeSettingsButton?.onClick.RemoveListener(CloseSettings);

        _exitButton?.onClick.RemoveListener(ExitGame);
    }

    private void StartNewGame()
    {
        PlayButtonSound();

        ResetGameProgress();

        LoadFirstLevel();
    }

    private void ContinueGame()
    {
        PlayButtonSound();

        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave())
        {
            LoadSavedGameLevel();
        }
        else
        {
            StartNewGame();
        }
    }

    private void OpenControls()
    {
        PlayButtonSound();

        if (_controlsPanel != null)
        {
            _controlsPanel.SetActive(true);
        }
    }

    private void CloseControls()
    {
        PlayButtonSound();

        if (_controlsPanel != null)
        {
            _controlsPanel.SetActive(false);
        }
    }

    private void OpenSettings()
    {
        PlayButtonSound();

        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(true);
        }
    }

    private void CloseSettings()
    {
        PlayButtonSound();

        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(false);
        }
    }

    private void ExitGame()
    {
        PlayButtonSound();
        QuitApplication();
    }

    private void InitializeGameState()
    {
        Time.timeScale = 1f;
    }

    private void InitializeButtonListeners()
    {
        _newGameButton?.onClick.AddListener(StartNewGame);
        _continueButton?.onClick.AddListener(ContinueGame);

        _controlsButton?.onClick.AddListener(OpenControls);
        _closeControlsButton?.onClick.AddListener(CloseControls);

        _settingsButton?.onClick.AddListener(OpenSettings);
        _closeSettingsButton?.onClick.AddListener(CloseSettings);

        _exitButton?.onClick.AddListener(ExitGame);

        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }
    }

    private void UpdateContinueButtonVisibility()
    {
        if (_continueButton == null)
        {
            return;
        }

        _continueButton.gameObject.SetActive(CheckForExistingSave());
    }

    private bool CheckForExistingSave()
    {
        if (SaveSystem.Instance != null)
        {
            return SaveSystem.Instance.HasSave();
        }

        return PlayerPrefs.HasKey(GameSavedKey) &&
               PlayerPrefs.GetInt(GameSavedKey) == GameSavedValue;
    }

    private void PlayButtonSound()
    {
        if (_buttonClickSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_buttonClickSound);
        }
    }

    private void ResetGameProgress()
    {
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.8f);

        SaveSystem.Instance?.DeleteSaveData();
        EnemyManager.Instance?.ResetAllEnemies();
        GameStateManager.ResetGameState();

        PlayerPrefs.DeleteAll();

        PlayerPrefs.SetFloat("MusicVolume", music);
        PlayerPrefs.SetFloat("SFXVolume", sfx);

        PlayerPrefs.Save();
    }

    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(FirstLevelName);
    }

    private void LoadSavedGameLevel()
    {
        string savedSceneName = SaveSystem.Instance.CurrentSave.sceneName;
        string sceneToLoad = string.IsNullOrEmpty(savedSceneName) ? FirstLevelName : savedSceneName;

        SceneManager.LoadScene(sceneToLoad);
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR        
        UnityEditor.EditorApplication.isPlaying = false;
#else        
        Application.Quit();
#endif
    }

    private void HandleKeyboardInput()
    {
        if (_controlsPanel != null && _controlsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseControls();
            }
            return;
        }

        if (_settingsPanel != null && _settingsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseSettings();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartNewGame();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ContinueGame();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ExitGame();
        }
    }
}