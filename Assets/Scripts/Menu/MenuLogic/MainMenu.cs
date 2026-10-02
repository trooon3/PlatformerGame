using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public sealed class MainMenu : MonoBehaviour
{
    private const string FirstLevelName = "LevelDungeon";
    private const string GameSavedKey = "GameSaved";
    private const int GameSavedValue = 1;

    [Header("UI Panels")]
    [SerializeField] private GameObject _controlsPanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _videoPanel;
    [SerializeField] private GameObject _loadingPanel;

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

    [Header("Video Settings")]
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private string _videoFileNameRussian = "introRu.mp4";
    [SerializeField] private string _videoFileNameEnglish = "introEn.mp4";
    [SerializeField] private string _videoFileNameTurkish = "introTr.mp4";

    [Header("Leaderboard")]
    [SerializeField] private GameObject _leaderboardPanel;
    [SerializeField] private Button _leaderboardButton;
    [SerializeField] private Button _closeLeaderboardButton;

    private void Start()
    {
        InitializeGameState();
        InitializeButtonListeners();
        UpdateContinueButtonVisibility();

        if (_leaderboardPanel != null) _leaderboardPanel.SetActive(false);
        if (_controlsPanel != null) _controlsPanel.SetActive(false);
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
        if (_videoPanel != null) _videoPanel.SetActive(false);
        if (_loadingPanel != null) _loadingPanel.SetActive(false);

        if (_videoPlayer != null)
        {
            _videoPlayer.loopPointReached += OnVideoFinished;
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

        if (_videoPlayer != null)
        {
            _videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    private void StartNewGame()
    {
        PlayButtonSound();
        ResetGameProgress();

        PlayIntroVideo();
    }

    private void PlayIntroVideo()
    {
        if (_videoPlayer != null && _videoPanel != null)
        {
            _videoPanel.SetActive(true);

            string targetFileName = LocalizationManager.CurrentLanguage switch
            {
                LocalizationManager.Language.English => _videoFileNameEnglish,
                LocalizationManager.Language.Turkish => _videoFileNameTurkish,
                _ => _videoFileNameRussian
            };

            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, targetFileName);
            _videoPlayer.url = videoPath;

            _videoPlayer.Play();
        }
        else
        {
            LoadFirstLevel();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        SkipOrEndVideo();
    }

    private void SkipOrEndVideo()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.Stop();
        }

        if (_videoPanel != null)
        {
            _videoPanel.SetActive(false);
        }

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

        if (_controlsPanel != null) _controlsPanel.SetActive(true);
    }

    private void CloseControls()
    {
        PlayButtonSound();

        if (_controlsPanel != null) _controlsPanel.SetActive(false);
    }

    private void OpenSettings()
    {
        PlayButtonSound();

        if (_settingsPanel != null) _settingsPanel.SetActive(true);
    }

    private void CloseSettings()
    {
        PlayButtonSound();

        if (_settingsPanel != null) _settingsPanel.SetActive(false);
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

        _leaderboardButton?.onClick.AddListener(OpenLeaderboard);
        _closeLeaderboardButton?.onClick.AddListener(CloseLeaderboard);

        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }
    }

    private void OpenLeaderboard()
    {
        PlayButtonSound();
        if (_leaderboardPanel != null)
        {
            _leaderboardPanel.SetActive(true);
        }
    }

    private void CloseLeaderboard()
    {
        PlayButtonSound();
        if (_leaderboardPanel != null) _leaderboardPanel.SetActive(false);
    }

    private void UpdateContinueButtonVisibility()
    {
        if (_continueButton == null) return;
        _continueButton.gameObject.SetActive(CheckForExistingSave());
    }

    private bool CheckForExistingSave()
    {
        if (SaveSystem.Instance != null)
        {
            return SaveSystem.Instance.HasSave();
        }

        return PlayerPrefs.HasKey(GameSavedKey) && PlayerPrefs.GetInt(GameSavedKey) == GameSavedValue;
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

        LevelStatsTracker.ResetAllStats();

        int savedLanguage = PlayerPrefs.GetInt("GameLanguage", 0);

        PlayerPrefs.DeleteAll();

        PlayerPrefs.SetFloat("MusicVolume", music);
        PlayerPrefs.SetFloat("SFXVolume", sfx);
        PlayerPrefs.SetInt("GameLanguage", savedLanguage);

        PlayerPrefs.Save();
    }

    private void LoadFirstLevel()
    {
        StartCoroutine(LoadSceneAsyncRoutine(FirstLevelName));
    }

    private void LoadSavedGameLevel()
    {
        string savedSceneName = SaveSystem.Instance.CurrentSave.sceneName;
        string sceneToLoad = string.IsNullOrEmpty(savedSceneName) ? FirstLevelName : savedSceneName;

        StartCoroutine(LoadSceneAsyncRoutine(sceneToLoad));
    }

    private IEnumerator LoadSceneAsyncRoutine(string sceneName)
    {
        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(true);
        }

        yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
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
        if (_videoPanel != null && _videoPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SkipOrEndVideo();
            }
            return;
        }

        if (_controlsPanel != null && _controlsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) CloseControls();
            return;
        }

        if (_settingsPanel != null && _settingsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) CloseSettings();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) StartNewGame();
        else if (Input.GetKeyDown(KeyCode.Alpha2)) ContinueGame();
        else if (Input.GetKeyDown(KeyCode.Alpha3)) ExitGame();
    }
}