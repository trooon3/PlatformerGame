using System.Collections.Generic;
using Player.Input;
using UnityEngine;
using UnityEngine.UI;
using YG;

public sealed class MiniMapController : MonoBehaviour
{
    private const KeyCode DefaultToggleKey = KeyCode.M;
    private const float NormalizedMinValue = -1f;
    private const float NormalizedMaxValue = 1f;

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

    [Header("MiniMap Textures")]
    [SerializeField] private List<MiniMapData> _miniMapDataList;
    [SerializeField] private Image _miniMapImage;

    private bool _isMiniMapVisible;
    private RectTransform _miniMapRectTransform;
    private int _currentMapIndex = 0;

    private const string MapUnlockedKey = "MapUnlocked";

    [System.Serializable]
    public class MiniMapData
    {
        private const float DefaultMapWidth = 50f;
        private const float DefaultMapHeight = 50f;

        public string locationName;
        public Sprite mapTexture;
        public Vector2 mapWorldSize = new Vector2(DefaultMapWidth, DefaultMapHeight);
        public Vector2 mapWorldCenter = Vector2.zero;
    }

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

    private void InitializeMiniMap()
    {
        if (_miniMapPanel == null)
        {
            return;
        }

        _isMiniMapVisible = false;

        if (_playerTransform == null)
        {
            var hero = FindObjectOfType<Hero>();

            if (hero != null)
            {
                _playerTransform = hero.transform;
            }
        }

        if (_miniMapImage == null)
        {
            _miniMapImage = _miniMapPanel.GetComponentInChildren<Image>();
        }

        if (_lockOverlay == null && _miniMapPanel != null)
        {
            var lockObj = _miniMapPanel.transform.Find("LockOverlay");

            if (lockObj != null)
            {
                _lockOverlay = lockObj.gameObject;
            }
        }

        _miniMapRectTransform = _miniMapPanel.GetComponent<RectTransform>();

        if (_isMapLocked)
        {
            _miniMapPanel.SetActive(false);
        }
        else
        {
            _miniMapPanel.SetActive(_isMiniMapVisible);
        }

        InitializePlayerMarker();
    }

    private void FindInputProvider()
    {
        if (_inputProvider == null)
        {
            if (YG2.envir.isDesktop)
            {
                _inputProvider = FindObjectOfType<OldInputProvider>();
            }
            else if (YG2.envir.isMobile)
            {
                _inputProvider = FindObjectOfType<JoystickInput>();
            }
        }

        if (_inputProvider == null)
            Debug.LogWarning("IInputProvider not found! Map toggle won't work with key.");
    }

