using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public sealed class MainMenu : MonoBehaviour
{
    private const string FirstLevelName = "LevelDungeon";
    private const string GameSavedKey = "GameSaved";
    private const int GameSavedValue = 1;

    [Header("Menu Buttons")]
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _exitButton;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip _buttonClickSound;
    [SerializeField] private AudioSource _audioSource;

    private void Start()
    {
        InitializeGameState();
        InitializeButtonListeners();
        UpdateContinueButtonVisibility();
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    private void InitializeGameState()
    {
        Time.timeScale = 1f;
    }

    private void InitializeButtonListeners()
    {
        _newGameButton?.onClick.AddListener(StartNewGame);
        _continueButton?.onClick.AddListener(ContinueGame);
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

        bool hasSaveGame = CheckForExistingSave();

        _continueButton.gameObject.SetActive(hasSaveGame);
    }

    private bool CheckForExistingSave()
    {
        if (SaveSystem.Instance != null)
        {
            return SaveSystem.Instance.HasSave();
        }

        return PlayerPrefs.HasKey(GameSavedKey) && PlayerPrefs.GetInt(GameSavedKey) == GameSavedValue;
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

    private void ExitGame()
    {
        PlayButtonSound();
        QuitApplication();
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
        SaveSystem.Instance?.DeleteSaveData();
        EnemyManager.Instance?.ResetAllEnemies();
        GameStateManager.ResetGameState();

        ClearPlayerPrefs();
    }

    private void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
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