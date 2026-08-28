using System.Collections.Generic;
using DoorControl;
using NPC;
using Player.Input;
using UnityEngine;
using UnityEngine.UI;
using YG;

public sealed class MiniMapController : MonoBehaviour
{
    private const KeyCode DefaultToggleKey = KeyCode.M;
    private const float NormalizedMinValue = -1f;
    private const float NormalizedMaxValue = 1f;
    private const string MapUnlockedKey = "MapUnlocked";

    [System.Serializable]
    public sealed class MiniMapData
    {
        private const float DefaultMapWidth = 50f;
        private const float DefaultMapHeight = 50f;

        public string locationName;
        public Sprite mapTexture;
        public Vector2 mapWorldSize = new Vector2(DefaultMapWidth, DefaultMapHeight);
        public Vector2 mapWorldCenter = Vector2.zero;
    }

    [Header("MiniMap Settings")]
    [SerializeField] private GameObject _miniMapPanel;
    [SerializeField] private KeyCode _toggleKey = DefaultToggleKey;

    [Header("Input")]
    [SerializeField] private IInputProvider _inputProvider;

    [Header("Lock State")]
    [SerializeField] private bool _isMapLocked = true;
    [SerializeField] private GameObject _lockOverlay;

    [Header("Player Marker")]
    [SerializeField] private RectTransform _playerMarker;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Vector2 _mapUISize = new Vector2(400f, 250f);

    [SerializeField] private List<Merchant> _shops;
    [SerializeField] private List<Key> _keys;
    [SerializeField] private List<RectTransform> _keyMarkers;
    [SerializeField] private List<Transform> _keyAnchors;

    [Header("MiniMap Textures")]
    [SerializeField] private List<MiniMapData> _miniMapDataList;
    [SerializeField] private Image _miniMapImage;

    private bool _isMiniMapVisible;
    private RectTransform _miniMapRectTransform;
    private int _currentMapIndex;

    private void Awake()
    {
        InitializeMiniMap();
        FindInputProvider();
    }

    private void Start()
    {
        LoadMapState();
        UpdateMapLockState();
        
    }

    private void Update()
    {
        if (_isMapLocked)
        {
            CheckMapUnlockStatus();

            return;
        }

        HandleMiniMapToggle();
        UpdatePlayerMarkerIfVisible();
    }

  

    public void ToggleMiniMap()
    {
        if (_isMapLocked || _miniMapPanel == null)
        {
            return;
        }

        _isMiniMapVisible = !_isMiniMapVisible;

        _miniMapPanel.SetActive(_isMiniMapVisible);

        if (_playerMarker == null)
        {
            return;
        }

        _playerMarker.gameObject.SetActive(_isMiniMapVisible);

        if (_isMiniMapVisible)
        {
            UpdatePlayerMarker();
        }
    }

    public void UnlockMap()
    {
        if (_isMapLocked == false)
        {
            return;
        }

        _isMapLocked = false;

        SaveMapState();
        UpdateMapLockState();
    }

    public void SetMiniMap(int mapIndex)
    {
        if (mapIndex < 0 || _miniMapDataList == null || mapIndex >= _miniMapDataList.Count)
        {
            return;
        }

        MiniMapData newMap = _miniMapDataList[mapIndex];

        if (_miniMapImage == null || newMap.mapTexture == null)
        {
            return;
        }

        _currentMapIndex = mapIndex;
        _miniMapImage.sprite = newMap.mapTexture;

        if (_isMiniMapVisible)
        {
            UpdatePlayerMarker();
        }
    }

    public void SetMiniMap(string locationName)
    {
        if (_miniMapDataList == null)
        {
            return;
        }

        for (int i = 0; i < _miniMapDataList.Count; i++)
        {
            if (_miniMapDataList[i].locationName == locationName)
            {
                SetMiniMap(i);

                return;
            }
        }
    }

