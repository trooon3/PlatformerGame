using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DoorControl
{
    public sealed class KeyCollection : MonoBehaviour
    {
        private const float DefaultSpacing = 80f;
        private const float CollectedAlpha = 1f;
        private const float UncollectedAlpha = 0.1f;

        [System.Serializable]
        public class KeySprite
        {
            public KeyColor color;
            public Sprite sprite;
            public Sprite collectedSprite;
        }

        [SerializeField] private RectTransform _keyDisplayPanel;
        [SerializeField] private GameObject _keyDisplayPrefab;
        [SerializeField] private List<KeySprite> _keySprites = new List<KeySprite>();
        [SerializeField] private float _spacing = DefaultSpacing;

        private Dictionary<KeyColor, bool> _collectedKeys = new Dictionary<KeyColor, bool>();
        private Dictionary<KeyColor, Image> _keyDisplayImages = new Dictionary<KeyColor, Image>();
        private Dictionary<KeyColor, Sprite> _keySpriteMap = new Dictionary<KeyColor, Sprite>();
        private Dictionary<KeyColor, Sprite> _collectedSpriteMap = new Dictionary<KeyColor, Sprite>();
        private bool _isInitialized = false;

        private void Start()
        {
            InitializeKeySystem();
            LoadSavedKeys();
        }

        private void InitializeKeySystem()
        {
            if (_isInitialized)
            {
                return;
            }

            InitializeSpriteMappings();
            InitializeKeyStates();
            CreateKeyDisplay();

            _isInitialized = true;
        }

        private void InitializeSpriteMappings()
        {
            _keySpriteMap.Clear();
            _collectedSpriteMap.Clear();

            foreach (KeySprite keySprite in _keySprites)
            {
                _keySpriteMap[keySprite.color] = keySprite.sprite;
                _collectedSpriteMap[keySprite.color] = keySprite.collectedSprite ?? keySprite.sprite;
            }
        }

        private void InitializeKeyStates()
        {
            _collectedKeys.Clear();

            foreach (KeyColor color in System.Enum.GetValues(typeof(KeyColor)))
            {
                _collectedKeys[color] = false;
            }
        }

        private void CreateKeyDisplay()
        {
            if (_keyDisplayPanel == null || _keyDisplayPrefab == null)
            {
                return;
            }

            ClearExistingDisplay();
            CreateKeyDisplayItems();
        }

        private void ClearExistingDisplay()
        {
            foreach (Transform child in _keyDisplayPanel)
            {
                Destroy(child.gameObject);
            }

            _keyDisplayImages.Clear();
        }

        private void CreateKeyDisplayItems()
        {
            int keyIndex = 0;

            foreach (KeyColor color in System.Enum.GetValues(typeof(KeyColor)))
            {
                CreateKeyDisplayItem(color, keyIndex);

                keyIndex++;
            }
        }

        private void CreateKeyDisplayItem(KeyColor color, int index)
        {
            GameObject keyDisplayObject = Instantiate(_keyDisplayPrefab, _keyDisplayPanel);

            ConfigureKeyDisplayObject(keyDisplayObject, color, index);

            Image keyImage = keyDisplayObject.GetComponent<Image>();

            if (keyImage != null)
            {
                _keyDisplayImages[color] = keyImage;
                InitializeKeyDisplay(color, keyImage);
            }
        }

        private void ConfigureKeyDisplayObject(GameObject displayObject, KeyColor color, int index)
        {
            displayObject.name = $"{color}Key";
            displayObject.SetActive(true);

            RectTransform rectTransform = displayObject.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(index * _spacing, 0);
                rectTransform.localScale = Vector3.one;
            }
        }

        private void InitializeKeyDisplay(KeyColor color, Image image)
        {
            image.enabled = true;

            if (_keySpriteMap.ContainsKey(color))
            {
                image.sprite = _keySpriteMap[color];
            }

            UpdateKeyDisplay(color, false);
        }

        private void LoadSavedKeys()
        {
            foreach (KeyColor color in System.Enum.GetValues(typeof(KeyColor)))
            {
                if (GameStateManager.HasKey(color))
                {
                    _collectedKeys[color] = true;
                    UpdateKeyDisplay(color, true);
                }
            }
        }

        public void AddKey(KeyColor color)
        {
            EnsureInitialized();

            if (_collectedKeys.ContainsKey(color))
            {
                _collectedKeys[color] = true;
                UpdateKeyDisplay(color, true);

                GameStateManager.AddKey(color);
            }
        }

        public bool HasKey(KeyColor color)
        {
            EnsureInitialized();

            return _collectedKeys.ContainsKey(color) && _collectedKeys[color];
        }

        public bool UseKey(KeyColor color)
        {
            if (!HasKey(color))
            {
                return false;
            }

            _collectedKeys[color] = false;

            UpdateKeyDisplay(color, false);

            return true;
        }

        public bool CanUseKey(KeyColor color)
        {
            return HasKey(color);
        }

        public void ResetAllKeys()
        {
            foreach (KeyColor color in System.Enum.GetValues(typeof(KeyColor)))
            {
                _collectedKeys[color] = false;

                UpdateKeyDisplay(color, false);
            }
        }

        public List<KeyColor> GetCollectedKeys()
        {
            List<KeyColor> collectedKeys = new List<KeyColor>();

            foreach (KeyValuePair<KeyColor, bool> keyPair in _collectedKeys)
            {
                if (keyPair.Value)
                {
                    collectedKeys.Add(keyPair.Key);
                }
            }

            return collectedKeys;
        }

        public void LoadCollectedKeys(List<KeyColor> collectedKeys)
        {
            if (collectedKeys == null)
            {
                return;
            }

            ResetAllKeys();

            foreach (KeyColor color in collectedKeys)
            {
                AddKey(color);
            }
        }

        private void UpdateKeyDisplay(KeyColor color, bool isCollected)
        {
            if (!_keyDisplayImages.ContainsKey(color) || _keyDisplayImages[color] == null)
            {
                return;
            }

            Image displayImage = _keyDisplayImages[color];

            UpdateDisplaySprite(displayImage, color, isCollected);
            UpdateDisplayAlpha(displayImage, isCollected);
        }

        private void UpdateDisplaySprite(Image image, KeyColor color, bool isCollected)
        {
            if (isCollected && _collectedSpriteMap.ContainsKey(color))
            {
                image.sprite = _collectedSpriteMap[color];
            }
            else if (_keySpriteMap.ContainsKey(color))
            {
                image.sprite = _keySpriteMap[color];
            }
        }

        private void UpdateDisplayAlpha(Image image, bool isCollected)
        {
            float targetAlpha = isCollected ? CollectedAlpha : UncollectedAlpha;
            UnityEngine.Color imageColor = image.color;
            imageColor.a = targetAlpha;
            image.color = imageColor;
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                InitializeKeySystem();
            }
        }
    }
}