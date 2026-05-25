using GeneralLogicEnemies;
using Shared.Damage;
using System.Collections;
using UnityEngine;

namespace MiddleBossLogic
{
    [RequireComponent(typeof(Animator))]
    public sealed class BossGolem : Entity, IDamageable
    {
        private const float MinimumHealthThreshold = 0f;
        private const float DefaultWaitDuration = 0f;
        private const float DestroyDelay = 1f;
        private const float DefaultSoundEffectVolume = 1f;

        [Header("Player Detection")]
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private float _detectionRadius = 1f;

        [Header("Movement and Attack")]
        [SerializeField] private float _moveSpeed = 2.5f;
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _attackCooldown = 2f;
        [SerializeField] private int _attackDamage = 1;

        [Header("Taking Damage")]
        [SerializeField] private float _hitStunDuration = 0.4f;

        [Header("Boss Music")]
        [SerializeField] private AudioClip _bossMusic;

        [Header("Boss Sound Effects")]
        [SerializeField] private AudioClip _attackSound;
        [SerializeField] private AudioClip _attackMissSound;
        [SerializeField] private AudioClip _takeDamageSound;
        [SerializeField] private AudioClip _deathSound;
        [SerializeField] private float _soundEffectVolume = DefaultSoundEffectVolume;

        private Animator _animator;
        private Rigidbody2D _rigidbody;
        private Transform _playerTransform;
        private AudioController _audioController;
        private BossHealthBar _healthBar;

        private float _nextAttackTime;
        private bool _isActivated;
        private bool _isHit;
        private bool _isDead;
        private bool _wasAttackSuccessful;
        private int StateParameterHash => Animator.StringToHash("state");

        private BossGolemState CurrentAnimatorState
        {
            get => (BossGolemState)_animator.GetInteger(StateParameterHash);
            set => _animator.SetInteger(StateParameterHash, (int)value);
        }

        private void Start()
        {
            _healthBar = GetComponent<BossHealthBar>();
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _audioController = FindFirstObjectByType<AudioController>();
        }

        private void Update()
        {
            if (_isDead)
            {
                return;
            }

            if (!_isActivated && IsPlayerOnTriggerPoint())
            {
                Activate();
            }

            if (!_isActivated || _isHit)
            {
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            if (distanceToPlayer <= _attackRange && Time.time >= _nextAttackTime)
            {
                StartCoroutine(AttackRoutine());

                return;
            }

            MoveTowardPlayer();
        }

        private bool IsPlayerOnTriggerPoint()
        {
            return Physics2D.OverlapCircle(_groundCheckPoint.position, _detectionRadius, _playerLayer);
        }

        private void Activate()
        {
            _isActivated = true;

            _playerTransform = Physics2D.OverlapCircle(_groundCheckPoint.position, _detectionRadius, _playerLayer).transform;

            StartBossMusic();
        }

        private void StartBossMusic()
        {
            if (_audioController != null && _bossMusic != null)
            {
                _audioController.PlayBossMusic(_bossMusic);
            }
        }

        private void StopBossMusic()
        {
            _audioController?.StopBossMusic();
        }

        private void MoveTowardPlayer()
        {
            float direction = Mathf.Sign(_playerTransform.position.x - transform.position.x);

            _rigidbody.velocity = new Vector2(direction * _moveSpeed, _rigidbody.velocity.y);

            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x) * direction,
                transform.localScale.y,
                transform.localScale.z
            );

            CurrentAnimatorState = BossGolemState.Move;
        }

        private IEnumerator AttackRoutine()
        {
            _rigidbody.velocity = Vector2.zero;
            CurrentAnimatorState = BossGolemState.Attack;

            _wasAttackSuccessful = false;

            yield break;
        }

        public override void TakeDamage(int amount)
        {
            if (_isDead || _isHit)
            {
                return;
            }

            PlayTakeDamageSound();
            StartCoroutine(HitRoutine());

            base.TakeDamage(amount);
        }

        public void DealDamage()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, _attackRange, _playerLayer);

            if (hit != null && hit.TryGetComponent(out Hero hero))
            {
                hero.TakeDamage(_attackDamage);

                _wasAttackSuccessful = true;
            }
        }

        private IEnumerator HitRoutine()
        {
            _isHit = true;

            _rigidbody.velocity = Vector2.zero;
            CurrentAnimatorState = BossGolemState.Hit;

            yield return new WaitForSeconds(_hitStunDuration);

            _isHit = false;
        }

        public override void Die()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;
            _isHit = true;

            _rigidbody.velocity = Vector2.zero;

            PlayDeathSound();
            StopBossMusic();

            base.Die();

            Destroy(gameObject, DestroyDelay);
        }

        private void PlayAttackSound()
        {
            if (_audioController == null)
            {
                return;
            }

            AudioClip clipToPlay = _attackSound;

            if (clipToPlay != null)
            {
                _audioController.PlayOneShotWithVolume(clipToPlay, _soundEffectVolume);
            }
        }

        private void PlayAttackMissSound()
        {
            if (_attackMissSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_attackMissSound, _soundEffectVolume);
            }
        }

        private void PlayTakeDamageSound()
        {
            if (_audioController == null)
            {
                return;
            }

            AudioClip clipToPlay = _takeDamageSound;

            if (clipToPlay != null)
            {
                _audioController.PlayOneShotWithVolume(clipToPlay, _soundEffectVolume);
            }
        }

        private void PlayDeathSound()
        {
            if (_deathSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_deathSound, _soundEffectVolume);
            }
        }

        public void AnimEvent_AttackHit()
        {
            PlayAttackSound();
            DealDamage();
        }

        public void AnimEvent_AttackEnd()
        {
            if (!_wasAttackSuccessful)
            {
                PlayAttackMissSound();
            }

            _nextAttackTime = Time.time + _attackCooldown;

            CurrentAnimatorState = BossGolemState.Idle;

            _wasAttackSuccessful = false;
        }

        public void AnimEvent_AttackStart()
        {
            PlayAttackSound();
        }

        public void AnimEvent_HitEnd()
        {
            _isHit = false;
        }

        public void AnimEvent_DeathEnd()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_isActivated)
            {
                StopBossMusic();
            }
        }

        private enum BossGolemState
        {
            Idle,
            Move,
            Attack,
            Hit
        }
    }
}