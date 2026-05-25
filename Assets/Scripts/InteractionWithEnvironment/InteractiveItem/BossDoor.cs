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

    private Transform _playerTransform;
    private AudioController _audioController;
    private bool _isPlayerNear;

    private GameObject _currentMessage;
    private TextMeshProUGUI _messageText;
    private Coroutine _messageCoroutine;

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
        _playerTransform = GameObject.FindGameObjectWithTag(PlayerTag)?.transform;

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

    private GameObject CreateDefaultUIPrefab(string name)
    {
        string textObjectName = "MessageText";
        string defaultMessage = "Message";
        int fontSize = 24;
        float width = 400f;
        float height = 50f;
        float anchorPosition = 0.5f;

        GameObject uiObject = new GameObject(name);

        Canvas canvas = uiObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        uiObject.AddComponent<CanvasScaler>();
        uiObject.AddComponent<GraphicRaycaster>();

        GameObject textObject = new GameObject(textObjectName);

        textObject.transform.SetParent(uiObject.transform);

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

        if (_isPlayerNear && !wasPlayerNear)
        {
            ShowBossStatus();
        }
        else if (!_isPlayerNear && wasPlayerNear)
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
        int aliveCount = 0;

        int aliveBossCount = CountAliveBosses();

        if (aliveBossCount == aliveCount)
        {
            OpenDoor();
        }
        else
        {
            string message = $"Не все боссы побеждены. Осталось ({_bossIds.Length - aliveBossCount}/{_bossIds.Length})";

            ShowTemporaryMessage(message, MessageDisplayDuration);
        }
    }

    private int CountAliveBosses()
    {
        int aliveCount = 0;

        if (EnemyManager.Instance != null)
        {
            foreach (string bossId in _bossIds)
            {
                if (EnemyManager.Instance.IsEnemyAlive(bossId))
                {
                    aliveCount++;
                }
            }
        }

        return aliveCount;
    }

    private string GetBossStatusMessage(int aliveBossCount)
    {
        int deathTreshold = 0;

        if (aliveBossCount > deathTreshold)
        {
            return $"({_bossIds.Length - aliveBossCount}/{_bossIds.Length}) боссов убито";
        }
        else
        {
            return $"Нажмите {InteractionKey} чтобы открыть дверь";
        }
    }

    private void OpenDoor()
    {
        _audioController?.PlayBossDoorOpenSound();

        ShowTemporaryMessage("Дверь открыта, Вы победили", MessageDisplayDuration);

        HideInteractionMessage();
        Destroy(gameObject);
    }

    private void ShowInteractionMessage(string message)
    {
        HideInteractionMessage();

        if (_interactionMessagePrefab != null)
        {
            _currentMessage = Instantiate(_interactionMessagePrefab);

            _currentMessage.SetActive(true);

            _messageText = _currentMessage.GetComponentInChildren<TextMeshProUGUI>();

            if (_messageText != null)
            {
                _messageText.text = message;
            }
        }
    }

    private void HideInteractionMessage()
    {
        if (_currentMessage != null)
        {
            Destroy(_currentMessage);

            _currentMessage = null;
            _messageText = null;
        }
    }

    private void ShowTemporaryMessage(string message, float duration)
    {
        if (_temporaryMessagePrefab != null)
        {
            GameObject tempMessage = Instantiate(_temporaryMessagePrefab);

            tempMessage.SetActive(true);

            TextMeshProUGUI textComponent = tempMessage.GetComponentInChildren<TextMeshProUGUI>();

            if (textComponent != null)
            {
                textComponent.text = message;
            }

            Destroy(tempMessage, duration);
        }
    }

    private void CleanupUI()
    {
        HideInteractionMessage();

        if (_messageCoroutine != null)
        {
            StopCoroutine(_messageCoroutine);

            _messageCoroutine = null;
        }
    }
}