    private void InitializePlayerMarker()
    {
        if (_playerMarker != null)
        {
            _playerMarker.gameObject.SetActive(_isMiniMapVisible && !_isMapLocked);
            _playerMarker.anchoredPosition = Vector2.zero;
        }
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

    private void CheckMapUnlockStatus()
    {
        var hero = FindObjectOfType<Hero>();

        if (hero != null && hero.AbilityManager != null)
        {
            if (hero.AbilityManager.HasMap && _isMapLocked)
            {
                UnlockMap();
            }
        }
    }

    private void HandleMiniMapToggle()
    {
        if (_inputProvider != null && _inputProvider.IsOpenMapPressed)
        {
            ToggleMiniMap();
        }
    }

    private void UpdatePlayerMarkerIfVisible()
    {
        if (_isMiniMapVisible && _playerTransform != null && _playerMarker != null && !_isMapLocked)
        {
            UpdatePlayerMarker();
        }
    }

    private void UpdatePlayerMarker()
    {
        Vector2 normalizedPosition = CalculateNormalizedPlayerPosition();
        Vector2 uiPosition = ConvertToUIPosition(normalizedPosition);

        _playerMarker.anchoredPosition = uiPosition;

        UpdatePlayerMarkerRotation();
    }

    private Vector2 CalculateNormalizedPlayerPosition()
    {
        float halfScale = 0.5f;

        if (_miniMapDataList.Count <= _currentMapIndex || _playerTransform == null)
        {
            return Vector2.zero;
        }

        MiniMapData currentMap = _miniMapDataList[_currentMapIndex];

        Vector2 worldOffset = (Vector2)_playerTransform.position - currentMap.mapWorldCenter;

        Vector2 normalizedPosition = new Vector2(worldOffset.x / (currentMap.mapWorldSize.x * halfScale), worldOffset.y / (currentMap.mapWorldSize.y * halfScale));

        normalizedPosition.x = Mathf.Clamp(normalizedPosition.x, NormalizedMinValue, NormalizedMaxValue);
        normalizedPosition.y = Mathf.Clamp(normalizedPosition.y, NormalizedMinValue, NormalizedMaxValue);

        return normalizedPosition;
    }

    private Vector2 ConvertToUIPosition(Vector2 normalizedPosition)
    {
        float halfScale = 0.5f;

        return new Vector2(normalizedPosition.x * (_mapUISize.x * halfScale), normalizedPosition.y * (_mapUISize.y * halfScale));
    }

    private void UpdatePlayerMarkerRotation()
    {
        if (_playerTransform == null || _playerMarker == null)
        {
            return;
        }

        float playerRotation = _playerTransform.eulerAngles.z;

        _playerMarker.localEulerAngles = new Vector3(0, 0, playerRotation);
    }

    public void ToggleMiniMap()
    {
        if (_isMapLocked)
        {
            return;
        }

        _isMiniMapVisible = !_isMiniMapVisible;

        _miniMapPanel.SetActive(_isMiniMapVisible);


        if (_playerMarker != null)
        {
            _playerMarker.gameObject.SetActive(_isMiniMapVisible);

            if (_isMiniMapVisible)
            {
                UpdatePlayerMarker();
            }
        }
    }

    public void UnlockMap()
    {
        if (_isMapLocked)
        {
            _isMapLocked = false;

            SaveMapState();
            UpdateMapLockState();
        }
    }

    private void UpdateMapLockState()
    {
        if (_lockOverlay != null)
        {
            _lockOverlay.SetActive(_isMapLocked);
        }

        if (_isMapLocked && _miniMapPanel != null)
        {
            _miniMapPanel.SetActive(false);
            _isMiniMapVisible = false;
        }

        if (_playerMarker != null)
        {
            _playerMarker.gameObject.SetActive(_isMiniMapVisible && !_isMapLocked);
        }
    }

    private void SaveMapState()
    {
        int unlockedValue = 1;
        int lockedValue = 0;

        PlayerPrefs.SetInt(MapUnlockedKey, _isMapLocked ? lockedValue : unlockedValue);
        PlayerPrefs.Save();
    }

    private void LoadMapState()
    {
        int unlockedStateValue = 0;
        int defaultUnlockedValue = 0;

        if (PlayerPrefs.HasKey(MapUnlockedKey))
        {
            int unlocked = PlayerPrefs.GetInt(MapUnlockedKey, unlockedStateValue);

            _isMapLocked = (unlocked == defaultUnlockedValue);
        }
        else
        {
            var hero = FindObjectOfType<Hero>();

            if (hero != null && hero.AbilityManager != null && hero.AbilityManager.HasMap)
            {
                _isMapLocked = false;
            }
        }
    }

    public void SetMiniMap(int mapIndex)
    {
        int minimimValidIndex = 0;

        if (mapIndex >= minimimValidIndex && mapIndex < _miniMapDataList.Count)
        {
            MiniMapData newMap = _miniMapDataList[mapIndex];

            if (_miniMapImage == null)
            {
                return;
            }

            if (newMap.mapTexture == null)
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
    }

    public void SetMiniMap(string locationName)
    {
        for (int i = 0; i < _miniMapDataList.Count; i++)
        {
            if (_miniMapDataList[i].locationName == locationName)
            {
                SetMiniMap(i);

                return;
            }
        }
    }
}