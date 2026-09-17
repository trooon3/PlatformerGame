using TMPro;
using UnityEngine;
using UnityEngine.UI;


public sealed class BossDoor : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const float MessageDisplayDuration = 2f;
    private const KeyCode InteractionKey = KeyCode.F;
    private const float MessageDisplayDistance = 3f;

    [Header("Boss Settings")]
    [SerializeField] private string[] _bossIds = { "226", "227", "228", "229" };

    [Header("UI Prefabs")]
    [SerializeField] private GameObject _interactionMessagePrefab;
    [SerializeField] private GameObject _temporaryMessagePrefab;

    public static event System.Action OnBossDoorOpenedGlobal;

    private Transform _playerTransform;
    private AudioController _audioController;
    private bool _isPlayerNear;

    private GameObject _currentMessage;
    private TextMeshProUGUI _messageText;

    private void Start()
    {
        InitializeReferences();
        CreateUIPrefabsIfNeeded();
    }

    private void Update()
    {
        if (_playerTransform == null)
        {
            return;
        }

        UpdatePlayerProximity();
        HandlePlayerInteraction();
    }

    private void OnDestroy()
    {
        CleanupUI();
    }

    private void InitializeReferences()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(PlayerTag);

        if (playerObject != null)
        {
            _playerTransform = playerObject.transform;
        }

        _audioController = FindFirstObjectByType<AudioController>();
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
        const float width = 400f;
        const float height = 50f;
        const float anchorPosition = 0.5f;

        GameObject uiObject = new GameObject(objectName);

        Canvas canvas = uiObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        uiObject.AddComponent<CanvasScaler>();
        uiObject.AddComponent<GraphicRaycaster>();

        GameObject textObject = new GameObject(textObjectName);
        textObject.transform.SetParent(uiObject.transform, false);

        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();

        textComponent.text = defaultMessage;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;

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

        bool isEnglish = LocalizationManager.CurrentLanguage == LocalizationManager.Language.English;

        string message = isEnglish
            ? $"Not all bosses are defeated. ({_bossIds.Length - aliveBossCount}/{_bossIds.Length})"
            : $"Не все боссы побеждены. Осталось ({_bossIds.Length - aliveBossCount}/{_bossIds.Length})";

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

    private string GetBossStatusMessage(int aliveBossCount)
    {
        bool isEnglish = LocalizationManager.CurrentLanguage == LocalizationManager.Language.English;

        if (aliveBossCount > 0)
        {
            return isEnglish
                ? $"({_bossIds.Length - aliveBossCount}/{_bossIds.Length}) bosses killed"
                : $"({_bossIds.Length - aliveBossCount}/{_bossIds.Length}) боссов убито";
        }

        string keyName = isEnglish ? InteractionKey.ToString() : "A";

        return isEnglish
            ? $"All the guards are defeated, Press {keyName} to open the door"
            : $"Все надзиратели побеждены, Нажмите {keyName} чтобы открыть дверь";
    }

    private void OpenDoor()
    {
        OnBossDoorOpenedGlobal?.Invoke();

        _audioController?.PlayBossDoorOpenSound();

        bool isEnglish = LocalizationManager.CurrentLanguage == LocalizationManager.Language.English;
        string victoryMessage = isEnglish ? "Door opened, you win" : "дверь открыта, вы победили";

        ShowTemporaryMessage(victoryMessage, MessageDisplayDuration);

        HideInteractionMessage();
        Destroy(gameObject);
    }

    private void ShowInteractionMessage(string message)
    {
        HideInteractionMessage();

        if (_interactionMessagePrefab == null)
        {
            return;
        }

        _currentMessage = Instantiate(_interactionMessagePrefab);
        _currentMessage.SetActive(true);

        _messageText = _currentMessage.GetComponentInChildren<TextMeshProUGUI>();

        if (_messageText != null)
        {
            _messageText.text = message;
        }
    }

    private void HideInteractionMessage()
    {
        if (_currentMessage == null)
        {
            return;
        }

        Destroy(_currentMessage);

        _currentMessage = null;
        _messageText = null;
    }

    private void ShowTemporaryMessage(string message, float duration)
    {
        if (_temporaryMessagePrefab == null)
        {
            return;
        }

        GameObject temporaryMessage = Instantiate(_temporaryMessagePrefab);

        temporaryMessage.SetActive(true);

        TextMeshProUGUI textComponent = temporaryMessage.GetComponentInChildren<TextMeshProUGUI>();

        if (textComponent != null)
        {
            textComponent.text = message;
        }

        Destroy(temporaryMessage, duration);
    }

    private void CleanupUI()
    {
        HideInteractionMessage();
    }
}