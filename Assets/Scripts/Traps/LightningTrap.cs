using Shared.Damage;
using UnityEngine;

namespace Traps
{
    public sealed class LightningTrap : MonoBehaviour
    {
        private const string ActivateTriggerName = "Activate";
        private const string PlayerTagDefault = "Player";
        private const float BoxRotationAngle = 0f;

        [Header("Trap Activation")]
        [SerializeField] private bool _activateOnTriggerEnter = true;
        [SerializeField] private LayerMask _activationLayer;
        [SerializeField] private string _playerTag = PlayerTagDefault;

        [Header("Trap Settings")]
        [SerializeField] private float _damage = 1f;
        [SerializeField] private Vector2 _damageAreaSize = new Vector2(1.5f, 5f);
        [SerializeField] private Vector2 _damageAreaOffset = Vector2.zero;

        [Header("Animation Events")]
        [SerializeField] private string _startDamageEventName = "StartDamage";
        [SerializeField] private string _endDamageEventName = "EndDamage";

        [Header("Sound Controller")]
        [SerializeField] private LightningTrapSoundController _soundController;

        [Header("Position Activation (Optional)")]
        [SerializeField] private bool _usePositionActivation = false;
        [SerializeField] private Transform _activationPoint;
        [SerializeField] private float _activationRange = 3f;

        private Animator _animator;
        private Collider2D _damageCollider;
        private Transform _player;

        private bool _isActive;
        private Vector2 _damageAreaCenter;

        public bool IsActive => _isActive;
        public Vector2 DamageAreaCenter => _damageAreaCenter;

        private void Awake()
        {
            InitializeComponents();
            InitializeSoundController();
            InitializeDamageCollider();
            InitializePlayerReference();
            CalculateDamageAreaCenter();
        }

        private void Start()
        {
            if (_animator != null)
            {
                _animator.enabled = false;
            }
        }

        private void Update()
        {
            if (_player == null && Hero.Instance != null)
            {
                _player = Hero.Instance.transform;
            }

            CalculateDamageAreaCenter();

            if (_usePositionActivation && _player != null && !_isActive)
            {
                CheckPositionActivation();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_activateOnTriggerEnter || _isActive)
            {
                return;
            }

            bool layerOk = IsActivationLayer(other.gameObject.layer);
            bool tagOk = other.CompareTag(_playerTag);

            if (layerOk && tagOk)
            {
                ActivateTrap();
            }
        }

        public void ActivateTrap()
        {
            if (_isActive)
            {
                return;
            }

            _isActive = true;

            if (_animator != null)
            {
                _animator.enabled = true;
                _animator.SetTrigger(ActivateTriggerName);
            }
        }

        public void ApplyDamage()
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                _damageAreaCenter,
                _damageAreaSize,
                BoxRotationAngle);

            foreach (Collider2D hit in hits)
            {
                if (hit == null || hit.CompareTag(_playerTag) == false)
                {
                    continue;
                }

                TryDamagePlayer(hit);
            }
        }

        public void SetDamageArea(Vector2 size, Vector2 offset)
        {
            _damageAreaSize = size;
            _damageAreaOffset = offset;

            if (_damageCollider is BoxCollider2D boxCollider)
            {
                boxCollider.size = size;
                boxCollider.offset = offset;
            }

            CalculateDamageAreaCenter();
        }

        public void Deactivate()
        {
            _isActive = false;

            if (_damageCollider != null)
            {
                _damageCollider.enabled = false;
            }

            if (_animator != null)
            {
                _animator.enabled = false;
            }

            _soundController?.StopSound();
        }

        public void SetSoundController(LightningTrapSoundController controller)
        {
            _soundController = controller;
        }

        public void SetLightningSound(AudioClip lightningSound)
        {
            _soundController?.SetLightningSound(lightningSound);
        }

        public void AnimationEvent_ApplyDamage()
        {
            ApplyDamage();
        }

        public void AnimationEvent_PlayLightningSound()
        {
            _soundController?.PlayLightningStrikeSound();
        }

        public void AnimationEvent_LightningStrike()
        {
            _soundController?.PlayLightningStrikeSound();
            ApplyDamage();
        }

        public void AnimationEvent_StartDamage()
        {
            if (_damageCollider != null)
            {
                _damageCollider.enabled = true;
            }
        }

        public void AnimationEvent_EndDamage()
        {
            if (_damageCollider != null)
            {
                _damageCollider.enabled = false;
            }
        }

        private void InitializeComponents()
        {
            _animator = GetComponent<Animator>();
            _damageCollider = GetComponent<Collider2D>();
        }

        private void InitializeSoundController()
        {
            if (_soundController == null)
            {
                _soundController = GetComponent<LightningTrapSoundController>();
            }

            if (_soundController == null)
            {
                _soundController = gameObject.AddComponent<LightningTrapSoundController>();
            }
        }

        private void InitializeDamageCollider()
        {
            if (_damageCollider == null)
            {
                _damageCollider = gameObject.AddComponent<BoxCollider2D>();
            }

            if (_damageCollider is BoxCollider2D boxCollider)
            {
                boxCollider.size = _damageAreaSize;
                boxCollider.offset = _damageAreaOffset;
                boxCollider.isTrigger = true;
            }

            _damageCollider.enabled = false;
        }

        private void InitializePlayerReference()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(_playerTag);

            if (playerObject != null)
            {
                _player = playerObject.transform;
            }
        }

        private void CalculateDamageAreaCenter()
        {
            _damageAreaCenter = (Vector2)transform.position + _damageAreaOffset;
        }

        private void CheckPositionActivation()
        {
            if (_activationPoint == null)
            {
                return;
            }

            float distanceToPlayer = Vector2.Distance(_activationPoint.position, _player.position);

            if (distanceToPlayer <= _activationRange)
            {
                ActivateTrap();
            }
        }

        private bool IsActivationLayer(int layer)
        {
            return (_activationLayer.value & (1 << layer)) != 0;
        }

        private void TryDamagePlayer(Collider2D playerCollider)
        {
            IDamageable damageable = playerCollider.GetComponent<IDamageable>();

            if (damageable == null)
            {
                damageable = playerCollider.GetComponentInParent<IDamageable>();
            }

            damageable?.TakeDamage(Mathf.RoundToInt(_damage));
        }

        private void OnDrawGizmosSelected()
        {
            CalculateDamageAreaCenter();

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_damageAreaCenter, _damageAreaSize);

            if (_activationPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_activationPoint.position, _activationRange);
            }
        }
    }
}