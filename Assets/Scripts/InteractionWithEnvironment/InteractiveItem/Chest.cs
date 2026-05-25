using Cainos.LucidEditor;
using DoorControl;
using UnityEngine;

namespace ChestControl
{
    public sealed class Chest : MonoBehaviour
    {
        private const KeyCode DefaultInteractKey = KeyCode.E;
        private const float DefaultCheckRadius = 0.5f;
        private const float DefaultKeySpawnForce = 2f;
        private const float GizmoSphereRadius = 0.2f;

        [FoldoutGroup("Reference")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [FoldoutGroup("Reference")]
        [SerializeField] private Sprite _spriteOpened;

        [FoldoutGroup("Reference")]
        [SerializeField] private Sprite _spriteClosed;

        [FoldoutGroup("Interaction")]
        [SerializeField] private KeyCode _interactKey = DefaultInteractKey;

        [FoldoutGroup("Interaction")]
        [SerializeField] private float _checkRadius = DefaultCheckRadius;

        [FoldoutGroup("Interaction")]
        [SerializeField] private LayerMask _playerLayer;

        [FoldoutGroup("Key Settings")]
        [SerializeField] private GameObject _keyPrefab;

        [FoldoutGroup("Key Settings")]
        [SerializeField] private DoorControl.KeyColor _keyColor = DoorControl.KeyColor.WhiteColor;

        [FoldoutGroup("Key Settings")]
        [SerializeField] private Vector2 _keySpawnOffset = new Vector2(0f, 1f);

        [FoldoutGroup("Key Settings")]
        [SerializeField] private float _keySpawnForce = DefaultKeySpawnForce;

        [FoldoutGroup("Sound Settings")]
        [SerializeField] private AudioClip _openSound;

        [FoldoutGroup("Sound Settings")]
        [SerializeField] private AudioClip _closeSound;

        [FoldoutGroup("Sound Settings")]
        [SerializeField] private AudioClip _keySpawnSound;

        [FoldoutGroup("Sound Settings")]
        [SerializeField] private float _soundVolume = 1f;

        [FoldoutGroup("Save Settings")]
        [SerializeField, HideInInspector]
        private bool _isOpened;

        [SerializeField, HideInInspector]
        private bool _isKeySpawned = false;

        [SerializeField]
        private string _chestId;

        private bool _isPlayerInRange;
        private Animator _animator;
        private AudioSource _audioSource;

        private Animator CachedAnimator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = GetComponent<Animator>();
                }

