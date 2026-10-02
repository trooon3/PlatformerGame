using Cainos.LucidEditor;
using DoorControl;
using UnityEngine;
using Player.Input;
using YG;

namespace ChestControl
{
    public sealed class Chest : MonoBehaviour
    {
        private const float DefaultCheckRadius = 0.5f;
        private const float DefaultKeySpawnForce = 2f;
        private const float GizmoSphereRadius = 0.2f;

        [FoldoutGroup("Input")]
        [SerializeField] private IInputProvider _inputProvider;

        [FoldoutGroup("Reference")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [FoldoutGroup("Reference")]
        [SerializeField] private Sprite _spriteOpened;

        [FoldoutGroup("Reference")]
        [SerializeField] private Sprite _spriteClosed;

        [FoldoutGroup("Interaction")]
        [SerializeField] private GameObject _interactionHint;

        [FoldoutGroup("Interaction")]
        [SerializeField] private float _checkRadius = DefaultCheckRadius;

        [FoldoutGroup("Interaction")]
        [SerializeField] private LayerMask _playerLayer;

        [FoldoutGroup("Key Settings")]
        [SerializeField] private GameObject _keyPrefab;

        [FoldoutGroup("Key Settings")]
        [SerializeField] private KeyColor _keyColor = KeyColor.WhiteColor;

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
        [SerializeField, HideInInspector] private bool _isOpened;

        [SerializeField, HideInInspector] private bool _isKeySpawned = false;

        [SerializeField] private string _chestId;

        private bool _isPlayerInRange;
        private bool _wasPlayerInside;
        private Animator _animator;
        private AudioSource _audioSource;
        private AudioController _audioController;

        [FoldoutGroup("Runtime"), ShowInInspector]
        public bool IsOpened => _isOpened;

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

        private void Start()
        {
            InitializeChest();
            FindInputProvider();
        }

        private void Update()
        {
            if (_inputProvider == null)
            {
                FindInputProvider();
            }

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
            _audioController = FindFirstObjectByType<AudioController>();

            if (_interactionHint != null)
            {
                _interactionHint.SetActive(false);
            }

            if (string.IsNullOrEmpty(_chestId))
            {
                _chestId = GenerateChestId();
            }

            LoadChestState();

            if (CachedAnimator != null)
            {
                CachedAnimator.Play(_isOpened ? "Opened" : "Closed");
            }

            ApplyVisualState(_isOpened);
        }

        private void FindInputProvider()
        {
            if (_inputProvider != null) return;

            _inputProvider = FindFirstObjectByType<AggregatedInputProvider>();

            if (_inputProvider == null && YG2.envir.isDesktop)
                _inputProvider = FindFirstObjectByType<OldInputProvider>();

            if (_inputProvider == null && YG2.envir.isMobile)
                _inputProvider = FindFirstObjectByType<JoystickInput>();
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
            _isPlayerInRange = Physics2D.OverlapCircle(transform.position, _checkRadius, _playerLayer) != null;

            if (_isPlayerInRange != _wasPlayerInside)
            {
                _wasPlayerInside = _isPlayerInRange;
                UpdateInteractionHint(_isPlayerInRange);
            }

            if (_isPlayerInRange && _inputProvider != null && _inputProvider.IsOpenShopOrChestPressed && _isOpened == false)
            {
                Open();
            }
        }

        private void UpdateInteractionHint(bool isInside)
        {
            if (_isOpened) return;

            if (_interactionHint != null)
            {
                _interactionHint.SetActive(isInside);
            }
        }

        private void SetOpened(bool opened)
        {
            if (_isOpened == opened)
            {
                return;
            }

            _isOpened = opened;

            if (_interactionHint != null && opened)
            {
                _interactionHint.SetActive(false);
            }

            if (Application.isPlaying)
            {
                ApplyRuntimeState(opened);
            }
            else
            {
                ApplyVisualState(_isOpened);
            }
        }

        private void ApplyRuntimeState(bool opened)
        {
            if (CachedAnimator != null)
            {
                CachedAnimator.SetBool("IsOpened", _isOpened);
            }

            PlayChestSound(opened);

            if (_isOpened && _isKeySpawned == false)
            {
                SpawnKey();

                _isKeySpawned = true;
            }

            GameStateManager.SetChestOpened(_chestId, _isOpened);
            GameStateManager.SetKeySpawned(_chestId, _isKeySpawned);
        }

        private void PlayChestSound(bool opened)
        {
            AudioClip soundToPlay = opened ? _openSound : _closeSound;

            PlaySound(soundToPlay);
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
            PlaySound(_keySpawnSound);
        }

        private void ConfigureKeyComponent(GameObject keyInstance)
        {
            Key keyComponent = keyInstance.GetComponent<Key>();

            if (keyComponent != null)
            {
                keyComponent.keyColor = _keyColor;

                return;
            }

            SimpleKey simpleKeyComponent = keyInstance.GetComponent<SimpleKey>();

            if (simpleKeyComponent != null)
            {
                simpleKeyComponent.SetKeyColor(_keyColor);
            }
        }

        private void ApplyKeyPhysics(GameObject keyInstance)
        {
            float minHorizontalForce = -0.3f;
            float maxHorizontalForce = 0.3f;
            float verticalForce = 1f;

            Rigidbody2D keyRigidbody = keyInstance.GetComponent<Rigidbody2D>();

            if (keyRigidbody == null)
            {
                return;
            }

            Vector2 forceDirection = new Vector2(
                Random.Range(minHorizontalForce, maxHorizontalForce),
                verticalForce).normalized;

            keyRigidbody.AddForce(forceDirection * _keySpawnForce, ForceMode2D.Impulse);
        }

        private void PlaySound(AudioClip sound)
        {
            if (sound == null)
            {
                return;
            }

            if (_audioController != null)
            {
                _audioController.PlayOneShotWithVolume(sound, _soundVolume);
            }
            else if (CachedAudioSource != null)
            {
                CachedAudioSource.PlayOneShot(sound, _soundVolume);
            }
            else
            {
                AudioSource.PlayClipAtPoint(sound, transform.position, _soundVolume);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _checkRadius);

            if (_keyPrefab == null)
            {
                return;
            }

            Gizmos.color = Color.green;

            Vector3 spawnPosition = transform.position + (Vector3)_keySpawnOffset;

            Gizmos.DrawWireSphere(spawnPosition, GizmoSphereRadius);
            Gizmos.DrawLine(transform.position, spawnPosition);
        }
    }
}