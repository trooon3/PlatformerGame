using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public sealed class BossDoor : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string MainMenuSceneName = "MainMenu";
    private const float MessageDisplayDuration = 2f;
    private const float AllBossesDefeatedMessageDuration = 4.5f;
    private const KeyCode InteractionKey = KeyCode.F;
    private const float MessageDisplayDistance = 3f;
    private const float PausedTimeScaleThreshold = 0.01f;

    [Header("Boss Settings")]
    [SerializeField] private string[] _bossIds = { "226", "227", "228", "229" };

    [Header("UI Prefabs & Sorting")]
    [SerializeField] private GameObject _interactionMessagePrefab;
    [SerializeField] private GameObject _temporaryMessagePrefab;
    [SerializeField] private int _messageSortingOrder = -1;

    [Header("Audio")]
    [SerializeField] private SfxPlayer _sfxPlayer;
    [SerializeField] private SoundConfiguration _soundConfiguration;

    public static event System.Action OnBossDoorOpenedGlobal;
    [Header("UI Visibility Control")]
    [Tooltip("Перетащите сюда панели паузы/настроек, если они не ставят Time.timeScale = 0")]
    [SerializeField] private GameObject[] _blockingUIObjects;

    [Header("Victory Video Settings")]
    [SerializeField] private VideoPlayer _victoryVideoPlayer;
    [SerializeField] private GameObject _videoPanel;
    [SerializeField] private string _videoFileName = "Victory.mp4";

    public static event Action OnBossDoorOpenedGlobal;

    private Transform _playerTransform;
    private bool _isPlayerNear;
    private bool _isOpened;
    private bool _hasShownAllBossesDefeatedMessage;

    private GameObject _currentInteractionMessage;
    private Canvas _interactionCanvas;
    private TextMeshProUGUI _interactionMessageText;

    private GameObject _currentTemporaryMessage;
    private Canvas _temporaryCanvas;
    private Coroutine _temporaryMessageCoroutine;

    private Coroutine _videoCoroutine;
    private bool _isVictorySequencePlaying;

    private void OnEnable()
    {
        EnemyManager.OnEnemyKilledGlobal += OnEnemyKilled;
    }

    private void OnDisable()
    {
        EnemyManager.OnEnemyKilledGlobal -= OnEnemyKilled;
    }

    private void Start()
    {
        InitializeReferences();
        CreateUIPrefabsIfNeeded();

        if (_videoPanel != null) _videoPanel.SetActive(false);

        if (AreAllBossesKilledInSave())
        {
            _hasShownAllBossesDefeatedMessage = true;
        }
    }

    private void Update()
    {
        if (_isVictorySequencePlaying)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) SkipOrEndVideo();
            return;
        }

        UpdateMessagesVisibility();

        if (_playerTransform == null || _isOpened || IsMenuOrPauseActive())
        {
            return;
        }

        UpdatePlayerProximity();
        HandlePlayerInteraction();
    }

    private void OnDestroy()
    {
        CleanupUI();
        if (_isVictorySequencePlaying) MiniMapController.IsCutscenePlaying = false;
    }

    private void UpdateMessagesVisibility()
    {
        bool shouldHide = IsMenuOrPauseActive();

        if (_interactionCanvas != null)
        {
            _interactionCanvas.enabled = !shouldHide;
        }

        if (_temporaryCanvas != null)
        {
            _temporaryCanvas.enabled = !shouldHide;
        }
    }

    private bool IsMenuOrPauseActive()
    {
        if (Time.timeScale < PausedTimeScaleThreshold)
        {
            return true;
        }

        if (_blockingUIObjects != null)
        {
            foreach (GameObject uiObject in _blockingUIObjects)
            {
                if (uiObject != null && uiObject.activeInHierarchy)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void OnEnemyKilled()
    {
        if (_hasShownAllBossesDefeatedMessage || _isOpened)
        {
            return;
        }

        if (AreAllBossesKilledInSave())
        {
            _hasShownAllBossesDefeatedMessage = true;
            ShowTemporaryMessage(GetAllBossesDefeatedMessage(), AllBossesDefeatedMessageDuration);

            if (_isPlayerNear)
            {
                ShowBossStatus();
            }
        }
    }

    private bool AreAllBossesKilledInSave()
    {
        if (EnemyManager.Instance == null || _bossIds == null || _bossIds.Length == 0)
        {
            return false;
        }

        HashSet<string> killedEnemies = EnemyManager.Instance.GetKilledEnemies();

        foreach (string bossId in _bossIds)
        {
            if (killedEnemies.Contains(bossId) == false)
            {
                return false;
            }
        }

        return true;
    }

    private void InitializeReferences()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(PlayerTag);

        if (playerObject != null)
        {
            _playerTransform = playerObject.transform;
        }
    }

    private void CreateUIPrefabsIfNeeded()
    {
        if (_interactionMessagePrefab == null)
        {
            _interactionMessagePrefab = CreateDefaultUIPrefab("InteractionMessage");
        }

        if (_temporaryMessagePrefab == null)
        {
            _temporaryMessagePrefab = CreateDefaultUIPrefab("TemporaryMessage");
        }
    }

    private GameObject CreateDefaultUIPrefab(string objectName)
    {
        const string textObjectName = "MessageText";
        const string defaultMessage = "Message";
        const int fontSize = 24;
        const float width = 600f;
        const float height = 80f;
        const float anchorPosition = 0.5f;

        GameObject uiObject = new GameObject(objectName);

        Canvas canvas = uiObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = _messageSortingOrder;

        uiObject.AddComponent<CanvasScaler>();

        GameObject textObject = new GameObject(textObjectName);
        textObject.transform.SetParent(uiObject.transform, false);

        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();

        textComponent.text = defaultMessage;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.raycastTarget = false;

        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(anchorPosition, anchorPosition);
        rectTransform.anchorMax = new Vector2(anchorPosition, anchorPosition);
        rectTransform.pivot = new Vector2(anchorPosition, anchorPosition);
        rectTransform.sizeDelta = new Vector2(width, height);
        rectTransform.anchoredPosition = Vector2.zero;

        uiObject.SetActive(false);

        return uiObject;
    }

    private void UpdatePlayerProximity()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
        bool wasPlayerNear = _isPlayerNear;

        _isPlayerNear = distanceToPlayer <= MessageDisplayDistance;

        if (_isPlayerNear && wasPlayerNear == false)
        {
            ShowBossStatus();
        }
        else if (_isPlayerNear == false && wasPlayerNear)
        {
            HideBossStatus();
        }
    }

    private void HandlePlayerInteraction()
    {
        if (_isPlayerNear && Input.GetKeyDown(InteractionKey))
        {
            TryOpenDoor();
        }
    }

    private void ShowBossStatus()
    {
        int aliveBossCount = CountAliveBosses();
        string statusMessage = GetBossStatusMessage(aliveBossCount);

        ShowInteractionMessage(statusMessage);
    }

    private void HideBossStatus()
    {
        HideInteractionMessage();
    }

    private void TryOpenDoor()
    {
        int aliveBossCount = CountAliveBosses();

        if (aliveBossCount == 0)
        {
            OpenDoor();
            return;
        }

        int defeatedCount = _bossIds.Length - aliveBossCount;

        string message = LocalizationManager.CurrentLanguage switch
        {
            LocalizationManager.Language.English => $"Not all bosses are defeated. ({defeatedCount}/{_bossIds.Length})",
            LocalizationManager.Language.Turkish => $"Tüm patronlar yenilmedi. ({defeatedCount}/{_bossIds.Length})",
            _ => $"Не все боссы побеждены. Осталось ({defeatedCount}/{_bossIds.Length})"
        };

        ShowTemporaryMessage(message, MessageDisplayDuration);
    }

    private int CountAliveBosses()
    {
        int aliveCount = 0;

        if (EnemyManager.Instance == null)
        {
            return aliveCount;
        }

        foreach (string bossId in _bossIds)
        {
            if (EnemyManager.Instance.IsEnemyAlive(bossId))
            {
                aliveCount++;
            }
        }

        return aliveCount;
    }

    private string GetAllBossesDefeatedMessage()
    {
        return LocalizationManager.CurrentLanguage switch
        {
            LocalizationManager.Language.English => "The last boss is defeated! Head to the locked door.",
            LocalizationManager.Language.Turkish => "Son patron yenildi! Kilitli kapıya git.",
            _ => "Последний босс повержен! Направляйтесь к запертой двери."
        };
    }

    private string GetBossStatusMessage(int aliveBossCount)
    {
        int defeatedCount = _bossIds.Length - aliveBossCount;

        if (aliveBossCount > 0)
        {
            return LocalizationManager.CurrentLanguage switch
            {
                LocalizationManager.Language.English => $"({defeatedCount}/{_bossIds.Length}) bosses killed",
                LocalizationManager.Language.Turkish => $"({defeatedCount}/{_bossIds.Length}) patron öldürüldü",
                _ => $"({defeatedCount}/{_bossIds.Length}) боссов убито"
            };
        }

        string keyName = LocalizationManager.CurrentLanguage == LocalizationManager.Language.Russian ? "А (F)" : InteractionKey.ToString();

        return LocalizationManager.CurrentLanguage switch
        {
            LocalizationManager.Language.English => $"All the guards are defeated, Press {keyName} to open the door",
            LocalizationManager.Language.Turkish => $"Tüm muhafızlar yenildi, Kapıyı açmak için {keyName} tuşuna basın",
            _ => $"Все надзиратели побеждены, Нажмите {keyName} чтобы открыть дверь"
        };
    }

    private void OpenDoor()
    {
        _isOpened = true;
        CleanupUI();
        OnBossDoorOpenedGlobal?.Invoke();

        if (_sfxPlayer != null && _soundConfiguration != null)
            _sfxPlayer.Play(_soundConfiguration.BossDoorOpenSound);

        StartVictorySequence();
    }

    private void StartVictorySequence()
    {
        _isVictorySequencePlaying = true;
        MiniMapController.IsCutscenePlaying = true;

        DisableDoorVisualsAndColliders();

        if (_audioController != null)
        {
            _audioController.PauseBackgroundMusic();
            _audioController.StopBossMusic();
        }

        if (_victoryVideoPlayer != null && _videoPanel != null)
        {
            ClearRenderTexture();

            _videoPanel.SetActive(true);
            AudioListener.pause = false;

            _victoryVideoPlayer.Stop();
            _victoryVideoPlayer.source = VideoSource.Url;
            _victoryVideoPlayer.url = Application.streamingAssetsPath + "/" + _videoFileName.Trim();

            _victoryVideoPlayer.loopPointReached -= OnVideoFinished;
            _victoryVideoPlayer.loopPointReached += OnVideoFinished;
            _victoryVideoPlayer.errorReceived -= OnVideoError;
            _victoryVideoPlayer.errorReceived += OnVideoError;

            _victoryVideoPlayer.Play();

            if (_videoCoroutine != null) StopCoroutine(_videoCoroutine);
            _videoCoroutine = StartCoroutine(VideoWatchdogRoutine());
        }
        else
        {
            FinishGameAndReturnToMenu();
        }
    }

    private IEnumerator VideoWatchdogRoutine()
    {
        float startWait = 5f;
        while (_isVictorySequencePlaying && !_victoryVideoPlayer.isPlaying && startWait > 0f)
        {
            startWait -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (_isVictorySequencePlaying && !_victoryVideoPlayer.isPlaying)
        {
            SkipOrEndVideo();
            yield break;
        }

        float maxPlayTime = 60f;
        while (_isVictorySequencePlaying && _victoryVideoPlayer.isPlaying && maxPlayTime > 0f)
        {
            maxPlayTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (_isVictorySequencePlaying)
        {
            SkipOrEndVideo();
        }
    }

    private void OnVideoFinished(VideoPlayer vp) { SkipOrEndVideo(); }
    private void OnVideoError(VideoPlayer vp, string message) { SkipOrEndVideo(); }

    private void SkipOrEndVideo()
    {
        if (!_isVictorySequencePlaying) return;
        _isVictorySequencePlaying = false;
        MiniMapController.IsCutscenePlaying = false;

        if (_videoCoroutine != null)
        {
            StopCoroutine(_videoCoroutine);
            _videoCoroutine = null;
        }

        if (_victoryVideoPlayer != null)
        {
            _victoryVideoPlayer.loopPointReached -= OnVideoFinished;
            _victoryVideoPlayer.errorReceived -= OnVideoError;
            _victoryVideoPlayer.Stop();
            ClearRenderTexture();
        }

        if (_videoPanel != null) _videoPanel.SetActive(false);

        FinishGameAndReturnToMenu();
    }

    private void ClearRenderTexture()
    {
        if (_victoryVideoPlayer == null || _victoryVideoPlayer.targetTexture == null) return;

        RenderTexture target = _victoryVideoPlayer.targetTexture;
        if (!target.IsCreated()) target.Create();

        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = target;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = rt;
    }

    private void FinishGameAndReturnToMenu()
    {
        ResetGameProgress();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetNewGameFlag();
        }

        SceneManager.LoadScene(MainMenuSceneName);
    }

    private void ResetGameProgress()
    {
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        int savedLanguage = PlayerPrefs.GetInt("GameLanguage", 0);

        SaveSystem.Instance?.DeleteSaveData();
        EnemyManager.Instance?.ResetAllEnemies();
        GameStateManager.ResetGameState();
        LevelStatsTracker.ResetAllStats();

        PlayerPrefs.DeleteAll();

        PlayerPrefs.SetFloat("MusicVolume", music);
        PlayerPrefs.SetFloat("SFXVolume", sfx);
        PlayerPrefs.SetInt("GameLanguage", savedLanguage);

        PlayerPrefs.Save();
    }

    private void DisableDoorVisualsAndColliders()
    {
        foreach (Renderer rendererComponent in GetComponentsInChildren<Renderer>())
        {
            rendererComponent.enabled = false;
        }

        foreach (Collider2D collider2DComponent in GetComponentsInChildren<Collider2D>())
        {
            collider2DComponent.enabled = false;
        }

        foreach (Collider colliderComponent in GetComponentsInChildren<Collider>())
        {
            colliderComponent.enabled = false;
        }
    }

    private void ShowInteractionMessage(string message)
    {
        HideInteractionMessage();

        if (_interactionMessagePrefab == null) return;

        _currentInteractionMessage = Instantiate(_interactionMessagePrefab);
        _currentInteractionMessage.SetActive(true);

        _interactionCanvas = _currentInteractionMessage.GetComponent<Canvas>();
        if (_interactionCanvas != null)
        {
            _interactionCanvas.sortingOrder = _messageSortingOrder;
            _interactionCanvas.enabled = !IsMenuOrPauseActive();
        }

        _interactionMessageText = _currentInteractionMessage.GetComponentInChildren<TextMeshProUGUI>();

        if (_interactionMessageText != null)
        {
            _interactionMessageText.text = message;
            _interactionMessageText.raycastTarget = false;
        }
    }

    private void HideInteractionMessage()
    {
        if (_currentInteractionMessage == null) return;

        Destroy(_currentInteractionMessage);

        _currentInteractionMessage = null;
        _interactionCanvas = null;
        _interactionMessageText = null;
    }

    private void ShowTemporaryMessage(string message, float duration)
    {
        if (_temporaryMessagePrefab == null) return;

        if (_temporaryMessageCoroutine != null) StopCoroutine(_temporaryMessageCoroutine);
        if (_currentTemporaryMessage != null) Destroy(_currentTemporaryMessage);

        _currentTemporaryMessage = Instantiate(_temporaryMessagePrefab);
        _currentTemporaryMessage.SetActive(true);

        _temporaryCanvas = _currentTemporaryMessage.GetComponent<Canvas>();
        if (_temporaryCanvas != null)
        {
            _temporaryCanvas.sortingOrder = _messageSortingOrder;
            _temporaryCanvas.enabled = !IsMenuOrPauseActive();
        }

        TextMeshProUGUI textComponent = _currentTemporaryMessage.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = message;
            textComponent.raycastTarget = false;
        }

        _temporaryMessageCoroutine = StartCoroutine(TemporaryMessageLifetimeRoutine(duration));
    }

    private IEnumerator TemporaryMessageLifetimeRoutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (IsMenuOrPauseActive() == false)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }

        if (_currentTemporaryMessage != null)
        {
            Destroy(_currentTemporaryMessage);
            _currentTemporaryMessage = null;
            _temporaryCanvas = null;
        }

        _temporaryMessageCoroutine = null;
    }

    private void CleanupUI()
    {
        HideInteractionMessage();

        if (_currentTemporaryMessage != null)
        {
            Destroy(_currentTemporaryMessage);
        }
    }
}