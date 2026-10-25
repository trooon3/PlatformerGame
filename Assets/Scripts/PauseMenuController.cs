using Player.Input;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public sealed class PauseMenuController : MonoBehaviour
{
    public static bool IsGamePaused { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _controlsPanel;
    [SerializeField] private GameObject _settingsPanel;

    [Header("Buttons")]
    [SerializeField] private UnityEngine.UI.Button _continueButton;
    [SerializeField] private UnityEngine.UI.Button _controlsButton;
    [SerializeField] private UnityEngine.UI.Button _closeControlsButton;
    [SerializeField] private UnityEngine.UI.Button _settingsButton; 
    [SerializeField] private UnityEngine.UI.Button _closeSettingsButton;
    [SerializeField] private UnityEngine.UI.Button _exitButton;

    private IInputProvider _inputProvider;
    private bool _isPaused;

    private void Start()
    {
        FindInputProvider();
        SetupButtons();
        HidePauseMenu();

        ResumeGame();
    }

    private void Update()
    {
        if (MiniMapController.IsCutscenePlaying)
        {
            return;
        }

        if (_inputProvider != null && _inputProvider.IsMenuPressed)
        {
            TogglePause();
        }
    }

    private void SetupButtons()
    {
        if (_continueButton != null)
        {
            _continueButton.onClick.AddListener(ResumeGame);
        }

        if (_controlsButton != null)
        {
            _controlsButton.onClick.AddListener(OpenControls);
        }

        if (_closeControlsButton != null)
        {
            _closeControlsButton.onClick.AddListener(CloseControls);
        }

        if (_settingsButton != null)
        {
            _settingsButton.onClick.AddListener(OpenSettings);
        }

        if (_closeSettingsButton != null)
        {
            _closeSettingsButton.onClick.AddListener(CloseSettings);
        }

        if (_exitButton != null)
        {
            _exitButton.onClick.AddListener(ExitToMainMenu);
        }
    }

    private void FindInputProvider()
    {
        _inputProvider = FindFirstObjectByType<AggregatedInputProvider>();

        if (_inputProvider == null && YG2.envir.isDesktop)
        {
            _inputProvider = FindFirstObjectByType<OldInputProvider>();
        }

        if (_inputProvider == null && YG2.envir.isMobile)
        {
            _inputProvider = FindFirstObjectByType<JoystickInput>();
        }
    }

    public void TogglePause()
    {
        if (_isPaused)
        {
            if (_controlsPanel != null && _controlsPanel.activeSelf)
            {
                CloseControls();
                return;
            }

            if (_settingsPanel != null && _settingsPanel.activeSelf)
            {
                CloseSettings();
                return;
            }

            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        _isPaused = true;
        IsGamePaused = true;

        Time.timeScale = 0f;
        AudioListener.pause = true;

        ShowPauseMenu();
    }

    private void ResumeGame()
    {
        _isPaused = false;
        IsGamePaused = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        HidePauseMenu();
        CloseControls();
        CloseSettings();
    }

    private void OpenControls()
    {
        if (_controlsPanel != null)
        {
            _controlsPanel.SetActive(true);
        }
    }

    private void CloseControls()
    {
        if (_controlsPanel != null)
        {
            _controlsPanel.SetActive(false);
        }
    }

    private void OpenSettings()
    {
        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(true);
        }
    }

    private void CloseSettings()
    {
        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(false);
        }
    }

    private void ExitToMainMenu()
    {
        ResumeGame();

        SceneManager.LoadScene("MainMenu");
    }

    private void ShowPauseMenu()
    {
        if (_pausePanel != null)
        {
            _pausePanel.SetActive(true);
        }
    }

    private void HidePauseMenu()
    {
        if (_pausePanel != null)
        {
            _pausePanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        ResumeGame();
    }
}