    private void InitializeMiniMap()
    {
        if (_miniMapPanel == null)
        {
            return;
        }

        _isMiniMapVisible = false;

        FindPlayerTransform();
        FindMiniMapImage();
        FindLockOverlay();

        _miniMapRectTransform = _miniMapPanel.GetComponent<RectTransform>();

        _miniMapPanel.SetActive(_isMapLocked == false && _isMiniMapVisible);

        InitializePlayerMarker();
    }

    private void FindPlayerTransform()
    {
        if (_playerTransform != null)
        {
            return;
        }

        Hero hero = FindFirstObjectByType<Hero>();

        if (hero != null)
        {
            _playerTransform = hero.transform;
        }
    }

    private void SetShopsPositions()
    {
        if (_shops == null) return;

        foreach (var merchant in _shops)
        {
            Vector2 normalizedPos = CalculateNormalizedPosition(merchant.transform.position);
            Vector2 uiPos = ConvertToUIPosition(normalizedPos);

            merchant.UpdateMarkerPosition(uiPos);
        }
    }

    private void FindMiniMapImage()
    {
        if (_miniMapImage == null && _miniMapPanel != null)
        {
            _miniMapImage = _miniMapPanel.GetComponentInChildren<Image>();
        }
    }

    private void FindLockOverlay()
    {
        if (_lockOverlay != null || _miniMapPanel == null)
        {
            return;
        }

        Transform lockObject = _miniMapPanel.transform.Find("LockOverlay");

        if (lockObject != null)
        {
            _lockOverlay = lockObject.gameObject;
        }
    }

    private void FindInputProvider()
    {
        if (_inputProvider != null)
        {
            return;
        }

        _inputProvider = FindFirstObjectByType<AggregatedInputProvider>();

        if (_inputProvider == null && YG2.envir.isDesktop)
        {
            _inputProvider = FindFirstObjectByType<OldInputProvider>();
        }

        if (_inputProvider == null && YG2.envir.isMobile)
        {
            _inputProvider = FindFirstObjectByType<JoystickInput>();
        }

        if (_inputProvider == null)
        {
            Debug.LogWarning("IInputProvider не найден");
        }
    }

    private void InitializePlayerMarker()
    {
        if (_playerMarker == null)
        {
            return;
        }

        _playerMarker.gameObject.SetActive(_isMiniMapVisible && _isMapLocked == false);
        _playerMarker.anchoredPosition = Vector2.zero;
    }

    private void CheckMapUnlockStatus()
    {
        Hero hero = FindFirstObjectByType<Hero>();

        if (hero != null && hero.AbilityManager != null && hero.AbilityManager.HasMap)
        {
            UnlockMap();
        }
    }

    private void HandleMiniMapToggle()
    {
        bool inputTogglePressed = _inputProvider != null && _inputProvider.IsOpenMapPressed;
        bool keyboardTogglePressed = Input.GetKeyDown(_toggleKey);

        if (inputTogglePressed || keyboardTogglePressed)
        {
            ToggleMiniMap();
        }
    }

    private void UpdatePlayerMarkerIfVisible()
    {
        if (_isMiniMapVisible && _playerTransform != null && _playerMarker != null && _isMapLocked == false)
        {
            UpdatePlayerMarker();
        }
    }

    private void UpdateKeyMarkers()
    {
        if (_keyAnchors == null || _keyMarkers == null) return;
        if (_keyAnchors.Count != _keyMarkers.Count)
        {
            Debug.LogWarning($"Количество якорей ({_keyAnchors.Count}) не совпадает с количеством маркеров ({_keyMarkers.Count})");
            return;
        }

        for (int i = 0; i < _keyAnchors.Count; i++)
        {
            if (_keyAnchors[i] == null || _keyMarkers[i] == null) continue;

            if (_keys[i].IsCollected)
            {
                _keyMarkers[i].gameObject.SetActive(false);
                continue;
            }

            // Вычисляем позицию на карте
            Vector2 normalizedPos = CalculateNormalizedPosition(_keyAnchors[i].position);
            Vector2 uiPos = ConvertToUIPosition(normalizedPos);

            _keyMarkers[i].anchoredPosition = uiPos;
            _keyMarkers[i].gameObject.SetActive(true);
        }
    }


