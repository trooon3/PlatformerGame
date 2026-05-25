using GeneralLogicEnemies;
using Player;
using Shared.Damage;
using System.Collections;
using UnityEngine;

namespace EasyBossLogic
{
    [RequireComponent(typeof(Animator))]
    public sealed class BossReaper : Entity, IDamageable
    {
        private const float InitialDodgeTime = -Mathf.Infinity;
        private const int IdleState = 0;
        private const int AttackState = 1;
        private const int SkillState = 2;
        private const int DeathState = 3;
        private const float DestroyDelay = 3f;
        private const float DefaultSoundEffectVolume = 1f;
        private const float AttackMissSoundVolumeMultiplier = 0.7f;

        [Header("Player Detection")]
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private float _detectionRadius = 1f;

        [Header("Movement and Attack")]
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _attackCooldown = 2f;
        [SerializeField] private int _attackDamage = 1;

        [Header("Evasion")]
        [SerializeField] private float _dodgeChance = 0.25f;
        [SerializeField] private float _dodgeDistance = 2f;
        [SerializeField] private float _dodgeDuration = 0.4f;
        [SerializeField] private float _dodgeCooldown = 1f;

        [Header("Boss Music")]
        [SerializeField] private AudioClip _bossMusic;

        [Header("Boss Sound Effects")]
        [SerializeField] private AudioClip _attackSound;
        [SerializeField] private AudioClip _attackMissSound;
        [SerializeField] private AudioClip _takeDamageSound;
        [SerializeField] private AudioClip _deathSound;
        [SerializeField] private AudioClip _dodgeSound;

        [SerializeField] private float _soundEffectVolume = DefaultSoundEffectVolume;

        private Animator _animator;
        private Rigidbody2D _rigidbody;
        private Transform _playerTransform;
        private AudioController _audioController;
        private BossHealthBar _healthBar;

        private bool _isActivated;
        private bool _isDead;
        private bool _isDodging;
        private float _lastDodgeTime = InitialDodgeTime;
        private float _nextAttackTime;

        private string StateParameter => "state";

        public float DetectionRadius => _detectionRadius;
        public float MoveSpeed => _moveSpeed;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public int AttackDamage => _attackDamage;

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

            if (!_isActivated)
            {
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            if (IsAttacking() && distanceToPlayer > _attackRange)
            {
                InterruptAttack();

                return;
            }

            if (!_isDodging && distanceToPlayer <= _attackRange && Time.time >= _nextAttackTime)
            {
                Attack();
            }

            if (!IsAttacking() && !_isDodging)
            {
                MoveTowardPlayer();
            }
            else
            {
                _rigidbody.velocity = new Vector2(0f, _rigidbody.velocity.y);
            }
        }

        private void InterruptAttack()
        {
            float interruptionCooldown = 0.5f;

            _animator.SetInteger(StateParameter, IdleState);

            _nextAttackTime = Time.time + interruptionCooldown;
        }

        public override void TakeDamage(int amount)
        {
            TryDodge();

            if (!_isDodging)
            {
                PlayTakeDamageSound();

                base.TakeDamage(amount);
            }
        }

        public void DealDamage()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            if (distanceToPlayer <= _attackRange && _playerTransform.TryGetComponent(out Hero hero))
            {
                hero.TakeDamage(_attackDamage);
            }
            else
            {
                PlayAttackMissSound();
            }
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
            if (_isDodging)
            {
                return;
            }

            float direction = Mathf.Sign(_playerTransform.position.x - transform.position.x);

            _rigidbody.velocity = new Vector2(direction * _moveSpeed, _rigidbody.velocity.y);

            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
        }

        private void Attack()
        {
            _nextAttackTime = Time.time + _attackCooldown;
            _animator.SetInteger(StateParameter, AttackState);

            PlayAttackSound();
        }

        private bool IsAttacking()
        {
            return _animator.GetInteger(StateParameter) == AttackState;
        }

        private IEnumerator DodgeRoutine()
        {
            _isDodging = true;

            _animator.SetInteger(StateParameter, SkillState);
            PlayDodgeSound();

            Vector2 direction = ((Vector2)transform.position - (Vector2)_playerTransform.position).normalized;
            Vector2 startPosition = transform.position;
            Vector2 targetPosition = startPosition + direction * _dodgeDistance;

            float elapsedTime = 0f;

            while (elapsedTime < _dodgeDuration)
            {
                Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime / _dodgeDuration);

                transform.position = new Vector2(newPosition.x, transform.position.y);

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            _isDodging = false;

            _animator.SetInteger(StateParameter, IdleState);
        }

        private void TryDodge()
        {
            if (_isDead || _isDodging || Time.time < _lastDodgeTime + _dodgeCooldown)
            {
                return;
            }

            if (Random.value <= _dodgeChance)
            {
                _lastDodgeTime = Time.time;

                StartCoroutine(DodgeRoutine());
            }
        }

        public void Die()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;

            _rigidbody.velocity = Vector2.zero;
            _animator.SetInteger(StateParameter, DeathState);

            PlayDeathSound();
            StopBossMusic();

            base.Die();

            Destroy(gameObject, DestroyDelay);
        }

        private void OnDestroy()
        {
            if (_isActivated)
            {
                StopBossMusic();
            }
        }

        private void PlayAttackSound()
        {
            if (_attackSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_attackSound, _soundEffectVolume);
            }
        }

        private void PlayAttackMissSound()
        {
            if (_attackMissSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_attackMissSound, _soundEffectVolume * AttackMissSoundVolumeMultiplier);
            }
        }

        private void PlayTakeDamageSound()
        {
            if (_takeDamageSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_takeDamageSound, _soundEffectVolume);
            }
        }

        private void PlayDeathSound()
        {
            if (_deathSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_deathSound, _soundEffectVolume);
            }
        }

        private void PlayDodgeSound()
        {
            if (_dodgeSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_dodgeSound, _soundEffectVolume);
            }
        }
    }
}