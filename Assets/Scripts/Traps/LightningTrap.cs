using UnityEngine;

namespace Traps
{
    public sealed class LightningTrap : MonoBehaviour
    {
        [Header("Trap Activation")]
        [SerializeField] private bool _activateOnTriggerEnter = true;
        [SerializeField] private LayerMask _activationLayer;
        [SerializeField] private string _playerTag = "Player";

        [Header("Trap Settings")]
        [SerializeField] private float _damage = 1;
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
        private SpriteRenderer _spriteRenderer;
        private Collider2D _damageCollider;
        private Transform _player;

        private bool _isActive = false;
        private Vector2 _damageAreaCenter;

        public bool IsActive => _isActive;
        public Vector2 DamageAreaCenter => _damageAreaCenter;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (_soundController == null)
            {
                _soundController = GetComponent<LightningTrapSoundController>();
                if (_soundController == null)
                {
                    _soundController = gameObject.AddComponent<LightningTrapSoundController>();
                }
            }

            InitializeDamageCollider();

            if (_usePositionActivation)
            {
                var playerObj = GameObject.FindGameObjectWithTag(_playerTag);

                if (playerObj != null)
                {
                    _player = playerObj.transform;
                }
            }

            CalculateDamageAreaCenter();
        }

        private void Start()
        {
            if (!_isActive)
            {
                _animator.enabled = false;
            }
        }

        private void Update()
        {
            CalculateDamageAreaCenter();

            if (_usePositionActivation && _player != null && !_isActive)
            {
                CheckPositionActivation();
            }
        }

        private void InitializeDamageCollider()
        {
            _damageCollider = GetComponent<Collider2D>();

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

        public void ActivateTrap()
        {
            if (_isActive)
            {
                return;
            }

            _isActive = true;
            _animator.enabled = true;

            _animator.SetTrigger("Activate");
        }

        public void ApplyDamage()
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(_damageAreaCenter, _damageAreaSize, 0f);

            foreach (var hit in hits)
            {
                if (hit.CompareTag(_playerTag))
                {
                    TryDamagePlayer(hit);
                }
            }
        }

        private void TryDamagePlayer(Collider2D playerCollider)
        {
            if (playerCollider.TryGetComponent<Hero>(out var hero))
            {
                hero.TakeDamage((int)_damage);
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

        public void AnimationEvent_ApplyDamage()
        {
            ApplyDamage();
        }

        public void AnimationEvent_PlayLightningSound()
        {
            if (_soundController != null)
            {
                _soundController.PlayLightningStrikeSound();
            }
        }

        public void AnimationEvent_LightningStrike()
        {
            if (_soundController != null)
            {
                _soundController.PlayLightningStrikeSound();
            }

            ApplyDamage();
        }

        public void Deactivate()
        {
            _isActive = false;
            _damageCollider.enabled = false;
            _animator.enabled = false;

            if (_soundController != null)
            {
                _soundController.StopSound();
            }
        }

        public void SetSoundController(LightningTrapSoundController controller)
        {
            _soundController = controller;
        }

        public void SetLightningSound(AudioClip lightningSound)
        {
            if (_soundController != null)
            {
                _soundController.SetLightningSound(lightningSound);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_damageAreaCenter, _damageAreaSize);
        }
    }
}