    private void UpdatePlayerMarker()
    {
        Vector2 normalizedPosition = CalculateNormalizedPlayerPosition();
        Vector2 uiPosition = ConvertToUIPosition(normalizedPosition);

        _playerMarker.anchoredPosition = uiPosition;

        SetShopsPositions();
        UpdateKeyMarkers();
        UpdatePlayerMarkerRotation();
    }

    private Vector2 CalculateNormalizedPosition(Vector2 position)
    {
        const float halfScale = 0.5f;

        if (_miniMapDataList == null ||
            _miniMapDataList.Count <= _currentMapIndex ||
            position == null)
        {
            return Vector2.zero;
        }

        MiniMapData currentMap = _miniMapDataList[_currentMapIndex];
        Vector2 worldOffset = position - currentMap.mapWorldCenter;

        Vector2 normalizedPosition = new Vector2(
            worldOffset.x / (currentMap.mapWorldSize.x * halfScale),
            worldOffset.y / (currentMap.mapWorldSize.y * halfScale));

        normalizedPosition.x = Mathf.Clamp(normalizedPosition.x, NormalizedMinValue, NormalizedMaxValue);
        normalizedPosition.y = Mathf.Clamp(normalizedPosition.y, NormalizedMinValue, NormalizedMaxValue);

        return normalizedPosition;
    }

    private Vector2 CalculateNormalizedPlayerPosition()
    {
        const float halfScale = 0.5f;

        if (_miniMapDataList == null ||
            _miniMapDataList.Count <= _currentMapIndex ||
            _playerTransform == null)
        {
            return Vector2.zero;
        }

        MiniMapData currentMap = _miniMapDataList[_currentMapIndex];
        Vector2 worldOffset = (Vector2)_playerTransform.position - currentMap.mapWorldCenter;

        Vector2 normalizedPosition = new Vector2(
            worldOffset.x / (currentMap.mapWorldSize.x * halfScale),
            worldOffset.y / (currentMap.mapWorldSize.y * halfScale));

        normalizedPosition.x = Mathf.Clamp(normalizedPosition.x, NormalizedMinValue, NormalizedMaxValue);
        normalizedPosition.y = Mathf.Clamp(normalizedPosition.y, NormalizedMinValue, NormalizedMaxValue);

        return normalizedPosition;
    }

    private Vector2 ConvertToUIPosition(Vector2 normalizedPosition)
    {
        const float halfScale = 0.5f;

        return new Vector2(
            normalizedPosition.x * (_mapUISize.x * halfScale),
            normalizedPosition.y * (_mapUISize.y * halfScale));
    }

    private void UpdatePlayerMarkerRotation()
    {
        if (_playerTransform == null || _playerMarker == null)
        {
            return;
        }

        float playerRotation = _playerTransform.eulerAngles.z;

        _playerMarker.localEulerAngles = new Vector3(0f, 0f, playerRotation);
    }

    private void UpdateMapLockState()
    {
        if (_lockOverlay != null)
        {
            _lockOverlay.SetActive(_isMapLocked);
        }

        if (_miniMapPanel != null && _isMapLocked)
        {
            _miniMapPanel.SetActive(false);
            _isMiniMapVisible = false;
        }

        if (_playerMarker != null)
        {
            _playerMarker.gameObject.SetActive(_isMiniMapVisible && _isMapLocked == false);
        }
    }

    private void SaveMapState()
    {
        PlayerPrefs.SetInt(MapUnlockedKey, _isMapLocked ? 0 : 1);
        PlayerPrefs.Save();
    }

    private void LoadMapState()
    {
        if (PlayerPrefs.HasKey(MapUnlockedKey))
        {
            _isMapLocked = PlayerPrefs.GetInt(MapUnlockedKey, 0) == 0;

            return;
        }

        Hero hero = FindFirstObjectByType<Hero>();

        if (hero != null && hero.AbilityManager != null && hero.AbilityManager.HasMap)
        {
            _isMapLocked = false;
        }
    }
}