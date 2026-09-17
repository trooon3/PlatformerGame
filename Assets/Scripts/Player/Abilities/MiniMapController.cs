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

        [Header("Точные границы (Polygon Collider)")]
        public Collider2D mapBoundsCollider;
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
    [SerializeField] private List<RectTransform> _keyMarkers;
    [SerializeField] private List<RectTransform> _keyCollectedMarkers;
    [SerializeField] private List<Transform> _keyAnchors;

    [SerializeField] private List<Checkpoint> _checkpoints;
    [SerializeField] private List<RectTransform> _checkpointMarkers;
    [SerializeField] private GameObject _checkpointMarkerPrefab;

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
        CreateCheckpointMarkers();
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
            if (_checkpointMarkers == null || _checkpointMarkers.Count == 0)
            {
                CreateCheckpointMarkers();
            }

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
        if (_shops == null)
        {
            return;
        }

        foreach (var merchant in _shops)
        {
            bool isInsideBounds = IsPositionInCurrentMapBounds(merchant.transform.position);

            merchant.SetMarkerVisible(isInsideBounds);

            if (isInsideBounds)
            {
                Vector2 normalizedPos = CalculateNormalizedPosition(merchant.transform.position);
                Vector2 uiPos = ConvertToUIPosition(normalizedPos);
                merchant.UpdateMarkerPosition(uiPos);
            }
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

    private void UpdateCheckpointMarkers()
    {
        if (_checkpoints == null || _checkpointMarkers == null)
        {
            return;
        }

        int markerIndex = 0;

        for (int i = 0; i < _checkpoints.Count; i++)
        {
            if (_checkpoints[i] == null)
            {
                continue;
            }

            if (markerIndex >= _checkpointMarkers.Count)
            {
                break;
            }

            RectTransform marker = _checkpointMarkers[markerIndex];

            if (marker == null || marker.gameObject == null)
            {
                markerIndex++;

                continue;
            }

            bool isInsideBounds = IsPositionInCurrentMapBounds(_checkpoints[i].transform.position);

            marker.gameObject.SetActive(isInsideBounds);

            if (isInsideBounds)
            {
                Vector2 normalizedPos = CalculateNormalizedPosition(_checkpoints[i].transform.position);
                Vector2 uiPos = ConvertToUIPosition(normalizedPos);
                marker.anchoredPosition = uiPos;
            }

            markerIndex++;
        }
    }

    private void CreateCheckpointMarkers()
    {
        if (_checkpoints == null || _checkpointMarkerPrefab == null || _miniMapPanel == null)
        {
            return;
        }

        if (_checkpointMarkers != null)
        {
            foreach (var marker in _checkpointMarkers)
            {
                if (marker != null)
                {
                    Destroy(marker.gameObject);
                }
            }

            _checkpointMarkers.Clear();
        }
        else
        {
            _checkpointMarkers = new List<RectTransform>();
        }

        foreach (var checkpoint in _checkpoints)
        {
            if (checkpoint == null)
            {
                continue;
            }

            GameObject marker = Instantiate(_checkpointMarkerPrefab, _miniMapPanel.transform, false);

            RectTransform rect = marker.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.localScale = Vector3.one; 
                _checkpointMarkers.Add(rect);
            }
        }
    }

    private void UpdateKeyMarkers()
    {
        if (_keyAnchors == null || _keyMarkers == null || _keyCollectedMarkers == null)
        {
            return;
        }

        if (_keyAnchors.Count != _keyMarkers.Count || _keyAnchors.Count != _keyCollectedMarkers.Count)
        {
            return;
        }

        KeyCollection keyCollection = FindFirstObjectByType<KeyCollection>();

        if (keyCollection == null)
        { 
            return;
        }

        for (int i = 0; i < _keyAnchors.Count; i++)
        {
            if (_keyAnchors[i] == null || _keyMarkers[i] == null || _keyCollectedMarkers[i] == null)
            {
                continue;
            }

            bool isInsideBounds = IsPositionInCurrentMapBounds(_keyAnchors[i].position);

            KeyColor color = (KeyColor)i;

            bool isCollected = keyCollection.HasKey(color);

            _keyMarkers[i].gameObject.SetActive(isInsideBounds && !isCollected);
            _keyCollectedMarkers[i].gameObject.SetActive(isInsideBounds && isCollected);

            if (isInsideBounds)
            {
                Vector2 normalizedPos = CalculateNormalizedPosition(_keyAnchors[i].position);
                Vector2 uiPos = ConvertToUIPosition(normalizedPos);

                if (isCollected == false)
                {
                    _keyMarkers[i].anchoredPosition = uiPos;
                }

                if (isCollected)
                {
                    _keyCollectedMarkers[i].anchoredPosition = uiPos;
                }
            }
        }
    }

    private void UpdatePlayerMarker()
    {
        Vector2 normalizedPosition = CalculateNormalizedPlayerPosition();
        Vector2 uiPosition = ConvertToUIPosition(normalizedPosition);

        _playerMarker.anchoredPosition = uiPosition;

        SetShopsPositions();
        UpdateKeyMarkers();
        UpdateCheckpointMarkers();
        UpdatePlayerMarkerRotation();
    }

    private Vector2 CalculateNormalizedPosition(Vector2 position)
    {
        const float halfScale = 0.5f;

        if (_miniMapDataList == null ||
            _miniMapDataList.Count <= _currentMapIndex)
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

        if (_miniMapDataList == null || _miniMapDataList.Count <= _currentMapIndex || _playerTransform == null)
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

        return new Vector2(normalizedPosition.x * (_mapUISize.x * halfScale), normalizedPosition.y * (_mapUISize.y * halfScale));
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

    private bool IsPositionInCurrentMapBounds(Vector2 position)
    {
        if (_miniMapDataList == null || _miniMapDataList.Count <= _currentMapIndex)
        {
            return false;
        }

        MiniMapData currentMap = _miniMapDataList[_currentMapIndex];

        if (currentMap.mapBoundsCollider != null)
        {
            return currentMap.mapBoundsCollider.OverlapPoint(position);
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (_miniMapDataList == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;

        foreach (var map in _miniMapDataList)
        {
            Gizmos.DrawWireCube(map.mapWorldCenter, map.mapWorldSize);
        }
    }
}