                return _animator;
            }
        }

        private AudioSource CachedAudioSource
        {
            get
            {
                if (_audioSource == null)
                {
                    _audioSource = GetComponent<AudioSource>();
                }

                return _audioSource;
            }
        }

        [FoldoutGroup("Runtime"), ShowInInspector]
        public bool IsOpened => _isOpened;

        private void Start()
        {
            InitializeChest();
        }

        private void Update()
        {
            HandlePlayerInteraction();
        }

        [FoldoutGroup("Runtime"), HorizontalGroup("Runtime/Button"), Button("Open")]
        public void Open()
        {
            SetOpened(true);
        }

        [FoldoutGroup("Runtime"), HorizontalGroup("Runtime/Button"), Button("Close")]
        public void Close()
        {
            SetOpened(false);
        }

        [FoldoutGroup("Runtime"), Button("Reset Chest")]
        public void ResetChest()
        {
            _isOpened = false;
            _isKeySpawned = false;

            ApplyVisualState(_isOpened);
        }

        private void InitializeChest()
        {
            if (string.IsNullOrEmpty(_chestId))
            {
                _chestId = GenerateChestId();
            }

            LoadChestState();

            if (CachedAnimator != null)
            {
                _animator.Play(_isOpened ? "Opened" : "Closed");
            }

            ApplyVisualState(_isOpened);
        }

        private string GenerateChestId()
        {
            return $"Chest_{gameObject.scene.name}_{transform.position.x:F2}_{transform.position.y:F2}";
        }

        private void LoadChestState()
        {
            _isOpened = GameStateManager.IsChestOpened(_chestId);
            _isKeySpawned = GameStateManager.IsKeySpawned(_chestId);
        }

        private void HandlePlayerInteraction()
        {
            if (_isPlayerInRange && Input.GetKeyDown(_interactKey) && !_isOpened)
            {
                Open();
            }
        }

        private void SetOpened(bool opened)
        {
            if (_isOpened == opened)
            {
                return;
            }

            _isOpened = opened;

            if (Application.isPlaying)
            {
                CachedAnimator.SetBool("IsOpened", _isOpened);

                PlayChestSound(opened);

                if (_isOpened && !_isKeySpawned)
                {
                    SpawnKey();

                    _isKeySpawned = true;
                }

                GameStateManager.SetChestOpened(_chestId, _isOpened);
                GameStateManager.SetKeySpawned(_chestId, _isKeySpawned);
            }
            else
            {
                ApplyVisualState(_isOpened);
            }
        }

        private void PlayChestSound(bool opened)
        {
            AudioClip soundToPlay = opened ? _openSound : _closeSound;

            if (soundToPlay != null)
            {
                AudioController audioController = FindFirstObjectByType<AudioController>();

                if (audioController != null)
                {
                    audioController.PlayOneShotWithVolume(soundToPlay, _soundVolume);
                }
                else if (CachedAudioSource != null)
                {
                    CachedAudioSource.PlayOneShot(soundToPlay, _soundVolume);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(soundToPlay, transform.position, _soundVolume);
                }
            }
        }

        private void ApplyVisualState(bool opened)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = opened ? _spriteOpened : _spriteClosed;
            }
        }

        private void SpawnKey()
        {
            if (_keyPrefab == null)
            {
                return;
            }

            Vector3 spawnPosition = transform.position + (Vector3)_keySpawnOffset;

            GameObject keyInstance = Instantiate(_keyPrefab, spawnPosition, Quaternion.identity);

            keyInstance.SetActive(true);

            ConfigureKeyComponent(keyInstance);
            ApplyKeyPhysics(keyInstance);
            PlayKeySpawnSound();
        }

        private void ConfigureKeyComponent(GameObject keyInstance)
        {
            Key keyComponent = keyInstance.GetComponent<Key>();

            if (keyComponent != null)
            {
                keyComponent.keyColor = _keyColor;
            }
            else
            {
                SimpleKey simpleKeyComponent = keyInstance.GetComponent<SimpleKey>();

                if (simpleKeyComponent != null)
                {
                    SetKeyColorViaReflection(simpleKeyComponent, _keyColor);
                }
            }
        }

        private void SetKeyColorViaReflection(SimpleKey simpleKey, KeyColor color)
        {
            var field = typeof(SimpleKey).GetField("_keyColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            field?.SetValue(simpleKey, color);
        }

        private void ApplyKeyPhysics(GameObject keyInstance)
        {
            float minHorizontalForce = -0.3f;
            float maxHorizontalForce = 0.3f;
            float verticalForce = 1f;

            Rigidbody2D keyRigidbody = keyInstance.GetComponent<Rigidbody2D>();

            if (keyRigidbody != null)
            {
                Vector2 forceDirection = new Vector2(Random.Range(-minHorizontalForce, maxHorizontalForce), verticalForce).normalized;

                keyRigidbody.AddForce(forceDirection * _keySpawnForce, ForceMode2D.Impulse);
            }
        }

        private void PlayKeySpawnSound()
        {
            if (_keySpawnSound != null)
            {
                AudioController audioController = FindFirstObjectByType<AudioController>();

                if (audioController != null)
                {
                    audioController.PlayOneShotWithVolume(_keySpawnSound, _soundVolume);
                }
                else if (CachedAudioSource != null)
                {
                    CachedAudioSource.PlayOneShot(_keySpawnSound, _soundVolume);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(_keySpawnSound, transform.position, _soundVolume);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            int bitShiftAmount = 1;
            int noLayerMatch = 0;

            if ((_playerLayer.value & (bitShiftAmount << other.gameObject.layer)) != noLayerMatch)
            {
                _isPlayerInRange = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            int bitShiftAmount = 1;
            int noLayerMatch = 0;

            if ((_playerLayer.value & (bitShiftAmount << other.gameObject.layer)) != noLayerMatch)
            {
                _isPlayerInRange = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _checkRadius);

            if (_keyPrefab != null)
            {
                Gizmos.color = Color.green;

                Vector3 spawnPosition = transform.position + (Vector3)_keySpawnOffset;

                Gizmos.DrawWireSphere(spawnPosition, GizmoSphereRadius);
                Gizmos.DrawLine(transform.position, spawnPosition);
            }
        }
    }
}