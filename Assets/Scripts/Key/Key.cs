using Cainos.LucidEditor;
using DoorControl;
using System.Collections;
using UnityEngine;

namespace DoorControl
{
    public sealed class Key : MonoBehaviour
    {
        private const string PlayerTag = "Player";
        private const float CollectAnimationDuration = 0.5f;
        private const float RotationSpeed = 360f;
        private const float ScaleMultiplier = 0.2f;

        [FoldoutGroup("Key Settings")]
        public KeyColor keyColor = KeyColor.WhiteColor;

        [FoldoutGroup("Collection Effects")]
        public AudioClip collectSound;

        [FoldoutGroup("Collection Effects")]
        public float collectSoundVolume = 0.5f;

        [Header("Map Marker")]
        [SerializeField] private RectTransform _keyMarker;
        [SerializeField] private RectTransform _collectedKeyMarker;

        private bool _isCollected;
        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;

        public KeyColor Color => keyColor;
        public bool IsCollected => _isCollected;

        private void Start()
        {
            InitializeKey();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isCollected || other.CompareTag(PlayerTag) == false)
            {
                return;
            }

            CollectKey(other.gameObject);
        }

        private void InitializeKey()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();

            if (_collider != null)
            {
                _collider.isTrigger = true;
            }

            ApplyKeyColor();
        }

        private void ApplyKeyColor()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            _spriteRenderer.color = keyColor switch
            {
                KeyColor.WhiteColor => UnityEngine.Color.white,
                KeyColor.BlackColor => UnityEngine.Color.black,
                KeyColor.YellowColor => UnityEngine.Color.yellow,
                KeyColor.BlueColor => UnityEngine.Color.blue,
                _ => UnityEngine.Color.white
            };
        }

        public void UpdateMarkerPosition(Vector2 uiPosition)
        {
            if (_keyMarker != null)
            {
                _keyMarker.anchoredPosition = uiPosition;
                _keyMarker.gameObject.SetActive(true);
            }
        }

        private void CollectKey(GameObject player)
        {

            Debug.Log($"Ключ {gameObject.name}: собирается!"); // ← Добавь

            _isCollected = true;
            Debug.Log($"Ключ {gameObject.name}: _isCollected = {_isCollected}"); // ← Добавь

            _isCollected = true;

            if (_collider != null)
            {
                _collider.enabled = false;
            }

            AddKeyToPlayer(player);

            if (_keyMarker != null)
                _keyMarker.gameObject.SetActive(false);
            _collectedKeyMarker.gameObject.SetActive(true);

            StartCoroutine(CollectAnimation());
        }

        private void AddKeyToPlayer(GameObject player)
        {
            KeyCollection keyCollection = player.GetComponent<KeyCollection>();

            if (keyCollection == null)
            {
                keyCollection = player.GetComponentInParent<KeyCollection>();
            }

            keyCollection?.AddKey(keyColor);
        }

        private IEnumerator CollectAnimation()
        {
            PlayCollectEffects();

            float elapsedTime = 0f;
            Vector3 originalScale = transform.localScale;

            while (elapsedTime < CollectAnimationDuration)
            {
                elapsedTime += Time.deltaTime;

                float progress = elapsedTime / CollectAnimationDuration;

                transform.Rotate(0f, 0f, RotationSpeed * Time.deltaTime);
                transform.localScale = originalScale * (1f + progress * ScaleMultiplier);

                if (_spriteRenderer != null)
                {
                    Color color = _spriteRenderer.color;

                    color.a = 1f - progress;

                    _spriteRenderer.color = color;
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        private void PlayCollectEffects()
        {
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position, collectSoundVolume);
            }
        }
    }
}